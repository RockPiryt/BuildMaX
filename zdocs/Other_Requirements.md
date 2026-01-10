## Atrybuty modelu wspierające generowanie pól w widokach (10%)
- [Display(Name="Adres działki")]
- [DataType(DataType.Currency)] / DataType.Date
- [DisplayFormat(DataFormatString = "{0:N2}")]
- [UIHint] (np. dedykowany edytor)
- [ScaffoldColumn(false)] (ukrycie)
- [HiddenInput] (w MVC)
- [EnumDataType]

Dzięki temu EditorFor i DisplayFor generują sensowne etykiety/formaty.

## Kontrolery (logowanie+role i własne LINQ)
### Logowanie i role (15%)

Role przykładowe:
- Admin – zarządza wariantami, dokumentami, widzi wszystkie analizy
- Analyst – może zmieniać status na Processing/Done, generować raporty
- Client – tworzy i przegląda swoje analizy

W kontrolerach używam:
- [Authorize]
- [Authorize(Roles = "Admin")]
- [Authorize(Roles = "Admin,Analyst")]

W Program.cs (lub Seed) dodaje:
- inicjalizację ról
- domyślnego admina

### Własne LINQ w akcjach (15%)
Nie chodzi o .ToList() na DbSet, tylko o realne zapytania: joiny, group by, projekcje, filtry, sortowanie.

Przykłady:
- Dashboard admina: liczba analiz per wariant i status w ostatnich 30 dniach.
- Lista analiz usera: filtr po statusie + sort po CreatedAt.
- Ranking opłacalności: top 10 analiz z BuiltUpPercent > 40, posortowane malejąco.
- Przykład „mięsnego” LINQ:
    from a in _db.AnalysisRequests join v in _db.Variants ... select new DashboardRow { ... }
    grupowanie po wariancie i statusie

## Widoki Razor: wymagane elementy (35%)
### Dwa widoki napisane ręcznie, nie CRUD (10%)
- Pricing / Wybór wariantu – karta z 3 wariantami, porównanie funkcji, CTA „Zamów analizę”.
- Dashboard / Wynik analizy – prezentacja procentów, alerty „obszary problemowe”, rekomendacja (np. „opłaca się” jeśli >40%).

Te widoki nie są CRUD (to nie jest Create/Edit/Details automatu), tylko ręczne UI.

### Helpery: podstawowe + 2 własne (15%)

`Podstawowe`:

- Html.BeginForm, Html.TextBoxFor, Html.ValidationSummary, Html.ValidationMessageFor
- asp-for, asp-action, asp-controller, asp-validation-for

`Dwa własne helpery o nietrywialnej funkcjonalności`:

### Helper 1 (TagHelper): ProfitabilityBadgeTagHelper
Renderuje badge na podstawie procentu zabudowy:

- 40%: „Opłacalna”
- 30–40%: „Ryzyko”
- < 30%: „Nieopłacalna”
Dodatkowo może generować ikonę i tooltip z wyjaśnieniem progów.

### Helper 2 (HTML Helper extension): LegalDocsList()
Generuje listę dokumentów prawnych jako accordion (kategoria -> elementy), automatycznie grupuje po Category, tworzy linki, dodaje liczbę dokumentów w kategorii.

To jest „nietrywialne”, bo:

- grupowanie
- generowanie złożonego HTML
- logika prezentacji

## Dwa layouty (10%)
Dwa własne layouty:
- _LayoutPublic.cshtml – strona marketingowa (landing, cennik, opis)
- _LayoutApp.cshtml – aplikacja po zalogowaniu (menu: Analizy, Nowa analiza, Profil, Admin)
Widoki publiczne używają public layoutu, a panel po loginie – app layoutu.

## Przepływ CRUD (co konkretnie CRUDujesz)
CRUD-y, które mają sens:
- AnalysisRequests (Client: Create + Read swoje; Analyst/Admin: Edit status; Admin: Delete)
- Variants (Admin pełny CRUD)
- LegalDocuments (Admin pełny CRUD)

## Logika „AI/analizy” bez wchodzenia w prawdziwe AI (żeby MVP działało)

Obecnie zamiast prawdziwego NLP do MPZP, robię:

### „Silnik reguł” w C# (klasa AnalysisCalculator), który liczy:

- max zabudowy = min(PlotArea * limitFromRules, PlotArea - greenRequirement)
- greenRequirement = PlotArea * X%
- parking = funkcja od modułu
- builtUpPercent = (builtUpArea / PlotArea)*100

A „obszary problemowe” można zasymulować:

