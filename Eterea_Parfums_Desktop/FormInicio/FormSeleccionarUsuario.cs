using System;
using System.Drawing;
using System.Windows.Forms;
using ComboBox = System.Windows.Forms.ComboBox;

namespace Eterea_Parfums_Desktop
{
    public partial class FormSeleccionarUsuario : Form
    {
        public string UsuarioSeleccionado { get; private set; }
        public FormSeleccionarUsuario()
        {
            InitializeComponent();

            // Cargar nombres de usuarios en el ComboBox
            comboBoxUsuarios.Items.Add("Servidor");
            comboBoxUsuarios.Items.Add("Adri");
            comboBoxUsuarios.Items.Add("Dami");
            comboBoxUsuarios.Items.Add("Luis");
            comboBoxUsuarios.Items.Add("Maxi");
            comboBoxUsuarios.Items.Add("Marino");
            comboBoxUsuarios.Items.Add("Notebook Adri");

            comboBoxUsuarios.SelectedIndex = 0; // Seleccionar el primer usuario por defecto

            //Diseño del combo box
            comboBoxUsuarios.DrawMode = DrawMode.OwnerDrawFixed;
            comboBoxUsuarios.DrawItem += comboBoxdiseño_DrawItem;
            comboBoxUsuarios.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        private void btnConfirmar_Click_1(object sender, EventArgs e)
        {
            if (comboBoxUsuarios.SelectedItem != null)
            {
                UsuarioSeleccionado = comboBoxUsuarios.SelectedItem.ToString();
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Seleccione un usuario antes de continuar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        //Diseño del combo box
        private void comboBoxdiseño_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            // Obtener el ComboBox y el texto del ítem actual
            ComboBox combo = sender as ComboBox;
            combo.DropDownWidth = combo.Width + 5; // Ajustar tamaño para evitar borde azul
            string text = combo.Items[e.Index].ToString();

            // Definir colores personalizados
            Color backgroundColor;
            Color textColor;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                // Color cuando el ítem está seleccionado
                backgroundColor = Color.FromArgb(195, 156, 164);
                textColor = Color.White;
            }
            else
            {
                // Color cuando el ítem NO está seleccionado
                backgroundColor = Color.FromArgb(250, 236, 239); // Color personalizado
                textColor = Color.FromArgb(195, 156, 164);
            }

            // Pintar el fondo del ítem
            using (SolidBrush brush = new SolidBrush(backgroundColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            // Dibujar el texto
            TextRenderer.DrawText(e.Graphics, text, e.Font, e.Bounds, textColor, TextFormatFlags.Left);

            // Evitar problemas visuales
            e.DrawFocusRectangle();
        }

    }


}
