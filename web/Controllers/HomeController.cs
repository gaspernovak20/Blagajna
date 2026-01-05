using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using web.Models;
using web.Data;
using System.Linq;
using Microsoft.AspNetCore.Identity;



namespace web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly BlagajnaContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

     public HomeController(BlagajnaContext context, ILogger<HomeController> logger, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
        }

    public async Task<IActionResult> Index()
    {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null)
            {
                var currentMonth = DateTime.Now.Month;
                var currentYear = DateTime.Now.Year;

                // Pridobimo transakcije trenutnega uporabnika za trenutni mesec
                var userTransactions = _context.Transactions
                                            .Where(t => t.User.Id == currentUser.Id &&
                                                        t.Date.Month == currentMonth &&
                                                        t.Date.Year == currentYear)
                                            .ToList();

                decimal spentThisMonth = userTransactions.Sum(t => t.Amount);

                // Pridobimo prihodke trenutnega uporabnika za trenutni mesec
                var userIncomes = _context.Incomes
                                        .Where(i => i.User.Id == currentUser.Id &&
                                                    i.Date.Month == currentMonth &&
                                                    i.Date.Year == currentYear)
                                        .ToList();

                decimal thisMonthIncome = userIncomes.Sum(i => i.Amount);
                
                var userSavedMoney = _context.SavedMoney
                                .Where(s => s.User.Id == currentUser.Id &&
                                            s.Date.Month == currentMonth &&
                                            s.Date.Year == currentYear)
                                .ToList();

                decimal savedThisMonth = userSavedMoney.Sum(s => s.Amount);

                // Investments this month
                var userInvestments = _context.Investments
                                    .Where(i => i.User.Id == currentUser.Id &&
                                                i.Date.Month == currentMonth &&
                                                i.Date.Year == currentYear)
                                    .ToList();

                decimal investedThisMonth = userInvestments.Sum(i => i.Amount);

                var userBudget = _context.Budgets
                                .Where(s => s.User.Id == currentUser.Id &&
                                            s.StartDate.Month == currentMonth &&
                                            s.StartDate.Year == currentYear)
                                .ToList();

                decimal thisMonthBudget = userBudget.Sum(s => s.Amount);

                decimal budgetBalance = thisMonthBudget - spentThisMonth - savedThisMonth;

                // Izračun stanja in prihrankov
                decimal totalSaved = _context.SavedMoney
                                      .Where(s => s.User.Id == currentUser.Id)
                                      .Sum(s => s.Amount);

                decimal totalIncome = _context.Incomes
                                       .Where(i => i.User.Id == currentUser.Id)
                                       .Sum(i => i.Amount);

                decimal totalSpent = _context.Transactions
                                     .Where(t => t.User.Id == currentUser.Id)
                                     .Sum(t => t.Amount);

                decimal totalBalance = totalIncome - totalSpent - totalSaved;

                // Shranimo izračune v ViewData za prikaz v View
                ViewData["savedThisMonth"] = savedThisMonth;
                ViewData["investedThisMonth"] = investedThisMonth;
                ViewData["totalbalance"] = totalBalance;
                ViewData["spentThisMonth"] = spentThisMonth;
                ViewData["thisMonthBudget"] = thisMonthBudget;
                ViewData["LeftInBudget"] = budgetBalance;
                ViewData["totalSaved"] = totalSaved;

                // Investment settings preview
                ViewData["investmentPercent"] = currentUser.InvestmentPercent;
                ViewData["autoAllocateInvestment"] = currentUser.AutoAllocateInvestments;
                var thisMonthInvest = Math.Round(thisMonthIncome * currentUser.InvestmentPercent / 100m, 2);
                ViewData["thisMonthInvest"] = thisMonthInvest;
            }
            else
            {
                ViewData["savedThisMonth"] = 0;
                ViewData["totalbalance"] = 0;
                ViewData["spentThisMonth"] = 0;
                ViewData["thisMonthBudget"] = 0;
                ViewData["LeftInBudget"] = 0;
                ViewData["totalSaved"] = 0;

            }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateInvestmentSettings(decimal investmentPercent, bool autoAllocateInvestment)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return RedirectToAction("Index");

        // Clamp to valid range
        if (investmentPercent < 0m) investmentPercent = 0m;
        if (investmentPercent > 100m) investmentPercent = 100m;

        currentUser.InvestmentPercent = investmentPercent;
        currentUser.AutoAllocateInvestments = autoAllocateInvestment;

        var result = await _userManager.UpdateAsync(currentUser);
        // For now ignore failure details and redirect back
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
