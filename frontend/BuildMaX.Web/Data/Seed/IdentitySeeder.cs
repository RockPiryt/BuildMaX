using BuildMaX.Web.Models.Identity;
using Microsoft.AspNetCore.Identity;
// (role + admin user)using BuildMaX.Web.Models.Identity;
namespace BuildMaX.Web.Data.Seed
{
    public static class IdentitySeeder
    {
        public static readonly string[] Roles = { "Admin", "Analyst", "Client" };

        public static async Task SeedRolesAsync(IServiceProvider sp)
        {
            var rm = sp.GetRequiredService<RoleManager<IdentityRole>>();
            foreach (var r in Roles)
                if (!await rm.RoleExistsAsync(r))
                    await rm.CreateAsync(new IdentityRole(r));
        }

        public static async Task SeedAdminAsync(IServiceProvider sp)
        {
            var um = sp.GetRequiredService<UserManager<ApplicationUser>>();
            await SeedUserAsync(um, "admin@buildmax.local", "Admin123!", "Admin");
        }
        // opcjonalnie: konto Client do testów
        public static async Task SeedDemoClientAsync(IServiceProvider sp)
        {
            var um = sp.GetRequiredService<UserManager<ApplicationUser>>();
            await SeedUserAsync(um, "client@buildmax.local", "Client123!", "Client");
        }
        // opcjonalnie: konto Analyst do testów
        public static async Task SeedDemoAnalystAsync(IServiceProvider sp)
        {
            var um = sp.GetRequiredService<UserManager<ApplicationUser>>();
            await SeedUserAsync(um, "analyst@buildmax.local", "Analyst123!", "Analyst");
        }

        private static async Task SeedUserAsync(UserManager<ApplicationUser> um, string email, string password, string role)
        {
            var user = await um.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await um.CreateAsync(user, password);
                if (!result.Succeeded) return;
            }

            if (!await um.IsInRoleAsync(user, role))
                await um.AddToRoleAsync(user, role);
        }
    }
}
