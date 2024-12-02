using HNOne.API.Services;
using HNOne.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HNOne.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<HubService> _hubContext;

        public NotificationController(IHubContext<HubService> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification([FromBody] RequestModel request)
        {
            // Lấy ConnectionId từ UserId
            var connectionId = HubService.GetConnectionIdByUserId(request.userId.ToString());
            if (connectionId != null)
            {
                // Gửi thông báo đến ConnectionId
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveMessage", request.opt);
            }    
            return Ok(new { Success = true });
        }
    }
}
