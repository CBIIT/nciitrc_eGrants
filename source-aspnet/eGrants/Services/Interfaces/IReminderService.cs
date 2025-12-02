using eGrants.Models;

namespace eGrants.Services.Interfaces
{
    public interface IReminderService
    {
        /// <summary>
        /// Loads all application records associated with the given serial number.
        /// </summary>
        /// <param name="serial_num">The unique serial number used to filter applications.</param>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a list of application objects.</returns>
        public Task<List<Appls>> LoadAppls(int serial_num);


        /// <summary>
        /// Loads a specific application record based on its unique application ID.
        /// </summary>
        /// <param name="appl_id">The unique identifier of the application to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a list with the selected application object(s).</returns>
        public Task<List<Appls>> LoadSelectedAppl(int appl_id);


        /// <summary>
        /// Executes a database operation for a given application event.
        /// </summary>
        /// <param name="event_type">The type of event to process (e.g., insert, update, delete).</param>
        /// <param name="appl_id">The unique identifier of the application affected.</param>
        /// <param name="effective_date">The effective date of the event in string format.</param>
        /// <param name="reminder_text">Optional reminder text associated with the event.</param>
        /// <param name="by_email">Flag or value indicating whether notification should be sent by email.</param>
        /// <param name="by_display">Flag or value indicating whether notification should be displayed in-app.</param>
        /// <param name="userid">The identifier of the user performing the operation.</param>
        /// <returns>A task that represents the asynchronous database operation.</returns>
        public Task run_db(string event_type, int appl_id, string effective_date, string reminder_text, string by_email, string by_display, string userid);
    }
}
