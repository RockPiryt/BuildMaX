namespace BuildMaX.Web.Data.Seed
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            await IdentitySeeder.SeedRolesAsync(sp);
            await IdentitySeeder.SeedAdminAsync(sp);
            await IdentitySeeder.SeedDemoClientAsync(sp);
            await IdentitySeeder.SeedDemoAnalystAsync(sp);

            await DemoDataSeeder.SeedVariantsAsync(sp);
            await DemoDataSeeder.SeedLegalDocumentsAsync(sp);
            await DemoDataSeeder.SeedAnalysisRequestsAsync(sp);
        }
    }
}
