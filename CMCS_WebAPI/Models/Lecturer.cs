using System.ComponentModel.DataAnnotations;

namespace CMCS_WebAPI.Models
{
    public class Lecturer
    {
        [Key]
        [Required]
        public int LecturerID { get; set; }

        [Required]
        public string? FirstName { get; set; }

        [Required]
        public string? LastName { get; set; }

        [Required]
        public string? IdentityNumber { get; set; }

        [Required]
        public string? EmailAddress { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
