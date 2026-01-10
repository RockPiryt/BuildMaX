namespace BuildMaX.Web.Services.Analysis;

//To jest wynik techniczny kalkulatora (do widoku/PDF). Nie wymaga migracji.
public sealed class AnalysisComputation
{
    // Dane wejściowe / geometryczne (po setback)
    public decimal PlotWidthM { get; init; }
    public decimal PlotLengthM { get; init; }
    public decimal PlotAreaM2 { get; init; }

    public decimal BuildableWidthM { get; init; }
    public decimal BuildableLengthM { get; init; }
    public decimal BuildableAreaM2 { get; init; }

    // Bilans powierzchni
    public decimal GreenAreaM2 { get; init; }
    public decimal HardenedAreaM2 { get; init; }
    public decimal MaxUsableForBuildingM2 { get; init; } // min(buildableArea, areaByBalance)

    // Moduły i zabudowa
    public decimal ModuleWidthM { get; init; }
    public decimal ModuleLengthM { get; init; }
    public decimal ModuleAreaM2 { get; init; }

    public int ModuleCount { get; init; }
    public decimal BuiltUpAreaM2 { get; init; }
    public decimal BuiltUpPercent { get; init; }

    // Parking
    public int CarParkingSpots { get; init; }
    public int TruckParkingSpots { get; init; }

    // Ostrzeżenia/uwagi
    public List<string> Warnings { get; init; } = new();
}
