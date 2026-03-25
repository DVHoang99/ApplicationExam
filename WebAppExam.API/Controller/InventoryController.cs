using Microsoft.AspNetCore.Mvc;

namespace WebAppExam.API.Controller;

[ApiController]
[Route("api/inventory")]
public class InventoryController : ControllerBase
{
    [HttpPost("decrease")]
    public IActionResult Decrease(Guid productId, int qty)
    {
        return Ok();
    }
}