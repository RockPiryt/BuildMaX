using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BuildMaX.Web.Models.Domain
{
    public class Variant
    {
        public int VariantId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Nazwa wariantu")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Range(0, 100_000)]
        [DataType(DataType.Currency)]
        [Display(Name = "Cena")]
        public decimal Price { get; set; }

        [StringLength(500)]
        [Display(Name = "Opis")]
        public string? Description { get; set; }

        [Display(Name = "Zawiera raport PDF")]
        public bool IncludesPdf { get; set; }

        [Display(Name = "Zawiera dane procentowe")]
        public bool IncludesPercentDetails { get; set; }

        [Display(Name = "Zawiera plan zagospodarowania")]
        public bool IncludesSitePlan { get; set; }

        // Relacja 1:N
        public ICollection<AnalysisRequest> AnalysisRequests { get; set; } = new List<AnalysisRequest>();
    }
}
