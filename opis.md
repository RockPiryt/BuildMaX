Poniżej masz konkretny, „akademicki” plan i szkic implementacji aplikacji BuildMaX w ASP.NET Core MVC tak, żeby spełnić wszystkie kryteria z Twojej specyfikacji (modele/kontrolery/widoki) oraz żeby projekt był sensowny domenowo. To nie jest tylko opis – dostajesz strukturę, modele, walidatory, pomysły na widoki „nie-CRUD”, helpery i przykładowe LINQ.

1) Zakres MVP (żeby to dowieźć jako CRUD + logika)
Cel aplikacji (MVP):


Użytkownik rejestruje konto, loguje się.


Wybiera wariant analizy (1/2/3).


Dodaje „Zlecenie analizy” dla działki (adres/parametry, moduł magazynowy, itd.).


System liczy „wynik analizy” (procent zabudowy, zielone, utwardzone, miejsca postojowe).


Dla wariantu 1 generuje PDF z podsumowaniem + listą dokumentów (na MVP może być „placeholder” z linkami i wygenerowany raport tekstowy w PDF).


Panel administracyjny: zarządzanie wariantami i przegląd zleceń.


To pozwala zrobić:


relacje 1:N (User → Analyses, Variant → Analyses)


walidacje, własny walidator, atrybuty modelu


role i autoryzację


własne LINQ w kontrolerach


ręczne widoki i helpery



2) Technologia i szkielet projektu
Rekomendacja: ASP.NET Core MVC + EF Core + ASP.NET Core Identity (logowanie i role).
Utwórz projekt:


ASP.NET Core Web App (Model-View-Controller)


Authentication: Individual Accounts (Identity)


Pakiety:


Microsoft.EntityFrameworkCore.SqlServer (lub SQLite do prostoty)


Microsoft.EntityFrameworkCore.Tools


PDF: na uczelnię wystarczy np. QuestPDF albo DinkToPdf (QuestPDF jest bardzo wygodny do raportów).



3) Modele i relacje (minimum 2 tabele 1:N)
Proponowane encje (domena BuildMaX)
1) Variant (wariant 1/2/3)


VariantId


Name


Price


Description


IncludesPdf


IncludesPercentDetails


IncludesSitePlan


2) AnalysisRequest (zlecenie analizy działki)


AnalysisRequestId


ApplicationUserId (FK do użytkownika)


VariantId (FK do Variant)


Address


PlotAreaM2


ModuleAreaM2 (np. moduł magazynowy)


CreatedAt


Status (np. New/Processing/Done/Rejected)


Result (nawigacja 1:1 lub po prostu pola wynikowe w tej tabeli)


3) LegalDocument (lista dokumentów / podstaw prawnych)


LegalDocumentId


VariantId (FK) albo AnalysisRequestId (FK) – zależy od podejścia


Title


Url


Category


Relacje 1:N (spełnią warunek):


Variant (1) -> AnalysisRequest (N)


ApplicationUser (1) -> AnalysisRequest (N)


(opcjonalnie) Variant (1) -> LegalDocument (N) lub AnalysisRequest (1) -> LegalDocument (N)


Więzy integralności danych (FK + OnDelete)
W EF Core ustawiasz:


AnalysisRequest wymaga istniejącego Variant i User.


sensownie: usunięcie Variant blokowane, jeśli są zlecenia (Restrict), a usunięcie usera może usuwać zlecenia (Cascade) lub też blokować – zależnie od wymagań.



4) Walidacja danych (atrybuty + własny walidator)
Atrybuty ograniczeń danych (5%)
Na modelach używasz:


[Required], [StringLength], [Range], [Display], [DataType], [RegularExpression]


Przykładowo dla AnalysisRequest:


Address – Required, StringLength


PlotAreaM2 – Range(100, 5_000_000)


ModuleAreaM2 – Range(200, 200_000)


Status – enum


Własny walidator (10%)
Wymóg mówi o własnym walidatorze. Najprościej i czytelnie:
Wariant A: własny atrybut walidacyjny
Np. [ModuleFitsPlot] – sprawdza, czy ModuleAreaM2 nie przekracza np. 80% PlotAreaM2 (albo innej reguły domenowej).
Wariant B: IValidatableObject
AnalysisRequest : IValidatableObject i w Validate() robisz reguły cross-field.
Ja bym zrobił A (atrybut), bo jest bardziej „na ocenę” i łatwo wykazać, że jest własny.
Przykładowa logika:


moduł nie może być większy niż 60% działki


jeśli PlotAreaM2 < 1000, to wariant 3 niedostępny (np. za mała działka pod pełny plan)



5) Atrybuty modelu wspierające generowanie pól w widokach (10%)
Tu wchodzą:


[Display(Name="Adres działki")]


[DataType(DataType.Currency)] / DataType.Date


[DisplayFormat(DataFormatString = "{0:N2}")]


[UIHint] (np. dedykowany edytor)


[ScaffoldColumn(false)] (ukrycie)


[HiddenInput] (w MVC)


[EnumDataType]


Dzięki temu EditorFor i DisplayFor generują sensowne etykiety/formaty.

6) Kontrolery (logowanie+role i własne LINQ)
Logowanie i role (15%)
Role przykładowe:


Admin – zarządza wariantami, dokumentami, widzi wszystkie analizy


Analyst – może zmieniać status na Processing/Done, generować raporty


Client – tworzy i przegląda swoje analizy


W kontrolerach używasz:


[Authorize]


[Authorize(Roles = "Admin")]


[Authorize(Roles = "Admin,Analyst")]


W Program.cs (lub Seed) dodajesz:


inicjalizację ról


