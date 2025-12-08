using System.ComponentModel.DataAnnotations;

namespace Airline_Management_System.Models
{
    public class Flight
    {
        public int Id { get; set; }

        [Required]
        public string FlightNumber { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public string AircraftType { get; set; }
        public int TotalSeats { get; set; }

        // A flight has many bookings
        public ICollection<Booking> Bookings { get; set; }
    }
}
