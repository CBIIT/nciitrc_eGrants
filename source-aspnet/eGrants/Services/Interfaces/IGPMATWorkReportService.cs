using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface IGPMATWorkReportService
    {
        /// <summary>
        /// The load reports.
        /// </summary>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<PMATWorkReports> LoadReports(string ic, string userid);
    }
}
