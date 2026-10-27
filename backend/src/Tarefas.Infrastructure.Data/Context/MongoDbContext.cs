using MongoDB.Driver;
using Tarefas.Infrastructure.Data.Documents;

namespace Tarefas.Infrastructure.Data.Context;

/// <summary>Ponto único de acesso ao banco e às coleções.</summary>
public sealed class MongoDbContext(IMongoClient client, string databaseName)
{
    public IMongoDatabase Database { get; } = client.GetDatabase(databaseName);

    public IMongoCollection<TarefaDocument> Tarefas => Database.GetCollection<TarefaDocument>("tarefas");
    public IMongoCollection<MembroDocument> Membros => Database.GetCollection<MembroDocument>("membros");
}