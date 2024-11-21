using System.ComponentModel.DataAnnotations;

namespace CMCS_WebAPI.Models
{
    public class RequestDocument
    {
        [Key]
        [Required]
        public int RequestDocumentID { get; set; }

        [Required]
        public int RequestID { get; set; }

        [Required]
        public int DocumentID { get; set; }
    }
}
