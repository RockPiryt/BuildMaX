using Microsoft.AspNetCore.Razor.TagHelpers;
using BuildMaX.Web.Models.Domain.Enums;

namespace BuildMaX.Web.Helpers.TagHelpers
{
    [HtmlTargetElement("status-badge")]
    public class StatusBadgeTagHelper : TagHelper
    {
        public AnalysisStatus Status { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var color = Status switch
            {
                AnalysisStatus.New => "#888",
                AnalysisStatus.Completed => "green",
                AnalysisStatus.Failed => "red",
                _ => "#444"
            };

            output.TagName = "span";
            output.Attributes.SetAttribute("style", $"color:{color}; font-weight:bold;");
            output.Content.SetContent(Status.ToString());
        }
    }
}
