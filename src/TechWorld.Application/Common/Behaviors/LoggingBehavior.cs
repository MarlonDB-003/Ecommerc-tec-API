using MediatR;
using Microsoft.Extensions.Logging;

namespace TechWorld.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var requestName = typeof(TRequest).Name;
        logger.LogInformation("Executando {RequestName}", requestName);

        var response = await next(ct);

        logger.LogInformation("Concluído {RequestName}", requestName);
        return response;
    }
}
