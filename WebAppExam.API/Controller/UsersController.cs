using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.Application.User.Commands;
using WebAppExam.Application.User.DTOs;
using WebAppExam.Application.User.Queries;

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
            var command = new CreateUserCommand
            {
                Username = request.Username,
                Password = request.Password,
                Name = request.Name,
                Role = request.Role
            };
            var result = await _mediator.Send(command);
            return Ok(new {id = result});
        }

        // [HttpPut("{id}")]
        // public async Task<IActionResult> Update([FromBody] UpdateUserDTO request)
        // {
        //     var command = new UpdateUserCommand(request.Username)
        //     {
        //         Name = request.Name,
        //         Role = request.Role,
        //         Password = request.Password
        //     };
        //     var result = await _mediator.Send(command);
        //     return Ok(result);
        // }
        //
        // [HttpGet]
        // [Authorize(Roles = "Admin")]
        // public async Task<IActionResult> GetAll()
        // {
        //     var query = new GetAllUsersQuery();
        //     var result = await _mediator.Send(query);
        //     return Ok(result);
        // }

        // [HttpGet("{username}")]
        // public async Task<IActionResult> GetById(string username)
        // {
        //     var query = new GetUserByUsernameQuery(username);
        //     var result = await _mediator.Send(query);
        //     if (result == null)
        //     {
        //         return NotFound();
        //     }
        //     return Ok(result);
        // }

        // [HttpDelete("{username}")]
        // [Authorize(Roles = "Admin")]
        // public async Task<IActionResult> Delete(string username)
        // {
        //     var command = new DeleteUserCommand(username);
        //     await _mediator.Send(command);
        //     return NoContent();
        // }
    }
}
