email: admin@buildmax.local
hasło: Admin123!
 
BuildMaX/
├─ BuildMaX.sln
└─ src/
   └─ BuildMaX.Web/
      ├─ BuildMaX.Web.csproj
      ├─ Program.cs
      ├─ appsettings.json
      ├─ appsettings.Development.json
      ├─ Properties/
      │  └─ launchSettings.json
      ├─ wwwroot/
      │  ├─ css/
      │  │  └─ site.css
      │  ├─ js/
      │  │  └─ site.js
      │  ├─ img/
      │  └─ lib/                      (jeśli używasz bibliotek frontowych)
      │
      ├─ Data/
      │  ├─ AppDbContext.cs
      │  ├─ Migrations/               (EF Core migrations)
      │  └─ Seed/
      │     ├─ IdentitySeeder.cs      (role + admin user)
      │     └─ DemoDataSeeder.cs      (warianty, dokumenty prawne)
      │
      ├─ Models/
      │  ├─ Identity/
      │  │  └─ ApplicationUser.cs
      │  ├─ Domain/
      │  │  ├─ Variant.cs
      │  │  ├─ AnalysisRequest.cs
      │  │  ├─ LegalDocument.cs
      │  │  └─ Enums/
      │  │     └─ AnalysisStatus.cs
      │  └─ ViewModels/
      │     ├─ PricingViewModel.cs
      │     ├─ DashboardViewModel.cs
      │     ├─ AnalysisCreateViewModel.cs
      │     └─ AnalysisResultViewModel.cs
      │
      ├─ Validation/
      │  ├─ ModuleFitsPlotAttribute.cs     (własny validator: cross-field)
      │  └─ VariantAvailabilityAttribute.cs (opcjonalnie: reguły wariantów)
      │
      ├─ Services/
      │  ├─ Analysis/
      │  │  ├─ IAnalysisCalculator.cs
      │  │  └─ AnalysisCalculator.cs       (logika obliczeń „chłonności”)
      │  ├─ Documents/
      │  │  ├─ IPdfReportService.cs
      │  │  └─ PdfReportService.cs         (wariant 1: raport PDF)
      │  └─ Time/
      │     └─ IClock.cs                   (opcjonalnie pod testy / spójny czas)
      │
      ├─ Helpers/
      │  ├─ HtmlHelpers/
      │  │  └─ LegalDocsHtmlHelpers.cs     (HTML Helper: accordion/lista dokumentów)
      │  └─ TagHelpers/
      │     └─ ProfitabilityBadgeTagHelper.cs (TagHelper: badge opłacalności)
      │
      ├─ Controllers/
      │  ├─ HomeController.cs              (public: landing/pricing)
      │  ├─ DashboardController.cs         (ręczny widok, statystyki LINQ)
      │  ├─ AnalysisRequestsController.cs  (CRUD zleceń; role + LINQ)
      │  ├─ VariantsController.cs          (CRUD admin)
      │  └─ LegalDocumentsController.cs    (CRUD admin)
      │
      ├─ Views/
      │  ├─ _ViewImports.cshtml
      │  ├─ _ViewStart.cshtml
      │  ├─ Shared/
      │  │  ├─ _LayoutPublic.cshtml        (layout 1)
      │  │  ├─ _LayoutApp.cshtml           (layout 2)
      │  │  ├─ _ValidationScriptsPartial.cshtml
      │  │  ├─ _LoginPartial.cshtml
      │  │  ├─ Components/                 (opcjonalnie ViewComponents)
      │  │  └─ Error.cshtml
      │  │
      │  ├─ Home/
      │  │  ├─ Index.cshtml                (landing)
      │  │  └─ Pricing.cshtml              (ręczny widok nie-CRUD)
      │  │
      │  ├─ Dashboard/
      │  │  └─ Index.cshtml                (ręczny widok nie-CRUD)
      │  │
      │  ├─ AnalysisRequests/
      │  │  ├─ Index.cshtml
      │  │  ├─ Create.cshtml
      │  │  ├─ Edit.cshtml
      │  │  ├─ Details.cshtml
      │  │  └─ Delete.cshtml
      │  │
      │  ├─ Variants/
      │  │  ├─ Index.cshtml
      │  │  ├─ Create.cshtml
      │  │  ├─ Edit.cshtml
      │  │  ├─ Details.cshtml
      │  │  └─ Delete.cshtml
      │  │
      │  ├─ LegalDocuments/
      │  │  ├─ Index.cshtml
      │  │  ├─ Create.cshtml
      │  │  ├─ Edit.cshtml
      │  │  ├─ Details.cshtml
      │  │  └─ Delete.cshtml
      │  │
      │  └─ Account/                       (jeśli nie używasz Razor Pages Identity UI)
      │     ├─ Login.cshtml
      │     └─ Register.cshtml
      │
      ├─ Areas/
      │  └─ Identity/                      (jeśli używasz scaffolded Identity UI)
      │     └─ Pages/
      │        ├─ Account/
      │        └─ ...
      │
      └─ Middleware/                       (opcjonalnie)
         └─ ExceptionHandlingMiddleware.cs
