using RoomManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;
using Microsoft.AspNetCore.Authorization;

namespace RoomManagement.Controllers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class RoomMapper
{
    [MapperIgnoreTarget(nameof(Room.Id))]
    public partial void RoomToRoom(Room oldRoom, Room newRoom);
}

[Authorize]
[ApiController]
[Route("/api/v1/[controller]")]
public class RoomController : ControllerBase
{
    private readonly ILogger<RoomController> _logger;
    private readonly RoomManagementContext _context;
    private readonly RoomMapper _mapper = new();

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
    public IActionResult NewRoom(Room room)
    {
        room.Id = 0;

        _context.Rooms.Add(room);
        _context.SaveChanges();

        return Ok(room);
    }

    [HttpPut]
    public IActionResult UpdateRoom(Room room)
    {
        var oldRoom = _context.Rooms.Single(r => r.Id == room.Id);

        _mapper.RoomToRoom(room, oldRoom);
        _context.SaveChanges();
    
        return Ok(oldRoom);
    }

    [HttpDelete]
    public IActionResult DeleteRoom(int id)
    {
       try
        {
            var room = _context.Database.ExecuteSql(@$"
                DELETE FROM Rooms
                WHERE Id = {id}
            ");
            return Ok($"Successfully deleted room #{id}.");
        }
        catch (Exception)
        {
            return NotFound($"Could not find event #{id}.");
        }
    }
}
