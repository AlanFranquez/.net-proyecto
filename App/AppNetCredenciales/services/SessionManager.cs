using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppNetCredenciales.models;
using Microsoft.Maui.Storage;

namespace AppNetCredenciales.services
{
    public static class SessionManager
    {

        public static async Task SaveUserAsync(int id, string email)
        {


            await SecureStorage.SetAsync("id", id.ToString());
            await SecureStorage.SetAsync("email", email);
        }

        public static async Task<int> GetUserRoleIdAsync()
        {
            var rolId = await SecureStorage.GetAsync("rol_id");
            return int.Parse(rolId);
        }

        public static async Task SaveUserRoleAsync(int idRole)
        {
            await SecureStorage.SetAsync("rol_id", idRole.ToString());
        }



        public static async Task<int> GetUserIdAsync()
        {
            var id = await SecureStorage.GetAsync("id");

            return int.Parse(id);
        }


        public static async Task<string> GetUserEmailAsync()
        {
            var email = await SecureStorage.GetAsync("email");
            return email.ToString();
        }

        public static async Task<bool> IsLoggedAsync()
        {
            var id = await SecureStorage.GetAsync("id");
            return !string.IsNullOrEmpty(id);
        }

        public static void Logout()
        {
            SecureStorage.Remove("id");
            SecureStorage.Remove("email");
        }
    }
}
