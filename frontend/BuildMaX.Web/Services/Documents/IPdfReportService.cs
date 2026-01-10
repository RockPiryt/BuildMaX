using ZAD3_BUILDMAX.Services.Analysis;

namespace ZAD3_BUILDMAX.Services.Documents;

public interface IPdfReportService
{
    byte[] GenerateAnalysisReportPdf(AnalysisResult result, string reportTitle);
}
