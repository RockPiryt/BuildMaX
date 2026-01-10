using BuildMaX.Web.Models.Domain;

namespace BuildMaX.Web.Services.Analysis;

public interface IAnalysisCalculator
{
    /// <summary>
    /// Liczy wynik analizy i aplikuje pola wyliczane do encji AnalysisRequest.
    /// Zwraca obiekt z detalami (np. liczba modułów, bilans powierzchni) do UI/PDF.
    /// </summary>
    AnalysisComputation ComputeAndApply(AnalysisRequest ar, AnalysisAssumptions? assumptions = null);
}
