using AppNetCredenciales.Data;
using AppNetCredenciales.Services;
using AppNetCredenciales.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using System;
using Frame = Microsoft.Maui.Controls.Frame;

namespace AppNetCredenciales.Views
{
    public class NFCChipManagementView : ContentPage
    {
        private readonly NFCChipManagementViewModel _viewModel;

        public NFCChipManagementView()
        {
            // Obtener servicios
            var db = App.Services?.GetService<LocalDBService>();
            var nfcService = App.Services?.GetService<NFCService>();

            if (db == null || nfcService == null)
            {
                DisplayAlert("Error", "No se pudieron inicializar los servicios necesarios", "OK");
                return;
            }

            _viewModel = new NFCChipManagementViewModel(db, nfcService);
            BindingContext = _viewModel;

            Title = "Gestión de Chips NFC";
            BackgroundColor = Colors.White;

            BuildUI();

            Loaded += OnPageLoaded;
        }

        private void BuildUI()
        {
            var mainGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },  // Header
                    new RowDefinition { Height = GridLength.Star },  // Lista
                    new RowDefinition { Height = GridLength.Auto }   // Footer
                },
                Padding = new Thickness(16),
                RowSpacing = 16
            };

            // === HEADER ===
            var headerFrame = new Frame
            {
                BackgroundColor = Color.FromArgb("#2196F3"),
                CornerRadius = 12,
                HasShadow = false,
                Padding = new Thickness(20, 16)
            };

            var headerStack = new VerticalStackLayout { Spacing = 8 };

            var titleLabel = new Label
            {
                Text = "📱 Escritura de Credenciales en Chips NFC",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center
            };

            var statusLabel = new Label
            {
                FontSize = 14,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };
            statusLabel.SetBinding(Label.TextProperty, nameof(NFCChipManagementViewModel.StatusMessage));

            headerStack.Children.Add(titleLabel);
            headerStack.Children.Add(statusLabel);
            headerFrame.Content = headerStack;

            Grid.SetRow(headerFrame, 0);
            mainGrid.Children.Add(headerFrame);

            // === LOADING INDICATOR ===
            var loadingIndicator = new ActivityIndicator
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Color = Color.FromArgb("#2196F3")
            };
            loadingIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(NFCChipManagementViewModel.IsLoading));
            loadingIndicator.SetBinding(IsVisibleProperty, nameof(NFCChipManagementViewModel.IsLoading));

            Grid.SetRow(loadingIndicator, 1);
            mainGrid.Children.Add(loadingIndicator);

            // === LISTA DE CREDENCIALES ===
            var collectionView = new CollectionView
            {
                SelectionMode = SelectionMode.None,
                VerticalOptions = LayoutOptions.Fill
            };
            collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(NFCChipManagementViewModel.Credenciales));

            // Empty View
            var emptyView = CreateEmptyView();
            collectionView.EmptyView = emptyView;

            // Item Template
            collectionView.ItemTemplate = new DataTemplate(() => CreateCredencialItem());

            Grid.SetRow(collectionView, 1);
            mainGrid.Children.Add(collectionView);

            // === FOOTER ===
            var footerStack = new HorizontalStackLayout
            {
                Spacing = 12,
                HorizontalOptions = LayoutOptions.Fill
            };

            var refreshButton = new Button
            {
                Text = "🔄 Actualizar Lista",
                BackgroundColor = Color.FromArgb("#4CAF50"),
                TextColor = Colors.White,
                CornerRadius = 10,
                HeightRequest = 50,
                HorizontalOptions = LayoutOptions.Fill
            };
            refreshButton.SetBinding(Button.CommandProperty, nameof(NFCChipManagementViewModel.RefreshCommand));

            footerStack.Children.Add(refreshButton);

            Grid.SetRow(footerStack, 2);
            mainGrid.Children.Add(footerStack);

            Content = mainGrid;
        }

        private View CreateEmptyView()
        {
            var stack = new VerticalStackLayout
            {
                Spacing = 16,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Padding = new Thickness(40)
            };

            stack.Children.Add(new Label
            {
                Text = "📱",
                FontSize = 64,
                HorizontalOptions = LayoutOptions.Center
            });

            stack.Children.Add(new Label
            {
                Text = "No hay credenciales disponibles",
                FontSize = 20,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Color.FromArgb("#666")
            });

            stack.Children.Add(new Label
            {
                Text = "Sincronice con el servidor para ver las credenciales",
                FontSize = 14,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Color.FromArgb("#999")
            });

            return stack;
        }

        private View CreateCredencialItem()
        {
            var frame = new Frame
            {
                CornerRadius = 12,
                HasShadow = false,
                BorderColor = Color.FromArgb("#ddd"),
                Padding = 0,
                Margin = new Thickness(0, 8)
            };
            frame.SetBinding(VisualElement.BackgroundColorProperty, nameof(CredencialItemViewModel.BackgroundColor));

            var mainGrid = new Grid
            {
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                Padding = new Thickness(16),
                RowSpacing = 12
            };

            // === ROW 0: INFORMACIÓN ===
            var infoGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                ColumnSpacing = 12
            };

            // Icono de estado
            var estadoFrame = new Frame
            {
                WidthRequest = 50,
                HeightRequest = 50,
                CornerRadius = 25,
                Padding = 0,
                VerticalOptions = LayoutOptions.Center,
                HasShadow = false
            };
            estadoFrame.SetBinding(VisualElement.BackgroundColorProperty, nameof(CredencialItemViewModel.EstadoColor));

            var iconoLabel = new Label
            {
                FontSize = 24,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
            iconoLabel.SetBinding(Label.TextProperty, nameof(CredencialItemViewModel.IconoEstado));
            estadoFrame.Content = iconoLabel;

            Grid.SetColumn(estadoFrame, 0);
            infoGrid.Children.Add(estadoFrame);

            // Información del usuario
            var userStack = new VerticalStackLayout
            {
                Spacing = 4,
                VerticalOptions = LayoutOptions.Center
            };

            var nombreLabel = new Label
            {
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#333")
            };
            nombreLabel.SetBinding(Label.TextProperty, nameof(CredencialItemViewModel.NombreUsuario));

            var documentoLabel = new Label
            {
                FontSize = 14,
                TextColor = Color.FromArgb("#666")
            };
            documentoLabel.SetBinding(Label.TextProperty, new Binding
            {
                Path = nameof(CredencialItemViewModel.DocumentoUsuario),
                StringFormat = "Doc: {0}"
            });

            var idCriptoLabel = new Label
            {
                FontSize = 12,
                TextColor = Color.FromArgb("#999"),
                LineBreakMode = LineBreakMode.TailTruncation
            };
            idCriptoLabel.SetBinding(Label.TextProperty, new Binding
            {
                Path = nameof(CredencialItemViewModel.IdCriptografico),
                StringFormat = "ID: {0}"
            });

            var estadoTextLabel = new Label
            {
                FontSize = 12,
                FontAttributes = FontAttributes.Bold
            };
            estadoTextLabel.SetBinding(Label.TextProperty, nameof(CredencialItemViewModel.EstadoTexto));
            estadoTextLabel.SetBinding(Label.TextColorProperty, nameof(CredencialItemViewModel.EstadoColor));

            userStack.Children.Add(nombreLabel);
            userStack.Children.Add(documentoLabel);
            userStack.Children.Add(idCriptoLabel);
            userStack.Children.Add(estadoTextLabel);

            Grid.SetColumn(userStack, 1);
            infoGrid.Children.Add(userStack);

            // Status Icon
            var statusLabel = new Label
            {
                FontSize = 32,
                VerticalOptions = LayoutOptions.Center
            };
            statusLabel.SetBinding(Label.TextProperty, nameof(CredencialItemViewModel.StatusIcon));
            statusLabel.SetBinding(IsVisibleProperty, new Binding
            {
                Path = nameof(CredencialItemViewModel.StatusIcon),
                Converter = new StringNotEmptyConverter()
            });

            Grid.SetColumn(statusLabel, 2);
            infoGrid.Children.Add(statusLabel);

            Grid.SetRow(infoGrid, 0);
            mainGrid.Children.Add(infoGrid);

            // === ROW 1: BOTÓN ===
            var writeButton = new Button
            {
                Text = "📝 Escribir en Chip NFC",
                BackgroundColor = Color.FromArgb("#FF9800"),
                TextColor = Colors.White,
                CornerRadius = 8,
                HeightRequest = 45,
                FontSize = 15,
                FontAttributes = FontAttributes.Bold
            };
            writeButton.SetBinding(Button.CommandProperty, nameof(CredencialItemViewModel.EscribirCommand));

            Grid.SetRow(writeButton, 1);
            mainGrid.Children.Add(writeButton);

            frame.Content = mainGrid;
            return frame;
        }

        private async void OnPageLoaded(object sender, EventArgs e)
        {
            await _viewModel.LoadCredencialesAsync();
        }
    }

    // Converter helper
    public class StringNotEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
