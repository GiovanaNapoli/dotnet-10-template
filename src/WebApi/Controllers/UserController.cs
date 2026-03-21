using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Features.Users.Commands.CreateUser;
using Application.Features.Users.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(
        [FromBody] CreateUserCommand command,
        CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] LoginCommand command,
            CancellationToken ct)
        {
            var result = await _mediator.Send(command, ct);
            return result.IsSuccess ? Ok(result) : Unauthorized(result);
        }
    }
}