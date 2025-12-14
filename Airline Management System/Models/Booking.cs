namespace Airline_Management_System.Models
{
    public enum BookingStatus
    {
        Booked,      // Default when created
        CheckedIn,   // When user checks in online
        Cancelled    // When user cancels (instead of deleting)
    }

    public class Booking
    {
        public int Id { get; set; }

        // Foreign Key to Flight
        public int FlightId { get; set; }
        public Flight? Flight { get; set; }

        // Foreign Key to Passenger
        public int PassengerId { get; set; }
        public Passenger? Passenger { get; set; }

        public string? SeatNumber { get; set; }
        public DateTime BookingDate { get; set; }
        public string? Status { get; set; }
    }
}
