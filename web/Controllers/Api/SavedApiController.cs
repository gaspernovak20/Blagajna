using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using web.Data;
using web.Models;
using web.Filters;

namespace web.Controllers_Api
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKeyAuth]

    public class SavedApiController : ControllerBase
    {
        private readonly BlagajnaContext _context;

        public SavedApiController(BlagajnaContext context)
        {
            _context = context;
        }

        // GET: api/SavedApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Saved>>> GetSavedMoney()
        {
            return await _context.SavedMoney.ToListAsync();
        }

        // GET: api/SavedApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Saved>> GetSaved(int id)
        {
            var saved = await _context.SavedMoney.FindAsync(id);

            if (saved == null)
            {
                return NotFound();
            }

            return saved;
        }

        // PUT: api/SavedApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSaved(int id, Saved saved)
        {
            if (id != saved.Id)
            {
                return BadRequest();
            }

            _context.Entry(saved).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SavedExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SavedApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Saved>> PostSaved(Saved saved)
        {
            _context.SavedMoney.Add(saved);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSaved", new { id = saved.Id }, saved);
        }

        // DELETE: api/SavedApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSaved(int id)
        {
            var saved = await _context.SavedMoney.FindAsync(id);
            if (saved == null)
            {
                return NotFound();
            }

            _context.SavedMoney.Remove(saved);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SavedExists(int id)
        {
            return _context.SavedMoney.Any(e => e.Id == id);
        }
    }
}
