using System.ComponentModel.DataAnnotations;

namespace Airline_Management_System.Models
{
    public class Feedback
    {
        public int Id { get; set; }

        [Required]
        public string? CustomerName { get; set; }

        [Required]
        [StringLength(500)]
        public string? Comments { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        public int FlightId { get; set; }
        public Flight? Flight { get; set; }
    }
}