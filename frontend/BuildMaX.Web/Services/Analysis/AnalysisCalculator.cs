using BuildMaX.Web.Models.Domain;

namespace BuildMaX.Web.Services.Analysis;

public sealed class AnalysisCalculator : IAnalysisCalculator
{
    public AnalysisComputation ComputeAndApply(AnalysisRequest ar, AnalysisAssumptions? assumptions = null)
    {
        assumptions ??= new AnalysisAssumptions();

        // WYMAGANE: w modelu AnalysisRequest musisz mieć:
        // PlotWidthM, PlotLengthM, ModuleWidthM, ModuleLengthM
        ValidateInput(ar, assumptions);

        // Pola wyliczane (systemowe)
        ar.PlotAreaM2 = Round2(ar.PlotWidthM * ar.PlotLengthM);
        ar.ModuleAreaM2 = Round2(ar.ModuleWidthM * ar.ModuleLengthM);

        var warnings = new List<string>();

        // Obszar budowalny po setback (10m od granic)
        var buildableWidth = ar.PlotWidthM - 2m * assumptions.SetbackFromBorderM;
        var buildableLength = ar.PlotLengthM - 2m * assumptions.SetbackFromBorderM;

        if (buildableWidth <= 0 || buildableLength <= 0)
        {
            warnings.Add("Po uwzględnieniu strefy 10 m od granicy nie zostaje obszar budowalny.");
            ApplyZeros(ar);

            return new AnalysisComputation
            {
                PlotWidthM = Round2(ar.PlotWidthM),
                PlotLengthM = Round2(ar.PlotLengthM),
                PlotAreaM2 = Round2(ar.PlotAreaM2),
                BuildableWidthM = 0,
                BuildableLengthM = 0,
                BuildableAreaM2 = 0,
                GreenAreaM2 = Round2(ar.PlotAreaM2 * assumptions.GreenPercent),
                HardenedAreaM2 = 0,
                MaxUsableForBuildingM2 = 0,
                ModuleWidthM = Round2(ar.ModuleWidthM),
                ModuleLengthM = Round2(ar.ModuleLengthM),
                ModuleAreaM2 = Round2(ar.ModuleAreaM2),
                ModuleCount = 0,
                BuiltUpAreaM2 = 0,
                BuiltUpPercent = 0,
                CarParkingSpots = 0,
                TruckParkingSpots = 0,
                Warnings = warnings
            };
        }

        var buildableArea = buildableWidth * buildableLength;

        // Zieleń (20–30%)
        var greenArea = ar.PlotAreaM2 * assumptions.GreenPercent;

        // Utwardzenia:
        // 1) korytarze drogowe: (droga + 2 chodniki) * długość przebiegu
        var corridorWidth = assumptions.RoadWidthM + 2m * assumptions.SidewalkWidthM; // 5 + 3 = 8 m

        // W MVP przyjmujemy, że korytarze biegną wzdłuż dłuższego boku obszaru budowalnego
        var corridorRun = Math.Max(buildableWidth, buildableLength);
        var corridorsArea = assumptions.RoadCorridorsCount * corridorWidth * corridorRun;

        // 2) trackout: 30m x długość budynku.
        // Długość budynku w MVP ~ dłuższy bok obszaru budowalnego (bo hala zwykle "ciągnie się" wzdłuż dłuższej osi)
        var buildingLengthApprox = corridorRun;
        var trackoutArea = assumptions.TrackoutLengthM * buildingLengthApprox;

        var hardenedRaw = corridorsArea + trackoutArea;

        // Redukcja utwardzeń dzięki geokratom
        var hardenedReduced = hardenedRaw * (1m - assumptions.GeoGridsHardenedReduction);

        // Maksymalna zabudowa ograniczona:
        // - geometrią (obszar budowalny)
        // - bilansem: działka - zieleń - utwardzenia
        var usableByBalance = ar.PlotAreaM2 - greenArea - hardenedReduced;

        var maxUsableForBuilding = MinNonNegative(buildableArea, usableByBalance);

        if (usableByBalance < 0)
            warnings.Add("Bilans ujemny: zieleń + utwardzenia przekraczają powierzchnię działki (dla przyjętych założeń).");

        // Ile modułów wejdzie (liczymy z powierzchni, nie z count podanym przez usera)
        var moduleCount = (int)Math.Floor((double)(maxUsableForBuilding / ar.ModuleAreaM2));
        if (moduleCount < 0) moduleCount = 0;

        var builtUpArea = moduleCount * ar.ModuleAreaM2;
        var builtUpPercent = ar.PlotAreaM2 > 0 ? (builtUpArea / ar.PlotAreaM2) * 100m : 0m;

        // Parking (MVP)
        var carSpots = moduleCount * assumptions.CarSpotsPerModule;
        var truckSpots = moduleCount >= assumptions.ModulesPerTruckSpot
            ? (int)Math.Ceiling((double)moduleCount / assumptions.ModulesPerTruckSpot)
            : 0;

        // Ryzyka (MVP)
        var archaeologyRisk = ar.PlotAreaM2 < 3000m;
        var earthworksRisk = hardenedReduced > (ar.PlotAreaM2 * 0.20m);

        // Ostrzeżenia biznesowe wg Twoich progów
        if (moduleCount == 0)
            warnings.Add("Wynik: 0 modułów. Zwiększ wymiary działki albo zmniejsz wymiary modułu / wymagania utwardzeń.");

        if (builtUpPercent < 40m)
            warnings.Add("Zabudowa < 40%: wg założeń opłacalność jest słaba.");

        if (assumptions.GreenPercent is < 0.20m or > 0.30m)
            warnings.Add("Zieleń poza typowym zakresem 20–30% (sprawdź założenia).");

        // Aplikacja do encji
        ar.GreenAreaM2 = Round2(greenArea);
        ar.HardenedAreaM2 = Round2(hardenedReduced);
        ar.BuiltUpPercent = Round2(builtUpPercent);
        ar.CarParkingSpots = carSpots;
        ar.TruckParkingSpots = truckSpots;
        ar.HasArchaeologyRisk = archaeologyRisk;
        ar.HasEarthworksRisk = earthworksRisk;

        return new AnalysisComputation
        {
            PlotWidthM = Round2(ar.PlotWidthM),
            PlotLengthM = Round2(ar.PlotLengthM),
            PlotAreaM2 = Round2(ar.PlotAreaM2),

            BuildableWidthM = Round2(buildableWidth),
            BuildableLengthM = Round2(buildableLength),
            BuildableAreaM2 = Round2(buildableArea),

            GreenAreaM2 = Round2(greenArea),
            HardenedAreaM2 = Round2(hardenedReduced),
            MaxUsableForBuildingM2 = Round2(maxUsableForBuilding),

            ModuleWidthM = Round2(ar.ModuleWidthM),
            ModuleLengthM = Round2(ar.ModuleLengthM),
            ModuleAreaM2 = Round2(ar.ModuleAreaM2),

            ModuleCount = moduleCount,
            BuiltUpAreaM2 = Round2(builtUpArea),
            BuiltUpPercent = Round2(builtUpPercent),

            CarParkingSpots = carSpots,
            TruckParkingSpots = truckSpots,
            Warnings = warnings
        };
    }

