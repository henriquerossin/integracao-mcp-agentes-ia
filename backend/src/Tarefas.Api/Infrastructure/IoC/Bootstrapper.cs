using MongoDB.Driver;
using Tarefas.Domain.Commands.CommandHandlers;
using Tarefas.Domain.Interfaces.Repositories;
using Tarefas.Infrastructure.Data.Context;
using Tarefas.Infrastructure.Data.Repositories;
using Tarefas.Infrastructure.Data.Seed;

namespace Tarefas.Api.Infrastructure.IoC;

/// <summary>Registro centralizado de todas as dependências da aplicação.</summary>
public static class Bootstrapper
{
    public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // ---- Domain: MediatR (registra todos os Command e Query Handlers do projeto Domain) ----
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TarefaCommandHandler).Assembly));

        // ---- Infrastructure: MongoDB ----
        var connectionString = configuration["Mongo:ConnectionString"] ?? "mongodb://localhost:27017";
        var databaseName = configuration["Mongo:Database"] ?? "tcc_tarefas";

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));
        services.AddSingleton(sp => new MongoDbContext(sp.GetRequiredService<IMongoClient>(), databaseName));
        services.AddScoped<DataSeeder>();

        // ---- Infrastructure: Repositories ----
        services.AddScoped<ITarefaRepository, TarefaRepository>();
        services.AddScoped<IMembroRepository, MembroRepository>();
    }
}