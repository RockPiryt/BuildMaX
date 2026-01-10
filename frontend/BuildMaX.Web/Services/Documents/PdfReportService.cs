//  (wariant 1: raport PDF)
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ZAD3_BUILDMAX.Services.Analysis;

namespace ZAD3_BUILDMAX.Services.Documents;

public sealed class PdfReportService : IPdfReportService
{
    public byte[] GenerateAnalysisReportPdf(AnalysisResult result, string reportTitle)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header().Column(col =>
                {
                    col.Item().Text(reportTitle).FontSize(18).SemiBold();
                    col.Item().Text($"Wygenerowano: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(Colors.Grey.Darken2);
                    col.Item().LineHorizontal(1);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text("Podsumowanie").FontSize(14).SemiBold();

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(3);
                        });

                        Row(table, "Powierzchnia działki", $"{result.PlotAreaM2:N2} m²");
                        Row(table, "Wymagana zieleń", $"{result.GreenRequiredM2:N2} m²");
                        Row(table, "Pow. modułu (10x12)", $"{result.ModuleFootprintM2:N2} m²");
                        Row(table, "Pow. zabudowy", $"{result.BuiltUpAreaM2:N2} m²");
                        Row(table, "Zabudowa %", $"{result.BuiltUpPercent:N2}%");
                        Row(table, "Wymagane parkingi", $"{result.ParkingSpacesRequired}");
                    });

                    col.Item().Text("Ocena").FontSize(14).SemiBold();
                    col.Item().Text($"{result.ProfitabilityLabel} – {result.ProfitabilityComment}");

                    col.Item().Text("Ostrzeżenia").FontSize(14).SemiBold();

                    if (result.Warnings.Count == 0)
                    {
                        col.Item().Text("Brak ostrzeżeń.").FontColor(Colors.Green.Darken2);
                    }
                    else
                    {
                        col.Item().Column(wcol =>
                        {
                            wcol.Spacing(5);
                            foreach (var w in result.Warnings)
                                wcol.Item().Text($"• {w}");
                        });
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("BuildMax – raport analizy | strona ");
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                }).FontSize(9).FontColor(Colors.Grey.Darken2);
            });
        });

        return doc.GeneratePdf();
    }

    private static void Row(TableDescriptor table, string label, string value)
    {
        table.Cell().Element(CellStyle).Text(label).SemiBold();
        table.Cell().Element(CellStyle).Text(value);

        static IContainer CellStyle(IContainer c) =>
            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4).PaddingRight(10);
    }
}
