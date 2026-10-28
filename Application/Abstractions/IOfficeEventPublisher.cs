using API.Domain.Events;

namespace API.Application.Abstractions;

public interface IOfficeEventPublisher
{
    Task PublishAsync(OfficeEvent officeEvent, CancellationToken cancellationToken = default);
}
