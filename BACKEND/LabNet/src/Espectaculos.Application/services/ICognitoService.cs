using System.Diagnostics;
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

    // 👇 NUEVO: cambio de contraseña
    Task ChangePasswordAsync(string email, string currentPassword, string newPassword, CancellationToken ct = default);
}

public class CognitoService : ICognitoService
{
    private readonly IAmazonCognitoIdentityProvider _provider;
    private readonly AwsCognitoSettings _settings;
    private readonly ILogger<CognitoService> _logger;

    public CognitoService(
        IAmazonCognitoIdentityProvider provider,
        IOptions<AwsCognitoSettings> settings,
        ILogger<CognitoService> logger)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (string.IsNullOrWhiteSpace(_settings.UserPoolId) || string.IsNullOrWhiteSpace(_settings.ClientId))
            throw new InvalidOperationException("AWS Cognito configuration incomplete: UserPoolId and ClientId are required.");

        // Basic config log
        _logger.LogInformation(
            "CognitoService initialized: UserPoolId={UserPoolId}, ClientIdPresent={HasClientId}, RegionSetting={RegionSetting}",
            _settings.UserPoolId,
            !string.IsNullOrEmpty(_settings.ClientId),
            _settings.Region
        );

        // Extra debug: what concrete client type / region are we using?
        try
        {
            if (_provider is AmazonCognitoIdentityProviderClient concrete)
            {
                var regionFromClient = concrete.Config.RegionEndpoint?.SystemName ?? "<null>";
                _logger.LogInformation(
                    "Cognito IAmazonCognitoIdentityProvider concrete type: {Type}, ClientRegion={ClientRegion}",
                    concrete.GetType().FullName,
                    regionFromClient
                );
            }
            else
            {
                _logger.LogWarning(
                    "IAmazonCognitoIdentityProvider is not AmazonCognitoIdentityProviderClient. Actual type: {Type}",
                    _provider.GetType().FullName
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while inspecting Cognito provider concrete type.");
        }
    }

    public async Task<string> RegisterUserAsync(string email, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));

        var sw = Stopwatch.StartNew();
        _logger.LogDebug("RegisterUserAsync started for email {Email}", email);

