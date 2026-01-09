using Microsoft.AspNetCore.Identity;
using BuildMaX.Web.Models.Domain;
using System.Collections.Generic;

namespace BuildMaX.Web.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public ICollection<AnalysisRequest> AnalysisRequests { get; set; } = new List<AnalysisRequest>();
    }
}
