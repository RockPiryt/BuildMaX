namespace ZAD3_BUILDMAX.Services.Analysis;

public sealed class AnalysisInput
{
    // Działka
    public decimal PlotAreaM2 { get; init; }                  // np. 3000
    public decimal RequiredGreenPercent { get; init; }        // np. 0.25m (25%)

    // Zabudowa modułowa
    public int ModuleCount { get; init; }                     // np. 8
    public decimal ModuleWidthM { get; init; } = 10m;         // 10m
    public decimal ModuleLengthM { get; init; } = 12m;        // 12m

    // Proste parametry/flagi ryzyka (MVP)
    public bool WetLand { get; init; }                        // teren podmokły
    public bool Archaeology { get; init; }                    // archeologia
}
