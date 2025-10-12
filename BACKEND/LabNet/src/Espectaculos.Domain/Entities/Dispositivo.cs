﻿using Espectaculos.Domain.Enums;

namespace Espectaculos.Domain.Entities;

public class Dispositivo
{
    public Guid DispositivoId { get; set; }
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = default!;
    public string? NumeroTelefono { get; set; }
    public PlataformaTipo Plataforma { get; set; }
    public string? HuellaDispositivo { get; set; }
    public bool BiometriaHabilitada { get; set; }
    public DispositivoTipo Estado { get; set; }

    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
    public ICollection<Sincronizacion> Sincronizaciones { get; set; } = new List<Sincronizacion>();
}