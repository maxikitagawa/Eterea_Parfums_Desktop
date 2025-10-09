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

