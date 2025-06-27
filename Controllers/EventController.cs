using RoomManagement.Models;
using RoomManagement.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;
using Microsoft.AspNetCore.SignalR;
using RoomManagement.Hubs;

namespace RoomManagement.Controllers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class EventMapper
{
    [MapperIgnoreTarget(nameof(Event.Id))]
    [MapperIgnoreTarget(nameof(Event.CreatedBy))]
    [MapperIgnoreTarget(nameof(Event.CreatedAt))]
    public partial void EventToEvent(Event oldEvent, Event newEvent);
}

[ApiController]
[Route("/api/v1/[controller]")]
public class EventController : ControllerBase
{
    private readonly ILogger<EventController> _logger;
    private readonly RoomManagementContext _context;
    private readonly EventMapper _mapper = new();
    private readonly IHubContext<EventHub> _eventHub;

    public EventController(ILogger<EventController> logger, RoomManagementContext context, IHubContext<EventHub> eventHub)
    {
        _logger = logger;
        _context = context;
        _eventHub = eventHub;
    }

    [HttpGet]
    public IActionResult GetEvent(int id)
    {
        try
        {
            var calendarEvent = _context.Database.SqlQuery<EventUserDTO>(@$"
                SELECT Events.*, Users.Email, Users.Phone, Users.Name AS UserName
                FROM Events
                INNER JOIN Users ON Events.CreatedBy = Users.Id
                WHERE Events.Id = {id}
            ").Single();
            return Ok(calendarEvent);
        }
        catch (Exception)
        {
            return NotFound($"Could not find event #{id}.");
        }
    }

    [HttpGet("Events")]
    public IActionResult GetEvents(DateOnly start, DateOnly end)
    {  
        try
        {
            var calendarEvents = _context.Database.SqlQuery<EventUserDTO>(@$"
                SELECT Events.*, Users.Email, Users.Phone, Users.Name AS UserName
                FROM Events
                INNER JOIN Users ON Events.CreatedBy = Users.Id
                WHERE StartAt >= {start}
                AND EndAt <= CONCAT({end}, ' 23:59')
            ");
            return Ok(calendarEvents);
        }
        catch (Exception)
        {
            return NotFound($"Could not find events.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> NewEvent(Event userEvent)
    {
        var room = _context.Rooms.Single(r => r.Id == userEvent.RoomId);

        var events = _context.Events.Where(e => e.RoomId == userEvent.RoomId && (
            userEvent.StartAt < e.EndAt && userEvent.EndAt > e.StartAt
        ));

        if (events != null && events.Count() > 0)
        {
            return BadRequest("This event overlaps with another event.");
        }
        else if (userEvent.Attendees > room.Capacity)
        {
            return BadRequest($"Room #{room.Id} does not have enough capacity for {userEvent.Attendees} attendees.");
        }
        else if (userEvent.Attendees < 0)
        {
            return BadRequest("Attendees entered is less than 0.");
        }
        else if (userEvent.ChairsNeeded != null && userEvent.ChairsNeeded < 0)
        {
            return BadRequest("Chairs entered is less than 0.");
        }
        else if (userEvent.TablesNeeded != null && userEvent.TablesNeeded < 0)
        {
            return BadRequest("Tables entered is less than 0.");
        }
        else if (userEvent.EndAt <= userEvent.StartAt)
        {
            return BadRequest("Invalid Start/End At.");
        }

        var newEvent = new Event();
        _mapper.EventToEvent(userEvent, newEvent);

        if (newEvent.Attendees <= room.Capacity)
        {
            newEvent.Id = 0;
            newEvent.UpdatedBy = null;
            newEvent.UpdatedAt = null;
            newEvent.CreatedBy = 5;
            newEvent.CreatedAt = DateTime.Now;

            await _context.AddAsync(newEvent);
            await _context.SaveChangesAsync();

            await _eventHub.Clients.All.SendAsync("NewEvent", @$"New event was created.");

            return Ok(newEvent);
        }
        else
        {
            return BadRequest($"{room.Id} does not have enough capacity for {userEvent.Attendees} attendees.");
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateEvent(Event userEvent)
    {
        var room = _context.Rooms.Single(r => r.Id == userEvent.RoomId);

        var events = _context.Events.Where(e => e.RoomId == userEvent.RoomId && (
            userEvent.StartAt < e.EndAt && userEvent.EndAt > e.StartAt
        ) && e.Id != userEvent.Id);

        if (events != null && events.Count() > 0)
        {
            return BadRequest("This event overlaps with another event.");
        }
        else if (userEvent.Attendees > room.Capacity)
        {
            return BadRequest($"Room #{room.Id} does not have enough capacity for {userEvent.Attendees} attendees.");
        }
        else if (userEvent.Attendees < 0)
        {
            return BadRequest("Attendees entered is less than 0.");
        }
        else if (userEvent.ChairsNeeded != null && userEvent.ChairsNeeded < 0)
        {
            return BadRequest("Chairs entered is less than 0.");
        }
        else if (userEvent.TablesNeeded != null && userEvent.TablesNeeded < 0)
        {
            return BadRequest("Tables entered is less than 0.");
        }
        else if (userEvent.EndAt <= userEvent.StartAt)
        {
            return BadRequest("Invalid Start/End At.");
        }

        var oldEvent = _context.Events.Single(e => e.Id == userEvent.Id);
        _mapper.EventToEvent(userEvent, oldEvent);
        oldEvent.UpdatedAt = DateTime.Now;
        oldEvent.UpdatedBy = 5;

        await _context.SaveChangesAsync();

        await _eventHub.Clients.All.SendAsync("UpdatedEvent", @$"Event #{userEvent.Id} was updated.");

        return Ok(oldEvent);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteEvent(int id)
    {
       try
        {
            var calendarEvent = _context.Database.ExecuteSql(@$"
                DELETE FROM Events
                WHERE Id = {id}
            ");

            await _eventHub.Clients.All.SendAsync("DeletedEvent", @$"Event #{id} was deleted successfully.");

            return Ok($"Event #{id} was deleted successfully.");
        }
        catch (Exception)
        {
            return NotFound($"Could not find event #{id}.");
        }
    }
}
