using API.Application.Features.Agents.Commands.TestConnection;
using API.Application.Features.Agents.Commands.UpdateConfig;
using API.Application.Features.Agents.Queries.GetAgents;
using API.Presentation.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Presentation.Http;

[ApiController]
[Route("api/agents")]
public sealed class AgentsController : ControllerBase
{
    private readonly ISender _sender;

    public AgentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAgents(CancellationToken cancellationToken)
    {
        var query = new GetAgentsQuery();
        var result = await _sender.Send(query, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("test-connection")]
    public async Task<IActionResult> TestConnection(
        [FromBody] TestLLMConnectionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new TestLLMConnectionCommand(request);
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}/config")]
    public async Task<IActionResult> UpdateConfig(
        Guid id,
        [FromBody] UpdateAgentConfigRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAgentConfigCommand(request with { AgentId = id });
        var result = await _sender.Send(command, cancellationToken);
        return result.ToActionResult();
    }
}
