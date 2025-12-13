using Airline_Management_System.Data;
using Airline_Management_System.Models;
using Airline_Management_System.Services;
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
    [Authorize] // Must be logged in
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailSender _emailSender;

        public BookingsController(ApplicationDbContext context, EmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var bookings = _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Passenger);

            if (User.IsInRole("Admin"))
            {
                // Admin sees EVERYTHING
                return View(await bookings.ToListAsync());
            }
            else
            {
                // Regular User sees ONLY THEIR OWN bookings
                var userEmail = User.Identity.Name;
                return View(await bookings
                    .Where(b => b.Passenger.ContactEmail == userEmail)
                    .ToListAsync());
            }
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Passenger)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Booking/Create
        public IActionResult Create()
        {
            var userEmail = User.Identity?.Name;
            var passenger = _context.Passengers.FirstOrDefault(p => p.ContactEmail == userEmail);

            if (passenger == null && !User.IsInRole("Admin"))
            {
                return RedirectToAction("Create", "Passenger");
            }

            if (User.IsInRole("Admin"))
            {
                ViewData["PassengerId"] = new SelectList(_context.Passengers, "Id", "FullName");
            }
            else
            {
                ViewData["PassengerId"] = passenger.Id;
            }

            // We select the flights and create a custom "DisplayText" combining multiple fields
            var flightList = _context.Flights
                .Where(f => f.DepartureTime > DateTime.Now)
                .Select(f => new
                {
                    Id = f.Id,
                    DisplayText = $"{f.FlightNumber}: {f.Origin} ➝ {f.Destination} ({f.DepartureTime.ToShortDateString()})"
                })
                .ToList();

            ViewData["FlightId"] = new SelectList(flightList, "Id", "DisplayText");

            return View();
        }

        // POST: Booking/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FlightId,PassengerId,SeatNumber,BookingDate")] Booking booking)
        {
            // 1. Auto-fill system fields
            booking.Status = "Confirmed";
            booking.BookingDate = DateTime.Now;

            // 2. IMPORTANT: Remove "Flight" and "Passenger" objects from validation
            // Since the form only sends IDs (FlightId, PassengerId), the Model Validator 
            // might complain that the full objects are null. We tell it to ignore them.
            ModelState.Remove("Flight");
            ModelState.Remove("Passenger");

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();

                // --- GET DETAILS FOR EMAIL ---
                // We need to fetch the Flight info to show in the email
                var flight = await _context.Flights.FindAsync(booking.FlightId);
                var passenger = await _context.Passengers.FindAsync(booking.PassengerId);

                if (flight != null && passenger != null)
                {
                    string subject = "Booking Confirmation - Flight " + flight.FlightNumber;
                    string body = $@"
            <h1>Booking Confirmed!</h1>
            <p>Dear {passenger.FullName},</p>
            <p>You are booked on <b>{flight.FlightNumber}</b> from {flight.Origin} to {flight.Destination}.</p>
            <p><b>Date:</b> {flight.DepartureTime}</p>
            <p><b>Seat:</b> {booking.SeatNumber}</p>
            <p>Have a safe trip!</p>";

                    try
                    {
                        await _emailSender.SendEmailAsync(passenger.ContactEmail, subject, body);
                    }
                    catch { /* Ignore email errors */ }
                }
                // -----------------------------

                return RedirectToAction(nameof(Index));
            }

            // --- ERROR HANDLING (If code reaches here, save failed) ---

            // 3. Reload Flight List (Your custom logic)
            var flightList = _context.Flights
                .Where(f => f.DepartureTime > DateTime.Now)
                .Select(f => new
                {
                    Id = f.Id,
                    DisplayText = $"{f.FlightNumber}: {f.Origin} ➝ {f.Destination} ({f.DepartureTime.ToShortDateString()})"
                })
                .ToList();

            // Ensure the previously selected flight is still selected
            ViewData["FlightId"] = new SelectList(flightList, "Id", "DisplayText", booking.FlightId);

            // 4. MISSING PART: Reload Passenger Data
            // We need to decide again if we show a Dropdown (Admin) or just keep the ID (User)
            if (User.IsInRole("Admin"))
            {
                ViewData["PassengerId"] = new SelectList(_context.Passengers, "Id", "FullName", booking.PassengerId);
            }
            else
            {
                ViewData["PassengerId"] = booking.PassengerId;
            }

            return View(booking);
        }

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["FlightId"] = new SelectList(_context.Flights, "Id", "FlightNumber", booking.FlightId);
            ViewData["PassengerId"] = new SelectList(_context.Passengers, "Id", "Id", booking.PassengerId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FlightId,PassengerId,SeatNumber,BookingDate,Status")] Booking booking)
        {
            if (id != booking.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.Id))
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
            ViewData["FlightId"] = new SelectList(_context.Flights, "Id", "FlightNumber", booking.FlightId);
            ViewData["PassengerId"] = new SelectList(_context.Passengers, "Id", "Id", booking.PassengerId);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Flight)
                .Include(b => b.Passenger)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }

        [HttpGet]
        public IActionResult GetTakenSeats(int flightId)
        {
            var flight = _context.Flights.Find(flightId);

            // Handle edge case: If capacity is 0 (old data), default to 20
            int safeCapacity = (flight != null && flight.TotalSeats > 0) ? flight.TotalSeats : 20;

            var takenSeats = _context.Bookings
                .Where(b => b.FlightId == flightId)
                .Select(b => b.SeatNumber)
                .ToList();

            return Json(new { taken = takenSeats, capacity = safeCapacity });
        }
    }
}
