using AppNetCredenciales.Data;
using System.Threading.Tasks;

namespace AppNetCredenciales
{
    public partial class MainPage : ContentPage
    {
        private readonly LocalDBService _dbService;
        private int _editCustomerId;
        public MainPage(LocalDBService dbService)
        {
            InitializeComponent();
            _dbService = dbService;
            Task.Run(async () => listView.ItemsSource = await _dbService.GetUsuariosAsync());
        }
       

        private async void saveButton_Clicked(object sender, EventArgs e)
        {
            await _dbService.SaveUsuarioAsync(new models.Usuario
            {
                Nombre = nameEntryField.Text,
                Apellido = lastNameEntryField.Text,
                email = emailEntryField.Text
            });

            listView.ItemsSource = await _dbService.GetUsuariosAsync();
        }


        private async void listView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var usuario = (models.Usuario)e.Item;
            var action = await DisplayActionSheet("Action", "Cancel", null, "Edit", "Delete");

            switch (action)
            {
                case "Edit":
                    nameEntryField.Text = usuario.Nombre;
                    lastNameEntryField.Text = usuario.Apellido;
                    emailEntryField.Text = usuario.email;
                    _editCustomerId = usuario.Id;
                    break;
                case "Delete":
                    await _dbService.DeleteUsuarioAsync(usuario);
                    listView.ItemsSource = await _dbService.GetUsuariosAsync();
                    break;
            }
        
        }
    }

}