domyślnego admina


Własne LINQ w akcjach (15%)
Nie chodzi o .ToList() na DbSet, tylko o realne zapytania: joiny, group by, projekcje, filtry, sortowanie.
Przykłady:


Dashboard admina: liczba analiz per wariant i status w ostatnich 30 dniach.


Lista analiz usera: filtr po statusie + sort po CreatedAt.


Ranking opłacalności: top 10 analiz z BuiltUpPercent > 40, posortowane malejąco.


Przykład „mięsnego” LINQ:


from a in _db.AnalysisRequests join v in _db.Variants ... select new DashboardRow { ... }


grupowanie po wariancie i statusie



7) Widoki Razor: wymagane elementy (35%)
Dwa widoki napisane ręcznie, nie CRUD (10%)
Propozycje, domenowo sensowne i „widać, że ręczne”:


Pricing / Wybór wariantu – karta z 3 wariantami, porównanie funkcji, CTA „Zamów analizę”.


Dashboard / Wynik analizy – prezentacja procentów, alerty „obszary problemowe”, rekomendacja (np. „opłaca się” jeśli >40%).


Te widoki nie są CRUD (to nie jest Create/Edit/Details automatu), tylko ręczne UI.
Helpery: podstawowe + 2 własne (15%)
Podstawowe:


Html.BeginForm, Html.TextBoxFor, Html.ValidationSummary, Html.ValidationMessageFor


asp-for, asp-action, asp-controller, asp-validation-for


Dwa własne helpery o nietrywialnej funkcjonalności:
Helper 1 (TagHelper): ProfitabilityBadgeTagHelper
Renderuje badge na podstawie procentu zabudowy:



40%: „Opłacalna”



30–40%: „Ryzyko”


< 30%: „Nieopłacalna”
Dodatkowo może generować ikonę i tooltip z wyjaśnieniem progów.


Helper 2 (HTML Helper extension): LegalDocsList()
Generuje listę dokumentów prawnych jako accordion (kategoria -> elementy), automatycznie grupuje po Category, tworzy linki, dodaje liczbę dokumentów w kategorii.
To jest „nietrywialne”, bo:


grupowanie


generowanie złożonego HTML


logika prezentacji


Dwa layouty (10%)
Wymóg: dwa własne layouty.
Propozycja:


_LayoutPublic.cshtml – strona marketingowa (landing, cennik, opis)


_LayoutApp.cshtml – aplikacja po zalogowaniu (menu: Analizy, Nowa analiza, Profil, Admin)


Widoki publiczne używają public layoutu, a panel po loginie – app layoutu.

8) Przepływ CRUD (co konkretnie CRUDujesz)
CRUD-y, które mają sens:


AnalysisRequests (Client: Create + Read swoje; Analyst/Admin: Edit status; Admin: Delete)


Variants (Admin pełny CRUD)


LegalDocuments (Admin pełny CRUD)


Dzięki temu spełniasz CRUD i jednocześnie budujesz domenę.

9) Logika „AI/analizy” bez wchodzenia w prawdziwe AI (żeby MVP działało)
Na potrzeby projektu zaliczeniowego nie musisz robić prawdziwego NLP do MPZP. Możesz zrobić:


„Silnik reguł” w C# (klasa AnalysisCalculator), który liczy:


max zabudowy = min(PlotArea * limitFromRules, PlotArea - greenRequirement)


greenRequirement = PlotArea * X%


parking = funkcja od modułu


builtUpPercent = (builtUpArea / PlotArea)*100




A „obszary problemowe” możesz zasymulować:


jeśli działka < 3000 m² -> „ryzyko ograniczeń logistycznych”


jeśli builtUpPercent < 25% -> „niska chłonność”


opcjonalnie checkboxy w formularzu: „teren podmokły”, „archeologia” – i system generuje ostrzeżenia.


To nadal wygląda jak analiza, a jest implementowalne.

10) Plan realizacji krok po kroku (żebyś to faktycznie zbudował)


Utwórz projekt MVC + Identity.


Dodaj modele: Variant, AnalysisRequest, LegalDocument, ApplicationUser.


Skonfiguruj EF Core: relacje, migracje, seed wariantów i ról.


Dodaj walidacje:


DataAnnotations


własny atrybut walidacyjny cross-field




Zrób kontrolery:


AnalysisRequestsController (Client/Analyst/Admin z [Authorize])


VariantsController (Admin)


LegalDocumentsController (Admin)


DashboardController (ręczny widok + LINQ)




Zrób ręczne widoki:


Home/Pricing (public)


Analysis/Result lub Dashboard/Index (po zalogowaniu)




Zrób 2 layouty.


Zrób 2 helpery (TagHelper + HTML helper).


Dodaj generowanie PDF dla wariantu 1 (nawet prosty raport).


Dopracuj LINQ (statystyki, filtry, projekcje).



11) Minimalna dokumentacja do oddania (żeby nauczyciel widział punkty)
W README opisz:


relacje 1:N (konkretne FK)


gdzie jest własny walidator i co waliduje


które atrybuty modelu wpływają na widoki (Display/Format/Range)


role i które akcje mają jaką autoryzację


przykłady LINQ (wskazanie akcji + opis)


2 ręczne widoki (linki)


2 helpery (linki + opis)


2 layouty (które widoki używają)



Jeżeli chcesz, mogę od razu przygotować Ci komplet: propozycję struktury folderów, konkretne klasy modeli z atrybutami, przykładowy własny ValidationAttribute, szkic Program.cs z seedem ról oraz definicje obu helperów i dwóch layoutów — tak, żebyś mógł to wkleić i rozwijać dalej.