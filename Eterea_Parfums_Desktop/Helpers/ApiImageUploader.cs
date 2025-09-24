using Eterea_Parfums_Desktop.DTOs;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

public static class ApiImageUploader
{
    private static readonly HttpClient _http = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(120)
    };

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
        {
            // Archivo
            using (var fileStream = File.OpenRead(localFilePath))
            {
                var fileContent = new StreamContent(fileStream);
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                var originalName = Path.GetFileName(localFilePath);
                form.Add(fileContent, "file", originalName);

                // Nombre deseado en el servidor
                if (!string.IsNullOrWhiteSpace(desiredFileName))
                    form.Add(new StringContent(desiredFileName), "fileName");

                // Header de autenticación (si se configura)
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
    }
}
