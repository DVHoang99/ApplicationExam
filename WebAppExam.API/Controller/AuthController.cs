using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.Application.Auth.Commands;
using WebAppExam.Application.Auth.DTOs;

namespace WebAppExam.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto request)
        {
            var command = new LoginCommand(request.Username, request.Password);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenDTO tokenApiModel)
        {

            var command = new RefreshTokenCommand(tokenApiModel.AccessToken, tokenApiModel.RefreshToken);
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string username)
        {

            var command = new LogoutCommand(username);
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
