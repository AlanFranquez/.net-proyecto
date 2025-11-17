using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using Espectaculos.Application.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Espectaculos.Application.Services;
public interface ICognitoService
{
    Task<string> RegisterUserAsync(string email, string password, CancellationToken ct = default);
    Task<string> LoginAsync(string username, string password, CancellationToken ct = default);
    Task DeleteUserByUsernameAsync(string username, CancellationToken ct = default);
}

public class CognitoService : ICognitoService
{
    private readonly IAmazonCognitoIdentityProvider _provider;
    private readonly AwsCognitoSettings _settings;
    private readonly ILogger<CognitoService> _logger;

    public CognitoService(IAmazonCognitoIdentityProvider provider, IOptions<AwsCognitoSettings> settings, ILogger<CognitoService> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(_settings.UserPoolId) || string.IsNullOrWhiteSpace(_settings.ClientId))
            throw new InvalidOperationException("AWS Cognito configuration incomplete: UserPoolId and ClientId are required.");

        _logger.LogInformation("CognitoService initialized: UserPoolId={UserPoolId}, ClientIdPresent={HasClientId}, Region={Region}",
            _settings.UserPoolId, !string.IsNullOrEmpty(_settings.ClientId), _settings.Region);
    }

    // (Puedes mantener/implementar RegisterUserAsync como antes; no la toco aquí)
    public async Task<string> RegisterUserAsync(string email, string password, CancellationToken ct = default)
    {
        // Ejemplo: AdminCreateUser + AdminSetUserPassword
        var createReq = new AdminCreateUserRequest
        {
            UserPoolId = _settings.UserPoolId,
            Username = email,
            UserAttributes = new List<AttributeType>
            {
                new AttributeType { Name = "email", Value = email },
                new AttributeType { Name = "email_verified", Value = "true" }
            },
            MessageAction = "SUPPRESS"
        };

        var createResp = await _provider.AdminCreateUserAsync(createReq, ct);

        await _provider.AdminSetUserPasswordAsync(new AdminSetUserPasswordRequest
        {
            UserPoolId = _settings.UserPoolId,
            Username = email,
            Password = password,
            Permanent = true
        }, ct);

        // obtener sub
        var sub = createResp.User?.Attributes?.FirstOrDefault(a => a.Name == "sub")?.Value;
        if (string.IsNullOrEmpty(sub))
        {
            var getUser = await _provider.AdminGetUserAsync(new AdminGetUserRequest { UserPoolId = _settings.UserPoolId, Username = email }, ct);
            sub = getUser.UserAttributes?.FirstOrDefault(a => a.Name == "sub")?.Value;
        }

        return sub ?? throw new Exception("No se pudo obtener el sub de Cognito.");
    }

   public async Task<string> LoginAsync(string username, string password, CancellationToken ct = default)
{
    if (_provider == null)
        throw new InvalidOperationException("IAmazonCognitoIdentityProvider no está inicializado (provider == null).");

    if (_settings == null)
        throw new InvalidOperationException("AwsCognitoSettings no está inicializado.");

    if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
    if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));

    try
    {
        var req = new AdminInitiateAuthRequest
        {
            UserPoolId = _settings.UserPoolId,
            ClientId = _settings.ClientId,
            AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
            // Inicializar colecciones para evitar NRE
            AuthParameters = new Dictionary<string, string>(),
            ClientMetadata = new Dictionary<string, string>()
        };

        req.AuthParameters["USERNAME"] = username;
        req.AuthParameters["PASSWORD"] = password;

        // Si usás ClientSecret y necesitás secretHash, calculalo y añadilo:
        // req.AuthParameters["SECRET_HASH"] = CalculateSecretHash(_settings.ClientId, _settings.ClientSecret, username);

        _logger.LogDebug("AdminInitiateAuthRequest preparado para user {User}. UserPoolId={PoolId}, ClientId present? {HasClientId}",
            username, _settings.UserPoolId, !string.IsNullOrEmpty(_settings.ClientId));

        var resp = await _provider.AdminInitiateAuthAsync(req, ct).ConfigureAwait(false);
        if (resp == null)
        {
            _logger.LogWarning("AdminInitiateAuthAsync devolvió null para user {User}", username);
            throw new InvalidOperationException("Respuesta nula de Cognito AdminInitiateAuth.");
        }

        // Puede que AuthenticationResult sea null (p. ej. cuando se requiere challenge)
        var authResult = resp.AuthenticationResult;
        if (authResult == null)
        {
            _logger.LogWarning("LoginAsync: AuthenticationResult es null para user {User}. Resp: {@Resp}", username, resp);
            // Si manejás challenges (MFA/NEW_PASSWORD_REQUIRED) deberías procesarlos aquí.
            throw new InvalidOperationException("Autenticación incompleta: se requirió un challenge o no se obtuvo token.");
        }

        var idToken = authResult.IdToken;
        if (string.IsNullOrEmpty(idToken))
        {
            _logger.LogWarning("LoginAsync: IdToken vacío para user {User}. AuthResult: {@AuthResult}", username, authResult);
            throw new InvalidOperationException("No se obtuvo id token al iniciar sesión.");
        }

        return idToken;
    }
    catch (NotAuthorizedException)
    {
        _logger.LogInformation("LoginAsync: Credenciales inválidas para {User}", username);
        throw;
    }
    catch (UserNotFoundException)
    {
        _logger.LogInformation("LoginAsync: Usuario no encontrado {User}", username);
        throw;
    }
    catch (AmazonCognitoIdentityProviderException ex)
    {
        _logger.LogError(ex, "Cognito API error durante LoginAsync para {User}: {Code} - {Message}", username, ex.ErrorCode, ex.Message);
        throw;
    }
}

    public async Task DeleteUserByUsernameAsync(string username, CancellationToken ct = default)
    {
        await _provider.AdminDeleteUserAsync(new AdminDeleteUserRequest { UserPoolId = _settings.UserPoolId, Username = username }, ct);
    }
}