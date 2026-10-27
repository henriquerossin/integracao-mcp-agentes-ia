using Tarefas.Domain.Entities;

namespace Tarefas.Domain.Interfaces.Repositories;

public interface IMembroRepository
{
    Task<Membro?> ObterPorIdAsync(string id, CancellationToken ct);
    Task<IReadOnlyList<Membro>> ListarAsync(CancellationToken ct);
    Task SalvarAsync(Membro membro, CancellationToken ct);
    Task<bool> ExisteEmailAsync(string email, CancellationToken ct);
    Task<bool> ExisteCpfAsync(string cpf, CancellationToken ct);
}