using AppNetCredenciales.Data;
using AppNetCredenciales.models;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace AppNetCredenciales.Views;

[QueryProperty(nameof(EventoId), "eventoId")]
public partial class AccesoPerfilView : ContentPage, INotifyPropertyChanged
{
    private readonly LocalDBService _db;
    private const int MAX_BENEFICIOS_SELECCIONADOS = 3;

    public int EventoId { get; set; }

    public EventoAcceso CurrentAcceso { get; set; } = new EventoAcceso();

    public ObservableCollection<BeneficioSeleccionable> Beneficios { get; set; } = new ObservableCollection<BeneficioSeleccionable>();

    private ObservableCollection<BeneficioSeleccionable> _beneficiosSeleccionados = new ObservableCollection<BeneficioSeleccionable>();
    public ObservableCollection<BeneficioSeleccionable> BeneficiosSeleccionados
    {
        get => _beneficiosSeleccionados;
        set
        {
            _beneficiosSeleccionados = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanRedeemBenefits));
            OnPropertyChanged(nameof(BeneficiosDisponiblesParaSeleccionar));
        }
    }

    private int _beneficiosYaCanjeados = 0;
    public int BeneficiosYaCanjeados
    {
        get => _beneficiosYaCanjeados;
        set
        {
            _beneficiosYaCanjeados = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(BeneficiosDisponiblesParaSeleccionar));
        }
    }

    public int BeneficiosDisponiblesParaSeleccionar => MAX_BENEFICIOS_SELECCIONADOS - BeneficiosYaCanjeados;

    public bool IsPermittedAccess => CurrentAcceso?.Resultado == AccesoTipo.Permitir;
    public bool IsNonPermittedAccess => !IsPermittedAccess;
    public bool CanRedeemBenefits => BeneficiosSeleccionados.Count > 0 && BeneficiosSeleccionados.Count <= BeneficiosDisponiblesParaSeleccionar;

    public AccesoPerfilView()
    {
        InitializeComponent();
        _db = App.Services?.GetRequiredService<LocalDBService>()
               ?? throw new InvalidOperationException("LocalDBService not registered in DI.");

        BeneficiosSeleccionados.CollectionChanged += (s, e) =>
        {
            OnPropertyChanged(nameof(CanRedeemBenefits));
        };

        BindingContext = this;
    }

    protected async override void OnAppearing()
    {
        base.OnAppearing();

        if (EventoId == 0) return;

        var acceso = await _db.GetEventoAccesoByIdAsync(EventoId);

        if (acceso != null)
        {
            if (acceso.Espacio == null && acceso.EspacioIdApi != null)
            {
                acceso.Espacio = await _db.GetEspacioByIdAsync(acceso.EspacioIdApi);
            }

            if (acceso.Resultado == AccesoTipo.Permitir)
            {
                var beneficios = await _db.GetBeneficiosAsync();
                var usuarioLogueado = await _db.GetLoggedUserAsync();

                if (beneficios != null && usuarioLogueado != null)
                {
                    var beneficiosFiltrados = beneficios
                        .Where(b => b.EspaciosIDs.Contains(acceso.EspacioIdApi))
                        .ToList();

                    // Contar beneficios ya canjeados
                    int canjeadosCount = 0;

                    Beneficios.Clear();
                    foreach (var beneficio in beneficiosFiltrados)
                    {
                        var beneficioSeleccionable = new BeneficioSeleccionable(beneficio);

                        // Verificar si el usuario ya canjeó este beneficio
                        var usuariosYaCanjeados = beneficio.UsuariosIDs ?? Array.Empty<string>();
                        bool yaCanjeado = usuariosYaCanjeados.Contains(usuarioLogueado.idApi);

                        if (yaCanjeado)
                        {
                            beneficioSeleccionable.YaCanjeado = true;
                            canjeadosCount++;
                        }

                        Beneficios.Add(beneficioSeleccionable);
                    }

                    BeneficiosYaCanjeados = canjeadosCount;
                }
            }

            CurrentAcceso = acceso;
            OnPropertyChanged(nameof(CurrentAcceso));
            OnPropertyChanged(nameof(IsPermittedAccess));
            OnPropertyChanged(nameof(IsNonPermittedAccess));
        }
    }

    private void OnBeneficioTapped(object sender, TappedEventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is BeneficioSeleccionable beneficio)
        {
            // No permitir interacción con beneficios ya canjeados
            if (beneficio.YaCanjeado)
            {
                DisplayAlert("Beneficio ya canjeado", "Este beneficio ya ha sido canjeado anteriormente.", "OK");
                return;
            }

            if (beneficio.IsSelected)
            {
                beneficio.IsSelected = false;
                BeneficiosSeleccionados.Remove(beneficio);
            }
            else if (BeneficiosSeleccionados.Count < BeneficiosDisponiblesParaSeleccionar)
            {
                beneficio.IsSelected = true;
                BeneficiosSeleccionados.Add(beneficio);
            }
            else
            {
                var mensaje = BeneficiosDisponiblesParaSeleccionar == 0
                    ? "Ya has canjeado el máximo de beneficios permitidos para este espacio."
                    : $"Solo puedes seleccionar {BeneficiosDisponiblesParaSeleccionar} beneficio(s) más. Ya tienes {BeneficiosYaCanjeados} canjeado(s).";

                DisplayAlert("Límite alcanzado", mensaje, "OK");
            }
        }
    }

    private async void OnCanjearBeneficiosClicked(object sender, EventArgs e)
    {
        if (BeneficiosSeleccionados.Count == 0)
        {
            await DisplayAlert("Error", "Debes seleccionar al menos un beneficio para canjear.", "OK");
            return;
        }

        if (BeneficiosSeleccionados.Count > BeneficiosDisponiblesParaSeleccionar)
        {
            await DisplayAlert("Error", $"Solo puedes canjear {BeneficiosDisponiblesParaSeleccionar} beneficio(s) más.", "OK");
            return;
        }

        var beneficiosNombres = string.Join(", ", BeneficiosSeleccionados.Select(b => b.Nombre));
        var result = await DisplayAlert("Confirmar canje",
            $"¿Estás seguro de que quieres canjear {BeneficiosSeleccionados.Count} beneficio(s)?\n\n{beneficiosNombres}",
            "Sí", "No");

        if (result)
        {
            try
            {
                var usuarioLogueado = await _db.GetLoggedUserAsync();
                if (usuarioLogueado == null)
                {
                    await DisplayAlert("Error", "No se encontró el usuario logueado.", "OK");
                    return;
                }

                // Validar que el usuario tenga idApi
                if (string.IsNullOrWhiteSpace(usuarioLogueado.idApi))
                {
                    await DisplayAlert("Error", "El usuario no tiene ID de API válido. Contacte al administrador.", "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[AccesoPerfilView] Iniciando canje de {BeneficiosSeleccionados.Count} beneficios para usuario: {usuarioLogueado.idApi}");

                // Usar el método del LocalDBService para cada beneficio
                var beneficiosCanjeadosExitosamente = new List<BeneficioSeleccionable>();

                foreach (var beneficioSeleccionable in BeneficiosSeleccionados.ToList())
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"[AccesoPerfilView] Canjeando beneficio: {beneficioSeleccionable.Beneficio.idApi}");

                        var beneficioActualizado = await _db.CanjearBeneficio(
                            usuarioLogueado.idApi,
                            beneficioSeleccionable.Beneficio.idApi);

                        if (beneficioActualizado != null)
                        {
                            // Actualizar el beneficio local con los datos actualizados
                            beneficioSeleccionable.Beneficio.UsuariosIDsJson = beneficioActualizado.UsuariosIDsJson;

                            // Marcar como canjeado en la UI
                            beneficioSeleccionable.YaCanjeado = true;
                            beneficioSeleccionable.IsSelected = false;

                            beneficiosCanjeadosExitosamente.Add(beneficioSeleccionable);

                            System.Diagnostics.Debug.WriteLine($"[AccesoPerfilView] ✅ Beneficio canjeado exitosamente: {beneficioSeleccionable.Beneficio.idApi}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[AccesoPerfilView] ❌ Error al canjear beneficio: {beneficioSeleccionable.Beneficio.idApi}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AccesoPerfilView] ❌ Excepción al canjear beneficio {beneficioSeleccionable.Beneficio.idApi}: {ex.Message}");
                    }
                }

                // Actualizar contador de canjeados solo con los exitosos
                if (beneficiosCanjeadosExitosamente.Count > 0)
                {
                    BeneficiosYaCanjeados += beneficiosCanjeadosExitosamente.Count;

                    // Limpiar selección
                    BeneficiosSeleccionados.Clear();

                    var mensaje = beneficiosCanjeadosExitosamente.Count == 1
                        ? "El beneficio ha sido canjeado exitosamente."
                        : $"{beneficiosCanjeadosExitosamente.Count} beneficios han sido canjeados exitosamente.";

                    await DisplayAlert("¡Éxito!", mensaje, "OK");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudieron canjear los beneficios seleccionados.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AccesoPerfilView] Error general al canjear beneficios: {ex}");
                await DisplayAlert("Error", "Ocurrió un error al procesar el canje.", "OK");
            }
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

// Helper class to handle selection state
public class BeneficioSeleccionable : INotifyPropertyChanged
{
    private bool _isSelected;
    private bool _yaCanjeado;

    public Beneficio Beneficio { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public bool YaCanjeado
    {
        get => _yaCanjeado;
        set
        {
            _yaCanjeado = value;
            OnPropertyChanged();
        }
    }

    // Expose Beneficio properties for binding
    public string? Nombre => Beneficio.Nombre;
    public string Tipo => Beneficio.Tipo;
    public string? Descripcion => Beneficio.Descripcion;
    public DateTime VigenciaInicio => Beneficio.VigenciaInicio;
    public DateTime VigenciaFin => Beneficio.VigenciaFin;

    public BeneficioSeleccionable(Beneficio beneficio)
    {
        Beneficio = beneficio;
        IsSelected = false;
        YaCanjeado = false;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}