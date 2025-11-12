using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.ControlesDeUsuario.PrepararEnvios;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop.ControlesDeUsuario
{
    public partial class BuscarPedidos_UC : UserControl
    {
        public BuscarPedidos_UC()
        {
            InitializeComponent();

            lbl_orden_error.Visible = false;

            
            dgv_resultado.SelectionChanged += (s, e) => ActualizarTextoBotonPrepararEnvio();
           
            txt_orden_n.KeyDown += Txt_orden_n_KeyDown;

            // SOLO NÚMEROS
            txt_orden_n.KeyPress += Txt_orden_n_KeyPress;     // bloquea letras al tipear
            txt_orden_n.TextChanged += Txt_orden_n_TextChanged; // limpia cuando pegan/arrastran
            txt_orden_n.MaxLength = 10; // límite razonable
            txt_orden_n.AllowDrop = false; // evita drag&drop de texto no deseado


            this.Cursor = Cursors.Default;
            this.UseWaitCursor = false;


        }

        private void btn_buscar_orden_Click(object sender, EventArgs e)
        {
            dgv_resultado.RowHeadersVisible = false;

            // vacío
            if (string.IsNullOrWhiteSpace(txt_orden_n.Text))
            {
                lbl_orden_error.Text = "Ingrese un número de orden.";
                lbl_orden_error.Visible = true;
                txt_orden_n.Focus();
                return;
            }

            // no numérico
            if (!int.TryParse(txt_orden_n.Text, out int numeroOrden))
            {
                lbl_orden_error.Text = "El número de orden debe contener solo números.";
                lbl_orden_error.Visible = true;
                txt_orden_n.Focus();
                txt_orden_n.SelectAll();
                return;
            }

            // válido: ocultar error
            lbl_orden_error.Visible = false;

            var controlador = new OrdenControlador();
            DataTable resultado = controlador.BuscarOrdenPorNumero(numeroOrden);

            dgv_resultado.Rows.Clear();

            if (resultado.Rows.Count == 0)
            {
                MessageBox.Show("Número de orden inexistente, vuelva a intentar");
                return;
            }

            foreach (DataRow row in resultado.Rows)
            {
                int estado = Convert.ToInt32(row["estado"]);
                string estadoTexto = estado == 1 ? "Para despachar" : "Ya despachada";
                string fechaCompra = Convert.ToDateTime(row["fecha_compra"]).ToShortDateString();

                dgv_resultado.Rows.Add(
                    row["numero_de_orden"].ToString(),
                    estadoTexto,
                    fechaCompra
                );
            }
            ActualizarTextoBotonPrepararEnvio();
        }

        private void ActualizarTextoBotonPrepararEnvio()
        {
            if (dgv_resultado.SelectedRows.Count == 0)
            {
                btn_preparar_envio.Text = "Ver detalles";
                return;
            }

            string estadoTexto = dgv_resultado.SelectedRows[0].Cells[1].Value?.ToString();

            if (estadoTexto == "Para despachar")
            {
                btn_preparar_envio.Text = "VER DETALLES / PREPARAR ENVIO";
            }
            else
            {
                btn_preparar_envio.Text = "VER DETALLES";
            }
        }

        private void btn_preparar_envio_Click(object sender, EventArgs e)
        {
            if (dgv_resultado.SelectedRows.Count == 0)
            {
                MessageBox.Show("Ingrese un número de orden y realice la búsqueda.");
                return;
            }

            int numeroOrden = Convert.ToInt32(dgv_resultado.SelectedRows[0].Cells[0].Value);
            string estadoTexto = dgv_resultado.SelectedRows[0].Cells[1].Value?.ToString();
            int estado = estadoTexto == "Para despachar" ? 1 : 0;

            PrepararEnvios_UC prepararEnviosFiltradoUC = new PrepararEnvios_UC(numeroOrden, estado, true, true);
            Control parentContainer = this.Parent;

            if (parentContainer != null)
            {
                parentContainer.Controls.Clear();
                prepararEnviosFiltradoUC.Dock = DockStyle.Fill;
                parentContainer.Controls.Add(prepararEnviosFiltradoUC);
            }
        }

        private void Txt_orden_n_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btn_buscar_orden.PerformClick();
                e.SuppressKeyPress = true; // evita el 'ding' del Enter
            }
        }

        // Bloquea cualquier carácter que no sea control (Backspace, etc.) o dígito al tipear
        private void Txt_orden_n_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        // Limpia lo que no sea dígito cuando pegan con Ctrl+V o inputs IME
        private void Txt_orden_n_TextChanged(object sender, EventArgs e)
        {
            var original = txt_orden_n.Text;
            var filtrado = new string(original.Where(char.IsDigit).ToArray());

            if (original != filtrado)
            {
                int delta = original.Length - filtrado.Length;
                int caret = Math.Max(0, txt_orden_n.SelectionStart - delta);

                txt_orden_n.Text = filtrado;
                txt_orden_n.SelectionStart = caret;
            }

            // Siempre ocultar el error mientras el usuario edita
            lbl_orden_error.Visible = false;
            // opcional: limpiar el texto del label
            // lbl_orden_error.Text = "";
        }

    }
}
