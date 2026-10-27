using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tarefas.Domain.Commands;
using Tarefas.Domain.DTOs;
using Tarefas.Domain.Queries;

namespace Tarefas.Api.Controllers;

[ApiController]
[Route("api/membros")]
public sealed class MembrosController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MembroDto>>> Listar(CancellationToken ct)
    {
        var membros = await mediator.Send(new ListarMembrosQuery(), ct);
        return Ok(membros);
    }

    [HttpPost]
    public async Task<ActionResult<MembroDto>> Cadastrar([FromBody] CadastrarMembroCommand command, CancellationToken ct)
    {
        var membro = await mediator.Send(command, ct);
        return Created($"/api/membros/{membro.Id}", membro);
    }
}