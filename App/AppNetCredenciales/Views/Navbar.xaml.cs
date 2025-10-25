using System.Windows.Input;

namespace AppNetCredenciales.Views;

public partial class Navbar : ContentView
{
    public ICommand NavigateCommand { get; }

    public Navbar()
    {
        InitializeComponent();

        NavigateCommand = new Command<string>(async (destino) =>
        {
            switch (destino)
            {
                case "eventos":
                    await Shell.Current.GoToAsync("espacio");
                    break;
                case "credencial":
                    await Shell.Current.GoToAsync("credencial");
                    break;
            }
        });

        BindingContext = this;
    }
}