namespace ZAD3_BUILDMAX.Services.Analysis;

public sealed class AnalysisResult
{
    // Podstawowe liczby
    public decimal PlotAreaM2 { get; init; }
    public decimal GreenRequiredM2 { get; init; }
    public decimal ModuleFootprintM2 { get; init; }           // 10x12=120
    public decimal BuiltUpAreaM2 { get; init; }               // footprint * count
    public decimal BuiltUpPercent { get; init; }              // (BuiltUp / PlotArea)*100

    // Parkingi (bardzo prosta funkcja)
    public int ParkingSpacesRequired { get; init; }

    // Ocena opłacalności wg progów z Twojego opisu
    public string ProfitabilityLabel { get; init; } = "";
    public string ProfitabilityComment { get; init; } = "";

    // Ostrzeżenia
    public IReadOnlyList<string> Warnings { get; init; } = Array.Empty<string>();
}
