using Eterea_Parfums_Desktop.DTOs;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public static class ApiImageUploader
{
    // HttpClient compartido (recomendado). Las cabeceras sensibles se ponen por-request.
    private static readonly HttpClient _http = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(120)
    };

    // =========================
    // Helpers de configuración
    // =========================
    private static string GetBaseUrl()
    {
        var baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"]?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("ApiBaseUrl no está configurado.");
        return baseUrl;
    }

    private static string GetUploadPath() => ConfigurationManager.AppSettings["ApiUploadPath"] ?? "/api/imagenes/upload";
    private static string GetReplacePath() => ConfigurationManager.AppSettings["ApiReplacePath"] ?? "/api/imagenes/replace";
    private static string GetRenamePath() => ConfigurationManager.AppSettings["ApiRenamePath"] ?? "/api/imagenes/rename";

    private static void ApplyApiKey(HttpRequestMessage req)
    {
        var apiKeyHdr = ConfigurationManager.AppSettings["ApiKeyHeaderName"];
        var apiKeyVal = ConfigurationManager.AppSettings["ApiKeyValue"];
        if (!string.IsNullOrWhiteSpace(apiKeyHdr) && !string.IsNullOrWhiteSpace(apiKeyVal))
            req.Headers.TryAddWithoutValidation(apiKeyHdr, apiKeyVal);
    }

    private static string ExtractFileNameFromUrl(string publicUrl)
    {
        var uri = new Uri(publicUrl, UriKind.Absolute);
        return Path.GetFileName(uri.LocalPath);
    }

    // =========================
    // SUBIR (simple)
    // POST /api/imagenes/upload
    // multipart:
    //   file   = (archivo)
    //   newName (opcional) => nombre final deseado
    // =========================
    public static async Task<UploadImageResult> UploadAsync(string localFilePath, string desiredFileName)
    {
        if (!File.Exists(localFilePath))
            throw new FileNotFoundException("No se encontró el archivo a subir.", localFilePath);

        var uploadUri = GetBaseUrl() + GetUploadPath();

        using (var form = new MultipartFormDataContent())
        using (var fileStream = File.OpenRead(localFilePath))
        {
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var originalName = Path.GetFileName(localFilePath);
            form.Add(fileContent, "file", originalName);

            // Importante: la API espera "newName" (no "fileName")
            if (!string.IsNullOrWhiteSpace(desiredFileName))
                form.Add(new StringContent(desiredFileName), "newName");

            using (var req = new HttpRequestMessage(HttpMethod.Post, uploadUri) { Content = form })
            {
                ApplyApiKey(req);

                var resp = await _http.SendAsync(req);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    throw new InvalidOperationException($"Error {resp.StatusCode} subiendo imagen: {body}");

                var result = JsonConvert.DeserializeObject<UploadImageResult>(body);
                if (result == null || string.IsNullOrWhiteSpace(result.url))
                    throw new InvalidOperationException("La API no devolvió una URL válida.");

                return result;
            }
        }
    }

    // =========================
    // REEMPLAZAR (sube nueva y borra vieja)
    // POST /api/imagenes/replace
    // multipart:
    //   file    = (archivo nuevo)  [requerido]
    //   oldName = nombre viejo     [opcional, si se envía y ≠ newName => se borra]
    //   newName = nombre final     [opcional; si no, se genera GUID.ext]
    // =========================
    public static async Task<UploadImageResult> ReplaceAsync(string localFilePath, string newNameOnServer, string oldNameOnServerOrNull)
    {
        if (!File.Exists(localFilePath))
            throw new FileNotFoundException("No se encontró el archivo a subir.", localFilePath);

        var replaceUri = GetBaseUrl() + GetReplacePath();

        using (var form = new MultipartFormDataContent())
        using (var fileStream = File.OpenRead(localFilePath))
        {
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var originalName = Path.GetFileName(localFilePath);
            form.Add(fileContent, "file", originalName);

            if (!string.IsNullOrWhiteSpace(oldNameOnServerOrNull))
                form.Add(new StringContent(oldNameOnServerOrNull), "oldName");

            if (!string.IsNullOrWhiteSpace(newNameOnServer))
                form.Add(new StringContent(newNameOnServer), "newName");

            using (var req = new HttpRequestMessage(HttpMethod.Post, replaceUri) { Content = form })
            {
                ApplyApiKey(req);

                var resp = await _http.SendAsync(req);
                var body = await resp.Content.ReadAsStringAsync();

                if (!resp.IsSuccessStatusCode)
                    throw new InvalidOperationException($"Error {resp.StatusCode} reemplazando imagen: {body}");

                var result = JsonConvert.DeserializeObject<UploadImageResult>(body);
                if (result == null || string.IsNullOrWhiteSpace(result.url))
                    throw new InvalidOperationException("La API no devolvió una URL válida.");

                return result;
            }
        }
    }

    // =========================
    // RENOMBRAR (sin subir archivo)
    // POST /api/imagenes/rename?oldName=...&newName=...
    // =========================
    public static async Task<(string fileName, string url)> RenameAsync(string oldNameOnServer, string newNameOnServer)
    {
        if (string.IsNullOrWhiteSpace(oldNameOnServer)) throw new ArgumentException("oldName requerido.", nameof(oldNameOnServer));
        if (string.IsNullOrWhiteSpace(newNameOnServer)) throw new ArgumentException("newName requerido.", nameof(newNameOnServer));

        var baseUrl = GetBaseUrl();
        var renameUrl = $"{baseUrl}{GetRenamePath()}?oldName={Uri.EscapeDataString(oldNameOnServer)}&newName={Uri.EscapeDataString(newNameOnServer)}";

        using (var req = new HttpRequestMessage(HttpMethod.Post, renameUrl))
        {
            ApplyApiKey(req);

            var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"Error {resp.StatusCode} renombrando imagen: {body}");

            // La API devuelve { oldName, newName, url, api }
            dynamic r = JsonConvert.DeserializeObject(body);
            string returnedName = r?.newName ?? newNameOnServer;
            string returnedUrl = r?.url;

            if (string.IsNullOrWhiteSpace(returnedUrl))
                returnedUrl = $"{baseUrl}/Uploads/{returnedName}";

            return (returnedName, returnedUrl);
        }
    }

    // =========================
    // ELIMINAR por nombre
    // DELETE /api/imagenes/{fileName}
    // =========================
    public static async Task<bool> DeleteAsync(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("fileName requerido (con extensión).", nameof(fileName));

        var baseUrl = GetBaseUrl();
        var uri = $"{baseUrl}/api/imagenes/{Uri.EscapeDataString(fileName)}";

        using (var req = new HttpRequestMessage(HttpMethod.Delete, uri))
        {
            ApplyApiKey(req);

            using (var resp = await _http.SendAsync(req))
            {
                // 204 NoContent = ok; 404 NotFound = ya no existe; otras => error
                if (resp.StatusCode == HttpStatusCode.NoContent || resp.StatusCode == HttpStatusCode.NotFound || resp.IsSuccessStatusCode)
                    return true;

                var body = await resp.Content.ReadAsStringAsync();
                throw new Exception($"DeleteAsync falló ({(int)resp.StatusCode}): {body}");
            }
        }
    }

    // =========================
    // ELIMINAR por URL pública
    // =========================
    public static async Task<bool> DeleteByUrlAsync(string publicUrl)
    {
        if (string.IsNullOrWhiteSpace(publicUrl))
            throw new ArgumentException("publicUrl requerido.", nameof(publicUrl));

        var fileName = ExtractFileNameFromUrl(publicUrl);
        if (string.IsNullOrWhiteSpace(fileName) || fileName == "/" || fileName.Contains("?"))
            throw new Exception("No se pudo inferir el nombre de archivo desde la URL.");

        return await DeleteAsync(fileName);
    }

    // =========================
    // DESCARGAR (para mostrar en UI)
    // =========================
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
                    var body = await resp.Content.ReadAsStringAsync();
                    Debug.WriteLine($"GET {url} -> {(int)resp.StatusCode} {resp.StatusCode}. Body: {body}");
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
