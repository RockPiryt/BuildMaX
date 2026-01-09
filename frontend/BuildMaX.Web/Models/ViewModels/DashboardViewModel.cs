using BuildMaX.Web.Models.Domain;
using BuildMaX.Web.Models.Domain.Enums;

namespace BuildMaX.Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        public List<StatusStatItem> StatusStats { get; set; } = new();
        public decimal? AverageBuiltUpPercent { get; set; }
        public List<AnalysisRequest> LatestRequests { get; set; } = new();
    }

    public class StatusStatItem
    {
        public AnalysisStatus Status { get; set; }
        public int Count { get; set; }
    }
}
