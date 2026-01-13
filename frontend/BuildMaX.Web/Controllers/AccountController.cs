using BuildMaX.Web.Models.Identity;
using BuildMaX.Web.Models.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BuildMaX.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _users; //(tworzenie użytkownika, role)
        private readonly SignInManager<ApplicationUser> _signIn; //(logowanie/wylogowanie)

        public AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn)
        {
            _users = users;
            _signIn = signIn;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // dane z formularza RegisterViewModel
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return View(vm);

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email
            };

            // ASP.NET Identity:, Hashuje hasło (domyślnie PBKDF2 / bcrypt zależnie od wersji)., Zapisuje do tabeli AspNetUsers.PasswordHash.
            var result = await _users.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                // domyślnie rola Client, To powoduje: Wstawienie wpisu do tabeli AspNetUserRoles, Połączenie UserId ↔ RoleId roli Client
                await _users.AddToRoleAsync(user, "Client");

                // Dla pewności wyczyść ewentualny stan poprzedniej sesji
                await _signIn.SignOutAsync();
                await _signIn.SignInAsync(user, isPersistent: false);

                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            foreach (var err in result.Errors)
                ModelState.AddModelError(string.Empty, err.Description);

            return View(vm);
        }

        // Wyświetla formularz logowania (Login.cshtml). returnUrl przechowuje adres, na który użytkownik zostanie przekierowany po zalogowaniu.
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            // jeśli już zalogowany, nie pokazuj loginu – odeślij na returnUrl albo Home
            if (User?.Identity?.IsAuthenticated == true)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            // opcjonalnie: wyczyść zewnętrzne ciasteczka (np. po social login) – bezpieczne w standardowych konfiguracjach
            await _signIn.SignOutAsync();

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
                return View(vm);

            // PasswordSignInAsync: waliduje login i hasło, porównuje z hashem w DB
            var result = await _signIn.PasswordSignInAsync(
                vm.Email,
                vm.Password,
                vm.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Nieprawidłowy login lub hasło.");
            return View(vm);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signIn.SignOutAsync();

            // Wymuś powrót na stronę publiczną
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied() => View();
    }
}
