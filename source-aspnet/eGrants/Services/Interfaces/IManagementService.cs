using System.Data;

using eGrants.Models;
using eGrants.ViewModels;

using Microsoft.Data.SqlClient;

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
    }

}

