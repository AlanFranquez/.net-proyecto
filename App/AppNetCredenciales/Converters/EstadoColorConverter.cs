using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Globalization;

namespace AppNetCredenciales.Converters
{
    /// <summary>
    /// Convertidor que transforma el estado de una credencial en un color
    /// </summary>
    public class EstadoColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return Colors.Gray;

            string estado = value.ToString() ?? "";

            return estado switch
            {
                "Emitida" => Color.FromArgb("#FFA500"),    // Naranja
                "Activada" => Color.FromArgb("#4CAF50"),   // Verde
                "Suspendida" => Color.FromArgb("#FF9800"), // Naranja oscuro
                "Expirada" => Color.FromArgb("#F44336"),   // Rojo
                _ => Colors.Gray
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack no está implementado para EstadoColorConverter");
        }
    }

    /// <summary>
    /// Convertidor que verifica si un string no está vacío
    /// </summary>
    public class StringNotEmptyConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value?.ToString());
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException("ConvertBack no está implementado para StringNotEmptyConverter");
        }
    }
}