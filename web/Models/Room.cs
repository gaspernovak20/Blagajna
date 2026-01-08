using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace web.Models;

public class Room
{
    public int Id { get; set; }

    [Required]
    [StringLength(32)]
    public required string Code { get; set; }

    [StringLength(128)]
    public string? Name { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSettled { get; set; } = false;
    public DateTime? SettledAt { get; set; }

    public ICollection<RoomMember> Members { get; set; } = new List<RoomMember>();
    public ICollection<RoomExpense> Expenses { get; set; } = new List<RoomExpense>();
}
