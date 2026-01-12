using System.ComponentModel.DataAnnotations;
using BuildMaX.Web.Models.Domain;
using BuildMaX.Web.Models.Domain.Enums;


namespace BuildMaX.Web.Models.ViewModels.Analysis
{
    public class CreateAnalysisRequestViewModel
    {
        [Required]
        public int VariantId { get; set; }

        [Required]
        public AddressKind AddressKind { get; set; } = AddressKind.Address;

        // Adres
        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(120)]
        public string? City { get; set; }

        [StringLength(20)]
        public string? PostalCode { get; set; }

        [StringLength(160)]
        public string? Street { get; set; }

        [StringLength(30)]
        public string? StreetNumber { get; set; }

        // Działka
        [StringLength(60)]
        public string? PlotNumber { get; set; }

        [StringLength(120)]
        public string? CadastralArea { get; set; }

        [StringLength(120)]
        public string? Commune { get; set; }

        // Wymiary
        [Required]
        [Range(1, 100_000)]
        public decimal PlotWidthM { get; set; }

        [Required]
        [Range(1, 100_000)]
        public decimal PlotLengthM { get; set; }

        [Required]
        [Range(0.1, 10_000)]
        public decimal ModuleWidthM { get; set; }

        [Required]
        [Range(0.1, 10_000)]
        public decimal ModuleLengthM { get; set; }
    }
}
