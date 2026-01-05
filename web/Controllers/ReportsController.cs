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

            // investments (added chart)
            var investQuery = await _db.Investments
                .AsNoTracking()
                .Where(i => i.Date >= start && i.Date <= end)
                .ToListAsync();

            // compute balances per month = incomes - transactions - saved
            var balances = new List<double>();
            var invested = new List<double>();
            for (int i = 0; i < 12; i++)
            {
                var month = start.AddMonths(i);
                var monthStart = new DateTime(month.Year, month.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

                var incomeSum = incomesQuery.Where(x => x.Date >= monthStart && x.Date <= monthEnd).Sum(x => x.Amount);
                var transSum = transQuery.Where(x => x.Date >= monthStart && x.Date <= monthEnd).Sum(x => x.Amount);
                var savedSum = savedQuery.Where(x => x.Date >= monthStart && x.Date <= monthEnd).Sum(x => x.Amount);
                var investSum = investQuery.Where(x => x.Date >= monthStart && x.Date <= monthEnd).Sum(x => x.Amount);

                // net change
                var net = (double)(incomeSum - transSum - savedSum);
                balances.Add(Math.Round(net, 2));

                // track invested per month
                invested.Add(Math.Round((double)investSum, 2));
            }

            // spending by category (last 12 months)
            var byCategory = transQuery
                .GroupBy(t => t.Category != null ? t.Category.Name : "Uncategorized")
                .Select(g => new { Category = g.Key, Amount = (double)g.Sum(x => x.Amount) })
                .OrderByDescending(x => x.Amount)
                .ToList();

            var categories = byCategory.Select(x => x.Category).ToArray();
            var spending = byCategory.Select(x => Math.Round(x.Amount, 2)).ToArray();

            // compute a sensible max for investments chart (20% headroom)
            var maxInvest = invested.Any() ? invested.Max() : 0.0;
            var investMax = Math.Ceiling(maxInvest * 1.2);
            if (investMax <= 0) investMax = 10;

            var data = new
            {
                months,
                balances,
                invested,
                investMax,
                categories,
                spending
            };

            return Json(data);
        }
    }
}
