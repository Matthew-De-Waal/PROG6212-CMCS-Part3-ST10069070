using System.ComponentModel.DataAnnotations;

namespace CMCS_WebAPI.Models
{
    // This class represents the Manager entity within the database.
    public class Manager
    {
        // Automatic Properties
        [Key]
        [Required]
        public int ManagerID { get; set; }

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
