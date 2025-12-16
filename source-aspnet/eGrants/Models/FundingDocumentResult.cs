namespace eGrants.Models
{
    /// <summary>
    /// Result model for funding document operations
    /// </summary>
    public class FundingDocumentResult
    {
        public bool Success { get; set; }
        public string Url { get; set; }
        public string Message { get; set; }
        public int? DocumentId { get; set; }
    }
}