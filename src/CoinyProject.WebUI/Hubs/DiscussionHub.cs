
using CoinyProject.Application.DTO.Discussion;
using Microsoft.AspNetCore.SignalR;

namespace CoinyProject.WebUI.Hubs
{
    public class DiscussionHub : Hub
    {
        /*  public async Task SendMessage(string message)
          {

              await Clients.All.SendAsync("receiveMessage", message);
          }*/
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
