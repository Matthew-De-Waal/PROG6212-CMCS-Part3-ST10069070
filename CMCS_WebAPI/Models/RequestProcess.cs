using System.ComponentModel.DataAnnotations;

namespace CMCS_WebAPI.Models
{
    // This class represents the RequestProcess entity within the database.
    public class RequestProcess
    {
        // Automatic Properties
        [Key]
        [Required]
        public int RequestProcessID { get; set; }

        [Required]
        public int RequestID { get; set; }

        [Required]
        public int ManagerID { get; set; }

        [Required]
        public string? Status { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}
