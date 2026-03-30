using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.Application.User.Commands;
using WebAppExam.Application.User.DTOs;

namespace WebAppExam.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Create([FromBody] UserDTO request)
        {
            var command = new CreateUserCommand
            {
                Username = request.Username,
                Password = request.Password,
                Name = request.Name,
                Role = request.Role
            };
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
