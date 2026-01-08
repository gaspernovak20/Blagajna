using System.ComponentModel.DataAnnotations;

namespace web.Models.ViewModels;

public class AddRoomExpenseViewModel
{
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Amount is required.")]
    [Range(0.01, 999999999, ErrorMessage = "Amount can be between 0.01 and 999999999.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(200)]
    public string? Description { get; set; }

    public List<ParticipantCheckbox> Members { get; set; } = new();

    public class ParticipantCheckbox
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
    }
}
