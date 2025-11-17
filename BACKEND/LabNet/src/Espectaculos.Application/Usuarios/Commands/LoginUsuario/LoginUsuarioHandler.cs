using Espectaculos.Application.Services;
using MediatR;

namespace Espectaculos.Application.Usuarios.Commands.LoginUsuario;

public class LoginUsuarioHandler : IRequestHandler<LoginUsuarioCommand, string>
{
    private readonly ICognitoService _cognito;

    public LoginUsuarioHandler(ICognitoService cognito)
    {
        _cognito = cognito;
    }

    public async Task<string> Handle(LoginUsuarioCommand command, CancellationToken ct)
    {
        var token = await _cognito.LoginAsync(command.Email, command.Password);
        return token;
    }
}
