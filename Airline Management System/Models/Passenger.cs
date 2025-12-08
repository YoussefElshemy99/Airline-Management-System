namespace Airline_Management_System.Models
{
    public class Passenger
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PassportNumber { get; set; }
        public string ContactEmail { get; set; }
        public string PhoneNumber { get; set; }

        // A passenger can have many bookings
        public ICollection<Booking> Bookings { get; set; }
    }
}
