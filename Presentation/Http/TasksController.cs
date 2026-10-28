using API.Application.Features.Tasks.Commands.ApproveHiring;
using API.Application.Features.Tasks.Commands.ExecuteTask;
using API.Application.Features.Tasks.Commands.FinalDecision;
using API.Presentation.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Presentation.Http;

[ApiController]
[Route("api/tasks")]
public sealed class TasksController : ControllerBase
{
    private readonly ISender _sender;

    public TasksController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> ExecuteTask([FromBody] ExecuteTaskRequest request, CancellationToken cancellationToken)
    {
        var command = new ExecuteTaskCommand(request);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/approve-hiring")]
    public async Task<IActionResult> ApproveHiring(
        Guid id,
        [FromBody] ApproveHiringRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ApproveHiringCommand(id, request);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/final-decision")]
    public async Task<IActionResult> FinalDecision(
        Guid id,
        [FromBody] FinalDecisionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new FinalDecisionCommand(id, request);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}
