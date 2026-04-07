using Confluent.Kafka;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.Application.Auth.Commands;
using WebAppExam.Application.Auth.DTOs;
using WebAppExam.Application.Common.Errors;

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
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            if (result.HasError<UnauthorizedError>())
            {
                return Unauthorized(ErrorResult.FromResult(result.Errors.Select(e => e.Message).ToList()));
            }

            return BadRequest(ErrorResult.FromResult(result.Errors.Select(e => e.Message).ToList()));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] TokenDTO tokenApiModel)
        {

            var command = RefreshTokenCommand.Refresh(tokenApiModel.AccessToken, tokenApiModel.RefreshToken);

            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ErrorResult.FromResult(result.Errors.Select(e => e.Message).ToList()));
        }

        [HttpPost("logout/{username}")]
        public async Task<IActionResult> Logout(string username)
        {

            var command = LogoutCommand.Logout(username);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ErrorResult.FromResult(result.Errors.Select(e => e.Message).ToList()));
        }
    }
}
