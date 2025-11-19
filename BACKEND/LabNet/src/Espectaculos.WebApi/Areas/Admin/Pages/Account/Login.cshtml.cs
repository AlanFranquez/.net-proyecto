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

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null, CancellationToken ct = default)
        {
            returnUrl ??=
                Url.Page("/Dashboard/Index", new { area = "Admin" })
                ?? "/";

            if (!ModelState.IsValid)
                return Page();

            try
            {
                // 1) Authenticate against Cognito (any user in the pool)
                var idToken = await _cognitoService.LoginAsync(Input.Email, Input.Password, ct);

                // 2) Decode token and extract email
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(idToken);
                var emailFromToken = jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

                var adminEmail = _config["Backoffice:AdminEmail"];

                // 3) Only allow the configured admin email
                if (string.IsNullOrWhiteSpace(adminEmail) ||
                    string.IsNullOrWhiteSpace(emailFromToken) ||
                    !string.Equals(emailFromToken, adminEmail, StringComparison.OrdinalIgnoreCase))
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
    }
}
