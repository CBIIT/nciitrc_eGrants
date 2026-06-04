using Microsoft.AspNetCore.SignalR;
using Serilog;

namespace eGrants.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time file upload progress notifications.
    /// Allows clients to receive updates about upload progress, completion, and errors.
    /// </summary>
    public class FileUploadHub : Hub
    {
        /// <summary>
        /// Called when a client connects to the hub.
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            Log.Information("Client connected to FileUploadHub. ConnectionId: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects from the hub.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            Log.Information("Client disconnected from FileUploadHub. ConnectionId: {ConnectionId}", Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Allows client to join a specific upload group for targeted notifications.
        /// </summary>
        /// <param name="uploadId">The unique identifier for the upload session.</param>
        public async Task JoinUploadGroup(string uploadId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, uploadId);
            Log.Information("Client {ConnectionId} joined upload group {UploadId}", Context.ConnectionId, uploadId);
        }

        /// <summary>
        /// Allows client to leave a specific upload group.
        /// </summary>
        /// <param name="uploadId">The unique identifier for the upload session.</param>
        public async Task LeaveUploadGroup(string uploadId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, uploadId);
            Log.Information("Client {ConnectionId} left upload group {UploadId}", Context.ConnectionId, uploadId);
        }
    }
}