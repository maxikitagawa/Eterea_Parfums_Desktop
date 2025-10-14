using System;
using System.Globalization;
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

            string normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var cat = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (cat != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            normalized = sb.ToString().Normalize(NormalizationForm.FormC);

            normalized = normalized.Replace('_', ' ').Replace('-', ' ');
            normalized = Regex.Replace(normalized, @"[^A-Za-z0-9 ]+", " ");
            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

            return normalized.Replace(' ', '-').ToLowerInvariant();
        }

        // Versión compacta: igual que Slugify pero sin guiones (p. ej. "envase y caja" -> "envaseycaja")
        public static string SlugifyCompact(string input)
        {
            var s = Slugify(input);
            return s.Replace("-", "");
        }

        /// <summary>
        /// NUEVO: {slug(nombre)}-{n}-{slug(sufijo)}. Si compactSuffix=true, el sufijo queda sin guiones.
        /// </summary>
        public static string BuildNombreImagen(string nombrePerfume, string sufijo, bool compactSuffix = false, int? numeroAleatorio = null)
        {
            var slugNombre = Slugify(nombrePerfume);
            var slugSufijo = compactSuffix ? SlugifyCompact(sufijo) : Slugify(sufijo);
            int n = numeroAleatorio ?? _rnd.Next(1000, 9999);
            return $"{slugNombre}-{n}-{slugSufijo}";
        }

        public static string ToPublicUrl(string basePublica, string nombreSinExtension, string extension = ".jpg")
        {
            if (string.IsNullOrWhiteSpace(nombreSinExtension)) return null;
            var file = nombreSinExtension + extension;
            var encodedFile = Uri.EscapeDataString(file);
            return $"{basePublica?.TrimEnd('/')}/{encodedFile}";
        }

        public static string EncodeLight(string url)
        {
            return string.IsNullOrWhiteSpace(url)
                ? null
                : url.Trim().Replace("\\", "/").Replace(" ", "%20");
        }
    


     public static string BuildPromoName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "sin-nombre";

            // a) pasar a minúsculas
            var lower = input.ToLowerInvariant();

            // b) remover acentos/diacríticos
            var normalized = lower.Normalize(NormalizationForm.FormD);
            var withoutDiacritics = new string(
                normalized.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray()
            ).Normalize(NormalizationForm.FormC);

            // c) reemplazar espacios por guiones
            var dashed = Regex.Replace(withoutDiacritics, @"\s+", "-");

            // d) eliminar todo lo que NO sea [a-z0-9-]
            var safe = Regex.Replace(dashed, @"[^a-z0-9\-]", "");

            // e) colapsar guiones dobles / extremos
            safe = Regex.Replace(safe, @"-+", "-").Trim('-');

            return string.IsNullOrWhiteSpace(safe) ? "sin-nombre" : safe;
        }

        public static string BuildPromoFileStem(string nombrePromo)
            => $"banner-{BuildPromoName(nombrePromo)}"; // <== sin extensión


        // ======================= PERFUMES con número aleatorio =======================
        private static readonly Regex _rxRandom4 = new Regex(@"\-(\d{4})\-", RegexOptions.Compiled);

        /// Extrae el token aleatorio (4 dígitos) del "stem" si existe. Ej: "bleu-1234-envase" -> 1234
        public static int? TryExtractRandomToken(string stemOrFileNameWithoutExt)
        {
            if (string.IsNullOrWhiteSpace(stemOrFileNameWithoutExt)) return null;
            var m = _rxRandom4.Match(stemOrFileNameWithoutExt);
            if (!m.Success) return null;
            if (int.TryParse(m.Groups[1].Value, out var n)) return n;
            return null;
        }

        /// Construye el "stem" con random PRESERVADO: perfume-{slug-nombre}-{random}-{sufijoCompacto}
        public static string BuildPerfumeStemWithRandom(string nombrePerfume, string variante, int random4digits)
        {
            var slugNombre = Slugify(nombrePerfume);
            var slugVar = SlugifyCompact(variante); // "envase" / "envaseycaja"
            return $"perfume-{slugNombre}-{random4digits}-{slugVar}";
        }

        /// Construye fileName con .jpg manteniendo random
        public static string BuildPerfumeFileNameWithRandom(string nombrePerfume, string variante, int random4digits, string ext = ".jpg")
            => BuildPerfumeStemWithRandom(nombrePerfume, variante, random4digits) + ext;

        /// Construye un random por defecto si no existía antes (1000-9999)
        public static int NewRandom4() => new Random().Next(1000, 9999);



    }



}