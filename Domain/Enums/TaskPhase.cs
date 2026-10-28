namespace API.Domain.Enums;

public enum TaskPhase
{
    Draft,
    Debating,
    WaitingHiringApproval,
    Executing,
    TestingAndReviewing,
    WaitingFinalApproval,
    Completed,
    Failed,
    Cancelled
}
