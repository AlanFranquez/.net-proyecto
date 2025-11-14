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
        System.Diagnostics.Debug.WriteLine("[HistorialView] OnAppearing - Loading eventos...");
        await LoadHistorialAsync();
    }

    private async Task LoadHistorialAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[HistorialView] === LOADING HISTORIAL ===");

            var usuario = await _db.GetLoggedUserAsync();
            if (usuario == null)
            {
                System.Diagnostics.Debug.WriteLine("[HistorialView] No logged user found");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[HistorialView] Usuario: {usuario.Email}, idApi: '{usuario.idApi}'");

            // Buscar credencial del usuario
            var credenciales = await _db.GetCredencialesAsync();
            Credencial credUsuario = null;

            foreach (var c in credenciales)
            {
                if (c.usuarioIdApi == usuario.idApi)
                {
                    credUsuario = c;
                    break;
                }
            }

            if (credUsuario == null)
            {
                System.Diagnostics.Debug.WriteLine("[HistorialView] ⚠️ No credential found for user");

                // Limpiar la lista si no hay credencial
                _viewModel.Accesos.Clear();
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[HistorialView] Credencial found: ID={credUsuario.CredencialId}, idApi='{credUsuario.idApi}'");

            // Obtener todos los eventos
            var eventos = await _db.GetEventosAccesoAsync();
            System.Diagnostics.Debug.WriteLine($"[HistorialView] Total eventos in DB: {eventos.Count}");

            List<EventoAcceso> eventosUsuario = new List<EventoAcceso>();

            // Filtrar eventos del usuario
            foreach (var evento in eventos)
            {
                if (evento.CredencialIdApi == credUsuario.idApi)
                {
                    // Cargar información del espacio si no está presente
                    if (evento.Espacio == null && evento.EspacioId > 0)
                    {
                        evento.Espacio = await _db.GetEspacioByIdAsync(evento.EspacioId);
                    }

                    // Si no se encontró por ID local, buscar por idApi
                    if (evento.Espacio == null && !string.IsNullOrEmpty(evento.EspacioIdApi))
                    {
                        var espacios = await _db.GetEspaciosAsync();
                        evento.Espacio = espacios.FirstOrDefault(e => e.idApi == evento.EspacioIdApi);
                    }

                    // Asignar la credencial al evento
                    if (evento.Credencial == null)
                    {
                        evento.Credencial = credUsuario;
                    }

                    eventosUsuario.Add(evento);

                    System.Diagnostics.Debug.WriteLine($"[HistorialView] Added evento: {evento.EventoId}, Espacio: {evento.Espacio?.Nombre ?? "Unknown"}, Resultado: {evento.Resultado}");
                }
            }

            System.Diagnostics.Debug.WriteLine($"[HistorialView] Found {eventosUsuario.Count} eventos for user");

            // ✅ ACTUALIZAR EL VIEWMODEL
            _viewModel.Accesos.Clear();

            // Ordenar por fecha más reciente primero
            var eventosOrdenados = eventosUsuario
                .OrderByDescending(e => e.MomentoDeAcceso)
                .ToList();

            foreach (var evento in eventosOrdenados)
            {
                _viewModel.Accesos.Add(evento);
            }

            System.Diagnostics.Debug.WriteLine($"[HistorialView] ✅ Updated ViewModel with {_viewModel.Accesos.Count} eventos");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistorialView] ❌ Error loading historial: {ex}");
            await DisplayAlert("Error", "Error cargando el historial de accesos.", "OK");
        }
    }

    private async void OnAccesoSelected(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.CurrentSelection?.Count > 0 && e.CurrentSelection[0] is EventoAcceso acceso)
            {
                ((CollectionView)sender).SelectedItem = null;

                System.Diagnostics.Debug.WriteLine($"[HistorialView] Selected evento: {acceso.EventoId}");
                await Shell.Current.GoToAsync($"accesoPerfil?eventoId={acceso.EventoId}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HistorialView] Error in OnAccesoSelected: {ex}");
        }
    }
}