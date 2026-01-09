using BuildMaX.Web.Models.Domain;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BuildMaX.Web.Helpers.HtmlHelpers
{
    public static class LegalDocsHtmlHelpers
    {
        public static IHtmlContent RenderLegalDocs(this IHtmlHelper html, IEnumerable<LegalDocument> docs)
        {
            var ul = new TagBuilder("ul");

            foreach (var d in docs)
            {
                var li = new TagBuilder("li");
                var category = string.IsNullOrWhiteSpace(d.Category) ? "Inne" : d.Category;
                li.InnerHtml.AppendHtml($"{d.Title} ({category}) - ");
                
                var a = new TagBuilder("a");
                a.Attributes["href"] = d.Url;
                a.Attributes["target"] = "_blank";
                a.InnerHtml.Append("link");
                li.InnerHtml.AppendHtml(a);

                ul.InnerHtml.AppendHtml(li);
            }

            return ul;
        }
    }
}
