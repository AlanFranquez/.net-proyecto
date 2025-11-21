using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Amazon.CognitoIdentityProvider.Model;
using Espectaculos.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ICognitoService _cognitoService;
        private readonly ILogger<LoginModel> _logger;
        private readonly IConfiguration _config;

        public LoginModel(
            ICognitoService cognitoService,
            ILogger<LoginModel> logger,
            IConfiguration config)
        {
            _cognitoService = cognitoService;
            _logger = logger;
            _config = config;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;

            [Required]
            public string Password { get; set; } = string.Empty;
        }

        public void OnGet()
        {
        }

        // LOGIN handler → matches asp-page-handler="Login"
        public async Task<IActionResult> OnPostLoginAsync(string? returnUrl = null, CancellationToken ct = default)
        {
            returnUrl ??=
                Url.Page("/Dashboard/Index", new { area = "Admin" }) 
                ?? "/";

            if (!ModelState.IsValid)
                return Page();

            try
            {
                // 1) Authenticate against Cognito
                var idToken = await _cognitoService.LoginAsync(Input.Email, Input.Password, ct);

                // 2) Decode token and extract email
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(idToken);
                var emailFromToken = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

                var adminEmail = _config["Backoffice:AdminEmail"];

                // 3) Only allow the configured admin email
                if (string.IsNullOrWhiteSpace(adminEmail) ||
                    string.IsNullOrWhiteSpace(emailFromToken) ||
                    !string.Equals(emailFromToken.Trim(), adminEmail.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "Login denied. User {EmailClaim} is not configured Backoffice admin ({AdminEmail}).",
                        emailFromToken ?? Input.Email,
                        adminEmail
                    );

                    ModelState.AddModelError(string.Empty, "No tenés permisos para acceder al backoffice.");
                    return Page();
                }

                // 4) Store token in cookie so JwtBearer can read it (OnMessageReceived)
                Response.Cookies.Append("espectaculos_session", idToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = HttpContext.Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true,
                    Expires = DateTimeOffset.UtcNow.AddHours(8)
                });

                _logger.LogInformation("Backoffice admin {Email} logged in successfully.", emailFromToken);

                return LocalRedirect(returnUrl);
            }
            catch (NotAuthorizedException)
            {
                ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos.");
                return Page();
            }
            catch (UserNotFoundException)
            {
                ModelState.AddModelError(string.Empty, "Usuario no encontrado.");
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login.");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al iniciar sesión.");
                return Page();
            }
        }

        // CREATE ADMIN handler → matches asp-page-handler="CreateAdmin"
        public async Task<IActionResult> OnPostCreateAdminAsync(CancellationToken ct = default)
        {
            var adminEmail = _config["Backoffice:AdminEmail"];
            var adminPassword = _config["Backoffice:AdminPassword"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                ModelState.AddModelError(string.Empty,
                    "Backoffice:AdminEmail y Backoffice:AdminPassword deben estar configurados en appsettings o variables de entorno.");
                return Page();
            }

            try
            {
                await _cognitoService.RegisterUserAsync(adminEmail, adminPassword, ct);

                TempData["Ok"] =
                    $"Admin creado en Cognito: {adminEmail}. Ahora podés iniciar sesión con ese email y esa contraseña.";
                _logger.LogInformation("Admin user {Email} created via CreateAdmin.", adminEmail);
            }
            catch (UsernameExistsException)
            {
                TempData["Ok"] =
                    $"El usuario {adminEmail} ya existe en Cognito. Probá iniciar sesión con ese email y la contraseña configurada.";
                _logger.LogInformation("CreateAdmin called but user {Email} already exists.", adminEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el admin en Cognito.");
                ModelState.AddModelError(string.Empty, "No se pudo crear el admin en Cognito.");
                return Page();
            }

            return RedirectToPage(); // recarga /Admin/Account/Login
        }
    }
}
