using System.Collections.Generic;
using BuildMaX.Web.Models.Domain;

namespace BuildMaX.Web.Models.ViewModels
{
    public class PricingViewModel
    {
        public IReadOnlyList<Variant> Variants { get; init; } = new List<Variant>();

        public string? HighlightedVariantName { get; init; }

        public bool IsUserLoggedIn { get; init; }
    }
}
