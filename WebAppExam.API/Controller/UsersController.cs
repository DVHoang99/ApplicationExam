using MediatR;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Admin")]
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

        // [HttpPut("{id}")]
        // [Authorize(Roles = "Admin")]
        // public async Task<IActionResult> Update(Ulid id, [FromBody] UpdateUserDTO request)
        // {
        //     var command = new UpdateUserCommand(id)
        //     {
        //         Name = request.Name,
        //         Role = request.Role
        //     };
        //     var result = await _mediator.Send(command);
        //     return Ok(result);
        // }

        // [HttpGet]
        // [Authorize(Roles = "Admin")]
        // public async Task<IActionResult> GetAll()
        // {
        //     var query = new GetAllUsersQuery();
        //     var result = await _mediator.Send(query);
        //     return Ok(result);
        // }

        // [HttpGet("{id}")]
        // public async Task<IActionResult> GetById(Ulid id)
        // {
        //     var query = new GetUserByIdQuery(id);
        //     var result = await _mediator.Send(query);
        //     if (result == null)
        //     {
        //         return NotFound();
        //     }
        //     return Ok(result);
        // }

        // [HttpDelete("{id}")]
        // [Authorize(Roles = "Admin")]
        // public async Task<IActionResult> Delete(Ulid id)
        // {
        //     var command = new DeleteUserCommand(id);
        //     await _mediator.Send(command);
        //     return NoContent();
        // }
    }
}
