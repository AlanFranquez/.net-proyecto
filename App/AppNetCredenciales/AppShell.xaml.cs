using AppNetCredenciales.Views;

namespace AppNetCredenciales
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("register", typeof(AppNetCredenciales.Views.RegisterView));
            Routing.RegisterRoute("espacio", typeof(AppNetCredenciales.Views.EspacioView));
            Routing.RegisterRoute("credencial", typeof(AppNetCredenciales.Views.CredencialView));
            Routing.RegisterRoute("espacioPerfil", typeof(AppNetCredenciales.Views.EspacioPerfilView));

        }

        
    }
}
