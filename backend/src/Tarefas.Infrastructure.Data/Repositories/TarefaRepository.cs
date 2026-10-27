using MongoDB.Driver;
using Tarefas.Domain.Entities;
using Tarefas.Domain.Enums;
using Tarefas.Domain.Interfaces.Repositories;
using Tarefas.Infrastructure.Data.Context;
using Tarefas.Infrastructure.Data.Documents;

namespace Tarefas.Infrastructure.Data.Repositories;

public sealed class TarefaRepository(MongoDbContext context) : ITarefaRepository
{
    public async Task<Tarefa?> ObterPorIdAsync(string id, CancellationToken cancellationToken)
    {
        var document = await context.Tarefas.Find(tarefaDocument => tarefaDocument.Id == id).FirstOrDefaultAsync(cancellationToken);

        var result = document?.ToEntity();

        return result;
    }

    public async Task<IReadOnlyList<Tarefa>> ListarAsync(
        StatusTarefa? status,
        string? responsavelId,
        Prioridade? prioridade,
        CancellationToken cancellationToken)
    {
        var builder = Builders<TarefaDocument>.Filter;

        var filtro = builder.Empty;

        if (status is not null) filtro &= builder.Eq(tarefaDocument => tarefaDocument.Status, status.Value);

        if (prioridade is not null) filtro &= builder.Eq(tarefaDocument => tarefaDocument.Prioridade, prioridade.Value);

        if (!string.IsNullOrWhiteSpace(responsavelId)) filtro &= builder.Eq(tarefaDocument => tarefaDocument.ResponsavelId, responsavelId);

        var documents = await context.Tarefas.Find(filtro).SortBy(tarefaDocument => tarefaDocument.Prazo).ToListAsync(cancellationToken);

        var result = documents.Select(tarefaDocument => tarefaDocument.ToEntity()).ToList();

        return result;

    }

    public Task SalvarAsync(Tarefa tarefa, CancellationToken cancellationToken)
    {
        var result = context.Tarefas
            .ReplaceOneAsync(
                tarefaDocument => tarefaDocument.Id == tarefa.Id,
                TarefaDocument.FromEntity(tarefa),
                new ReplaceOptions { IsUpsert = true }, cancellationToken
            );

        return result;
    }

    public async Task<bool> ExcluirAsync(string id, CancellationToken cancellationToken)
    {
        var result = await context.Tarefas.DeleteOneAsync(tarefaDocument => tarefaDocument.Id == id, cancellationToken);

        return result.DeletedCount > 0;
    }
}