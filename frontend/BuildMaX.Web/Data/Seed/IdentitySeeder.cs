// (role + admin user)using BuildMaX.Web.Models.Identity;
using Microsoft.AspNetCore.Identity;

namespace BuildMaX.Web.Data.Seed
{
    public static class IdentitySeeder
    {
        public static readonly string[] Roles = new[] { "Admin", "Analyst", "Client" };

        public static async Task SeedRolesAsync(IServiceProvider sp)
        {
            var roleManager = sp.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedAdminAsync(IServiceProvider sp)
        {
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            const string adminEmail = "admin@buildmax.local";
            const string adminPassword = "Admin123!";

            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, adminPassword);
                if (!result.Succeeded)
                    return; // nie rzucam wyjątku, żeby nie blokować startu; logowanie możesz dodać wg uznania
            }

            if (!await userManager.IsInRoleAsync(admin, "Admin"))
                await userManager.AddToRoleAsync(admin, "Admin");
        }

        // opcjonalnie: konto Client do testów
        public static async Task SeedDemoClientAsync(IServiceProvider sp)
        {
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            const string email = "client@buildmax.local";
            const string password = "Client123!";

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded) return;
            }

            if (!await userManager.IsInRoleAsync(user, "Client"))
                await userManager.AddToRoleAsync(user, "Client");
        }

        // opcjonalnie: konto Analyst do testów
        public static async Task SeedDemoAnalystAsync(IServiceProvider sp)
        {
            var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();

            const string email = "analyst@buildmax.local";
            const string password = "Analyst123!";

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, password);
                if (!result.Succeeded) return;
            }

            if (!await userManager.IsInRoleAsync(user, "Analyst"))
                await userManager.AddToRoleAsync(user, "Analyst");
        }
    }
}
