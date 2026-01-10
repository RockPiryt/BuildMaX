// (logika obliczeń „chłonności”)

namespace ZAD3_BUILDMAX.Services.Analysis;

public sealed class AnalysisCalculator : IAnalysisCalculator
{
    public AnalysisResult Calculate(AnalysisInput input)
    {
        Validate(input);

        var warnings = new List<string>();

        // 1) Zieleń wymagana
        var greenRequired = Round2(input.PlotAreaM2 * input.RequiredGreenPercent);

        // 2) Powierzchnia zabudowy z modułów
        var moduleFootprint = Round2(input.ModuleWidthM * input.ModuleLengthM); // 10*12=120
        var builtUpArea = Round2(moduleFootprint * input.ModuleCount);

        // 3) Maks. możliwa zabudowa w MVP: działka minus zieleń (upraszczamy)
        var maxBuildable = Round2(input.PlotAreaM2 - greenRequired);

        if (builtUpArea > maxBuildable)
        {
            warnings.Add($"Zabudowa ({builtUpArea:N2} m²) przekracza maksymalną przyjętą powierzchnię ({maxBuildable:N2} m²) po odjęciu zieleni.");
        }

        // 4) Procent zabudowy
        var builtUpPercent = Round2((builtUpArea / input.PlotAreaM2) * 100m);

        // 5) Parkingi (MVP): 1 miejsce / 120 m² zabudowy (zaokr. w górę)
        var parking = (int)Math.Ceiling((double)(builtUpArea / 120m));
        if (parking < 1 && input.ModuleCount > 0) parking = 1;

        // 6) Ostrzeżenia heurystyczne
        if (input.PlotAreaM2 < 3000m)
            warnings.Add("Działka < 3000 m²: ryzyko ograniczeń logistycznych i manewrowych.");

        if (builtUpPercent < 25m)
            warnings.Add("Niska chłonność (zabudowa < 25%): możliwa słaba opłacalność inwestycji.");

        if (input.WetLand)
            warnings.Add("Zaznaczono teren podmokły: możliwe koszty odwodnienia/wzmocnienia gruntu.");

        if (input.Archaeology)
            warnings.Add("Zaznaczono archeologię: możliwe opóźnienia i koszty nadzoru archeologicznego.");

        // 7) Etykieta opłacalności wg progów
        var (label, comment) = ProfitabilityFromPercent(builtUpPercent);

        return new AnalysisResult
        {
            PlotAreaM2 = Round2(input.PlotAreaM2),
            GreenRequiredM2 = greenRequired,
            ModuleFootprintM2 = moduleFootprint,
            BuiltUpAreaM2 = builtUpArea,
            BuiltUpPercent = builtUpPercent,
            ParkingSpacesRequired = parking,
            ProfitabilityLabel = label,
            ProfitabilityComment = comment,
            Warnings = warnings
        };
    }

    private static void Validate(AnalysisInput input)
    {
        if (input.PlotAreaM2 <= 0) throw new ArgumentOutOfRangeException(nameof(input.PlotAreaM2), "PlotAreaM2 musi być > 0.");
        if (input.RequiredGreenPercent < 0m || input.RequiredGreenPercent > 1m)
            throw new ArgumentOutOfRangeException(nameof(input.RequiredGreenPercent), "RequiredGreenPercent musi być w zakresie 0..1.");
        if (input.ModuleCount < 0) throw new ArgumentOutOfRangeException(nameof(input.ModuleCount), "ModuleCount nie może być ujemny.");
        if (input.ModuleWidthM <= 0 || input.ModuleLengthM <= 0)
            throw new ArgumentOutOfRangeException("Wymiary modułu muszą być > 0.");
    }

    private static (string label, string comment) ProfitabilityFromPercent(decimal builtUpPercent)
    {
        if (builtUpPercent >= 40m)
            return ("Opłacalna", "Zabudowa >= 40%: parametry wskazują na dobrą chłonność działki.");
        if (builtUpPercent >= 30m)
            return ("Ryzyko", "Zabudowa 30–40%: możliwa opłacalność, ale warto zweryfikować koszty i ograniczenia.");
        return ("Nieopłacalna", "Zabudowa < 30%: parametry sugerują niską chłonność i ryzyko słabej rentowności.");
    }

    private static decimal Round2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);
}
