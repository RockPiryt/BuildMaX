using System;
using System.Collections.Generic;
using BuildMaX.Web.Models.Domain;
using BuildMaX.Web.Services.Analysis;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BuildMaX.Web.Services.Documents;

public sealed class PdfReportService : IPdfReportService
{
    public byte[] GenerateAnalysisRequestReport(AnalysisRequest ar, Variant variant, AnalysisComputation? computation = null)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var title = $"Raport analizy – {variant.Name}";
        var created = ar.CreatedAt.ToLocalTime();

        // Jeśli computation nie podane, a chcesz mieć np. ModuleCount w PDF,
        // to po prostu PDF pokaże dane z encji (BuiltUpPercent itd.) bez modułów.
        var moduleCountText = computation is null ? "-" : computation.ModuleCount.ToString();
        var warnings = computation?.Warnings ?? new List<string>();

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);

                page.Header().Column(col =>
                {
                    col.Item().Text(title).FontSize(18).SemiBold();
                    col.Item().Text($"Adres: {ar.Address}").FontSize(10);
                    col.Item().Text($"Data: {created:yyyy-MM-dd HH:mm}").FontSize(10).FontColor(Colors.Grey.Darken2);
                    col.Item().LineHorizontal(1);
                });

                page.Content().Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text("Wariant").FontSize(13).SemiBold();
                    col.Item().Text($"{variant.Name} ({variant.Price:N2} zł)");

                    col.Item().Text("Dane wejściowe").FontSize(13).SemiBold();
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                        });

                        // Jeśli masz już w encji PlotWidthM/PlotLengthM/ModuleWidthM/ModuleLengthM, pokaż je:
                        // (Jeśli jeszcze ich nie masz, usuń te linie i zostań przy PlotAreaM2/ModuleAreaM2)
                        Row(t, "Działka", $"{GetDecimal(ar, "PlotWidthM"):N2} m × {GetDecimal(ar, "PlotLengthM"):N2} m");
                        Row(t, "Moduł", $"{GetDecimal(ar, "ModuleWidthM"):N2} m × {GetDecimal(ar, "ModuleLengthM"):N2} m");

                        Row(t, "Powierzchnia działki", $"{ar.PlotAreaM2:N2} m²");
                        Row(t, "Powierzchnia modułu", $"{ar.ModuleAreaM2:N2} m²");
                    });

                    col.Item().Text("Wyniki (MVP)").FontSize(13).SemiBold();
                    col.Item().Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(2);
                            c.RelativeColumn(3);
                        });

                        Row(t, "Liczba modułów (wyliczona)", moduleCountText);
                        Row(t, "Procent zabudowy", ar.BuiltUpPercent is null ? "-" : $"{ar.BuiltUpPercent:N2}%");
                        Row(t, "Powierzchnia zielona", ar.GreenAreaM2 is null ? "-" : $"{ar.GreenAreaM2:N2} m²");
                        Row(t, "Powierzchnia utwardzona", ar.HardenedAreaM2 is null ? "-" : $"{ar.HardenedAreaM2:N2} m²");
                        Row(t, "Parking osobowy", ar.CarParkingSpots is null ? "-" : $"{ar.CarParkingSpots}");
                        Row(t, "Parking TIR", ar.TruckParkingSpots is null ? "-" : $"{ar.TruckParkingSpots}");
                    });

                    col.Item().Text("Ryzyka").FontSize(13).SemiBold();
                    col.Item().Column(r =>
                    {
                        r.Spacing(4);
                        r.Item().Text($"• Archeologia: {(ar.HasArchaeologyRisk ? "TAK" : "NIE")}");
                        r.Item().Text($"• Duże roboty ziemne: {(ar.HasEarthworksRisk ? "TAK" : "NIE")}");
                    });

                    col.Item().Text("Ocena").FontSize(13).SemiBold();
                    col.Item().Text(EvaluateProfitability(ar.BuiltUpPercent));

                    col.Item().Text("Ostrzeżenia").FontSize(13).SemiBold();
                    if (warnings.Count == 0)
                    {
                        col.Item().Text("Brak ostrzeżeń.").FontColor(Colors.Green.Darken2);
                    }
                    else
                    {
                        col.Item().Column(w =>
                        {
                            w.Spacing(4);
                            foreach (var warn in warnings)
                                w.Item().Text($"• {warn}");
                        });
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken2));

                    t.Span("BuildMaX – raport | strona ");
                    t.CurrentPageNumber();
                    t.Span(" / ");
                    t.TotalPages();
                });
            });
        });

        return doc.GeneratePdf();
    }

    private static string EvaluateProfitability(decimal? builtUpPercent)
    {
        if (builtUpPercent is null) return "Brak wyliczonego procentu zabudowy.";

        var p = builtUpPercent.Value;
        if (p >= 40m) return "Ocena: Opłacalna (>= 40% zabudowy).";
        if (p >= 30m) return "Ocena: Ryzyko (30–40% zabudowy).";
        return "Ocena: Nieopłacalna (< 30% zabudowy).";
    }

    private static void Row(TableDescriptor table, string label, string value)
    {
        table.Cell().Element(CellStyle).Text(label).SemiBold();
        table.Cell().Element(CellStyle).Text(value);

        static IContainer CellStyle(IContainer c) =>
            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(4).PaddingRight(10);
    }

    /// <summary>
    /// Bezpieczny odczyt decimal po nazwie property (żeby PDF nie wywalał się jeśli jeszcze nie dodałaś tych pól do encji).
    /// Gdy pola istnieją, pokaże ich wartości; gdy nie istnieją, zwróci 0.
    /// </summary>
    private static decimal GetDecimal(object obj, string propName)
    {
        var p = obj.GetType().GetProperty(propName);
        if (p is null) return 0m;

        var val = p.GetValue(obj);
        if (val is null) return 0m;

        // Obsłuż decimal
        if (val is decimal d) return d;

        // Obsłuż decimal? bez używania wzorca "is decimal? x"
        var t = Nullable.GetUnderlyingType(val.GetType());
        if (t == typeof(decimal))
            return (decimal)val;

        return 0m;
    }

}
