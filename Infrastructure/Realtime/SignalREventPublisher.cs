using API.Application.Abstractions;
using API.Domain.Events;
using Microsoft.AspNetCore.SignalR;

namespace API.Infrastructure.Realtime;

public sealed class SignalREventPublisher : IOfficeEventPublisher
{
    private readonly IHubContext<OfficeHub> _hubContext;

    public SignalREventPublisher(IHubContext<OfficeHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PublishAsync(OfficeEvent officeEvent, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.Group("VirtualOffice").SendAsync("ReceiveOfficeEvent", officeEvent, cancellationToken);
    }
}
