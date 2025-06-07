using Microsoft.AspNetCore.SignalR;
using UltraStrore.Models.ViewModels;

namespace UltraStrore.Hubs
{
    public class GiaoDienHub : Hub
    {
        public async Task SendGiaoDienUpdate()
        {
            await Clients.All.SendAsync("ReceiveGiaoDienUpdate");
        }

        public async Task SendGiaoDienAdded(GiaoDienView giaoDien)
        {
            await Clients.All.SendAsync("ReceiveGiaoDienAdded", giaoDien);
        }

        public async Task SendGiaoDienUpdated(GiaoDienView giaoDien)
        {
            await Clients.All.SendAsync("ReceiveGiaoDienUpdated", giaoDien);
        }

        public async Task SendGiaoDienDeleted(int maGiaoDien)
        {
            await Clients.All.SendAsync("ReceiveGiaoDienDeleted", maGiaoDien);
        }

        public async Task SendGiaoDienSetActive(int maGiaoDien)
        {
            await Clients.All.SendAsync("ReceiveGiaoDienSetActive", maGiaoDien);
        }
    }
}
