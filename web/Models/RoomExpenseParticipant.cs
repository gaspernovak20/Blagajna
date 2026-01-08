using System.ComponentModel.DataAnnotations;

namespace web.Models;

public class RoomExpenseParticipant
{
    public int Id { get; set; }

    public int RoomExpenseId { get; set; }
    public RoomExpense? RoomExpense { get; set; }

    [Required]
    public required string UserId { get; set; }

    public ApplicationUser? User { get; set; }
}
