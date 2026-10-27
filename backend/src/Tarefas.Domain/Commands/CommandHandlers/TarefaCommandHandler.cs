using MediatR;
using Tarefas.Domain.DTOs;
using Tarefas.Domain.Entities;
using Tarefas.Domain.Exceptions;
using Tarefas.Domain.Interfaces.Repositories;

namespace Tarefas.Domain.Commands.CommandHandlers;

public sealed class TarefaCommandHandler(
    ITarefaRepository tarefaRepository,
    IMembroRepository membroRepository
) :
    IRequestHandler<CriarTarefaCommand, TarefaDto>,
    IRequestHandler<AlterarStatusTarefaCommand, TarefaDto>,
    IRequestHandler<AtribuirResponsavelCommand, TarefaDto>,
    IRequestHandler<ExcluirTarefaCommand>
{
    public async Task<TarefaDto> Handle(CriarTarefaCommand command, CancellationToken ct)
    {
        string? responsavelNome = null;

        if (!string.IsNullOrWhiteSpace(command.ResponsavelId))
        {
            var membro = await membroRepository.ObterPorIdAsync(command.ResponsavelId, ct)
                ?? throw new NotFoundException($"Membro '{command.ResponsavelId}' não encontrado.");

            responsavelNome = membro.Nome;
        }

        var tarefa = Tarefa.Criar(
            command.Titulo,
            command.Descricao,
            command.Prioridade,
            command.Prazo,
            command.ResponsavelId,
            DateTime.UtcNow
        );

        await tarefaRepository.SalvarAsync(tarefa, ct);

        return TarefaDto.From(tarefa, responsavelNome);
    }

    public async Task<TarefaDto> Handle(AlterarStatusTarefaCommand command, CancellationToken ct)
    {
        var tarefa = await ObterTarefaAsync(command.TarefaId, ct);

        tarefa.AlterarStatus(command.NovoStatus, DateTime.UtcNow);

        await tarefaRepository.SalvarAsync(tarefa, ct);

        var responsavel = tarefa.ResponsavelId is null
            ? null
            : await membroRepository.ObterPorIdAsync(tarefa.ResponsavelId, ct);

        return TarefaDto.From(tarefa, responsavel?.Nome);
    }

    public async Task<TarefaDto> Handle(AtribuirResponsavelCommand command, CancellationToken ct)
    {
        var tarefa = await ObterTarefaAsync(command.TarefaId, ct);

        var membro = await membroRepository.ObterPorIdAsync(command.MembroId, ct)
            ?? throw new NotFoundException($"Membro '{command.MembroId}' não encontrado.");

        tarefa.AtribuirResponsavel(membro.Id, DateTime.UtcNow);

        await tarefaRepository.SalvarAsync(tarefa, ct);

        return TarefaDto.From(tarefa, membro.Nome);
    }

    public async Task Handle(ExcluirTarefaCommand command, CancellationToken ct)
    {
        if (!await tarefaRepository.ExcluirAsync(command.TarefaId, ct))
            throw new NotFoundException($"Tarefa '{command.TarefaId}' não encontrada.");
    }

    private async Task<Tarefa> ObterTarefaAsync(string id, CancellationToken ct)
        => await tarefaRepository.ObterPorIdAsync(id, ct)
            ?? throw new NotFoundException($"Tarefa '{id}' não encontrada.");
}
