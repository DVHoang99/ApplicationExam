using MediatR;
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
        public async Task<IActionResult> Create([FromBody] UserDTO request)
        {
            var command = CreateUserCommand.Init(request.Username, request.Password, request.Role, request.Name);
            var result = await _mediator.Send(command);
            return Ok(result.Value);
        }
    }
}
