using Application.Common;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using MediatR;

namespace Application.Common.Behaviors;

public class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _uow;

    public UnitOfWorkBehavior(IUnitOfWork uow) => _uow = uow;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var response = await next();

        // Só persiste se o handler retornou sucesso
        var isSuccess = response switch
        {
            ResponseBase rb => rb.IsSuccess,
            ResponseBase<object> rbg => rbg.IsSuccess,
            _ => true // se não é ResponseBase, persiste sempre
        };

        if (isSuccess)
            await _uow.SaveChangesAsync(ct);

        return response;
    }
}