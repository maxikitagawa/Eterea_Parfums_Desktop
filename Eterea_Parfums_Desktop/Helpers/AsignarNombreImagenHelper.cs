using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Eterea_Parfums_Desktop.Helpers
{
    // Hacelo static para usarlo sin instanciar
    public static class AsignarNombreImagenHelper
    {
        private static readonly Random _rnd = new Random();

        /// <summary>
        /// Convierte un texto a un "slug" web-friendly:
        /// - quita tildes/ñ
        /// - elimina símbolos raros
        /// - colapsa espacios
        /// - reemplaza espacios por guiones
        /// - minúsculas
        /// </summary>
        public static string Slugify(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "sin-nombre";

            // 1) Normaliza (quita diacríticos: á->a, ñ->n)
            string normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            normalized = sb.ToString().Normalize(NormalizationForm.FormC);

            // 2) Unifica separadores: guiones/bajos -> espacio
            normalized = normalized.Replace('_', ' ').Replace('-', ' ');

            // 3) Deja solo letras/números/espacios
            normalized = Regex.Replace(normalized, @"[^A-Za-z0-9 ]+", " ");

            // 4) Colapsa espacios y recorta
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

            // 5) Espacios -> guiones y minúsculas
            var slug = normalized.Replace(' ', '-').ToLowerInvariant();

            return string.IsNullOrWhiteSpace(slug) ? "sin-nombre" : slug;
        }

        /// <summary>
        /// Arma el nombre "limpio" SIN extensión. Ej: "fame-envase-2697"
        /// </summary>
        public static string BuildNombreImagen(string nombrePerfume, string sufijo, int? numeroAleatorio = null)
        {
            // Sufijo también se “slugifica” (p.ej. "envase y caja" -> "envase-y-caja")
            var baseNombre = Slugify($"{nombrePerfume} {sufijo}");
            int n = numeroAleatorio ?? _rnd.Next(1000, 9999);
            return $"{baseNombre}-{n}";
        }

        /// <summary>
        /// Crea la URL pública a partir de base + nombreSinExt.
        /// </summary>
        public static string ToPublicUrl(string basePublica, string nombreSinExtension, string extension = ".jpg")
        {
            if (string.IsNullOrWhiteSpace(nombreSinExtension)) return null;
            var file = nombreSinExtension + extension;

            // escapá el nombre del archivo por si quedó algún char especial
            var encodedFile = Uri.EscapeDataString(file);
            return $"{basePublica?.TrimEnd('/')}/{encodedFile}";
        }

        /// <summary>
        /// Normalización “liviana” de URL: recorta, / en vez de \, y espacios -> %20
        /// (útil para datos viejos en BD)
        /// </summary>
        public static string EncodeLight(string url)
        {
            return string.IsNullOrWhiteSpace(url)
                ? null
                : url.Trim().Replace("\\", "/").Replace(" ", "%20");
        }
    }
}
