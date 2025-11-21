using AppNetCredenciales.models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AppNetCredenciales.Services
{
    /// <summary>
    /// Interfaz para el servicio de gestión de eventos de acceso
    /// </summary>
    public interface IEventosService
    {
        /// <summary>
        /// Valida una credencial y registra el evento de acceso
        /// </summary>
        Task<EventoAccesoResult> ValidarYRegistrarAcceso(string idCriptografico, int espacioId);

        /// <summary>
        /// Obtiene el historial de eventos de un espacio
        /// </summary>
        Task<List<EventoAcceso>> ObtenerHistorial(int espacioId, DateTime? fechaInicio = null, DateTime? fechaFin = null);

        /// <summary>
        /// Registra un evento de acceso manualmente
        /// </summary>
        Task<EventoAcceso> RegistrarEvento(EventoAcceso evento);

        /// <summary>
        /// Sincroniza eventos locales con la API
        /// </summary>
        Task<bool> SincronizarEventos();
    }

    /// <summary>
    /// Resultado de la validación de acceso
    /// </summary>
    public class EventoAccesoResult
    {
        public bool AccesoConcedido { get; set; }
        public string Motivo { get; set; }
        public Credencial Credencial { get; set; }
        public Usuario Usuario { get; set; }
        public EventoAcceso Evento { get; set; }
        public string NombreCompleto { get; set; }
        public string Documento { get; set; }
    }
}
