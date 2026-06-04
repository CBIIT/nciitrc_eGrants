using eGrants.Models;
using Microsoft.AspNetCore.Http;

namespace eGrants.Services.Interfaces
{
    /// <summary>
    /// Service for handling out-of-process file uploads with progress tracking.
    /// </summary>
    public interface IBackgroundFileUploadService
    {
        /// <summary>
        /// Queues a file upload for background processing.
        /// </summary>
        /// <param name="uploadId">Unique identifier for this upload session.</param>
        /// <param name="file">The file to upload.</param>
        /// <param name="uploadContext">Context information for the upload (doc_id, appl_id, etc.).</param>
        /// <returns>Task representing the queued operation.</returns>
        Task QueueFileUpload(string uploadId, IFormFile file, FileUploadContext uploadContext);

        /// <summary>
        /// Gets the current status of an upload.
        /// </summary>
        /// <param name="uploadId">The upload identifier.</param>
        /// <returns>Current upload progress, or null if not found.</returns>
        Task<FileUploadProgress> GetUploadStatus(string uploadId);

        /// <summary>
        /// Cancels an in-progress upload.
        /// </summary>
        /// <param name="uploadId">The upload identifier.</param>
        /// <returns>True if cancellation succeeded, false otherwise.</returns>
        Task<bool> CancelUpload(string uploadId);
    }

    /// <summary>
    /// Context information for a file upload operation.
    /// </summary>
    public class FileUploadContext
    {
        /// <summary>
        /// Document ID (for uploads replacing existing documents).
        /// </summary>
        public int? DocId { get; set; }

        /// <summary>
        /// Application ID (for new document creation).
        /// </summary>
        public int? ApplId { get; set; }

        /// <summary>
        /// Category ID for the document.
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// Sub-category name.
        /// </summary>
        public string SubCategory { get; set; }

        /// <summary>
        /// Document date.
        /// </summary>
        public DateTime? DocDate { get; set; }

        /// <summary>
        /// Admin code.
        /// </summary>
        public string AdminCode { get; set; }

        /// <summary>
        /// Serial number.
        /// </summary>
        public int? SerialNum { get; set; }

        /// <summary>
        /// Institution code.
        /// </summary>
        public string Ic { get; set; }

        /// <summary>
        /// User ID.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Upload type: "create" or "replace".
        /// </summary>
        public string UploadType { get; set; }

        /// <summary>
        /// Whether this is a PDF conversion upload.
        /// </summary>
        public bool ConvertToPdf { get; set; }

        /// <summary>
        /// Web grant URL for file storage.
        /// </summary>
        public string WebGrantUrl { get; set; }

        /// <summary>
        /// Image server URL for document access.
        /// </summary>
        public string ImageServerUrl { get; set; }

        /// <summary>
        /// Relative path for new documents.
        /// </summary>
        public string EgrantsDocNewRelativePath { get; set; }

        /// <summary>
        /// Relative path for modified documents.
        /// </summary>
        public string EgrantsDocModifyRelativePath { get; set; }
    }
}