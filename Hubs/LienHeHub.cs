using Microsoft.AspNetCore.SignalR;

namespace UltraStrore.Hubs
{
    public class LienHeHub : Hub
    {
        public async Task SendLienHeUpdate()
        {
            await Clients.All.SendAsync("ReceiveLienHeUpdate");
        }
    }
}