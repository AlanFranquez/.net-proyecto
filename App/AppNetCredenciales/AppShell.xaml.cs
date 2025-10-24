using AppNetCredenciales.Views;

namespace AppNetCredenciales
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("register", typeof(AppNetCredenciales.Views.RegisterView));
            Routing.RegisterRoute("login", typeof(AppNetCredenciales.Views.LoginView));
            Routing.RegisterRoute("evento", typeof(AppNetCredenciales.Views.EventoView));


        }

        
    }
}
