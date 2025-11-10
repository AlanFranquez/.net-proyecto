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

    public HistorialView(AuthService auth, LocalDBService db)
    {
        InitializeComponent();
        _authService = auth;
        _viewModel = new HistorialViewModel(auth, db);
        BindingContext = _viewModel;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();
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