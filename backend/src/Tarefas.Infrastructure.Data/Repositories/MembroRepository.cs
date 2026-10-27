using MongoDB.Driver;
using Tarefas.Domain.Entities;
using Tarefas.Domain.Interfaces.Repositories;
using Tarefas.Infrastructure.Data.Context;
using Tarefas.Infrastructure.Data.Documents;

namespace Tarefas.Infrastructure.Data.Repositories;

public sealed class MembroRepository(MongoDbContext context) : IMembroRepository
{
    public async Task<Membro?> ObterPorIdAsync(string id, CancellationToken ct)
    {
        var document = await context.Membros.Find(membroDocument => membroDocument.Id == id).FirstOrDefaultAsync(ct);
        return document?.ToEntity();
    }

    public async Task<IReadOnlyList<Membro>> ListarAsync(CancellationToken cancellationToken)
    {
        var documents = await context.Membros
            .Find(FilterDefinition<MembroDocument>.Empty)
            .SortBy(membroDocument => membroDocument.Nome)
            .ToListAsync(cancellationToken);

        var result = documents
            .Select(membroDocument => membroDocument.ToEntity())
            .ToList();

        return result;
    }

    public Task SalvarAsync(Membro membro, CancellationToken cancellationToken)
        => context.Membros
            .ReplaceOneAsync(
                membroDocument => membroDocument.Id == membro.Id,
                MembroDocument.FromEntity(membro),
                new ReplaceOptions { IsUpsert = true },
                cancellationToken
            );

    public async Task<bool> ExisteEmailAsync(string email, CancellationToken cancellationToken) 
        => await context.Membros
            .CountDocumentsAsync(membroDocument => membroDocument.Email == email, cancellationToken: cancellationToken) > 0;

    public async Task<bool> ExisteCpfAsync(string cpf, CancellationToken cancellationToken)
        => await context.Membros
            .CountDocumentsAsync(membroDocument => membroDocument.Cpf == cpf, cancellationToken: cancellationToken) > 0;
}