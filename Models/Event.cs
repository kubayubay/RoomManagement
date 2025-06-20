using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RoomManagement.Models;

[Table("Events")]
public class Event
{
    [Key]
    public int Id { get; set; }
    public int RoomId { get; set; }
    public string Name { get; set; } = "";
    public int CreatedBy { get; set; }
    public int? UpdatedBy { get; set; }
    public int Attendees { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? Comments { get; set; } = "";
    public string? Options { get; set; } = "";
    public int ChairsNeeded { get; set; }
    public int TablesNeeded { get; set; }
}