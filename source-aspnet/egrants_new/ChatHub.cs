using Microsoft.AspNet.SignalR;
using System;
using System.Web;

namespace egrants_new
{
    public class ChatHub : Hub
    {
        public void Send(string name, string message)
        {
            // Call the broadcastMessage method to update clients.
            Clients.All.addNewMessageToPage(name, message);
        }
    }
}