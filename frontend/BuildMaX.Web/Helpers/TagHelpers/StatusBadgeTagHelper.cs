using BuildMaX.Web.Models.Domain.Enums;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BuildMaX.Web.Helpers.TagHelpers
{
    [HtmlTargetElement("status-badge")]
    public class StatusBadgeTagHelper : TagHelper
    {
        public AnalysisStatus Status { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var (bg, fg) = Status switch
            {
                AnalysisStatus.New => ("#2b2b2b", "#ffffff"),
                AnalysisStatus.Completed => ("#0f5132", "#ffffff"),
                AnalysisStatus.Failed => ("#842029", "#ffffff"),
                _ => ("#2b2b2b", "#ffffff")
            };

            output.TagName = "span";
            output.Attributes.SetAttribute(
                "style",
                $"display:inline-block;padding:4px 10px;border-radius:999px;font-size:12px;background:{bg};color:{fg};"
            );
            output.Content.SetContent(Status.ToString());
        }
    }
}
