using System.Security.Cryptography;
using System.Text;

namespace Tarefas.Api.Middlewares;

/// <summary>
/// Controle de acesso: toda rota /api exige o cabeçalho X-Api-Key com a chave configurada.
/// </summary>
public sealed class ApiKeyMiddleware
{
    private const string NomeCabecalho = "X-Api-Key";

    private readonly RequestDelegate _next;
    private readonly byte[] _chaveEsperada;

    public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        _next = next;

        var chave = configuration["Seguranca:ApiKey"];
        if (string.IsNullOrWhiteSpace(chave))
            throw new InvalidOperationException("Configure a chave de acesso em Seguranca:ApiKey.");

        _chaveEsperada = Encoding.UTF8.GetBytes(chave);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        var chaveRecebida = Encoding.UTF8.GetBytes(context.Request.Headers[NomeCabecalho].ToString());

        if (!CryptographicOperations.FixedTimeEquals(chaveRecebida, _chaveEsperada))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { erro = "Chave de acesso ausente ou inválida." });
            return;
        }

        await _next(context);
    }
}