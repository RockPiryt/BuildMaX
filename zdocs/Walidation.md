## Walidacja danych (atrybuty + własny walidator)

### Atrybuty ograniczeń danych (5%)

Na modelach używam:
[Required], [StringLength], [Range], [Display], [DataType], [RegularExpression]
Przykładowo dla AnalysisRequest:
- Address – Required, StringLength
- PlotAreaM2 – Range(100, 5_000_000)
- ModuleAreaM2 – Range(200, 200_000)
- Status – enum

### Własny walidator (10%)

- Wariant A: własny atrybut walidacyjny
Np. [ModuleFitsPlot] – sprawdza, czy ModuleAreaM2 nie przekracza np. 80% PlotAreaM2 (albo innej reguły domenowej).

Przykładowa logika:
- moduł nie może być większy niż 60% działki
- jeśli PlotAreaM2 < 1000, to wariant 3 niedostępny (np. za mała działka pod pełny plan)

- Wariant B: IValidatableObject
AnalysisRequest : IValidatableObject i w Validate() robisz reguły cross-field.

