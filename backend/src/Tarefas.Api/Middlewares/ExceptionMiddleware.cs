using Tarefas.Domain.Exceptions;

namespace Tarefas.Api.Middlewares;

/// <summary>
/// Converte exceções do domínio em respostas HTTP com mensagens claras,
/// que depois o agente de IA consegue repassar ao usuário.
/// </summary>
public sealed class ExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (DomainException ex)
        {
            await EscreverErroAsync(context, StatusCodes.Status422UnprocessableEntity, ex.Message);
        }
        catch (NotFoundException ex)
        {
            await EscreverErroAsync(context, StatusCodes.Status404NotFound, ex.Message);
        }
    }

    private static Task EscreverErroAsync(HttpContext context, int statusCode, string mensagem)
    {
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsJsonAsync(new { erro = mensagem });
    }
}