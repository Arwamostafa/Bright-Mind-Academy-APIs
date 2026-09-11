using Microsoft.AspNetCore.SignalR;

namespace API.Hubs
{
    public class ChatHub : Hub
    {
        public void SendMessage(string name, string message)
        {
            Clients.All.SendAsync("newMsg", name, message);

        }
    }
}
