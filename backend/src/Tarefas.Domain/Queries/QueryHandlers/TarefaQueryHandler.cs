using MediatR;
using Tarefas.Domain.DTOs;
using Tarefas.Domain.Exceptions;
using Tarefas.Domain.Interfaces.Repositories;

namespace Tarefas.Domain.Queries.QueryHandlers;

public sealed class TarefaQueryHandler(
    ITarefaRepository tarefaRepository,
    IMembroRepository membroRepository
) :
    IRequestHandler<ListarTarefasQuery, IReadOnlyList<TarefaDto>>,
    IRequestHandler<ObterTarefaQuery, TarefaDto>
{
    public async Task<IReadOnlyList<TarefaDto>> Handle(ListarTarefasQuery query, CancellationToken cancellationToken)
    {
        var tarefas = await tarefaRepository.ListarAsync(query.Status, query.ResponsavelId, query.Prioridade, cancellationToken);
        var nomes = (await membroRepository.ListarAsync(cancellationToken)).ToDictionary(membro => membro.Id, membro => membro.Nome);

        var result = tarefas
            .Select(tarefa => TarefaDto
            .From(tarefa, tarefa.ResponsavelId is not null && nomes.TryGetValue(tarefa.ResponsavelId, out var nome) ? nome : null))
            .ToList();

        return result;
    }

    public async Task<TarefaDto> Handle(ObterTarefaQuery query, CancellationToken cancellationToken)
    {
        var tarefa = await tarefaRepository.ObterPorIdAsync(query.TarefaId, cancellationToken)
            ?? throw new NotFoundException($"Tarefa '{query.TarefaId}' não encontrada.");

        var responsavel = tarefa.ResponsavelId is null
            ? null
            : await membroRepository.ObterPorIdAsync(tarefa.ResponsavelId, cancellationToken);

        var result = TarefaDto.From(tarefa, responsavel?.Nome);

        return result;
    }
}