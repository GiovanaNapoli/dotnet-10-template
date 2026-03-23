using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Common;
using Domain.Entities.Users;
using AutoMapper;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using MediatR;
using Application.Common;

namespace Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ResponseBase>
    {
        private readonly IUserRepository _userRepository;
        private readonly IIdentityService _identityService;
        private readonly IMapper _mapper;

        public CreateUserCommandHandler(IUserRepository userRepository, IIdentityService identityService, IMapper mapper)
        {
            _userRepository = userRepository;
            _identityService = identityService;
            _mapper = mapper;
        }

        public async Task<ResponseBase> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var emailAlreadyExists = await _userRepository.ExistsByEmailAsync(request.User.Email);
            if (emailAlreadyExists)
            {
                return ResponseBase.Failure("Email already in use.");
            }

            var profilePicture = request.User.ProfilePicture is not null
                ? _mapper.Map<ImageFile?>(request.User.ProfilePicture)
                : null;

            var user = User.Create(request.User.Name, request.User.Email, profilePicture);
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