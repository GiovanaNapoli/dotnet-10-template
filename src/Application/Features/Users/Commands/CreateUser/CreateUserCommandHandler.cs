using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Common;
using Domain.Entities.Users;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using MediatR;

namespace Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ResponseBase>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;

        public CreateUserCommandHandler(IUserRepository userRepository, IIdentityService identityService)
        {
            _userRepository = userRepository;
            _identityService = identityService;
        }

        public async Task<ResponseBase> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var emailAlreadyExists = await _userRepository.ExistsByEmailAsync(request.User.Email);
            if (emailAlreadyExists)
            {
                return ResponseBase.Failure("Email already in use.");
            }

            var user = User.Create(request.User.Name, request.User.Email, request.User.ProfilePicture);
            await _userRepository.AddAsync(user, cancellationToken);

            var (success, error) = await _identityService.RegisterAsync(request.User.Email, request.User.Password, user.Id);
            if (!success)
            {
                await _userRepository.DeleteAsync(user, cancellationToken);
                return ResponseBase.Failure(error!);
            }

            return ResponseBase.Success(["User created successfully."]);
        }
    }
}