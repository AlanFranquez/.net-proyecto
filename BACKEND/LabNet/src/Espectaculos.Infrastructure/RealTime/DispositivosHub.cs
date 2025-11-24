using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Espectaculos.Infrastructure.RealTime
{
    [Authorize]
    public class DispositivosHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var http = Context.GetHttpContext();
            var browserId = http?.Request.Query["browserId"].ToString();

            if (!string.IsNullOrWhiteSpace(browserId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, DeviceGroup(browserId));
            }

            await base.OnConnectedAsync();
        }

        public static string DeviceGroup(string browserId) => $"device:{browserId}";
    }
}