using BuildMaX.Web.Models.Domain;

namespace BuildMaX.Web.Models.ViewModels
{
    public class DashboardViewModel
    {
        public List<BuildMaX.Web.Controllers.StatusStat> StatusStats { get; set; } = [];
        public decimal? AverageBuiltUpPercent { get; set; }
        public List<AnalysisRequest> LatestRequests { get; set; } = [];
    }
}
