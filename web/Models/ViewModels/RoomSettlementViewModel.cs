namespace web.Models.ViewModels;

public class RoomSettlementViewModel
{
    public int RoomId { get; set; }
    public string RoomCode { get; set; } = string.Empty;

    public decimal TotalSpending { get; set; }
    public bool IsSettled { get; set; }
    public DateTime? SettledAt { get; set; }
    public List<MemberBalance> Balances { get; set; } = new();
    public List<Transfer> Transfers { get; set; } = new();

    public class MemberBalance
    {
        public string UserId { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        // Positive => should receive money; Negative => owes money
        public decimal Balance { get; set; }
    }

    public class Transfer
    {
        public string FromUserId { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;

        public string ToUserId { get; set; } = string.Empty;
        public string ToName { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }
}
