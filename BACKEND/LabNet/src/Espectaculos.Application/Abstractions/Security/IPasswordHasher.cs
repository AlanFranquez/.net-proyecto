using System;

namespace Espectaculos.Application.Abstractions.Security
{
    /// <summary>
    /// Abstracción para hashear y verificar contraseñas.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Devuelve el hash de la contraseña.
        /// </summary>
        string Hash(string password);

        /// <summary>
        /// Verifica si la contraseña en texto plano coincide con el hash almacenado.
        /// </summary>
        bool Verify(string password, string passwordHash);
    }
}