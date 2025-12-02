using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface IManagementService
    {
        /// <summary>
        /// Loads the list of Quality Control (QC) reasons associated with the given identifier code.
        /// </summary>
        /// <param name="ic">Identifier code used to filter QC reasons.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of QCReasons.</returns>
        public Task<List<QCReasons>> LoadQCReasons(string ic);

        /// <summary>
        /// Retrieves the list of eGrants specialists associated with the given identifier code.
        /// </summary>
        /// <param name="ic">Identifier code used to filter specialists.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of EgrantsUsers.</returns>
        public Task<List<EgrantsUsers>> LoadSpecialists(string ic);

        /// <summary>
        /// Loads the list of QC persons associated with the given identifier code.
        /// </summary>
        /// <param name="ic">Identifier code used to filter QC persons.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of QCPersons.</returns>
        public Task<List<QCPersons>> LoadQCPersons(string ic);

        /// <summary>
        /// Retrieves QC reports associated with the given identifier code.
        /// </summary>
        /// <param name="ic">Identifier code used to filter QC reports.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of QCReports.</returns>
        public Task<List<QCReports>> LoadQCReport(string ic);

        /// <summary>
        /// Executes a database operation related to QC management.
        /// </summary>
        /// <param name="act">The action to perform (e.g., insert, update, delete).</param>
        /// <param name="qcPersonId">The identifier of the QC person involved in the operation.</param>
        /// <param name="qcReason">The QC reason associated with the operation.</param>
        /// <param name="percent">The percentage value relevant to the QC operation.</param>
        /// <param name="personId">The identifier of the person associated with the operation.</param>
        /// <param name="ic">Identifier code used to scope the operation.</param>
        /// <param name="userId">The identifier of the user performing the operation.</param>
        public Task run_db(string act, int qcPersonId, string qcReason, int percent, int personId, string ic, string userId);

        /// <summary>
        /// Loads the transaction history for a given document type and person.
        /// </summary>
        /// <param name="transactionType">The type of transaction to filter (e.g., "create", "update").</param>
        /// <param name="personId">The unique identifier of the person associated with the transactions.</param>
        /// <param name="startDate">The start date (inclusive) for filtering transaction history.</param>
        /// <param name="endDate">The end date (exclusive) for filtering transaction history.</param>
        /// <param name="dateRange">Optional date range descriptor (e.g., "last30days").</param>
        /// <param name="ic">The institution or context code used for scoping the query.</param>
        /// <param name="userId">The identifier of the user requesting the data.</param>
        /// <returns>A task representing the asynchronous operation, with a list of <see cref="DocTransactionHistory"/> records.</returns>
        public Task<List<DocTransactionHistory>> LoadDocTransactionHistory(
            string transactionType,
            int personId,
            string startDate,
            string endDate,
            string dateRange,
            string ic,
            string userId);


        /// <summary>
        /// Retrieves accession records for the specified institution or context.
        /// </summary>
        /// <param name="ic">The institution or context code used for scoping the query.</param>
        /// <returns>A task representing the asynchronous operation, with a list of <see cref="EgrantAccessions"/> records.</returns>
        public Task<List<EgrantAccessions>> LoadAccessions(string ic);


        /// <summary>
        /// Loads folder information based on activity type and search criteria.
        /// </summary>
        /// <param name="act">The activity type or action code used to filter folders.</param>
        /// <param name="searchNumber">The numeric search key used to refine results.</param>
        /// <param name="ic">The institution or context code used for scoping the query.</param>
        /// <param name="userId">The identifier of the user requesting the data.</param>
        /// <returns>A task representing the asynchronous operation, with a list of <see cref="EgrantFolders"/> records.</returns>
        public Task<List<EgrantFolders>> LoadFolders(
            string act,
            int searchNumber,
            string ic,
            string userId);

    }

}

