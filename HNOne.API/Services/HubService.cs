using Microsoft.AspNetCore.SignalR;

namespace HNOne.API.Services
{
    public class HubService : Hub
    {
        private static readonly Dictionary<string, string> UserConnections = new();

        public override Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"];

            if (!string.IsNullOrEmpty(userId))
            {
                UserConnections[userId] = Context.ConnectionId;
                Console.WriteLine($"User connected: {userId} - ConnectionId: {Context.ConnectionId}");
            }

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"];
            if (!string.IsNullOrEmpty(userId) && UserConnections.ContainsKey(userId))
            {
                UserConnections.Remove(userId);
            }

            return base.OnDisconnectedAsync(exception);
        }

        // Phương thức lấy ConnectionId từ UserId
        public static string? GetConnectionIdByUserId(string userId)
        {
            UserConnections.TryGetValue(userId, out var connectionId);
            return connectionId;
        }

        /// <summary>
        /// Gửi thông báo đến User
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task ReceiveMessage(string userId, string message)
            => await Clients.User(userId).SendAsync("ReceiveMessage", message);
    }
}
