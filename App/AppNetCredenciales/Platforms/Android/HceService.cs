using Android.App;
using Android.Content;
using Android.Nfc.CardEmulators;
using Android.OS;
using System.Text;
using AppNetCredenciales.Services;
using Microsoft.Extensions.DependencyInjection;
using Android.Runtime;

namespace AppNetCredenciales.Platforms.Android
{
    /// <summary>
    /// Servicio HCE que emula una tarjeta NFC contactless
    /// </summary>
    [Service(Exported = true, Enabled = true, Permission = "android.permission.BIND_NFC_SERVICE")]
    [IntentFilter(new[] { "android.nfc.cardemulation.action.HOST_APDU_SERVICE" })]
    [MetaData("android.nfc.cardemulation.host_apdu_service", Resource = "@xml/apduservice")]
    [Register("appnetcredenciales.platforms.android.HceService")]
    public class HceService : HostApduService
    {
        // AID (Application ID) - Debe coincidir con el que está en apduservice.xml
        private static readonly byte[] SELECT_APDU_HEADER = { 0x00, 0xA4, 0x04, 0x00 };
        private static readonly byte[] AID = { 0xF0, 0x39, 0x41, 0x48, 0x14, 0x81, 0x00 };
        
        private bool _isSelected = false;
        private string? _credencialId;

        public HceService()
        {
            System.Diagnostics.Debug.WriteLine("[HceService] ???????????????????????????????????????");
            System.Diagnostics.Debug.WriteLine("[HceService] Constructor llamado");
            System.Diagnostics.Debug.WriteLine("[HceService] ???????????????????????????????????????");
        }

        public override void OnCreate()
        {
            base.OnCreate();
            System.Diagnostics.Debug.WriteLine("[HceService] ???????????????????????????????????????");
            System.Diagnostics.Debug.WriteLine("[HceService] ? OnCreate - Servicio HCE INICIADO");
            System.Diagnostics.Debug.WriteLine("[HceService] ???????????????????????????????????????");
        }

        /// <summary>
        /// Llamado cuando se recibe un comando APDU del lector NFC
        /// </summary>
        public override byte[]? ProcessCommandApdu(byte[]? commandApdu, Bundle? extras)
        {
            try
            {
                if (commandApdu == null || commandApdu.Length < 4)
                {
                    System.Diagnostics.Debug.WriteLine("[HceService] APDU inválido o vacío");
                    return GetErrorResponse();
                }

                System.Diagnostics.Debug.WriteLine($"[HceService] APDU recibido: {BitConverter.ToString(commandApdu)}");

                // Verificar si es un comando SELECT
                if (IsSelectAidApdu(commandApdu))
                {
                    System.Diagnostics.Debug.WriteLine("[HceService] SELECT AID detectado");
                    _isSelected = true;
                    return GetSuccessResponse();
                }

                // Si ya está seleccionado, enviar el ID criptográfico
                if (_isSelected)
                {
                    System.Diagnostics.Debug.WriteLine("[HceService] Enviando credencial");
                    return SendCredentialData();
                }

                System.Diagnostics.Debug.WriteLine("[HceService] Comando no reconocido");
                return GetErrorResponse();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HceService] Error en ProcessCommandApdu: {ex.Message}");
                return GetErrorResponse();
            }
        }

        /// <summary>
        /// Llamado cuando se pierde la conexión con el lector
        /// </summary>
        public override void OnDeactivated(DeactivationReason reason)
        {
            System.Diagnostics.Debug.WriteLine($"[HceService] Desactivado - Razón: {reason}");
            _isSelected = false;
            _credencialId = null;
        }

        /// <summary>
        /// Verifica si el APDU es un comando SELECT AID
        /// </summary>
        private bool IsSelectAidApdu(byte[] apdu)
        {
            if (apdu.Length < SELECT_APDU_HEADER.Length + AID.Length)
                return false;

            // Verificar header SELECT
            for (int i = 0; i < SELECT_APDU_HEADER.Length; i++)
            {
                if (apdu[i] != SELECT_APDU_HEADER[i])
                    return false;
            }

            // Verificar AID
            int aidLength = apdu[4];
            if (aidLength != AID.Length)
                return false;

            for (int i = 0; i < AID.Length; i++)
            {
                if (apdu[5 + i] != AID[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Envía los datos de la credencial al lector
        /// </summary>
        private byte[] SendCredentialData()
        {
            try
            {
                // Obtener la credencial activa del servicio
                var hceManager = App.Services?.GetService<HceManager>();
                var credencialId = hceManager?.GetActiveCredentialId();

                if (string.IsNullOrEmpty(credencialId))
                {
                    System.Diagnostics.Debug.WriteLine("[HceService] No hay credencial activa");
                    return GetErrorResponse();
                }

                System.Diagnostics.Debug.WriteLine($"[HceService] Enviando credencial: {credencialId}");

                // Convertir el ID a bytes
                byte[] data = Encoding.UTF8.GetBytes(credencialId);
                
                // Agregar status word de éxito (0x9000)
                byte[] response = new byte[data.Length + 2];
                Array.Copy(data, 0, response, 0, data.Length);
                response[data.Length] = 0x90;
                response[data.Length + 1] = 0x00;

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HceService] Error enviando datos: {ex.Message}");
                return GetErrorResponse();
            }
        }

        /// <summary>
        /// Respuesta de éxito (solo status word)
        /// </summary>
        private byte[] GetSuccessResponse()
        {
            return new byte[] { 0x90, 0x00 };
        }

        /// <summary>
        /// Respuesta de error
        /// </summary>
        private byte[] GetErrorResponse()
        {
            return new byte[] { 0x6F, 0x00 }; // SW_UNKNOWN
        }
    }
}
