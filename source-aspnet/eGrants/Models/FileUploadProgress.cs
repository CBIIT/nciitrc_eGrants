namespace eGrants.Models
{
    /// <summary>
    /// Represents the status and progress of a file upload operation.
    /// </summary>
    public class FileUploadProgress
    {
        /// <summary>
        /// Unique identifier for the upload session.
        /// </summary>
        public string UploadId { get; set; }

        /// <summary>
        /// Name of the file being uploaded.
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Total file size in bytes.
        /// </summary>
        public long TotalBytes { get; set; }

        /// <summary>
        /// Number of bytes uploaded so far.
        /// </summary>
        public long BytesUploaded { get; set; }

        /// <summary>
        /// Upload progress percentage (0-100).
        /// </summary>
        public int PercentComplete => TotalBytes > 0 ? (int)((BytesUploaded * 100) / TotalBytes) : 0;

        /// <summary>
        /// Current status of the upload.
        /// </summary>
        public UploadStatus Status { get; set; }

        /// <summary>
        /// Optional message (e.g., error description or completion message).
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// URL of the uploaded document (populated after successful upload).
        /// </summary>
        public string DocumentUrl { get; set; }

        /// <summary>
        /// Document ID (populated after successful upload).
        /// </summary>
        public int? DocumentId { get; set; }

        /// <summary>
        /// Timestamp when upload started.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Timestamp when upload completed (or failed).
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Duration of the upload in seconds.
        /// </summary>
        public double? DurationSeconds => EndTime.HasValue ? (EndTime.Value - StartTime).TotalSeconds : null;
    }

    /// <summary>
    /// Enumeration of possible upload statuses.
    /// </summary>
    public enum UploadStatus
    {
        /// <summary>
        /// Upload queued for processing.
        /// </summary>
        Queued,

        /// <summary>
        /// Upload in progress.
        /// </summary>
        Uploading,

        /// <summary>
        /// Upload completed successfully.
        /// </summary>
        Completed,

        /// <summary>
        /// Upload failed with an error.
        /// </summary>
        Failed,

        /// <summary>
        /// Upload was cancelled by the user.
        /// </summary>
        Cancelled
    }
}