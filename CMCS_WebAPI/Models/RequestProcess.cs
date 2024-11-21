using System.ComponentModel.DataAnnotations;

namespace CMCS_WebAPI.Models
{
    public class RequestProcess
    {
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
