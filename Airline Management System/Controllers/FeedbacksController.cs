using Airline_Management_System.Data;
using Airline_Management_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Airline_Management_System.Controllers
{
    public class FeedbacksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbacksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Feedback/Index
        public async Task<IActionResult> Index()
        {
            // Fetch all reviews and include the Flight details
            var reviews = _context.Feedbacks.Include(f => f.Flight);
            return View(await reviews.ToListAsync());
        }

        // GET: Feedbacks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // GET: Feedback/Create
        public IActionResult Create()
        {
            var userEmail = User.Identity?.Name;

            // 1. Find all COMPLETED flights for this specific user
            // Logic: Must match email AND Flight date must be in the past
            var completedBookings = _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Passenger)
                .Where(b => b.Passenger.ContactEmail == userEmail && b.Flight.DepartureTime < DateTime.Now)
                .Select(b => b.Flight)
                .ToList();

            // 2. If list is empty, kick them out
            if (!completedBookings.Any())
            {
                TempData["Error"] = "You can only review flights you have already completed.";
                return RedirectToAction("Index", "Home");
            }

            // 3. Create a nice dropdown list (e.g., "AA123 - Dec 1st")
            var flightList = completedBookings.Select(f => new
            {
                Id = f.Id,
                DisplayText = $"{f.FlightNumber} ({f.Origin} -> {f.Destination}) - {f.DepartureTime.ToShortDateString()}"
            });

            ViewData["FlightId"] = new SelectList(flightList, "Id", "DisplayText");

            // 4. Pre-fill name
            var passenger = _context.Passengers.FirstOrDefault(p => p.ContactEmail == userEmail);
            return View(new Feedback { CustomerName = passenger?.FullName ?? userEmail });
        }

        // POST: Feedback/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CustomerName,Comments,Rating,FlightId")] Feedback feedback)
        {
            // Ignore the "Flight" object validation (we only need the ID)
            ModelState.Remove("Flight");

            if (ModelState.IsValid)
            {
                _context.Add(feedback);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            // If error, reload the dropdown so it doesn't disappear
            var userEmail = User.Identity?.Name;
            var completedBookings = _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Passenger)
                .Where(b => b.Passenger.ContactEmail == userEmail && b.Flight.DepartureTime < DateTime.Now)
                .Select(b => b.Flight)
                .ToList();

            var flightList = completedBookings.Select(f => new
            {
                Id = f.Id,
                DisplayText = $"{f.FlightNumber} ({f.Origin} -> {f.Destination}) - {f.DepartureTime.ToShortDateString()}"
            });

            ViewData["FlightId"] = new SelectList(flightList, "Id", "DisplayText", feedback.FlightId);

            return View(feedback);
        }

        // GET: Feedbacks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return NotFound();
            }
            return View(feedback);
        }

        // POST: Feedbacks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CustomerName,Comments,Rating")] Feedback feedback)
        {
            if (id != feedback.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(feedback);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeedbackExists(feedback.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(feedback);
        }

        // GET: Feedbacks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var feedback = await _context.Feedbacks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (feedback == null)
            {
                return NotFound();
            }

            return View(feedback);
        }

        // POST: Feedback/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Security: Only Admins can delete
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool FeedbackExists(int id)
        {
            return _context.Feedbacks.Any(e => e.Id == id);
        }
    }
}
