using eGrants.Hubs;
using eGrants.Models;
using eGrants.Services.Interfaces;
using EmailConcatenation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Serilog;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Channels;

namespace eGrants.Services
{
    /// <summary>
    /// Background service for processing file uploads with SignalR progress notifications.
    /// </summary>
    public class BackgroundFileUploadService : BackgroundService, IBackgroundFileUploadService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<BackgroundFileUploadService> _logger;
        private readonly IHubContext<FileUploadHub> _hubContext;
        private readonly ConcurrentDictionary<string, FileUploadProgress> _uploadStatuses = new();
        private readonly Channel<(string uploadId, string tempPath, string fileName, long fileSize, FileUploadContext context)> _uploadQueue;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _cancellationTokens = new();

        public BackgroundFileUploadService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<BackgroundFileUploadService> logger,
            IHubContext<FileUploadHub> hubContext)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
            _hubContext = hubContext;

            // Create unbounded channel for upload queue
            _uploadQueue = Channel.CreateUnbounded<(string, string, string, long, FileUploadContext)>(new UnboundedChannelOptions
            {
                SingleReader = false, // Allow multiple background workers
                SingleWriter = false
            });
        }

        /// <summary>
        /// Queues a file upload for background processing.
        /// </summary>
        public async Task QueueFileUpload(string uploadId, IFormFile file, FileUploadContext uploadContext)
        {
            var progress = new FileUploadProgress
            {
                UploadId = uploadId,
                FileName = file.FileName,
                TotalBytes = file.Length,
                BytesUploaded = 0,
                Status = UploadStatus.Queued,
                StartTime = DateTime.UtcNow,
                Message = "Upload queued for processing..."
            };

            _uploadStatuses[uploadId] = progress;

            // Create cancellation token for this upload
            var cts = new CancellationTokenSource();
            _cancellationTokens[uploadId] = cts;

            // Notify client of queued status
            await _hubContext.Clients.Group(uploadId).SendAsync("UploadProgress", progress);

            // =====================================================================
            // FIX: Save file to temp location BEFORE queuing
            // IFormFile is disposed when HTTP request completes, so we must
            // copy the file data synchronously before the controller returns.
            // =====================================================================
            var tempPath = Path.Combine(Path.GetTempPath(), $"{uploadId}_{file.FileName}");
            
            try
            {
                using (var tempStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
                using (var uploadStream = file.OpenReadStream())
                {
                    await uploadStream.CopyToAsync(tempStream);
                }
                
                Log.Information("File copied to temp storage. UploadId: {UploadId}, TempPath: {TempPath}", uploadId, tempPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to copy file to temp storage. UploadId: {UploadId}", uploadId);
                
                progress.Status = UploadStatus.Failed;
                progress.Message = $"Failed to queue upload: {ex.Message}";
                progress.EndTime = DateTime.UtcNow;
                
                await _hubContext.Clients.Group(uploadId).SendAsync("UploadProgress", progress);
                throw;
            }

            // Queue the upload with temp path instead of IFormFile
            await _uploadQueue.Writer.WriteAsync((uploadId, tempPath, file.FileName, file.Length, uploadContext));

            Log.Information("File upload queued. UploadId: {UploadId}, FileName: {FileName}, Size: {Size}",
                uploadId, file.FileName, file.Length);
        }

        /// <summary>
        /// Gets current upload status.
        /// </summary>
        public Task<FileUploadProgress> GetUploadStatus(string uploadId)
        {
            _uploadStatuses.TryGetValue(uploadId, out var progress);
            return Task.FromResult(progress);
        }

        /// <summary>
        /// Cancels an in-progress upload.
        /// </summary>
        public Task<bool> CancelUpload(string uploadId)
        {
            if (_cancellationTokens.TryRemove(uploadId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();

                if (_uploadStatuses.TryGetValue(uploadId, out var progress))
                {
                    progress.Status = UploadStatus.Cancelled;
                    progress.Message = "Upload cancelled by user.";
                    progress.EndTime = DateTime.UtcNow;

                    _ = _hubContext.Clients.Group(uploadId).SendAsync("UploadProgress", progress);
                }

                Log.Information("Upload cancelled. UploadId: {UploadId}", uploadId);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        /// <summary>
        /// Background worker that processes queued uploads.
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("BackgroundFileUploadService started.");

            // Start multiple workers for parallel processing
            var workerTasks = Enumerable.Range(0, Environment.ProcessorCount)
                .Select(_ => ProcessUploadsAsync(stoppingToken))
                .ToArray();

            await Task.WhenAll(workerTasks);

            Log.Information("BackgroundFileUploadService stopped.");
        }

        /// <summary>
        /// Worker method that continuously processes uploads from the queue.
        /// </summary>
        private async Task ProcessUploadsAsync(CancellationToken stoppingToken)
        {
            await foreach (var (uploadId, tempPath, fileName, fileSize, context) in _uploadQueue.Reader.ReadAllAsync(stoppingToken))
            {
                if (!_cancellationTokens.TryGetValue(uploadId, out var cts))
                {
                    // Upload was cancelled - cleanup temp file
                    try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
                    continue;
                }

                try
                {
                    using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cts.Token, stoppingToken);
                    await ProcessUploadAsync(uploadId, tempPath, fileName, fileSize, context, combinedCts.Token);
                }
                catch (OperationCanceledException)
                {
                    Log.Information("Upload cancelled during processing. UploadId: {UploadId}", uploadId);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to process upload. UploadId: {UploadId}", uploadId);

                    if (_uploadStatuses.TryGetValue(uploadId, out var progress))
                    {
                        progress.Status = UploadStatus.Failed;
                        progress.Message = $"Upload failed: {ex.Message}";
                        progress.EndTime = DateTime.UtcNow;

                        await _hubContext.Clients.Group(uploadId).SendAsync("UploadProgress", progress, stoppingToken);
                    }
                }
                finally
                {
                    // Cleanup
                    _cancellationTokens.TryRemove(uploadId, out var removedCts);
                    removedCts?.Dispose();
                    
                    // Delete temp file
                    try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { }
                }
            }
        }

        /// <summary>
        /// Processes a single file upload with progress tracking.
        /// </summary>
        private async Task ProcessUploadAsync(
            string uploadId, 
            string tempPath, 
            string fileName, 
            long fileSize, 
            FileUploadContext context, 
            CancellationToken cancellationToken)
        {
            if (!_uploadStatuses.TryGetValue(uploadId, out var progress))
            {
                return;
            }

            progress.Status = UploadStatus.Uploading;
            progress.Message = "Processing file...";
            await _hubContext.Clients.Group(uploadId).SendAsync("UploadProgress", progress, cancellationToken);

            try
            {
                // File is already saved to temp location, simulate progress by reading it
                await using (var fileStream = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true))
                {
                    var buffer = new byte[81920]; // 80KB buffer
                    int bytesRead;
                    int lastReportedPercent = 0;

                    while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                    {
                        progress.BytesUploaded += bytesRead;
                        progress.Message = $"Processing... {progress.PercentComplete}%";

                        // Send progress update every 5%
                        int currentPercent = progress.PercentComplete;
                        if (currentPercent >= lastReportedPercent + 5)
                        {
                            await _hubContext.Clients.Group(uploadId).SendAsync("UploadProgress", progress, cancellationToken);
                            lastReportedPercent = currentPercent;
                            
                            Log.Debug("Upload progress: {UploadId} - {Percent}% ({BytesUploaded}/{TotalBytes} bytes)",
                                uploadId, currentPercent, progress.BytesUploaded, progress.TotalBytes);
                        }
                    }
                }

                // Ensure we send 100% before final processing
                progress.BytesUploaded = progress.TotalBytes;
                progress.Message = "Upload complete. Finalizing...";
                await _hubContext.Clients.Group(uploadId).SendAsync("UploadProgress", progress, cancellationToken);

                Log.Information("File processing completed, moving to final destination. UploadId: {UploadId}", uploadId);

                // Process the uploaded file
                var result = await ProcessUploadedFileAsync(tempPath, fileName, context, cancellationToken);

                // Update progress with result
                progress.Status = UploadStatus.Completed;
                progress.Message = result.Message;
                progress.DocumentUrl = result.Url;
                progress.DocumentId = result.DocumentId;
                progress.EndTime = DateTime.UtcNow;

                await _hubContext.Clients.Group(uploadId).SendAsync("UploadProgress", progress, cancellationToken);

                Log.Information("Upload completed successfully. UploadId: {UploadId}, Duration: {Duration}s, DocumentId: {DocumentId}",
                    uploadId, progress.DurationSeconds, result.DocumentId);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error processing upload. UploadId: {UploadId}", uploadId);

                progress.Status = UploadStatus.Failed;
                progress.Message = $"Upload failed: {ex.Message}";
                progress.EndTime = DateTime.UtcNow;

                await _hubContext.Clients.Group(uploadId).SendAsync("UploadProgress", progress, cancellationToken);
                throw;
            }
        }

        /// <summary>
        /// Processes the uploaded file (saves to final destination).
        /// </summary>
        private async Task<(string Url, string Message, int? DocumentId)> ProcessUploadedFileAsync(
            string tempPath,
            string originalFileName,
            FileUploadContext context,
            CancellationToken cancellationToken)
        {
            var fileExtension = Path.GetExtension(originalFileName);
            string docName;
            string url;
            int? documentId = null;

            if (context.UploadType == "replace")
            {
                // Replacing existing document
                // Validate required fields for replacement
                if (!context.DocId.HasValue)
                {
                    throw new InvalidOperationException("DocId is required for replace operations.");
                }

                documentId = context.DocId;
                docName = $"{documentId}{fileExtension}";

                // Update document metadata
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var documentService = scope.ServiceProvider.GetRequiredService<IDocumentService>();
                    documentService.DocModify(
                        "to_upload",
                        0,
                        0,
                        string.Empty,
                        string.Empty,
                        documentId.ToString(),
                        fileExtension,
                        context.Ic,
                        context.UserId);
                }

#if DEBUG
                var fileFolder = @"C:\PdfFileOutput\";
#else
                var fileFolder = @"\\" + context.WebGrantUrl + "\\egrants\\funded\\nci\\modify\\";
#endif

                var filePath = Path.Combine(fileFolder, docName);

                // Copy from temp to final location
                File.Copy(tempPath, filePath, overwrite: true);

                url = context.ImageServerUrl + context.EgrantsDocModifyRelativePath + docName;
            }
            else if (context.UploadType == "create")
            {
                // Creating new document
                // Validate required fields for creation
                if (!context.ApplId.HasValue || !context.CategoryId.HasValue || !context.DocDate.HasValue)
                {
                    throw new InvalidOperationException(
                        "ApplId, CategoryId, and DocDate are required for create operations.");
                }

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var documentService = scope.ServiceProvider.GetRequiredService<IDocumentService>();
                    documentId = documentService.GetDocID(
                        context.ApplId.Value,
                        context.CategoryId.Value,
                        context.SubCategory ?? string.Empty,
                        context.DocDate.Value,
                        fileExtension,
                        context.Ic,
                        context.UserId);
                }

                docName = $"{documentId}{fileExtension}";

#if DEBUG
                var fileFolder = @"C:\PdfFileOutput\";
#else
                var fileFolder = @"\\" + context.WebGrantUrl + "\\egrants\\funded2\\nci\\main\\";
#endif

                var filePath = Path.Combine(fileFolder, docName);

                // Copy from temp to final location
                File.Copy(tempPath, filePath, overwrite: true);

                url = context.ImageServerUrl + context.EgrantsDocNewRelativePath + docName;
            }
            else
            {
                throw new InvalidOperationException($"Unknown upload type: {context.UploadType}");
            }

            return (url, "Done! Document has been uploaded successfully.", documentId);
        }
    }
}