using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace UltraStrore.Hubs
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            Console.WriteLine($"User {userId} connected.");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            Console.WriteLine($"User {userId} disconnected.");
            await base.OnDisconnectedAsync(exception);
        }
    }
}