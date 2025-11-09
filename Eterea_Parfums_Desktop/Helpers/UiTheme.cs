using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop.UI
{
    public static class UiTheme
    {
        // Paleta (tu paleta)
        public static readonly Color RoseText = Color.FromArgb(195, 156, 164); // texto
        public static readonly Color RoseBack = Color.FromArgb(249, 225, 230); // fondo input
        public static readonly Color RoseFocus = Color.FromArgb(242, 217, 222); // foco suave
        public static readonly Color RoseDisabled = Color.FromArgb(235, 199, 206); // disabled suave
        public static readonly Color RoseSelect = Color.FromArgb(195, 156, 164); // seleccionado (dropdown)
        public static readonly Color RoseSelectFg = Color.White;                   // texto seleccionado

        private static TextBox GetComboEditBox(ComboBox combo)
            => combo?.Controls?.OfType<TextBox>()?.FirstOrDefault();

        /// Prep básico para combos editables con autocompletar + owner draw
        public static void PrepareAutocompleteCombo(ComboBox combo, DrawItemEventHandler drawHandler)
        {
            if (combo == null) return;
            combo.DropDownStyle = ComboBoxStyle.DropDown; // editable
            combo.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            combo.AutoCompleteSource = AutoCompleteSource.CustomSource;
            combo.DrawMode = DrawMode.OwnerDrawFixed;
            // Evitar múltiples suscripciones
            combo.DrawItem -= drawHandler;
            combo.DrawItem += drawHandler;
        }

        /// Estilo del área editable (textbox interno) sin tocar la lógica de datos
        public static void ApplyEditableComboTheme(ComboBox combo)
        {
            if (combo == null) return;

            combo.FlatStyle = FlatStyle.Flat;
            combo.ForeColor = RoseText;
            combo.BackColor = combo.Enabled ? RoseBack : RoseDisabled;

            combo.HandleCreated += (s, e) =>
            {
                var tb = GetComboEditBox(combo);
                if (tb != null)
                {
                    tb.BorderStyle = BorderStyle.None;
                    tb.ForeColor = RoseText;
                    tb.BackColor = combo.Enabled ? RoseBack : RoseDisabled;

                    tb.GotFocus += (s2, e2) =>
                    {
                        tb.BackColor = RoseFocus;
                        tb.ForeColor = RoseText;
                    };
                    tb.LostFocus += (s2, e2) =>
                    {
                        tb.BackColor = combo.Enabled ? RoseBack : RoseDisabled;
                        tb.ForeColor = RoseText;
                    };
                }
            };

            combo.EnabledChanged += (s, e) =>
            {
                var tb = GetComboEditBox(combo);
                if (tb != null)
                {
                    tb.BackColor = combo.Enabled ? RoseBack : RoseDisabled;
                    tb.ForeColor = RoseText;
                }
                combo.BackColor = combo.Enabled ? RoseBack : RoseDisabled;
                combo.ForeColor = RoseText;
            };
        }

        /// Forzar colores si el tema de Windows no los toma al inicio
        public static void ForceEditableColors(ComboBox combo)
        {
            var tb = GetComboEditBox(combo);
            if (tb != null)
            {
                tb.ForeColor = RoseText;
                tb.BackColor = combo.Focused ? RoseFocus : (combo.Enabled ? RoseBack : RoseDisabled);
            }
        }

        /// DrawItem para la lista desplegable (usa la misma paleta)
        public static void DrawItemRose(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var combo = (ComboBox)sender;
            string text = combo.Items[e.Index]?.ToString() ?? "";

            Color bg = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? RoseSelect : RoseBack;
            Color fg = ((e.State & DrawItemState.Selected) == DrawItemState.Selected) ? RoseSelectFg : RoseText;

            using (var brush = new SolidBrush(bg))
                e.Graphics.FillRectangle(brush, e.Bounds);

            TextRenderer.DrawText(e.Graphics, text, e.Font, e.Bounds, fg,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            e.DrawFocusRectangle();
        }

        /// (Opcional) Bordecito temático al foco
        public static void AttachFocusBorder(ComboBox combo)
        {
            if (combo.Parent == null) return;

            var parent = combo.Parent;
            var idx = parent.Controls.GetChildIndex(combo);
            var wrapper = new Panel
            {
                BackColor = RoseBack,
                Padding = new Padding(1),
                Margin = combo.Margin,
                Size = combo.Size
            };

            parent.Controls.Add(wrapper);
            parent.Controls.SetChildIndex(wrapper, idx);
            wrapper.Location = combo.Location;

            combo.Margin = Padding.Empty;
            combo.Location = new Point(1, 1);
            combo.Dock = DockStyle.Fill;

            parent.Controls.Remove(combo);
            wrapper.Controls.Add(combo);

            void paintBorder(bool focused)
                => wrapper.BackColor = focused ? RoseSelect : RoseBack;

            combo.GotFocus += (s, e) => paintBorder(true);
            combo.LostFocus += (s, e) => paintBorder(false);
            combo.EnabledChanged += (s, e) =>
            {
                wrapper.BackColor = combo.Enabled ? (combo.Focused ? RoseSelect : RoseBack) : RoseDisabled;
            };
        }
    }
}