- jeśli działka < 3000 m² -> „ryzyko ograniczeń logistycznych”
- jeśli builtUpPercent < 25% -> „niska chłonność”
- pcjonalnie checkboxy w formularzu: „teren podmokły”, „archeologia” – i system generuje ostrzeżenia.

## PDF 
MVP -  QuestPDF (bardzo popularne i proste w użyciu w ASP.NET). 
QuestPDF z NuGet.

# Plan realizacji krok po kroku (żebyś to faktycznie zbudował)

1. Utwórze projekt MVC + Identity.
2. Dodaje modele: Variant, AnalysisRequest, LegalDocument, ApplicationUser.
3. Skonfiguruje EF Core: relacje, migracje, seed wariantów i ról.
4. Dodaje walidacje:
- DataAnnotations
- własny atrybut walidacyjny cross-field
5. Zróbie kontrolery:
- AnalysisRequestsController (Client/Analyst/Admin z [Authorize])
- VariantsController (Admin)
- LegalDocumentsController (Admin)
- DashboardController (ręczny widok + LINQ)
6. Zróbie ręczne widoki:
- Home/Pricing (public)
- Analysis/Result lub Dashboard/Index (po zalogowaniu)
7. Zróbie 2 layouty.
8. Zróbie 2 helpery (TagHelper + HTML helper).
9. Dodam generowanie PDF dla wariantu 1 (nawet prosty raport).
10. Dopracuj LINQ (statystyki, filtry, projekcje).

#  Minimalna dokumentacja:
W README opisuje:
- relacje 1:N (konkretne FK)
- gdzie jest własny walidator i co waliduje
- które atrybuty modelu wpływają na widoki (Display/Format/Range)
- role i które akcje mają jaką autoryzację
- przykłady LINQ (wskazanie akcji + opis)
- 2 ręczne widoki (linki)
- 2 helpery (linki + opis)
- 2 layouty (które widoki używają)

## Opis
Rozumiem wymaganie i w tej wersji kalkulator ma:

1. przyjąć PlotAreaM2 oraz ModuleAreaM2 (powierzchnia pojedynczego modułu),
2. sam wyliczyć ile modułów realnie “wejdzie”, uwzględniając:
- 10 m strefę niebudowalną od granicy (setback),
- drogi 5 m + chodniki 2 × 1,5 m,
- trackout: 30 m × długość budynku,
- zieleń jako typowo 20–30%,
- utwardzenia (drogi/trackout) z opcją redukcji dzięki geokratom,

3. policzyć wynikowe pola Twojej encji: BuiltUpPercent, GreenAreaM2, HardenedAreaM2, parkingi, ryzyka.

----------------
1. użytkownik klika „Wybierz/Zamów” na karcie wariantu,

2. przechodzi do formularza Create, gdzie wariant jest już ustawiony, a user uzupełnia tylko dane działki i modułu,

user nie może zmienić wariantu (albo może, ale domyślnie jest wybrany).

---------
Poniżej masz kompletny zestaw plików dla Services/Analysis (MVP pod prostokątną działkę + wymiary działki A×B i modułu C×D), zgodny z Twoim podejściem:

user podaje: PlotWidthM, PlotLengthM, ModuleWidthM, ModuleLengthM

kalkulator liczy:

PlotAreaM2, ModuleAreaM2

BuiltUpPercent, GreenAreaM2, HardenedAreaM2

liczbę modułów (nie zapisujemy do DB, ale zwracamy w wyniku)

CarParkingSpots, TruckParkingSpots

ryzyka i ostrzeżenia

uwzględnia:

setback 10 m od granicy

drogi 5 m + chodniki 2×1,5 m

trackout 30 m × długość budynku

redukcję utwardzeń geokratami

Nazwy namespace: BuildMaX.Web.Services.Analysis.
---
2) Poprawiony AnalysisRequestsController.cs

Zmiany kluczowe:

Dodany IAnalysisCalculator _calc;

Create(int variantId) – wariant przychodzi z Pricing, wczytujemy go i ustawiamy w modelu

Create POST – binduje tylko pola wejściowe i VariantId, liczy wyniki kalkulatorem, zapisuje

Usunięte PopulateVariantsSelectListAsync() z Create (bo wariant nie jest wybierany z dropdowna)

Edit zostawiam prawie jak masz, ale Admin edytuje wejście (wymiary), a wyniki możesz:

liczyć automatycznie po zapisie (ja tak robię), albo

zostawić Analystowi ręczną kontrolę (Twoje założenie). Poniżej: Admin po zmianie wejścia -> auto przeliczenie.