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
        public async Task<IActionResult> Login([FromBody] UserLoginDTO request)
        {
            var command = LoginCommand.Login(request.Username, request.Password);
            var result = await _mediator.Send(command);
            return Ok(result.Value);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenDTO tokenApiModel)
        {
            var command = RefreshTokenCommand.Refresh(tokenApiModel.AccessToken, tokenApiModel.RefreshToken);
            var result = await _mediator.Send(command);
            return Ok(result.Value);
        }

        [HttpPost("logout/{username}")]
        public async Task<IActionResult> Logout(string username)
        {
            var command = LogoutCommand.Logout(username);
            var result = await _mediator.Send(command);
            return Ok(result.Value);
        }
    }
}
