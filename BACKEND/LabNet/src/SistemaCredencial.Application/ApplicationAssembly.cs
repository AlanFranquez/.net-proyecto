// SistemaCredencial.Application/ApplicationAssembly.cs
using System.Reflection;

namespace Espectaculos.Application;

public static class ApplicationAssembly
{
    // Opción A: propiedad estática clara (recomendada)
    public static Assembly Value => typeof(ApplicationAssembly).Assembly;
}