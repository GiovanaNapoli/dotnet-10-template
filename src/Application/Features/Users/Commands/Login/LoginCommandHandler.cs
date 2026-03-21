using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Common;
using Domain.Interfaces.Repositories;
using Domain.Interfaces.Services;
using MediatR;

namespace Application.Features.Users.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, ResponseBase<LoginResult>>
    {
        private readonly IIdentityService _identityService;
        private readonly IJwtService _jwtService;
        private readonly IUserRepository _userRepository;

        public LoginCommandHandler(
            IIdentityService identityService,
            IJwtService jwtService,
            IUserRepository userRepository)
        {
            _identityService = identityService;
            _jwtService = jwtService;
            _userRepository = userRepository;
        }

        public async Task<ResponseBase<LoginResult>> Handle(LoginCommand cmd, CancellationToken ct)
        {
            var isValidPassword = await _identityService.CheckPasswordAsync(cmd.Email, cmd.Password);
            if (!isValidPassword)
                return ResponseBase<LoginResult>.Failure("E-mail or password is incorrect.");

            var user = await _userRepository.GetByEmailAsync(cmd.Email, ct);
            if (user is null)
                return ResponseBase<LoginResult>.Failure("User not found.");

            var token = _jwtService.GenerateToken(user.Id, user.Email, []);
            return ResponseBase<LoginResult>.Success(new LoginResult(token));
        }
    }
}