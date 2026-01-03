using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web.Data;
using System.Globalization;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace web.Controllers
{
    public class ReportsController : Controller
    {
        private readonly BlagajnaContext _db;

        public ReportsController(BlagajnaContext db)
        {
            _db = db;
        }

        // GET: /Reports/Charts
        public IActionResult Charts()
        {
            return View();
        }

        // Returns chart data gathered from the database
        [HttpGet]
        public async Task<IActionResult> GetChartData()
        {
            // last 12 months (including current)
            var end = DateTime.Now;
            var start = new DateTime(end.Year, end.Month, 1).AddMonths(-11);

            // prepare month labels
            var months = Enumerable.Range(0, 12)
                .Select(i => start.AddMonths(i))
                .Select(d => d.ToString("MMM", CultureInfo.InvariantCulture))
                .ToArray();

            // monthly incomes
            var incomesQuery = await _db.Incomes
                .AsNoTracking()
                .Where(i => i.Date >= start && i.Date <= end)
                .ToListAsync();

            // monthly transactions (spending)
            var transQuery = await _db.Transactions
                .Include(t => t.Category)
                .AsNoTracking()
                .Where(t => t.Date >= start && t.Date <= end)
                .ToListAsync();

            // optional saved money entries
            var savedQuery = await _db.SavedMoney
                .AsNoTracking()
                .Where(s => s.Date >= start && s.Date <= end)
                .ToListAsync();

            // compute balances per month = incomes - transactions - saved
            var balances = new List<double>();
            for (int i = 0; i < 12; i++)
            {
                var month = start.AddMonths(i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

                var incomeSum = incomesQuery.Where(x => x.Date >= monthStart && x.Date <= monthEnd).Sum(x => x.Amount);
                var transSum = transQuery.Where(x => x.Date >= monthStart && x.Date <= monthEnd).Sum(x => x.Amount);
                var savedSum = savedQuery.Where(x => x.Date >= monthStart && x.Date <= monthEnd).Sum(x => x.Amount);

                // net change
                var net = (double)(incomeSum - transSum - savedSum);
                balances.Add(Math.Round(net, 2));
            }

            // spending by category (last 12 months)
            var byCategory = transQuery
                .GroupBy(t => t.Category != null ? t.Category.Name : "Uncategorized")
                .Select(g => new { Category = g.Key, Amount = (double)g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Amount)
                .ToList();

            var categories = byCategory.Select(x => x.Category).ToArray();
            var spending = byCategory.Select(x => Math.Round(x.Amount, 2)).ToArray();

            var data = new
            {
                months,
                balances,
                categories,
                spending
            };

            return Json(data);
        }
    }
}
