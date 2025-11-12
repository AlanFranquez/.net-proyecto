using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using AppNetCredenciales.services;
using AppNetCredenciales.ViewModel;
using System;
using Microsoft.Maui.Controls;

namespace AppNetCredenciales.Views;

public partial class HistorialView : ContentPage
{
    private readonly HistorialViewModel _viewModel;
    private readonly AuthService _authService;
    private readonly LocalDBService _db;

    public HistorialView(AuthService auth, LocalDBService db)
    {
        InitializeComponent();
        _authService = auth;
        _db = db;
        _viewModel = new HistorialViewModel(auth, db);
        BindingContext = _viewModel;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        await LoadHistorialAsync();
    }

    private async Task LoadHistorialAsync()
    {
        try
        {
            var usuario = await _db.GetLoggedUserAsync();
            if (usuario != null && usuario.CredencialId > 0)
            {
                var eventos = await _db.GetEventosAccesoByUsuarioIdAsync(usuario.CredencialId);
                // Update the ViewModel's collection here if needed
                // _viewModel.UpdateEventos(eventos);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistorialView] Error loading historial: {ex}");
        }
    }

    private async void OnAccesoSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection?.Count > 0 && e.CurrentSelection[0] is EventoAcceso acceso)
        {
            ((CollectionView)sender).SelectedItem = null;

            await Shell.Current.GoToAsync($"accesoPerfil?eventoId={acceso.EventoId}");
        }
    }
}