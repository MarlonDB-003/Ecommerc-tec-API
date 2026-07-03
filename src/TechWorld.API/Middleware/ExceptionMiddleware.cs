using System.Text.Json;
using FluentValidation;
using TechWorld.Application.Common.Exceptions;
using TechWorld.Domain.Exceptions;

namespace TechWorld.API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exceção não tratada: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            ValidationException ve => (400, "Erro de validação", string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))),
            NotFoundException nfe => (404, "Não encontrado", nfe.Message),
            ForbiddenException => (403, "Acesso negado", exception.Message),
            ConflictException => (409, "Conflito", exception.Message),
            UnauthorizedAccessException => (401, "Não autorizado", exception.Message),
            DomainException => (422, "Regra de negócio", exception.Message),
            InvalidOperationException => (400, "Operação inválida", exception.Message),
            _ => (500, "Erro interno", "Ocorreu um erro inesperado.")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new
        {
            type = $"https://httpstatuses.io/{statusCode}",
            title,
            status = statusCode,
            detail,
            traceId = context.TraceIdentifier
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    }
}
