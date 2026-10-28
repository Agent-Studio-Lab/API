using Microsoft.AspNetCore.SignalR;

namespace API.Infrastructure.Realtime;

public sealed class OfficeHub : Hub
{
    public async Task JoinOfficeAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "VirtualOffice");
    }

    public async Task LeaveOfficeAsync()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "VirtualOffice");
    }
}
