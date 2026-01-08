using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;
using web.Models.ViewModels;

namespace web.Controllers;

[Authorize]
public class RoomsController : Controller
{
    private readonly BlagajnaContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoomsController(BlagajnaContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Rooms/Join
    public IActionResult Join()
    {
        return View();
    }

    public class JoinRoomDto
    {
        public string? Code { get; set; }
    }

    // POST: /Rooms/Join
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(JoinRoomDto dto)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var code = (dto.Code ?? "").Trim();
        if (string.IsNullOrWhiteSpace(code))
        {
            ModelState.AddModelError("", "Vnesi številko sobe.");
            return View(dto);
        }

        var room = await _context.Rooms.FirstOrDefaultAsync(r => r.Code == code);
        if (room == null)
        {
            room = new Room { Code = code };
            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();
        }

        var alreadyMember = await _context.RoomMembers
            .AnyAsync(m => m.RoomId == room.Id && m.UserId == currentUser.Id);

        if (!alreadyMember)
        {
            _context.RoomMembers.Add(new RoomMember
            {
                RoomId = room.Id,
                UserId = currentUser.Id
            });
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Details), new { id = room.Id });
    }

    // GET: /Rooms/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var room = await _context.Rooms
            .Include(r => r.Members)
                .ThenInclude(m => m.User)
            .Include(r => r.Expenses)
                .ThenInclude(e => e.PayerUser)
            .Include(r => r.Expenses)
                .ThenInclude(e => e.Participants)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return NotFound();

        // preveri, da je user član sobe
        var isMember = room.Members.Any(m => m.UserId == currentUser.Id);
        if (!isMember) return Forbid();

        return View(room);
    }


    // GET: /Rooms/AddExpense?roomId=5
    public async Task<IActionResult> AddExpense(int roomId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var isMember = await _context.RoomMembers
            .AnyAsync(m => m.RoomId == roomId && m.UserId == currentUser.Id);
        if (!isMember) return Forbid();

        var room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == roomId);
        if (room == null) return NotFound();

        if (room.IsSettled)
        {
            return RedirectToAction(nameof(Settle), new { id = roomId });
        }

        var members = await _context.RoomMembers
            .Where(m => m.RoomId == roomId)
            .Include(m => m.User)
            .ToListAsync();

        var viewModel = new AddRoomExpenseViewModel
        {
            RoomId = roomId,
            Members = members.Select(m => new AddRoomExpenseViewModel.ParticipantCheckbox
            {
                UserId = m.User!.Id,
                DisplayName = GetUserDisplayName(m.User),
                IsSelected = m.UserId == currentUser.Id
            }).ToList()
        };

        return View(viewModel);
    }
    private string GetUserDisplayName(ApplicationUser user)
    {
        if (!string.IsNullOrWhiteSpace(user.FirstName) || !string.IsNullOrWhiteSpace(user.LastName))
        {
            return $"{user.FirstName} {user.LastName}".Trim();
        }
        return user.UserName ?? user.Id;
    }

    // POST: /Rooms/AddExpense
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExpense(AddRoomExpenseViewModel model)
        {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var isMember = await _context.RoomMembers
            .AnyAsync(m => m.RoomId == model.RoomId && m.UserId == currentUser.Id);
        if (!isMember) return Forbid();

        var room = await _context.Rooms
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == model.RoomId);
        if (room == null) return NotFound();

        if (room.IsSettled)
        {
            return RedirectToAction(nameof(Settle), new { id = model.RoomId });
        }

        var selectedParticipants = model.Members.Where(m => m.IsSelected).ToList();
        if (selectedParticipants.Count == 0)
        {
            ModelState.AddModelError("", "Select at least one participant.");
            var members = await _context.RoomMembers
                .Where(m => m.RoomId == model.RoomId)
                .Include(m => m.User)
                .ToListAsync();
            model.Members = members.Select(m => new AddRoomExpenseViewModel.ParticipantCheckbox
            {
                UserId = m.User!.Id,
                DisplayName = GetUserDisplayName(m.User),
                IsSelected = model.Members.FirstOrDefault(p => p.UserId == m.User.Id)?.IsSelected ?? false
            }).ToList();
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            var members = await _context.RoomMembers
                .Where(m => m.RoomId == model.RoomId)
                .Include(m => m.User)
                .ToListAsync();
            model.Members = members.Select(m => new AddRoomExpenseViewModel.ParticipantCheckbox
            {
                UserId = m.User!.Id,
                DisplayName = GetUserDisplayName(m.User),
                IsSelected = model.Members.FirstOrDefault(p => p.UserId == m.User.Id)?.IsSelected ?? false
            }).ToList();
            return View(model);
        }

        var expense = new RoomExpense
        {
            RoomId = model.RoomId,
            PayerUserId = currentUser.Id,
            Amount = model.Amount,
            Description = model.Description,
            CreatedAt = DateTime.UtcNow
        };
        _context.RoomExpenses.Add(expense);
        await _context.SaveChangesAsync();

        foreach (var participant in selectedParticipants)
        {
            _context.RoomExpenseParticipants.Add(new RoomExpenseParticipant
            {
                RoomExpenseId = expense.Id,
                UserId = participant.UserId
            });
        }
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = model.RoomId });
    }

    public async Task<IActionResult> Settle(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var room = await _context.Rooms
            .Include(r => r.Members).ThenInclude(m => m.User)
            .Include(r => r.Expenses).ThenInclude(e => e.PayerUser)
            .Include(r => r.Expenses).ThenInclude(e => e.Participants)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return NotFound();
        if (!room.Members.Any(m => m.UserId == currentUser.Id)) return Forbid();

        string DisplayName(ApplicationUser? u, string fallbackId)
        {
            if (u == null) return fallbackId;
            if (!string.IsNullOrWhiteSpace(u.FirstName) || !string.IsNullOrWhiteSpace(u.LastName))
                return $"{u.FirstName} {u.LastName}".Trim();
            return u.UserName ?? u.Id;
        }

        var balances = room.Members.ToDictionary(
            m => m.UserId,
            m => 0m
        );
        foreach (var e in room.Expenses)
        {
            if (!balances.ContainsKey(e.PayerUserId))
                balances[e.PayerUserId] = 0m;

            var participantIds = (e.Participants?.Select(p => p.UserId).Distinct().ToList()) ?? new List<string>();
            if (participantIds.Count == 0) continue;

            balances[e.PayerUserId] += e.Amount;

            var share = e.Amount / participantIds.Count;
            foreach (var uid in participantIds)
            {
                if (!balances.ContainsKey(uid))
                    balances[uid] = 0m;

                balances[uid] -= share;
            }
        }

        var vm = new web.Models.ViewModels.RoomSettlementViewModel
        {
            RoomId = room.Id,
            RoomCode = room.Code,
            TotalSpending = room.Expenses.Sum(x => x.Amount),
            Balances = room.Members.Select(m => new web.Models.ViewModels.RoomSettlementViewModel.MemberBalance
            {
                UserId = m.UserId,
                DisplayName = DisplayName(m.User, m.UserId),
                Balance = balances.TryGetValue(m.UserId, out var b) ? b : 0m
            })
            .OrderByDescending(b => b.Balance)
            .ToList()
        };

        var creditors = vm.Balances
            .Where(b => b.Balance > 0.00001m)
            .Select(b => new { b.UserId, b.DisplayName, Amount = b.Balance })
            .ToList();

        var debtors = vm.Balances
            .Where(b => b.Balance < -0.00001m)
            .Select(b => new { b.UserId, b.DisplayName, Amount = -b.Balance })
            .ToList();

        int i = 0, j = 0;
        while (i < debtors.Count && j < creditors.Count)
        {
            var pay = Math.Min(debtors[i].Amount, creditors[j].Amount);

            vm.Transfers.Add(new web.Models.ViewModels.RoomSettlementViewModel.Transfer
            {
                FromUserId = debtors[i].UserId,
                FromName = debtors[i].DisplayName,
                ToUserId = creditors[j].UserId,
                ToName = creditors[j].DisplayName,
                Amount = decimal.Round(pay, 2)
            });

            debtors[i] = new { debtors[i].UserId, debtors[i].DisplayName, Amount = debtors[i].Amount - pay };
            creditors[j] = new { creditors[j].UserId, creditors[j].DisplayName, Amount = creditors[j].Amount - pay };

            if (debtors[i].Amount <= 0.00001m) i++;
            if (creditors[j].Amount <= 0.00001m) j++;
        }

        vm.IsSettled = room.IsSettled;
        vm.SettledAt = room.SettledAt;

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizeSettlement(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var room = await _context.Rooms
            .Include(r => r.Members)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return NotFound();
        if (!room.Members.Any(m => m.UserId == currentUser.Id)) return Forbid();

        if (!room.IsSettled)
        {
            room.IsSettled = true;
            room.SettledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Settle), new { id });
    }

    // GET: /Rooms/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var room = await _context.Rooms
            .Include(r => r.Members)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return NotFound();

        if (!room.Members.Any(m => m.UserId == currentUser.Id)) return Forbid();

        if (!room.IsSettled)
        {
            TempData["Error"] = "Room must be finalized before it can be deleted.";
            return RedirectToAction(nameof(Details), new { id });
        }

        return View(room);
    }

    // POST: /Rooms/DeleteConfirmed
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Challenge();

        var room = await _context.Rooms
            .Include(r => r.Members)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return NotFound();

        if (!room.Members.Any(m => m.UserId == currentUser.Id)) return Forbid();

        if (!room.IsSettled)
        {
            TempData["Error"] = "Room must be finalized before it can be deleted.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // 1) Pobriši participants
        var expenseIds = await _context.RoomExpenses
            .Where(e => e.RoomId == id)
            .Select(e => e.Id)
            .ToListAsync();

        var participants = await _context.RoomExpenseParticipants
            .Where(p => expenseIds.Contains(p.RoomExpenseId))
            .ToListAsync();

        _context.RoomExpenseParticipants.RemoveRange(participants);

        // 2) Pobriši expenses
        var expenses = await _context.RoomExpenses
            .Where(e => e.RoomId == id)
            .ToListAsync();

        _context.RoomExpenses.RemoveRange(expenses);

        // 3) Pobriši members
        var members = await _context.RoomMembers
            .Where(m => m.RoomId == id)
            .ToListAsync();

        _context.RoomMembers.RemoveRange(members);

        // 4) Pobriši sobo
        _context.Rooms.Remove(room);

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Room {room.Code} was deleted.";
        return RedirectToAction("Index", "Home");
    }
}