        try
        {
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

            _logger.LogDebug("AdminCreateUserAsync request: {@Request}", createReq);

            var createResp = await _provider.AdminCreateUserAsync(createReq, ct).ConfigureAwait(false);

            _logger.LogDebug(
                "AdminCreateUserAsync completed for {Email}. StatusCode={StatusCode}",
                email,
                createResp?.HttpStatusCode
            );

            var setPwdReq = new AdminSetUserPasswordRequest
            {
                UserPoolId = _settings.UserPoolId,
                Username = email,
                Password = password,
                Permanent = true
            };

            await _provider.AdminSetUserPasswordAsync(setPwdReq, ct).ConfigureAwait(false);
            _logger.LogDebug("AdminSetUserPasswordAsync completed for {Email}", email);

            // obtener sub
            var sub = createResp.User?.Attributes?.FirstOrDefault(a => a.Name == "sub")?.Value;
            if (string.IsNullOrEmpty(sub))
            {
                _logger.LogDebug("Sub not found in AdminCreateUser response; calling AdminGetUserAsync for {Email}", email);

                var getUser = await _provider.AdminGetUserAsync(
                    new AdminGetUserRequest
                    {
                        UserPoolId = _settings.UserPoolId,
                        Username = email
                    }, ct).ConfigureAwait(false);

                sub = getUser.UserAttributes?.FirstOrDefault(a => a.Name == "sub")?.Value;
            }

            if (string.IsNullOrEmpty(sub))
            {
                _logger.LogError("No se pudo obtener el sub de Cognito para {Email}", email);
                throw new Exception("No se pudo obtener el sub de Cognito.");
            }

            return sub;
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            _logger.LogError(
                ex,
                "Cognito API error durante RegisterUserAsync para {Email}: {Code} - {Message}",
                email,
                ex.ErrorCode,
                ex.Message
            );
            throw;
        }
        finally
        {
            sw.Stop();
            _logger.LogDebug("RegisterUserAsync finished for {Email} in {ElapsedMs} ms", email, sw.ElapsedMilliseconds);
        }
    }

    public async Task<string> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        if (_provider == null)
            throw new InvalidOperationException("IAmazonCognitoIdentityProvider no está inicializado (provider == null).");

        if (_settings == null)
            throw new InvalidOperationException("AwsCognitoSettings no está inicializado.");

        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));

        var sw = Stopwatch.StartNew();
        _logger.LogDebug("LoginAsync started for user {User}", username);

        try
        {
            var req = new AdminInitiateAuthRequest
            {
                UserPoolId = _settings.UserPoolId,
                ClientId = _settings.ClientId,
                AuthFlow = AuthFlowType.ADMIN_NO_SRP_AUTH,
                AuthParameters = new Dictionary<string, string>(),
                ClientMetadata = new Dictionary<string, string>()
            };

            req.AuthParameters["USERNAME"] = username;
            req.AuthParameters["PASSWORD"] = password;

            // Si usás ClientSecret y necesitás secretHash, calculalo y añadilo:
            // req.AuthParameters["SECRET_HASH"] = CalculateSecretHash(_settings.ClientId, _settings.ClientSecret, username);

            _logger.LogDebug(
                "AdminInitiateAuthRequest preparado para user {User}. UserPoolId={PoolId}, ClientId present? {HasClientId}, AuthFlow={AuthFlow}",
                username,
                _settings.UserPoolId,
                !string.IsNullOrEmpty(_settings.ClientId),
                req.AuthFlow
            );

            var resp = await _provider.AdminInitiateAuthAsync(req, ct).ConfigureAwait(false);

            sw.Stop();
            _logger.LogDebug(
                "AdminInitiateAuthAsync completado para {User} en {ElapsedMs} ms. ChallengeName={ChallengeName}, HttpStatus={Status}",
                username,
                sw.ElapsedMilliseconds,
                resp?.ChallengeName,
                resp?.HttpStatusCode
            );

            if (resp == null)
            {
                _logger.LogWarning("AdminInitiateAuthAsync devolvió null para user {User}", username);
                throw new InvalidOperationException("Respuesta nula de Cognito AdminInitiateAuth.");
            }

            var authResult = resp.AuthenticationResult;
            if (authResult == null)
            {
                _logger.LogWarning(
                    "LoginAsync: AuthenticationResult es null para user {User}. Resp: {@Resp}",
                    username,
                    resp
                );

                // Si manejás challenges (MFA/NEW_PASSWORD_REQUIRED) deberías procesarlos aquí.
                throw new InvalidOperationException("Autenticación incompleta: se requirió un challenge o no se obtuvo token.");
            }

            var idToken = authResult.IdToken;
            if (string.IsNullOrEmpty(idToken))
            {
                _logger.LogWarning(
                    "LoginAsync: IdToken vacío para user {User}. AuthResult: {@AuthResult}",
                    username,
                    authResult
                );
                throw new InvalidOperationException("No se obtuvo id token al iniciar sesión.");
            }

            _logger.LogDebug("LoginAsync: IdToken obtenido correctamente para user {User}", username);
            return idToken;
        }
        catch (NotAuthorizedException ex)
        {
            _logger.LogInformation(ex, "LoginAsync: Credenciales inválidas para {User}", username);
            throw;
        }
        catch (UserNotFoundException ex)
        {
            _logger.LogInformation(ex, "LoginAsync: Usuario no encontrado {User}", username);
            throw;
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            // Caso específico: token de credenciales AWS expirado
            if (ex.Message != null &&
                ex.Message.IndexOf("The security token included in the request is expired",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                _logger.LogError(
                    ex,
                    "LoginAsync: AWS credentials token expired when calling Cognito for {User}. ErrorCode={Code}",
                    username,
                    ex.ErrorCode
                );
            }
            else
            {
                _logger.LogError(
                    ex,
                    "Cognito API error durante LoginAsync para {User}: {Code} - {Message}",
                    username,
                    ex.ErrorCode,
                    ex.Message
                );
            }

            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoginAsync: Unexpected error for user {User}", username);
            throw;
        }
        finally
        {
            if (sw.IsRunning)
            {
                sw.Stop();
            }

            _logger.LogDebug("LoginAsync finished for {User} in {ElapsedMs} ms", username, sw.ElapsedMilliseconds);
        }
    }

    public async Task DeleteUserByUsernameAsync(string username, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));

        var sw = Stopwatch.StartNew();
        _logger.LogDebug("DeleteUserByUsernameAsync started for {User}", username);

        try
        {
            await _provider.AdminDeleteUserAsync(
                new AdminDeleteUserRequest
                {
                    UserPoolId = _settings.UserPoolId,
                    Username = username
                }, ct).ConfigureAwait(false);

            _logger.LogInformation("Usuario {User} eliminado de Cognito (UserPoolId={PoolId})", username, _settings.UserPoolId);
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            _logger.LogError(
                ex,
                "Cognito API error durante DeleteUserByUsernameAsync para {User}: {Code} - {Message}",
                username,
                ex.ErrorCode,
                ex.Message
            );
            throw;
        }
        finally
        {
            sw.Stop();
            _logger.LogDebug("DeleteUserByUsernameAsync finished for {User} in {ElapsedMs} ms", username, sw.ElapsedMilliseconds);
        }
    }

    // 👇 NUEVO: cambio de contraseña usando AdminSetUserPassword
    public async Task ChangePasswordAsync(string email, string currentPassword, string newPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email));
        if (string.IsNullOrWhiteSpace(currentPassword)) throw new ArgumentNullException(nameof(currentPassword));
        if (string.IsNullOrWhiteSpace(newPassword)) throw new ArgumentNullException(nameof(newPassword));

        var sw = Stopwatch.StartNew();
        _logger.LogDebug("ChangePasswordAsync started for {Email}", email);

        try
        {
            // 1) Validar contraseña actual intentando login
            _logger.LogDebug("ChangePasswordAsync: validating current password via LoginAsync for {Email}", email);
            await LoginAsync(email, currentPassword, ct); // lanza NotAuthorizedException si está mal

            // 2) Setear nueva contraseña permanente
            var req = new AdminSetUserPasswordRequest
            {
                UserPoolId = _settings.UserPoolId,
                Username = email,
                Password = newPassword,
                Permanent = true
            };

            _logger.LogDebug("ChangePasswordAsync: calling AdminSetUserPasswordAsync for {Email}", email);
            await _provider.AdminSetUserPasswordAsync(req, ct).ConfigureAwait(false);

            _logger.LogInformation("ChangePasswordAsync: contraseña cambiada correctamente para {Email}", email);
        }
        catch (NotAuthorizedException ex)
        {
            _logger.LogInformation(ex, "ChangePasswordAsync: contraseña actual incorrecta para {Email}", email);
            throw;
        }
        catch (AmazonCognitoIdentityProviderException ex)
        {
            _logger.LogError(
                ex,
                "Cognito API error durante ChangePasswordAsync para {Email}: {Code} - {Message}",
                email,
                ex.ErrorCode,
                ex.Message
            );
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ChangePasswordAsync: Unexpected error for {Email}", email);
            throw;
        }
        finally
        {
            sw.Stop();
            _logger.LogDebug("ChangePasswordAsync finished for {Email} in {ElapsedMs} ms", email, sw.ElapsedMilliseconds);
        }
    }
}
