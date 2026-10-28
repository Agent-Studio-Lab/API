namespace API.Domain.Common;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
