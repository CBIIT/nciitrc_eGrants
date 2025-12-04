using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface IFlagMaintenanceService
    {
        /// <summary>
        ///     The load flag types.
        /// </summary>
        /// <returns>
        ///     The <see cref="System.Collections.Generic.List`1" /> .
        /// </returns>
        public List<FlagTypes> LoadFlagTypes();

        // load flags
        /// <summary>
        /// The load flags.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="flag_type">
        /// The flag_type.
        /// </param>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <param name="id_string">
        /// The id_string.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="System.Collections.Generic.List`1"/> .
        /// </returns>
        public List<Flags> LoadFlags(
            string act,
            string flag_type,
            string admin_code,
            int serial_num,
            string id_string,
            string ic,
            string userid);

        // add, delete or edit flag
        /// <summary>
        /// The run_db.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="flag_type">
        /// The flag_type.
        /// </param>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <param name="id_string">
        /// The id_string.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        public void run_db(string act, string flag_type, string admin_code, int serial_num, string id_string, string ic, string userid);

        /// <summary>
        /// The load appls.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="flag_type">
        /// The flag_type.
        /// </param>
        /// <param name="admin_code">
        /// The admin_code.
        /// </param>
        /// <param name="serial_num">
        /// The serial_num.
        /// </param>
        /// <param name="id_string">
        /// The id_string.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="System.Collections.Generic.List`1"/> .
        /// </returns>
        public List<ApplFlags> LoadAppls(
            string act,
            string flag_type,
            string admin_code,
            int serial_num,
            string id_string,
            string ic,
            string userid);
    }
}