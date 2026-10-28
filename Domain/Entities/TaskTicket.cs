using API.Domain.Common;
using API.Domain.Enums;

namespace API.Domain.Entities;

public sealed class TaskTicket : AggregateRoot
{
    public string Title { get; private set; }
    public string UserPrompt { get; private set; }
    public TaskPhase CurrentPhase { get; private set; }
    public string? Specification { get; private set; }
    public string? ImplementationCode { get; private set; }
    public string? ReviewSummary { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public TaskTicket(string title, string userPrompt, Guid? id = null) : base(id)
    {
        Title = title;
        UserPrompt = userPrompt;
        CurrentPhase = TaskPhase.Backlog;
    }

    public void SetSpecification(string specification)
    {
        Specification = specification;
    }

    public void SetImplementationCode(string code)
    {
        ImplementationCode = code;
    }

    public void SetReviewSummary(string reviewSummary)
    {
        ReviewSummary = reviewSummary;
    }

    public void ChangePhase(TaskPhase phase)
    {
        CurrentPhase = phase;
        if (phase == TaskPhase.Completed)
        {
            CompletedAt = Clock.Now;
        }
    }
}
