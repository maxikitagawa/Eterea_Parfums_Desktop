using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using System;
using System.IO;
using File = Google.Apis.Drive.v3.Data.File;

namespace Eterea_Parfums_Desktop.Helpers
{
    public class GoogleDriveUploader
    {
        private static DriveService service;

        public GoogleDriveUploader()
        {
            InicializarServicio();
        }

        private void InicializarServicio()
        {
            string credPath = "credentials.json"; // Tenerlo junto al .exe, sino especificar la ruta absoluta correcta

            GoogleCredential credential;
            using (var stream = new FileStream(credPath, FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(DriveService.ScopeConstants.Drive);
            }

            service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Eterea Imagenes"
            });
        }

        public string SubirImagen(string rutaArchivo, string nombreArchivo, string idCarpetaDrive = null)
        {
            try
            {
                if (!System.IO.File.Exists(rutaArchivo))
                    throw new FileNotFoundException("Archivo no encontrado: " + rutaArchivo);

                Console.WriteLine("📂 Archivo a subir: " + rutaArchivo);
                Console.WriteLine("📁 Nombre en Drive: " + nombreArchivo);
                Console.WriteLine("📂 Carpeta ID Drive: " + idCarpetaDrive);

                var archivoMetadata = new File()
                {
                    Name = nombreArchivo
                    // Parents = idCarpetaDrive != null ? new[] { idCarpetaDrive } : null // COMENTADO para probar subida a Mi unidad
                };

                using (var stream = new FileStream(rutaArchivo, FileMode.Open))
                {
                    var request = service.Files.Create(archivoMetadata, stream, "image/jpeg");
                    request.Fields = "id";
                    var progreso = request.Upload();

                    Console.WriteLine("📅 Estado de subida: " + progreso.Status);

                    if (progreso.Status == UploadStatus.Failed || request.ResponseBody == null)
                        throw new Exception("La subida a Google Drive falló. Verificá la conexión, permisos o credenciales.");

                    var permiso = new Google.Apis.Drive.v3.Data.Permission()
                    {
                        Role = "reader",
                        Type = "anyone"
                    };

                    service.Permissions.Create(permiso, request.ResponseBody.Id).Execute();

                    Console.WriteLine("📄 Archivo subido correctamente. ID: " + request.ResponseBody.Id);
                    return $"https://drive.google.com/uc?id={request.ResponseBody.Id}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error durante la subida: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw new Exception("La subida a Google Drive falló. Detalle: " + ex.Message + "\n" + ex.StackTrace);
            }
        }
    }
}
