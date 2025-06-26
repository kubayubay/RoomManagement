using RoomManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RoomManagement.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly RoomManagementContext _context;

    public UserController(ILogger<UserController> logger, RoomManagementContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public IActionResult GetUser(int id)
    {
        try
        {
            var user = _context.Database.SqlQuery<User>(@$"
                SELECT *
                FROM Users
                WHERE Id = {id}
            ").Single();

            return Ok(user);
        }
        catch (Exception)
        {
            return NotFound($"Could not find user #{id}.");
        }
    }
}