namespace BuildMaX.Web.Services.Analysis;

// user podaje: PlotWidthM, PlotLengthM, ModuleWidthM, ModuleLengthM
// kalkulator liczy:
// - PlotAreaM2, ModuleAreaM2BuiltUpPercent, GreenAreaM2, HardenedAreaM2
// - liczbę modułów (nie zapisujemy do DB, ale zwracamy w wyniku)
// - CarParkingSpots, TruckParkingSpots
// - ryzyka i ostrzeżenia

// uwzględnia:
// - setback 10 m od granicy
// - drogi 5 m + chodniki 2×1,5 m
// - trackout 30 m × długość budynku
// - redukcję utwardzeń geokratami

public sealed class AnalysisAssumptions
{
    // Typowa zieleń 20–30%
    public decimal GreenPercent { get; init; } = 0.25m;

    // Strefa bez zabudowy od granic działki
    public decimal SetbackFromBorderM { get; init; } = 10m;

    // Drogi i chodniki
    public decimal RoadWidthM { get; init; } = 5m;
    public decimal SidewalkWidthM { get; init; } = 1.5m;

    // Liczba pasów komunikacyjnych (MVP: 1)
    public int RoadCorridorsCount { get; init; } = 1;

    // Trackout: 30m x długość budynku
    public decimal TrackoutLengthM { get; init; } = 30m;

    // Redukcja utwardzeń dzięki geokratom (0..1), np. 0.15 = -15%
    public decimal GeoGridsHardenedReduction { get; init; } = 0.00m;

    // Parking (MVP)
    public int CarSpotsPerModule { get; init; } = 1;     // 1 osobowe / moduł
    public int ModulesPerTruckSpot { get; init; } = 6;   // 1 TIR / 6 modułów
}
