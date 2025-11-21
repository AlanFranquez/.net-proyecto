using AppNetCredenciales.Data;
using AppNetCredenciales.ViewModel;
using Microsoft.Maui.Controls;

namespace AppNetCredenciales.Views
{
    public class NFCEspacioSelectionView : ContentPage
    {
        private readonly NFCEspacioSelectionViewModel _viewModel;

        public NFCEspacioSelectionView(LocalDBService db)
        {
            Title = "Seleccionar Espacio";
            
            _viewModel = new NFCEspacioSelectionViewModel(db);
            BindingContext = _viewModel;

            // Crear el indicador de actividad
            var activityIndicator = new ActivityIndicator
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, nameof(NFCEspacioSelectionViewModel.IsLoading));
            activityIndicator.SetBinding(IsVisibleProperty, nameof(NFCEspacioSelectionViewModel.IsLoading));

            // Crear la lista de espacios
            var collectionView = new CollectionView
            {
                SelectionMode = SelectionMode.None,
                Margin = new Thickness(0, 6)
            };
            collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(NFCEspacioSelectionViewModel.Espacios));
            
            // Empty view cuando no hay espacios
            var emptyView = new VerticalStackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                Spacing = 12
            };
            
            emptyView.Children.Add(new Label
            {
                Text = "??",
                FontSize = 48,
                HorizontalOptions = LayoutOptions.Center
            });
            
            emptyView.Children.Add(new Label
            {
                Text = "No hay espacios disponibles",
                FontSize = 20,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Color.FromArgb("#666")
            });
            
            emptyView.Children.Add(new Label
            {
                Text = "Los espacios con lector NFC aparecerán aquí",
                FontSize = 14,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Color.FromArgb("#999")
            });
            
            collectionView.EmptyView = emptyView;
            
            // Plantilla de datos con botón
            collectionView.ItemTemplate = new DataTemplate(() =>
            {
                var outerFrame = new Frame
                {
                    Margin = new Thickness(12, 6),
                    Padding = 15,
                    CornerRadius = 10,
                    HasShadow = false,
                    BorderColor = Color.FromArgb("#ddd"),
                    BackgroundColor = Colors.White
                };

                var mainGrid = new Grid
                {
                    RowDefinitions = new RowDefinitionCollection
                    {
                        new RowDefinition { Height = GridLength.Auto },
                        new RowDefinition { Height = GridLength.Auto }
                    },
                    RowSpacing = 12
                };

                // Fila 0: Información del espacio
                var infoGrid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = GridLength.Auto },
                        new ColumnDefinition { Width = GridLength.Star }
                    },
                    VerticalOptions = LayoutOptions.Center
                };

                // Icono del espacio
                var iconFrame = new Frame
                {
                    WidthRequest = 44,
                    HeightRequest = 44,
                    CornerRadius = 22,
                    BackgroundColor = Color.FromArgb("#E3F2FD"),
                    Padding = 0,
                    VerticalOptions = LayoutOptions.Center
                };
                
                var iconLabel = new Label
                {
                    Text = "??",
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    FontSize = 20
                };
                iconLabel.SetBinding(Label.TextProperty, "TipoIcon");
                
                iconFrame.Content = iconLabel;
                Grid.SetColumn(iconFrame, 0);
                infoGrid.Children.Add(iconFrame);

                // Información del espacio
                var infoStack = new VerticalStackLayout
                {
                    Padding = new Thickness(12, 0),
                    VerticalOptions = LayoutOptions.Center,
                    Spacing = 2
                };

                var nombreLabel = new Label
                {
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#333"),
                    LineBreakMode = LineBreakMode.TailTruncation
                };
                nombreLabel.SetBinding(Label.TextProperty, "Nombre");
                
                var descripcionLabel = new Label
                {
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    LineBreakMode = LineBreakMode.TailTruncation
                };
                descripcionLabel.SetBinding(Label.TextProperty, "Descripcion");
                
                var tipoLabel = new Label
                {
                    FontSize = 12,
                    TextColor = Color.FromArgb("#666"),
                    LineBreakMode = LineBreakMode.TailTruncation
                };
                tipoLabel.SetBinding(Label.TextProperty, "TipoTexto");

                infoStack.Children.Add(nombreLabel);
                infoStack.Children.Add(descripcionLabel);
                infoStack.Children.Add(tipoLabel);
                
                Grid.SetColumn(infoStack, 1);
                infoGrid.Children.Add(infoStack);

                Grid.SetRow(infoGrid, 0);
                mainGrid.Children.Add(infoGrid);

                // Fila 1: Botón de selección
                var selectButton = new Button
                {
                    Text = "?? Activar Lector en este Espacio",
                    FontSize = 16,
                    BackgroundColor = Color.FromArgb("#4CAF50"),
                    TextColor = Colors.White,
                    CornerRadius = 8,
                    HeightRequest = 45,
                    Margin = new Thickness(0, 4, 0, 0),
                    FontAttributes = FontAttributes.Bold
                };
                
                selectButton.SetBinding(Button.CommandProperty, new Binding("SelectEspacioCommand", source: _viewModel));
                selectButton.SetBinding(Button.CommandParameterProperty, ".");
                
                Grid.SetRow(selectButton, 1);
                mainGrid.Children.Add(selectButton);

                outerFrame.Content = mainGrid;
                
                return outerFrame;
            });

            // Crear el grid principal
            var mainGrid = new Grid
            {
                Padding = new Thickness(12),
                BackgroundColor = Colors.White
            };
            mainGrid.Children.Add(collectionView);
            mainGrid.Children.Add(activityIndicator);

            Content = mainGrid;

            Loaded += OnPageLoaded;
        }

        private async void OnPageLoaded(object sender, EventArgs e)
        {
            await _viewModel.LoadEspaciosAsync();
        }
    }
}
