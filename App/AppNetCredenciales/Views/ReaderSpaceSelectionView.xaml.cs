using AppNetCredenciales.ViewModel;
using AppNetCredenciales.Data;
using Microsoft.Extensions.DependencyInjection;

namespace AppNetCredenciales.Views
{
    public partial class ReaderSpaceSelectionView : ContentPage
    {
        public ReaderSpaceSelectionView()
        {
            InitializeComponent();
            
            var db = App.Services?.GetRequiredService<LocalDBService>();
            if (db != null)
            {
                BindingContext = new ReaderSpaceSelectionViewModel(db);
            }
        }
    }
}
