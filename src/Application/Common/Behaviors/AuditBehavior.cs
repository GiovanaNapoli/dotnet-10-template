using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors
{
    public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        private readonly ILogger<AuditBehavior<TRequest, TResponse>> _logger;

        public AuditBehavior(ILogger<AuditBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken ct)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogInformation("Executando {Request}", requestName);
            var start = DateTime.UtcNow;

            var response = await next();

            _logger.LogInformation(
                "{Request} finalizado em {ElapsedMs}ms",
                requestName,
                (DateTime.UtcNow - start).TotalMilliseconds);

            return response;
        }
    }
}