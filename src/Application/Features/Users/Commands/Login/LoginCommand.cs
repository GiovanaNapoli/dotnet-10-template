using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Common;
using MediatR;

namespace Application.Features.Users.Commands.Login
{
    public record LoginCommand(string Email, string Password) : IRequest<ResponseBase<LoginResult>>;
    public record LoginResult(string Token);

}