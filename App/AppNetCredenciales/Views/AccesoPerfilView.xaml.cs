using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using Microsoft.Maui.Controls;

namespace AppNetCredenciales.Views;

[QueryProperty(nameof(EventoId), "eventoId")]
public partial class AccesoPerfilView : ContentPage
{
    private readonly LocalDBService _db;

    public int EventoId { get; set; }

    public EventoAcceso CurrentAcceso { get; set; } = new EventoAcceso();

    public AccesoPerfilView()
    {
        InitializeComponent();
        _db = App.Services?.GetRequiredService<LocalDBService>()
               ?? throw new InvalidOperationException("LocalDBService not registered in DI.");
        BindingContext = CurrentAcceso;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        if (EventoId == 0) return;

        var acceso = await _db.GetEventoAccesoByIdAsync(EventoId);
        if (acceso != null)
        {
            // ensure Espacio is loaded for the UI
            if (acceso.Espacio == null && acceso.EspacioId != 0)
            {
                acceso.Espacio = await _db.GetEspacioByIdAsync(acceso.EspacioId);
            }

            CurrentAcceso = acceso;
            BindingContext = CurrentAcceso;
        }
    }
}