using System.Data;

using eGrants.Models;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

using Serilog;

namespace eGrants.Services.Interfaces
{
    public interface IDashboardService
    {
        /// <summary>
        /// Retrieves the total number of widgets available.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains the total widget count as a string.</returns>
        public Task<string> GetTotalWidgets();


        /// <summary>
        /// Loads widget assignments based on provided parameters.
        /// </summary>
        /// <param name="act">The action or category identifier.</param>
        /// <param name="idstr">The widget ID string.</param>
        /// <param name="ic">The internal code or context identifier.</param>
        /// <param name="userid">The user ID requesting the widgets.</param>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a list of widget assignments.</returns>
        public Task<List<WidgetAssigments>> LoadWidgets(string act, string idstr, string ic, string userid);


        /// <summary>
        /// Loads the widgets that a user has selected.
        /// </summary>
        /// <param name="userid">The user ID whose selected widgets are being retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a list of selected widgets.</returns>
        public Task<List<SelectedWidgets>> LoadSeletedWidgets(string userid);


        /// <summary>
        /// Saves the selected widgets for a given user.
        /// </summary>
        /// <param name="act">The action or category identifier.</param>
        /// <param name="idstr">The widget ID string.</param>
        /// <param name="ic">The internal code or context identifier.</param>
        /// <param name="userid">The user ID whose selections are being saved.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public Task save_selected(string act, string idstr, string ic, string userid);


        /// <summary>
        /// Loads grant data for "To-Go CC" type grants associated with a user.
        /// </summary>
        /// <param name="userid">The user ID requesting the grants.</param>
        /// <param name="type">The type of grant filter.</param>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a list of grant data.</returns>
        public Task<List<WidgetData>> LoadGrantsTogoCC(string userid, string type);


        /// <summary>
        /// Loads grant data for "To-Go NC" type grants associated with a user.
        /// </summary>
        /// <param name="userid">The user ID requesting the grants.</param>
        /// <param name="type">The type of grant filter.</param>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a list of grant data.</returns>
        public Task<List<WidgetData>> LoadGrantsTogoNC(string userid, string type);


        /// <summary>
        /// Loads expedited grants associated with a user.
        /// </summary>
        /// <param name="userid">The user ID requesting the expedited grants.</param>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a list of expedited grant data.</returns>
        public Task<List<WidgetData>> LoadGrantsExpedited(string userid);


        /// <summary>
        /// Loads delayed grants associated with a user.
        /// </summary>
        /// <param name="userid">The user ID requesting the delayed grants.</param>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a list of delayed grant data.</returns>
        public Task<List<WidgetData>> LoadGrantsDelayed(string userid);


        /// <summary>
        /// Loads new grants associated with a user.
        /// </summary>
        /// <param name="userid">The user ID requesting the new grants.</param>
        /// <param name="type">The type of grant filter.</param>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a list of new grant data.</returns>
        public Task<List<WidgetData>> LoadGrantsNew(string userid, string type);


        /// <summary>
        /// Loads the list of available links.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a list of link lists.</returns>
        public Task<List<LinkLists>> LoadLinkList();


        /// <summary>
        /// Loads average time statistics for a given user.
        /// </summary>
        /// <param name="userid">The user ID whose average time data is being retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a list of average time records.</returns>
        public Task<List<avgtime>> LoadAvgtime(string userid);


        /// <summary>
        /// Loads the current status of grants.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a list of grant statuses.</returns>
        public Task<List<GrantStatus>> LoadGrantsStatus();


        /// <summary>
        /// Loads audit report data.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. 
        /// The task result contains a list of audit reports.</returns>
        public Task<List<AuditReport>> LoadAuditReport();

    }
}
