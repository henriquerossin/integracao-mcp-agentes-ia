using Tarefas.Domain.Entities;
using Tarefas.Domain.Enums;

namespace Tarefas.Domain.Interfaces.Repositories;

public interface ITarefaRepository
{
    Task<Tarefa?> ObterPorIdAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<Tarefa>> ListarAsync(
        StatusTarefa? status, 
        string? responsavelId, 
        Prioridade? prioridade, 
        CancellationToken ct
    );
    Task SalvarAsync(Tarefa tarefa, CancellationToken ct);
    Task<bool> ExcluirAsync(string id, CancellationToken ct);
}