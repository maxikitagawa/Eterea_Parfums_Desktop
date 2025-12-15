using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Forms;

public class ComboPrefixGuard
{
    private readonly ComboBox _combo;
    private readonly ToolTip _tip = new ToolTip { ShowAlways = true };
    private bool _suppressTextChanged = false;

    private Func<string, string, bool> Match = (item, text) =>
        item.StartsWith(text, StringComparison.CurrentCultureIgnoreCase);

    public ComboPrefixGuard(ComboBox combo)
    {
        _combo = combo ?? throw new ArgumentNullException(nameof(combo));

        // Aseguramos estilo de edición
        _combo.DropDownStyle = ComboBoxStyle.DropDown;
        _combo.AutoCompleteSource = AutoCompleteSource.ListItems;
        _combo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

        _combo.KeyPress += OnKeyPress;
        _combo.KeyDown += OnKeyDown;   // para pegar/cortar
        _combo.TextChanged += OnTextChanged; // para cambios programáticos/selección
        _combo.Disposed += (_, __) => { _tip.Dispose(); };
    }

    private IEnumerable<string> Items()
        => _combo.Items.Cast<object>().Select(o => o?.ToString() ?? string.Empty);

    private bool HasMatches(string probe)
    {
        if (string.IsNullOrWhiteSpace(probe)) return true; // permitir limpiar
        return Items().Any(s => Match(s, probe));
    }

    private void BeepAndTip(string message)
    {
        SystemSounds.Asterisk.Play();
        try
        {
            _tip.Show(message, _combo, _combo.Width / 2, _combo.Height, 1400);
        }
        catch { /* evitar problemas si el control aún no está handleado */ }
    }

    private void OnKeyPress(object sender, KeyPressEventArgs e)
    {
        if (char.IsControl(e.KeyChar))
            return; // permitir Backspace, Enter, etc. (Enter no se usa acá para confirmar)

        // Construimos el texto resultante si aceptáramos esta tecla
        var selStart = _combo.SelectionStart;
        var selLength = _combo.SelectionLength;

        var sb = new StringBuilder(_combo.Text ?? string.Empty);
        if (selLength > 0)
            sb.Remove(selStart, selLength);

        sb.Insert(selStart, e.KeyChar);
        var prospective = sb.ToString();

        if (!HasMatches(prospective))
        {
            e.Handled = true; // bloquear
            BeepAndTip("Sin coincidencias");
        }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        // Soporte para pegar (Ctrl+V) y cortar (Ctrl+X)
        if (e.Control && (e.KeyCode == Keys.V || e.KeyCode == Keys.X))
        {
            string incoming = e.KeyCode == Keys.V ? Clipboard.GetText() : string.Empty;

            var selStart = _combo.SelectionStart;
            var selLength = _combo.SelectionLength;

            var sb = new StringBuilder(_combo.Text ?? string.Empty);
            if (selLength > 0)
                sb.Remove(selStart, selLength);

            if (e.KeyCode == Keys.V)
                sb.Insert(selStart, incoming);
            else
                ; // Ctrl+X ya queda vacío en la selección

            var prospective = sb.ToString();
            if (!HasMatches(prospective))
            {
                e.SuppressKeyPress = true; // bloquear la acción
                BeepAndTip("Sin coincidencias");
            }
        }
    }

    private void OnTextChanged(object sender, EventArgs e)
    {
        if (_suppressTextChanged) return;

        // Este evento también dispara al elegir del desplegable.
        // Si el texto actual no tiene coincidencias (por un cambio programático raro),
        // lo limpiamos para no dejar un estado imposible.
        var t = _combo.Text ?? string.Empty;
        if (!HasMatches(t))
        {
            _suppressTextChanged = true;
            _combo.Text = t.Length > 0 ? t.Substring(0, t.Length - 1) : string.Empty;
            _combo.SelectionStart = _combo.Text.Length;
            _suppressTextChanged = false;
            BeepAndTip("Sin coincidencias");
        }
    }

    // Opcional: exponer un modo “contiene en cualquier parte”
    public ComboPrefixGuard UseContainsMode()
    {
        Match = (item, text) =>
            item.IndexOf(text, StringComparison.CurrentCultureIgnoreCase) >= 0;
        return this;
    }
}
