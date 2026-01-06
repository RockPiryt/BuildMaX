using System.Collections.Generic;

namespace BuildMaX.Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalAnalyses { get; init; }
        public int CompletedAnalyses { get; init; }
        public int ProcessingAnalyses { get; init; }

        public IReadOnlyList<VariantUsageRow> VariantUsage { get; init; } = new List<VariantUsageRow>();
        public IReadOnlyList<RecentAnalysisRow> RecentAnalyses { get; init; } = new List<RecentAnalysisRow>();
    }

    public class VariantUsageRow
    {
        public string VariantName { get; init; } = string.Empty;
        public int Count { get; init; }
    }

    public class RecentAnalysisRow
    {
        public int AnalysisRequestId { get; init; }
        public string Address { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public decimal? BuiltUpPercent { get; init; }
    }
}
