using BuildMaX.Web.Data;
using BuildMaX.Web.Data.Seed;
using BuildMaX.Web.Models.Identity;
using BuildMaX.Web.Services.Analysis;
using BuildMaX.Web.Services.Documents;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Tworzy obiekt konfigurujący aplikację (kontener DI + konfiguracja + logowanie).
var builder = WebApplication.CreateBuilder(args);

//Dodaje obsługę kontrolerów MVC i widoków Razor.
builder.Services.AddControllersWithViews();

//Połączenie z bazą danych
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//Rejestruje Entity Framework i SQL Server.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Konfiguracja Identity (logowanie, role, hasła)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ustawienia cookie, Ustawia gdzie przekierować użytkownika gdy:nie jest zalogowany,nie ma dostępu.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});


// Własne serwisy
// kalkulator analizy
builder.Services.AddScoped<IAnalysisCalculator, AnalysisCalculator>();

// PDF
builder.Services.AddScoped<IPdfReportService, PdfReportService>();


//finalizacja konfig, budowanie app
var app = builder.Build();

//H. Obsługa błędów i HTTPS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
// Middleware (kolejność ma znaczenie!), Definiuje jak każde żądanie HTTP jest obsługiwane.
app.UseHttpsRedirection(); //Jeśli ktoś wejdzie przez http://, przekieruje na https://.
app.UseStaticFiles(); //Obsługuje pliki statyczne: .css, .js, .png.Bez tego strona byłaby bez stylów.
app.UseRouting();// Analizuje URL i ustala który kontroler i akcja ma obsłużyć żądanie.
app.UseAuthentication();//Sprawdza czy użytkownik jest zalogowany (odczytuje cookie sesji).
app.UseAuthorization(); //Sprawdza czy użytkownik ma dostęp (role, polityki).
//Definiuje standardowy adres URL.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


//Seed danych startowych
await DbSeeder.SeedAsync(app.Services);

app.Run();
