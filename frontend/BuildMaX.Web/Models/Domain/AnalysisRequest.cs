using System;
using System.ComponentModel.DataAnnotations;
using BuildMaX.Web.Models.Domain.Enums;
using BuildMaX.Web.Models.Identity;
using Microsoft.AspNetCore.Mvc;

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

        // ====== INPUT (user) ======

        [Required]
        [Range(1, 100_000)]
        [Display(Name = "Szerokość działki (m)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal PlotWidthM { get; set; }

        [Required]
        [Range(1, 100_000)]
        [Display(Name = "Długość działki (m)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal PlotLengthM { get; set; }

        [Required]
        [Range(0.1, 10_000)]
        [Display(Name = "Szerokość modułu (m)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal ModuleWidthM { get; set; }

        [Required]
        [Range(0.1, 10_000)]
        [Display(Name = "Długość modułu (m)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal ModuleLengthM { get; set; }

        // ====== CALCULATED (system) ======

        [ScaffoldColumn(false)]
        [Display(Name = "Powierzchnia działki (m²)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal PlotAreaM2 { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "Powierzchnia modułu (m²)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal ModuleAreaM2 { get; set; }

        [ScaffoldColumn(false)]
        [Range(0, 100)]
        [Display(Name = "Procent zabudowy")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? BuiltUpPercent { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "Powierzchnia zielona (m²)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? GreenAreaM2 { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "Powierzchnia utwardzona (m²)")]
        [DisplayFormat(DataFormatString = "{0:N2}")]
        public decimal? HardenedAreaM2 { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "Miejsca parkingowe TIR")]
        public int? TruckParkingSpots { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "Miejsca parkingowe osobowe")]
        public int? CarParkingSpots { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "Wykryto ryzyko archeologiczne")]
        public bool HasArchaeologyRisk { get; set; }

        [ScaffoldColumn(false)]
        [Display(Name = "Wykryto duże roboty ziemne")]
        public bool HasEarthworksRisk { get; set; }

        // ====== SYSTEM ======

        [Required]
        [EnumDataType(typeof(AnalysisStatus))]
        [Display(Name = "Status")]
        public AnalysisStatus Status { get; set; } = AnalysisStatus.New;

        [DataType(DataType.DateTime)]
        [Display(Name = "Data utworzenia")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
