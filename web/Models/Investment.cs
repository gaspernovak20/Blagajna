using System;
using System.ComponentModel.DataAnnotations;

namespace web.Models;
public class Investment
{
    public int Id { get; set; }

    public required decimal Amount { get; set; }

    public required DateTime Date { get; set; }

    public string? Description { get; set; }

    public ApplicationUser? User { get; set; }

    // Link to Income that produced this investment (optional)
    public int? IncomeId { get; set; }
    public Income? Income { get; set; }
}
