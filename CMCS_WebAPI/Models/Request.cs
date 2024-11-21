using System.ComponentModel.DataAnnotations;

namespace CMCS_WebAPI.Models
{
    public class Request
    {
        [Key]
        [Required]
        public int RequestID { get; set; }

        [Required]
        public int LecturerID { get; set; }

        [Required]
        public string? RequestFor { get; set; }

        [Required]
        public double HoursWorked { get; set; }

        [Required]
        public double HourlyRate { get; set; }

        [Required]
        public string? Description { get; set; }

        [Required]
        public string? RequestStatus { get; set; }

        [Required]
        public DateTime? DateSubmitted { get; set; }
    }
}
