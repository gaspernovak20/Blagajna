using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace web.Models;
public class ApplicationUser : IdentityUser
{
    // First Name
    public string? FirstName { get; set; }

    // Last Name
    public string? LastName { get; set; }

    // Percentage of income to allocate to investments (0-100)
    [Range(0, 100)]
    public decimal InvestmentPercent { get; set; } = 0m;

    // If true, automatically create investments when income is added
    public bool AutoAllocateInvestments { get; set; } = false;
}
