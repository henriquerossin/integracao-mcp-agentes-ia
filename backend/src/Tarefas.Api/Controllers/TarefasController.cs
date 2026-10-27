using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tarefas.Api.Models;
using Tarefas.Domain.Commands;
using Tarefas.Domain.DTOs;
using Tarefas.Domain.Enums;
using Tarefas.Domain.Queries;

namespace Tarefas.Api.Controllers;

[ApiController]
[Route("api/tarefas")]
public sealed class TarefasController(IMediator mediator) : ControllerBase
{
    // ===================== QUERIES (leitura) =====================

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TarefaDto>>> Listar(
        [FromQuery] StatusTarefa? status,
        [FromQuery] string? responsavelId,
        [FromQuery] Prioridade? prioridade,
        CancellationToken ct)
    {
        var tarefas = await mediator.Send(new ListarTarefasQuery(status, responsavelId, prioridade), ct);
        return Ok(tarefas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TarefaDto>> Obter(string id, CancellationToken ct)
    {
        var tarefa = await mediator.Send(new ObterTarefaQuery(id), ct);
        return Ok(tarefa);
    }

    // ===================== COMMANDS (escrita) =====================

    [HttpPost]
    public async Task<ActionResult<TarefaDto>> Criar([FromBody] CriarTarefaCommand command, CancellationToken ct)
    {
        var tarefa = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(Obter), new { id = tarefa.Id }, tarefa);
    }

    [HttpPatch("{id}/status")]
    public async Task<ActionResult<TarefaDto>> AlterarStatus(
        string id, [FromBody] AlterarStatusRequest request, CancellationToken ct)
    {
        var tarefa = await mediator.Send(new AlterarStatusTarefaCommand(id, request.Status), ct);
        return Ok(tarefa);
    }

    [HttpPatch("{id}/responsavel")]
    public async Task<ActionResult<TarefaDto>> AtribuirResponsavel(
        string id, [FromBody] AtribuirResponsavelRequest request, CancellationToken ct)
    {
        var tarefa = await mediator.Send(new AtribuirResponsavelCommand(id, request.MembroId), ct);
        return Ok(tarefa);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Excluir(string id, CancellationToken ct)
    {
        await mediator.Send(new ExcluirTarefaCommand(id), ct);
        return NoContent();
    }
}