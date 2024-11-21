using System.ComponentModel.DataAnnotations;

namespace CMCS_WebAPI.Models
{
    public class Document
    {
        // Automatic Properties
        [Key]
        [Required]
        public int DocumentID { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public long Size { get; set; }

        [Required]
        public string? Type { get; set; }

        [Required]
        public string? Section { get; set; }

        [Required]
        public string? Content { get; set; }

        [Required]
        public string? UserID { get; set; }
    }
}
