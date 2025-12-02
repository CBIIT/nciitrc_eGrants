using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface IEgrantsAccessService
    {
        // return user list
        /// <summary>
        /// The load users.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="index_id">
        /// The index_id.
        /// </param>
        /// <param name="active_id">
        /// The active_id.
        /// </param>
        /// <param name="user_id">
        /// The user_id.
        /// </param>
        /// <param name="login_id">
        /// The login_id.
        /// </param>
        /// <param name="last_name">
        /// The last_name.
        /// </param>
        /// <param name="first_name">
        /// The first_name.
        /// </param>
        /// <param name="middle_name">
        /// The middle_name.
        /// </param>
        /// <param name="email_address">
        /// The email_address.
        /// </param>
        /// <param name="phone_number">
        /// The phone_number.
        /// </param>
        /// <param name="coordinator_id">
        /// The coordinator_id.
        /// </param>
        /// <param name="position_id">
        /// The position_id.
        /// </param>
        /// <param name="ic_id">
        /// The ic_id.
        /// </param>
        /// <param name="egrants_tab">
        /// The egrants_tab.
        /// </param>
        /// <param name="mgt_tab">
        /// The mgt_tab.
        /// </param>
        /// <param name="admin_tab">
        /// The admin_tab.
        /// </param>
        /// <param name="docman_tab">
        /// The docman_tab.
        /// </param>
        /// <param name="cft_tab">
        /// The cft_tab.
        /// </param>
        /// <param name="dashboard_tab">
        /// The dashboard_tab.
        /// </param>
        /// <param name="iccoord_tab">
        /// The iccoord_tab.
        /// </param>
        /// <param name="is_coordinator">
        /// The is_coordinator.
        /// </param>
        /// <param name="end_date">
        /// The end_date.
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
        /// 
        public List<EgrantsUsers> LoadUsers(
            string act,
            int index_id,
            int active_id,
            int user_id,
            string login_id,
            string last_name,
            string first_name,
            string middle_name,
            string email_address,
            string phone_number,
            int coordinator_id,
            int position_id,
            int ic_id,
            int egrants_tab,
            int mgt_tab,
            int admin_tab,
            int docman_tab,
            int cft_tab,
            int dashboard_tab,
            int iccoord_tab,
            int is_coordinator,
            string end_date,
            string ic,
            string userid);

        // check userid if exists in the system
        /// <summary>
        /// The to check userid.
        /// </summary>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int ToCheckUserid(string userid);

        /// <summary>
        /// The run_db.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="index_id">
        /// The index_id.
        /// </param>
        /// <param name="active_id">
        /// The active_id.
        /// </param>
        /// <param name="user_id">
        /// The user_id.
        /// </param>
        /// <param name="login_id">
        /// The login_id.
        /// </param>
        /// <param name="last_name">
        /// The last_name.
        /// </param>
        /// <param name="first_name">
        /// The first_name.
        /// </param>
        /// <param name="middle_name">
        /// The middle_name.
        /// </param>
        /// <param name="email_address">
        /// The email_address.
        /// </param>
        /// <param name="phone_number">
        /// The phone_number.
        /// </param>
        /// <param name="coordinator_id">
        /// The coordinator_id.
        /// </param>
        /// <param name="position_id">
        /// The position_id.
        /// </param>
        /// <param name="ic_id">
        /// The ic_id.
        /// </param>
        /// <param name="egrants_tab">
        /// The egrants_tab.
        /// </param>
        /// <param name="mgt_tab">
        /// The mgt_tab.
        /// </param>
        /// <param name="admin_tab">
        /// The admin_tab.
        /// </param>
        /// <param name="docman_tab">
        /// The docman_tab.
        /// </param>
        /// <param name="cft_tab">
        /// The cft_tab.
        /// </param>
        /// <param name="dashboard_tab">
        /// The dashboard_tab.
        /// </param>
        /// <param name="iccoord_tab">
        /// The iccoord_tab.
        /// </param>
        /// <param name="is_coordinator">
        /// The is_coordinator.
        /// </param>
        /// <param name="end_date">
        /// The end_date.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        public void run_db(
            string act,
            int index_id,
            int active_id,
            int user_id,
            string login_id,
            string last_name,
            string first_name,
            string middle_name,
            string email_address,
            string phone_number,
            int coordinator_id,
            int position_id,
            int ic_id,
            int egrants_tab,
            int mgt_tab,
            int admin_tab,
            int docman_tab,
            int cft_tab,
            int dashboard_tab,
            int iccoord_tab,
            int is_coordinator,
            string end_date,
            string ic,
            string userid);

        // to prevent user data duplicate, before create new or update, check user data and get return notice
        /// <summary>
        /// The to_preview.
        /// </summary>
        /// <param name="act">
        /// The act.
        /// </param>
        /// <param name="index_id">
        /// The index_id.
        /// </param>
        /// <param name="active_id">
        /// The active_id.
        /// </param>
        /// <param name="user_id">
        /// The user_id.
        /// </param>
        /// <param name="login_id">
        /// The login_id.
        /// </param>
        /// <param name="last_name">
        /// The last_name.
        /// </param>
        /// <param name="first_name">
        /// The first_name.
        /// </param>
        /// <param name="middle_name">
        /// The middle_name.
        /// </param>
        /// <param name="email_address">
        /// The email_address.
        /// </param>
        /// <param name="phone_number">
        /// The phone_number.
        /// </param>
        /// <param name="coordinator_id">
        /// The coordinator_id.
        /// </param>
        /// <param name="position_id">
        /// The position_id.
        /// </param>
        /// <param name="ic_id">
        /// The ic_id.
        /// </param>
        /// <param name="egrants_tab">
        /// The egrants_tab.
        /// </param>
        /// <param name="mgt_tab">
        /// The mgt_tab.
        /// </param>
        /// <param name="admin_tab">
        /// The admin_tab.
        /// </param>
        /// <param name="docman_tab">
        /// The docman_tab.
        /// </param>
        /// <param name="cft_tab">
        /// The cft_tab.
        /// </param>
        /// <param name="dashboard_tab">
        /// The dashboard_tab.
        /// </param>
        /// <param name="iccoord_tab">
        /// The iccoord_tab.
        /// </param>
        /// <param name="is_coordinator">
        /// The is_coordinator.
        /// </param>
        /// <param name="end_date">
        /// The end_date.
        /// </param>
        /// <param name="ic">
        /// The ic.
        /// </param>
        /// <param name="userid">
        /// The userid.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public string to_preview(
            string act,
            int index_id,
            int active_id,
            int user_id,
            string login_id,
            string last_name,
            string first_name,
            string middle_name,
            string email_address,
            string phone_number,
            int coordinator_id,
            int position_id,
            int ic_id,
            int egrants_tab,
            int mgt_tab,
            int admin_tab,
            int docman_tab,
            int cft_tab,
            int dashboard_tab,
            int iccoord_tab,
            int is_coordinator,
            string end_date,
            string ic,
            string userid);

        /// <summary>
        /// The get character index.
        /// </summary>
        /// <param name="first_letter">
        /// The first_letter.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int getCharacterIndex(string first_letter);
    }
}