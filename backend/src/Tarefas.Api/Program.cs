using System.Text.Json.Serialization;
using Tarefas.Api.Infrastructure.IoC;
using Tarefas.Api.Middlewares;
using Tarefas.Infrastructure.Data.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

// Todas as dependências (repositórios, handlers, MongoDB) são registradas no Bootstrapper.
Bootstrapper.RegisterServices(builder.Services, builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();

if (app.Environment.IsDevelopment())
{
    // Documento OpenAPI em /openapi/v1.json (pode ser importado no Postman).
    app.MapOpenApi();

    // Popula o banco com dados de exemplo. Use "dotnet run -- --reset" para recriar do zero.
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<DataSeeder>().ExecutarAsync(recriar: args.Contains("--reset"));
}

app.MapControllers();

app.Run();