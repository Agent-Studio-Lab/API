namespace API.Domain.Abstractions;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
