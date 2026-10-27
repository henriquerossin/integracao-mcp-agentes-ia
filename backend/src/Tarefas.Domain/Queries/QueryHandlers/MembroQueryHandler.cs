using MediatR;
using Tarefas.Domain.DTOs;
using Tarefas.Domain.Interfaces.Repositories;

namespace Tarefas.Domain.Queries.QueryHandlers;

public sealed class MembroQueryHandler(IMembroRepository membroRepository) : IRequestHandler<ListarMembrosQuery, IReadOnlyList<MembroDto>>
{
    public async Task<IReadOnlyList<MembroDto>> Handle(ListarMembrosQuery query, CancellationToken ct)
    {
        return (await membroRepository.ListarAsync(ct)).Select(MembroDto.From).ToList();
    }
}