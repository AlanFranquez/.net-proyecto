using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Espectaculos.WebApi.Authentication;

public class MockAuthenticationSchemeOptions : AuthenticationSchemeOptions { }

public class MockAuthenticationHandler : AuthenticationHandler<MockAuthenticationSchemeOptions>
{
    public MockAuthenticationHandler(IOptionsMonitor<MockAuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Buscar token mock en cookie o header
        var token = Request.Cookies["espectaculos_session"] ?? 
                   Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (string.IsNullOrEmpty(token) || !token.StartsWith("mock."))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        try
        {
            // Decodificar el payload del token mock
            var parts = token.Split('.');
            if (parts.Length != 3) // mock.payload.dev
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid mock token format"));
            }

            var payloadBase64 = parts[1];
            var payloadJson = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payloadBase64));
            var payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(payloadJson);

            var claims = new List<Claim>();
            
            if (payload.TryGetValue("sub", out var sub))
                claims.Add(new Claim(ClaimTypes.NameIdentifier, sub.GetString()));
                
            if (payload.TryGetValue("email", out var email))
            {
                claims.Add(new Claim(ClaimTypes.Email, email.GetString()));
                claims.Add(new Claim(ClaimTypes.Name, email.GetString()));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Error decodificando token mock");
            return Task.FromResult(AuthenticateResult.Fail("Invalid mock token"));
        }
    }
}