    private static void ValidateInput(AnalysisRequest ar, AnalysisAssumptions a)
    {
        // Działka
        if (ar.PlotWidthM <= 0) throw new ArgumentOutOfRangeException(nameof(ar.PlotWidthM), "PlotWidthM musi być > 0.");
        if (ar.PlotLengthM <= 0) throw new ArgumentOutOfRangeException(nameof(ar.PlotLengthM), "PlotLengthM musi być > 0.");

        // Moduł
        if (ar.ModuleWidthM <= 0) throw new ArgumentOutOfRangeException(nameof(ar.ModuleWidthM), "ModuleWidthM musi być > 0.");
        if (ar.ModuleLengthM <= 0) throw new ArgumentOutOfRangeException(nameof(ar.ModuleLengthM), "ModuleLengthM musi być > 0.");

        // Założenia
        if (a.GreenPercent < 0.20m || a.GreenPercent > 0.30m)
            throw new ArgumentOutOfRangeException(nameof(a.GreenPercent), "GreenPercent w MVP powinno być w zakresie 0.20–0.30.");

        if (a.GeoGridsHardenedReduction < 0m || a.GeoGridsHardenedReduction > 1m)
            throw new ArgumentOutOfRangeException(nameof(a.GeoGridsHardenedReduction), "GeoGridsHardenedReduction musi być w zakresie 0..1.");

        if (a.RoadCorridorsCount < 0 || a.RoadCorridorsCount > 10)
            throw new ArgumentOutOfRangeException(nameof(a.RoadCorridorsCount), "RoadCorridorsCount ma niepoprawną wartość.");

        // Setback nie może "zjeść" działki
        if (2m * a.SetbackFromBorderM >= ar.PlotWidthM || 2m * a.SetbackFromBorderM >= ar.PlotLengthM)
            throw new ArgumentOutOfRangeException(nameof(a.SetbackFromBorderM), "Setback jest zbyt duży względem wymiarów działki.");
    }

    private static void ApplyZeros(AnalysisRequest ar)
    {
        ar.BuiltUpPercent = 0m;
        ar.GreenAreaM2 = null;
        ar.HardenedAreaM2 = 0m;
        ar.CarParkingSpots = 0;
        ar.TruckParkingSpots = 0;
        ar.HasArchaeologyRisk = true;
        ar.HasEarthworksRisk = false;
    }

    private static decimal MinNonNegative(decimal a, decimal b)
    {
        var m = Math.Min(a, b);
        return m < 0 ? 0 : m;
    }

    private static decimal Round2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
}
