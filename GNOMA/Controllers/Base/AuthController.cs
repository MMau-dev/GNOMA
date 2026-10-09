using GNOMA.Application.Services.Interfaces;
using GNOMA.Application.Services.Supabase;
using GNOMA.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GNOMA.Controllers.Base
{
    public sealed class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View(new AuthRequest());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            AuthRequest model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _authService.SignInAsync(
                    model.Email,
                    model.Password,
                    cancellationToken);

                await CreateApplicationSessionAsync(result);

                return RedirectToAction("Index", "Home");
            }
            catch (SupabaseRequestException ex)
            {
                _logger.LogWarning(
                    "Supabase rechazó una solicitud de autenticación. " +
                    "HTTP {StatusCode}.",
                    ex.StatusCode);

                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible iniciar sesión. " +
                    "Verifica tus credenciales e inténtalo de nuevo.");

                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogInformation(
                    "No se pudo crear la sesión: {Reason}",
                    ex.Message);

                ModelState.AddModelError(
                    string.Empty,
                    "La cuenta necesita atención adicional. " +
                    "Verifica tu correo o intenta nuevamente.");

                return View(model);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new AuthRequest());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            AuthRequest model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var result = await _authService.SignUpAsync(
                    model.Email,
                    model.Password,
                    cancellationToken);

                await CreateApplicationSessionAsync(result);

                return RedirectToAction("Index", "Home");
            }
            catch (SupabaseRequestException ex)
            {
                _logger.LogWarning(
                    "Supabase rechazó el registro. HTTP {StatusCode}.",
                    ex.StatusCode);

                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible registrar la cuenta. " +
                    "Comprueba los datos o si el correo ya existe.");

                return View(model);
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Registro recibido. Revisa tu correo para confirmar " +
                    "la cuenta antes de iniciar sesión.");

                return View(model);
            }
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(
            CancellationToken cancellationToken)
        {
            var accessToken = User.FindFirstValue("supabase_access_token");

            try
            {
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    await _authService.SignOutAsync(
                        accessToken,
                        cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "No se pudo invalidar la sesión remota de Supabase.");
            }
            finally
            {
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
            }

            return RedirectToAction(nameof(Login));
        }

        private async Task CreateApplicationSessionAsync(AuthResult result)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.UserId),
            new(ClaimTypes.Email, result.Email),
            new("supabase_access_token", result.AccessToken),
            new("supabase_refresh_token", result.RefreshToken)
        };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    AllowRefresh = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(50)
                });
        }
    }
}
