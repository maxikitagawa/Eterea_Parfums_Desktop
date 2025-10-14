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
using Newtonsoft.Json;

public sealed class UploadImageResult
{
    public string fileName { get; set; } // ej: "banner-black-friday.jpg"
    public string url { get; set; }      // ej: "https://.../imagenes/banner-black-friday.jpg"
    public string api { get; set; }      // ej: "https://.../api/imagenes/banner-black-friday.jpg"
}

public static class ApiImageUploader
{
    // HttpClient único (recomendado)
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

    private static string GetUploadPath() { return ConfigurationManager.AppSettings["ApiUploadPath"] ?? "/api/imagenes/upload"; }
    private static string GetReplacePath() { return ConfigurationManager.AppSettings["ApiReplacePath"] ?? "/api/imagenes/replace"; }
    private static string GetRenamePath() { return ConfigurationManager.AppSettings["ApiRenamePath"] ?? "/api/imagenes/rename"; }

    private static void ApplyApiKey(HttpRequestMessage req)
    {
        var apiKeyHdr = ConfigurationManager.AppSettings["ApiKeyHeaderName"];
        var apiKeyVal = ConfigurationManager.AppSettings["ApiKeyValue"];
        if (!string.IsNullOrWhiteSpace(apiKeyHdr) && !string.IsNullOrWhiteSpace(apiKeyVal))
        {
            // TryAddWithoutValidation para evitar problemas con caracteres raros
            req.Headers.TryAddWithoutValidation(apiKeyHdr, apiKeyVal);
        }
    }

    private static string ExtractFileNameFromUrl(string publicUrl)
    {
        // Robusto y compatible con C# 7.3
        var uri = new Uri(publicUrl, UriKind.Absolute);
        var path = uri.LocalPath; // ej: "/imagenes/banner-black-friday.jpg"
        var name = Path.GetFileName(path); // ej: "banner-black-friday.jpg"
        return name;
    }

    // =========================
    // SUBIR (simple)
    // POST /api/imagenes/upload
    // multipart:
    //   file    = (archivo)
    //   newName = nombre final deseado (opcional)
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

            // La API espera el campo "newName"
            if (!string.IsNullOrWhiteSpace(desiredFileName))
                form.Add(new StringContent(desiredFileName), "newName");

            using (var req = new HttpRequestMessage(HttpMethod.Post, uploadUri))
            {
                req.Content = form;
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
    // REEMPLAZAR (sube nueva y borra vieja si corresponde)
    // POST /api/imagenes/replace
    // multipart:
    //   file    = (archivo nuevo)        [requerido]
    //   oldName = "vieja.jpg"            [opcional; si se envía y != newName => se borra]
    //   newName = "nueva.jpg"            [opcional; si se omite, server pone GUID.ext]
    // =========================
    // Firma clara: (localPath, oldName, newName)
    public static async Task<UploadImageResult> ReplaceAsync(string localFilePath, string oldNameOnServerOrNull, string newNameOnServerOrNull)
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

            if (!string.IsNullOrWhiteSpace(newNameOnServerOrNull))
                form.Add(new StringContent(newNameOnServerOrNull), "newName");

            using (var req = new HttpRequestMessage(HttpMethod.Post, replaceUri))
            {
                req.Content = form;
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

    // (Overload opcional por si te resulta más cómodo el orden: localPath, newName, oldName)
    public static Task<UploadImageResult> ReplaceAsync(string localFilePath, string newNameOnServer, string oldNameOnServerOrNull, bool overload)
    {
        // 'overload' no se usa; es para diferenciar firma.
        return ReplaceAsync(localFilePath, oldNameOnServerOrNull, newNameOnServer);
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
        var renameUrl = string.Concat(
            baseUrl, GetRenamePath(),
            "?oldName=", Uri.EscapeDataString(oldNameOnServer),
            "&newName=", Uri.EscapeDataString(newNameOnServer)
        );

        using (var req = new HttpRequestMessage(HttpMethod.Post, renameUrl))
        {
            ApplyApiKey(req);

            var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new InvalidOperationException($"Error {resp.StatusCode} renombrando imagen: {body}");

            // La API devuelve { oldName, newName, url, api }
            var r = JsonConvert.DeserializeObject<dynamic>(body);
            string returnedName = r != null && r.newName != null ? (string)r.newName : newNameOnServer;
            string returnedUrl = r != null && r.url != null ? (string)r.url : null;

            // Fallback por compatibilidad (si la API no incluyera url)
            if (string.IsNullOrWhiteSpace(returnedUrl))
                returnedUrl = baseUrl + "/imagenes/" + returnedName;

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
        var uri = baseUrl + "/api/imagenes/" + Uri.EscapeDataString(fileName);

        using (var req = new HttpRequestMessage(HttpMethod.Delete, uri))
        {
            ApplyApiKey(req);

            using (var resp = await _http.SendAsync(req))
            {
                // 204 NoContent = ok; 404 NotFound = ya no existe; otras => error
                if (resp.StatusCode == HttpStatusCode.NoContent ||
                    resp.StatusCode == HttpStatusCode.NotFound ||
                    resp.IsSuccessStatusCode)
                {
                    return true;
                }

                var body = await resp.Content.ReadAsStringAsync();
                throw new Exception("DeleteAsync falló (" + (int)resp.StatusCode + "): " + body);
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
        if (string.IsNullOrWhiteSpace(fileName) || fileName == "/" || fileName.IndexOf('?') >= 0)
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
            if (!_http.DefaultRequestHeaders.UserAgent.Any())
                _http.DefaultRequestHeaders.UserAgent.ParseAdd("EtereaDesktop/1.0");

            using (var resp = await _http.GetAsync(url))
            {
                if (!resp.IsSuccessStatusCode)
                {
                    var body = await resp.Content.ReadAsStringAsync();
                    Debug.WriteLine("GET " + url + " -> " + (int)resp.StatusCode + " " + resp.StatusCode + ". Body: " + body);
                    return Eterea_Parfums_Desktop.Properties.Resources.sinImagen;
                }

                var bytes = await resp.Content.ReadAsByteArrayAsync();
                using (var ms = new MemoryStream(bytes))
                {
                    // Clon a Bitmap para que no quede atado al stream
                    using (var img = Image.FromStream(ms))
                    {
                        return new Bitmap(img);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Excepción GET " + url + ": " + ex);
            return Eterea_Parfums_Desktop.Properties.Resources.sinImagen;
        }
    }
}
