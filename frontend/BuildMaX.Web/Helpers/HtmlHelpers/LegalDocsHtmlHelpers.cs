using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using BuildMaX.Web.Models.Domain;

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
                li.InnerHtml.Append($"{d.Title} ({d.DocumentType})");
                ul.InnerHtml.AppendHtml(li);
            }

            return ul;
        }
    }
}
