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
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Create([FromBody] UserDTO request)
        {
            var command = new CreateUserCommand
            {
                Username = request.Username,
                Password = request.Password,
                Name = request.Name,
                Role = request.Role
            };
            var result = _mediator.Send(command);
            return Ok(result);
        }
    }
}
