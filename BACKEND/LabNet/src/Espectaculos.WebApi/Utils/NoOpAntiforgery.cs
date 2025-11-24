namespace Espectaculos.WebApi.Utils;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

public class NoOpAntiforgery : IAntiforgery
{
    public AntiforgeryTokenSet GetAndStoreTokens(HttpContext httpContext)
        => new AntiforgeryTokenSet(string.Empty, string.Empty, string.Empty, string.Empty);

    public AntiforgeryTokenSet GetTokens(HttpContext httpContext)
        => new AntiforgeryTokenSet(string.Empty, string.Empty, string.Empty, string.Empty);

    public Task ValidateRequestAsync(HttpContext httpContext)
        => Task.CompletedTask;

    public void SetCookieTokenAndHeader(HttpContext httpContext)
    {
        // no-op
    }

    // <-- Esta es la que faltaba en tu implementación
    public Task<bool> IsRequestValidAsync(HttpContext httpContext)
        => Task.FromResult(true);
}
