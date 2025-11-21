using AppNetCredenciales.Services;
using AppNetCredenciales.ViewModel;

namespace AppNetCredenciales.Views
{
    public partial class NFCReaderActiveView : ContentPage
    {
        private readonly NFCReaderActiveViewModel _viewModel;

        public NFCReaderActiveView(NFCService nfcService, IEventosService eventosService)
        {
            InitializeComponent();

            _viewModel = new NFCReaderActiveViewModel(nfcService, eventosService);
            BindingContext = _viewModel;

            Loaded += OnPageLoaded;
            Unloaded += OnPageUnloaded;
        }

        private async void OnPageLoaded(object sender, EventArgs e)
        {
            await _viewModel.IniciarLectorAsync();
        }

        private void OnPageUnloaded(object sender, EventArgs e)
        {
            _viewModel.DetenerLector();
        }

        private async void OnDetenerClicked(object sender, EventArgs e)
        {
            _viewModel.DetenerLector();
            await Shell.Current.GoToAsync("..");
        }

        protected override bool OnBackButtonPressed()
        {
            _viewModel.DetenerLector();
            return base.OnBackButtonPressed();
        }
    }
}
