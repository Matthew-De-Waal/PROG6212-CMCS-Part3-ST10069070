using System.ComponentModel.DataAnnotations;

namespace CMCS_WebAPI.Models
{
    // This class represents the RequestDocument entity within the database.
    public class RequestDocument
    {
        // Automatic Properties
        [Key]
        [Required]
        public int RequestDocumentID { get; set; }

        [Required]
        public int RequestID { get; set; }

        [Required]
        public int DocumentID { get; set; }
    }
}
