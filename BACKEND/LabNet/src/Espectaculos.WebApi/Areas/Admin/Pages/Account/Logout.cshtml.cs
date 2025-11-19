// Areas/Admin/Pages/Account/Logout.cshtml.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Espectaculos.Backoffice.Areas.Admin.Pages.Account
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnPost()
        {
            // Remove JWT cookie
            Response.Cookies.Delete("espectaculos_session");

            // Optionally clear any other cookies / session info here

            return RedirectToPage("/Account/Login", new { area = "Admin" });
        }
    }
}