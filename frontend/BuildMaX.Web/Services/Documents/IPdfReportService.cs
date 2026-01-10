using BuildMaX.Web.Models.Domain;
using BuildMaX.Web.Services.Analysis;

namespace BuildMaX.Web.Services.Documents;

public interface IPdfReportService
{
    // MVP: PDF na bazie encji + wariantu; computation jest opcjonalne
    byte[] GenerateAnalysisRequestReport(AnalysisRequest ar, Variant variant, AnalysisComputation? computation = null);
}
