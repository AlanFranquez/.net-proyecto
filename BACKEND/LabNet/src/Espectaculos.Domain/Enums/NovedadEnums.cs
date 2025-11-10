namespace Espectaculos.Domain.Enums;

public enum NovedadTipo
{
    Beneficio = 1,
    Comunicado = 2,
    Campaña = 3
}

public enum NovedadEstado
{
    Borrador = 0,
    Programada = 1,
    Publicada = 2,
    Archivada = 3
}

public enum NovedadAudiencia
{
    Todos = 0,
    SoloEstudiantes = 1,
    SoloFuncionarios = 2,
    RolesEspecificos = 3
}

public enum NovedadCanal
{
    Backoffice = 0,  
    FrontOffice = 1,
    NotificacionPush = 2
}

public enum NovedadPrioridad
{
    Baja = 0,
    Media = 1,
    Alta = 2
}