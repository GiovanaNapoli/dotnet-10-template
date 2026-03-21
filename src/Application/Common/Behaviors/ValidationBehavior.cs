using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Common;
using FluentValidation;
using MediatR;

namespace Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken ct)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var errors = _validators
                .Select(v => v.Validate(context))
                .SelectMany(r => r.Errors)
                .Where(e => e is not null)
                .Select(e => e.ErrorMessage)
                .ToList();

            if (errors.Count == 0)
                return await next();

            var responseType = typeof(TResponse);

            if (responseType == typeof(ResponseBase))
                return (TResponse)(object)ResponseBase.Failure(errors);

            if (responseType.IsGenericType &&
                responseType.GetGenericTypeDefinition() == typeof(ResponseBase<>))
            {
                var failureMethod = responseType.GetMethod(
                    "Failure",
                    [typeof(IEnumerable<string>)]);

                return (TResponse)failureMethod!.Invoke(null, [errors])!;
            }

            throw new ValidationException(string.Join(", ", errors));
        }
    }
}