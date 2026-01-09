using System;
using System.ComponentModel.DataAnnotations;
using BuildMaX.Web.Models.Domain.Enums;
using BuildMaX.Web.Models.Identity;

namespace BuildMaX.Web.Models.Domain
{
    public class AnalysisRequest
    {
        public int AnalysisRequestId { get; set; }
        // Owner (ASP.NET Identity user)
        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser? ApplicationUser { get; set; }

        [Required]
        [Display(Name = "Wariant")]
        public int VariantId { get; set; }

        public Variant Variant { get; set; } = null!;

        [Required]
        [StringLength(250)]
        [Display(Name = "Adres działki")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Range(100, 5_000_000)]
        [Display(Name = "Powierzchnia działki (m²)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal PlotAreaM2 { get; set; }

        [Required]
        [Range(200, 200_000)]
        [Display(Name = "Powierzchnia modułu (m²)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal ModuleAreaM2 { get; set; }

        [Range(0, 100)]
        [Display(Name = "Procent zabudowy")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? BuiltUpPercent { get; set; }

        [Display(Name = "Powierzchnia zielona (m²)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? GreenAreaM2 { get; set; }

        [Display(Name = "Powierzchnia utwardzona (m²)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? HardenedAreaM2 { get; set; }

        [Display(Name = "Miejsca parkingowe TIR")]
        public int? TruckParkingSpots { get; set; }

        [Display(Name = "Miejsca parkingowe osobowe")]
        public int? CarParkingSpots { get; set; }

        [Display(Name = "Wykryto ryzyko archeologiczne")]
        public bool HasArchaeologyRisk { get; set; }

        [Display(Name = "Wykryto duże roboty ziemne")]
        public bool HasEarthworksRisk { get; set; }

        [Required]
        [EnumDataType(typeof(AnalysisStatus))]
        [Display(Name = "Status")]
        public AnalysisStatus Status { get; set; } = AnalysisStatus.New;

        [DataType(DataType.DateTime)]
        [Display(Name = "Data utworzenia")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
