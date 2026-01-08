using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace web.Models;

public class RoomExpense
{
    public int Id { get; set; }

    public int RoomId { get; set; }
    public Room? Room { get; set; }

    public required string PayerUserId { get; set; }
    public ApplicationUser? PayerUser { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<RoomExpenseParticipant> Participants { get; set; } = new List<RoomExpenseParticipant>();
}
