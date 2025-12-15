using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

public static class NotaFiltroHelper
{
    // Hacelo PUBLIC para evitar el error de accesibilidad
    public static string NormalizeForCompare(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        string formD = input.Normalize(NormalizationForm.FormD).ToLowerInvariant();
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != UnicodeCategory.NonSpacingMark) sb.Append(ch);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    // Devuelve la mejor coincidencia (o null si no hay)
    public static string MejorCoincidencia(string textoFiltro, IEnumerable<Nota> notas)
    {
        if (string.IsNullOrWhiteSpace(textoFiltro)) return null;

        string nfiltro = NormalizeForCompare(textoFiltro);

        var q = notas
            .Where(x => !string.IsNullOrWhiteSpace(x?.nombre))
            .Select(x => new
            {
                Nota = x,
                NNombre = NormalizeForCompare(x.nombre)
            })
            .Where(x => x.NNombre.StartsWith(nfiltro))
            .OrderByDescending(x => x.NNombre.Equals(nfiltro)) // exacta primero
            .ThenBy(x => x.NNombre.Length)                     // luego más corta
            .ThenBy(x => x.Nota.nombre, StringComparer.CurrentCultureIgnoreCase) // alfabético
            .Select(x => x.Nota.nombre);

        return q.FirstOrDefault();
    }

    // Para conseguir todas las coincidencias ordenadas
    public static List<string> CoincidenciasOrdenadas(string textoFiltro, IEnumerable<Nota> notas)
    {
        if (string.IsNullOrWhiteSpace(textoFiltro)) return new List<string>();
        string nfiltro = NormalizeForCompare(textoFiltro);

        return notas
            .Where(x => !string.IsNullOrWhiteSpace(x?.nombre))
            .Select(x => new
            {
                Texto = x.nombre,
                NNombre = NormalizeForCompare(x.nombre)
            })
            .Where(x => x.NNombre.StartsWith(nfiltro))
            .OrderByDescending(x => x.NNombre.Equals(nfiltro))
            .ThenBy(x => x.NNombre.Length)
            .ThenBy(x => x.Texto, StringComparer.CurrentCultureIgnoreCase)
            .Select(x => x.Texto)
            .ToList();
    }
}
