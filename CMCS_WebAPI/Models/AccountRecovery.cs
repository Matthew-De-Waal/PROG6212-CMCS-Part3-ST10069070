using System.ComponentModel.DataAnnotations;

namespace CMCS_WebAPI.Models
{
    // This class represents the AccountRecovery entity within the database.
    public class AccountRecovery
    {
        // Automatic Properties
        [Key]
        [Required]
        public int AccountRecoveryID { get; set; }

        [Required]
        public string? Method { get; set; }

        [Required]
        public string? Value { get; set; }

        [Required]
        public string? UserID { get; set; }
    }
}
