using System.ComponentModel.DataAnnotations;

namespace web.Models;

public class RoomMember
{
    public int Id { get; set; }

    public int RoomId { get; set; }
    public Room? Room { get; set; }

    [Required]
    public required string UserId { get; set; }

    public ApplicationUser? User { get; set; }
}
