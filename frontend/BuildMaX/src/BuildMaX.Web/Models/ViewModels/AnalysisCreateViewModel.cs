using System;
using System.ComponentModel.DataAnnotations;
using BuildMaX.Web.Validation;

namespace BuildMaX.Web.Models.ViewModels
{
    public class AnalysisCreateViewModel
    {
        [Required]
        [Display(Name = "Wariant analizy")]
        public int VariantId { get; set; }

        [Required]
        [Display(Name = "Adres działki")]
        [StringLength(250, MinimumLength = 5)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Powierzchnia działki (m²)")]
        [Range(100, 5_000_000, ErrorMessage = "Powierzchnia działki musi mieścić się w zakresie 100 – 5 000 000 m².")]
        public decimal PlotAreaM2 { get; set; }

        [Required]
        [Display(Name = "Powierzchnia modułu magazynowego (m²)")]
        [Range(200, 200_000, ErrorMessage = "Powierzchnia modułu musi mieścić się w zakresie 200 – 200 000 m².")]
        [ModuleFitsPlot(nameof(PlotAreaM2))]
        public decimal ModuleAreaM2 { get; set; }

        [Display(Name = "Uwzględnij możliwe stanowiska archeologiczne")]
        public bool HasArchaeologyRisk { get; set; }

        [Display(Name = "Teren podmokły / duże roboty ziemne")]
        public bool HasEarthworksRisk { get; set; }

        [VariantAvailability(nameof(PlotAreaM2))]
        public int? RequestedVariantOverride { get; set; }

        [Display(Name = "Data złożenia")]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
