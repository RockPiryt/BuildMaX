using System;
using System.Collections.Generic;

namespace BuildMaX.Web.Models.ViewModels
{
    public class AnalysisResultViewModel
    {
        public int AnalysisRequestId { get; init; }

        public string Address { get; init; } = string.Empty;
        public string VariantName { get; init; } = string.Empty;

        public decimal PlotAreaM2 { get; init; }
        public decimal ModuleAreaM2 { get; init; }

        public decimal BuiltUpPercent { get; init; }
        public decimal GreenAreaM2 { get; init; }
        public decimal HardenedAreaM2 { get; init; }

        public int TruckParkingSpots { get; init; }
        public int CarParkingSpots { get; init; }

        public bool HasArchaeologyRisk { get; init; }
        public bool HasEarthworksRisk { get; init; }

        public IReadOnlyList<string> ProblemAreas { get; init; } = new List<string>();

        public IReadOnlyList<LegalDocRow> LegalDocuments { get; init; } = new List<LegalDocRow>();

        public bool CanDownloadPdf { get; init; }
        public bool CanViewPercentDetails { get; init; }
        public bool CanViewSitePlan { get; init; }

        public DateTime CompletedAt { get; init; }
    }

    public class LegalDocRow
    {
        public string Title { get; init; } = string.Empty;
        public string Url { get; init; } = string.Empty;
        public string? Category { get; init; }
    }
}
