using RoomManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RoomManagement.Controllers;

[ApiController]
[Route("/api/v1/[controller]")]
public class RoomController : ControllerBase
{
    private readonly ILogger<RoomController> _logger;
    private readonly RoomManagementContext _context;

    public RoomController(ILogger<RoomController> logger, RoomManagementContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public IActionResult GetRoom(int id)
    {
        try
        {
            var room = _context.Database.SqlQuery<Room>(@$"
                SELECT * FROM Rooms
                WHERE Id = {id}
            ").Single();
            return Ok(room);
        }
        catch (Exception)
        {
            return NotFound($"Could not find room #{id}.");
        }
    }

    [HttpGet("Rooms")]
    public IActionResult GetRooms()
    {
            var rooms = _context.Database.SqlQuery<Room>(@$"
                SELECT * FROM Rooms
            ");
        return Ok(rooms);
    }

    [HttpPost]
    public IActionResult NewRoom()
    {
        // TODO: Create a new room
        return Ok();
    }

    [HttpPut]
    public IActionResult UpdateRoom()
    {
        // TODO: Update an existing room
        return Ok();
    }

    [HttpDelete]
    public IActionResult DeleteRoom(int id)
    {
       try
        {
            var room = _context.Database.SqlQuery<Room>(@$"
                DELETE FROM Events
                WHERE Id = {id}
            ").Single();
            return Ok(room);
        }
        catch (Exception)
        {
            return NotFound($"Could not find event #{id}.");
        }
    }
}
