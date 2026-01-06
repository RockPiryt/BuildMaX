using System.ComponentModel.DataAnnotations;

namespace BuildMaX.Web.Models.Domain
{
    public class LegalDocument
    {
        public int LegalDocumentId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Tytuł dokumentu")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        [Display(Name = "Adres URL")]
        [DataType(DataType.Url)]
        public string Url { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Kategoria")]
        public string? Category { get; set; }

        // Relacja 1:N do Variant (opcjonalna)
        public int? VariantId { get; set; }
        public Variant? Variant { get; set; }
    }
}
