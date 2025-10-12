using Eterea_Parfums_Desktop.DTOs;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


public static class ApiImageUploader
{
    private static readonly HttpClient _http = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(120)
    };

    // 🔹 SUBIR IMAGEN
    public static async Task<UploadImageResult> UploadAsync(string localFilePath, string desiredFileName)
    {
        if (!File.Exists(localFilePath))
            throw new FileNotFoundException("No se encontró el archivo a subir.", localFilePath);

        var baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"]?.TrimEnd('/');
        var uploadRel = ConfigurationManager.AppSettings["ApiUploadPath"] ?? "/api/imagenes/upload";
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("ApiBaseUrl no está configurado.");
        var uploadUri = baseUrl + uploadRel;

        var apiKeyHdr = ConfigurationManager.AppSettings["ApiKeyHeaderName"];
        var apiKeyVal = ConfigurationManager.AppSettings["ApiKeyValue"];

        using (var form = new MultipartFormDataContent())
        using (var fileStream = File.OpenRead(localFilePath))
        {
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            var originalName = Path.GetFileName(localFilePath);
            form.Add(fileContent, "file", originalName);

            if (!string.IsNullOrWhiteSpace(desiredFileName))
                form.Add(new StringContent(desiredFileName), "fileName");

            if (!string.IsNullOrWhiteSpace(apiKeyHdr))
            {
                _http.DefaultRequestHeaders.Remove(apiKeyHdr);
                if (!string.IsNullOrWhiteSpace(apiKeyVal))
                    _http.DefaultRequestHeaders.Add(apiKeyHdr, apiKeyVal);
            }

            var resp = await _http.PostAsync(uploadUri, form);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"Error {resp.StatusCode} subiendo imagen: {body}");

            var result = JsonConvert.DeserializeObject<UploadImageResult>(body);
            if (result == null || string.IsNullOrWhiteSpace(result.url))
                throw new InvalidOperationException("La API no devolvió una URL válida.");

            return result;
        }
    }


    /// <summary>
    /// Elimina un archivo del servidor por nombre exacto (con extensión).
    /// Ej.: "banner-black-friday.jpg"
    /// </summary>
    public static async Task<bool> DeleteAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("fileName requerido (con extensión).", nameof(fileName));

        var baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"]?.TrimEnd('/');
        var uri = $"{baseUrl}/api/imagenes/{Uri.EscapeDataString(fileName)}";

        using (var req = new HttpRequestMessage(HttpMethod.Delete, uri))
        {
            // (opcional) API key
            var apiKeyHdr = ConfigurationManager.AppSettings["ApiKeyHeaderName"];
            var apiKeyVal = ConfigurationManager.AppSettings["ApiKeyValue"];
            if (!string.IsNullOrWhiteSpace(apiKeyHdr) && !string.IsNullOrWhiteSpace(apiKeyVal))
                req.Headers.TryAddWithoutValidation(apiKeyHdr, apiKeyVal);

            using (var resp = await _http.SendAsync(req))
            {
                // 204 NoContent = ok; 404 NotFound = ya no existe (lo consideramos ok)
                if ((int)resp.StatusCode == 204 || (int)resp.StatusCode == 404 || resp.IsSuccessStatusCode)
                    return true;

                var body = await resp.Content.ReadAsStringAsync();
                throw new Exception($"DeleteAsync falló ({(int)resp.StatusCode}): {body}");
            }
        }
    }

    /// <summary>
    /// Conveniencia: elimina a partir de la URL pública.
    /// Extrae el fileName de la URL (lo que sigue a '/imagenes/').
    /// </summary>
    public static async Task<bool> DeleteByUrlAsync(string publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl))
            throw new ArgumentException("publicUrl requerido.", nameof(publicUrl));

        // intenta detectar el segmento final (fileName)
        var uri = new Uri(publicUrl, UriKind.Absolute);
        var fileName = Path.GetFileName(uri.LocalPath); // ej: "banner-black-friday.jpg"

        if (string.IsNullOrWhiteSpace(fileName) || fileName == "/" || fileName.Contains("?"))
            throw new Exception("No se pudo inferir el nombre de archivo desde la URL.");

        return await DeleteAsync(fileName);
    }



    // 🔹 BAJAR IMAGEN (para usar en tu DataGridView)
    public static async Task<Image> DownloadImageAsync(string url)
        {
            try
            {
                // opcional: setear User-Agent
                if (!_http.DefaultRequestHeaders.UserAgent.Any())
                    _http.DefaultRequestHeaders.UserAgent.ParseAdd("EtereaDesktop/1.0");

                using (var resp = await _http.GetAsync(url))
                {
                    if (!resp.IsSuccessStatusCode)
                    {
                        // Logueá por qué falló
                        var body = await resp.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"GET {url} -> {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
                        return Eterea_Parfums_Desktop.Properties.Resources.sinImagen;
                    }

                    var bytes = await resp.Content.ReadAsByteArrayAsync();
                    using (var ms = new MemoryStream(bytes))
                    {
                        return Image.FromStream(ms);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Excepción GET {url}: {ex}");
                return Eterea_Parfums_Desktop.Properties.Resources.sinImagen;
            }
        }

}

