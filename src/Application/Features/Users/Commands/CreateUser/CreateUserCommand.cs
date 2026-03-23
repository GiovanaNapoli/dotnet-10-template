using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Common;
using Application.DTOs.Users;
using Domain.Entities.Common;
using MediatR;

namespace Application.Features.Users.Commands.CreateUser
{
    public record CreateUserCommand(CreateUserDto User): IRequest<ResponseBase>;
}