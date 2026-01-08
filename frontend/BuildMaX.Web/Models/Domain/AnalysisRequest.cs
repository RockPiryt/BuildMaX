using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BuildMaX.Web.Models.Domain.Enums;

namespace BuildMaX.Web.Models.Domain
{
    public class AnalysisRequest
    {
        public int AnalysisRequestId { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Wariant")]
        public int VariantId { get; set; }

        [Required]
        [StringLength(250)]
        [Display(Name = "Adres działki")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Range(100, 5_000_000)]
        [Display(Name = "Powierzchnia działki (m²)")]
        public decimal PlotAreaM2 { get; set; }

        [Required]
        [Range(200, 200_000)]
        [Display(Name = "Powierzchnia modułu (m²)")]
        public decimal ModuleAreaM2 { get; set; }

        [Display(Name = "Procent zabudowy")]
        [Range(0, 100)]
        public decimal? BuiltUpPercent { get; set; }

        [Display(Name = "Powierzchnia zielona (m²)")]
        public decimal? GreenAreaM2 { get; set; }

        [Display(Name = "Powierzchnia utwardzona (m²)")]
        public decimal? HardenedAreaM2 { get; set; }

        [Display(Name = "Miejsca parkingowe TIR")]
        public int? TruckParkingSpots { get; set; }

        [Display(Name = "Miejsca parkingowe osobowe")]
        public int? CarParkingSpots { get; set; }

        [Display(Name = "Wykryto ryzyko archeologiczne")]
        public bool HasArchaeologyRisk { get; set; }

        [Display(Name = "Wykryto duże roboty ziemne")]
        public bool HasEarthworksRisk { get; set; }

        [Display(Name = "Status")]
        public AnalysisStatus Status { get; set; } = AnalysisStatus.New;

        [Display(Name = "Data utworzenia")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Relacje
        public Variant Variant { get; set; } = null!;
    }
}
