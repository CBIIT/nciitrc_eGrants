using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface IApplDestructedService
    {
        /// <summary>
        /// The load years.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<DestructionYears> LoadYears();

        /// <summary>
        /// The load descrip codes.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<DescripCodes> LoadDescripCodes();

        /// <summary>
        /// The load exception codes.
        /// </summary>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<ExceptionCodes> LoadExceptionCodes();

        /// <summary>
        /// The load appls.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="year">
        /// The year.
        /// </param>
        /// <param name="status_code">
        /// The status_code.
        /// </param>
        /// <param name="exception_code">
        /// The exception_code.
        /// </param>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <param name="id_string">
        /// The id_string.
        /// </param>
        /// <param name="exception_type">
        /// The exception_type.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<DestructedsAppls> LoadAppls(
            string act,
            int year,
            string status_code,
            string exception_code,
            string str,
            string id_string,
            string exception_type,
            string ic,
            string userid);

        /// <summary>
        /// The load search info.
        /// </summary>
        /// <param name="year">
        /// The year.
        /// </param>
        /// <param name="status_code">
        /// The status_code.
        /// </param>
        /// <param name="exception_code">
        /// The exception_code.
        /// </param>
        /// <param name="str">
        /// The str.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public List<SearchInfo> LoadSearchInfo(int year, string status_code, string exception_code, string str);

        /// <summary>
        /// The check permission.
        /// </summary>
        /// <param name="year">
        /// The year.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string CheckPermission(int year, string userid);

        /// <summary>
        /// The edit exception code.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="detail">
        /// The detail.
        /// </param>
        /// <param name="code">
        /// The code.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        public void EditExceptionCode(string act, int id, string detail, string code, string ic, string userid);
    }
}