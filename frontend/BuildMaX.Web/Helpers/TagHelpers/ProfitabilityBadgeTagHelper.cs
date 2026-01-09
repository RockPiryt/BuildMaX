using Microsoft.AspNetCore.Razor.TagHelpers;

namespace BuildMaX.Web.Helpers.TagHelpers
{
    [HtmlTargetElement("profit-badge")]
    public class ProfitabilityBadgeTagHelper : TagHelper
    {
        public decimal? Percent { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            string text;
            string bg;

            if (Percent is null)
            {
                text = "Brak danych";
                bg = "#6c757d";
            }
            else if (Percent >= 40)
            {
                text = "Opłacalna";
                bg = "#198754";
            }
            else if (Percent >= 30)
            {
                text = "Ryzyko";
                bg = "#ffc107";
            }
            else
            {
                text = "Nieopłacalna";
                bg = "#dc3545";
            }

            output.TagName = "span";
            output.Attributes.SetAttribute(
                "style",
                $"display:inline-block;padding:4px 10px;border-radius:999px;font-size:12px;background:{bg};color:white;"
            );
            output.Content.SetContent(text);
        }
    }
}
