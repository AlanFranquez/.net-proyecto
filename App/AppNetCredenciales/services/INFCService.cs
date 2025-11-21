using System;
using System.Threading.Tasks;

namespace AppNetCredenciales.Services
{
    /// <summary>
    /// Interfaz para el servicio de lectura y escritura NFC
    /// </summary>
    public interface INFCService
    {
        /// <summary>
        /// Inicia la escucha de tags NFC
        /// </summary>
        Task<bool> IniciarLectura();

        /// <summary>
        /// Detiene la escucha de tags NFC
        /// </summary>
        Task DetenerLectura();

        /// <summary>
        /// Emite un mensaje NFC con el IdCriptográfico
        /// </summary>
        Task<bool> EmitirCredencial(string idCriptografico);

        /// <summary>
        /// Evento que se dispara cuando se detecta un tag NFC
        /// </summary>
        event EventHandler<string> TagDetectado;

        /// <summary>
        /// Indica si el dispositivo tiene capacidad NFC
        /// </summary>
        bool EstaDisponible { get; }

        /// <summary>
        /// Indica si está actualmente escuchando tags
        /// </summary>
        bool EstaEscuchando { get; }
    }
}
