using System;
using System.Windows.Forms;
using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Helpers;
using Eterea_Parfums_Desktop.Modelos;

namespace Eterea_Parfums_Desktop
{
    public partial class FormIngresoCodigoManual : Form
    {
        private Form _formInvocador;

        private Perfume _perfumeParaVer;

        private bool _cerradoPorExito = false;


        public FormIngresoCodigoManual(Form formInvocador)
        {
            InitializeComponent();

            _formInvocador = formInvocador;

            this.lbl_codigo_erroneo.Visible = false;

            this.FormClosed += FormIngresoCodigoManual_FormClosed;

            btnIngresarManual.Visible = false;
            btnVolverEscanear.Visible = false;
        }

        private void FormEscanear_Load(object sender, EventArgs e)
        {
            this.Activate();
            txt_codigo_barras.Focus(); // Poner foco automáticamente
        }

        private void txt_codigo_barras_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                BuscarCodigo();
            }
        }

        private void btn_confirmar_Click(object sender, EventArgs e)
        {
            BuscarCodigo();
        }

        private void BuscarCodigo()
        {
            string codigoBarras = txt_codigo_barras.Text.Trim();

            if (string.IsNullOrEmpty(codigoBarras))
            {
                MessageBox.Show("Ingrese un código de barras.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txt_codigo_barras.Focus();
                return;
            }

            Perfume perfumeEncontrado = PerfumeControlador.getByCodigo(codigoBarras);

            if (perfumeEncontrado != null)
            {

                lbl_codigo_erroneo.Visible = false; // Ocultamos error anterior si lo había

                _perfumeParaVer = perfumeEncontrado;
                _cerradoPorExito = true;

                // cerramos la ventana "pregunta" si existe
                this.Owner?.Close();

                using (var detalleForm = new FormVerDetallePerfume(perfumeEncontrado))
                {
                    ModalHelper.MostrarModalSinAgregarNuevoFondo(detalleForm);
                }

                this.Close();
                return;
            }
            else
            {
                // Código inválido: mostrar opciones
                lbl_codigo_erroneo.Text = "¿Qué desea hacer?";
                lbl_codigo_erroneo.Visible = true;
                lbl_numero_codigo.Visible = false;

                txt_codigo_barras.Visible = false;
                btnIngresarManual.Visible = true;
                btnVolverEscanear.Visible = true;
            }
        }

        private void FormIngresoCodigoManual_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (_cerradoPorExito) return;

            var metodoReset = _formInvocador?.GetType().GetMethod("ResetAutoConsulta");
            metodoReset?.Invoke(_formInvocador, null);

            var metodoPreparar = _formInvocador?.GetType().GetMethod("PrepararParaNuevoEscaneo");
            metodoPreparar?.Invoke(_formInvocador, null);
        }



        private void btn_cancelar_Click(object sender, EventArgs e)
        {
            this.Close();
            var metodoReset = _formInvocador.GetType().GetMethod("ResetAutoConsulta");
            metodoReset?.Invoke(_formInvocador, null);

            var metodoPreparar = _formInvocador.GetType().GetMethod("PrepararParaNuevoEscaneo");
            metodoPreparar?.Invoke(_formInvocador, null);

        }

        private void txt_codigo_barras_TextChanged(object sender, EventArgs e)
        {
            string codigoIngresado = txt_codigo_barras.Text.Trim();

            // Si tiene exactamente 13 caracteres, hacer la búsqueda automática
            if (codigoIngresado.Length == 13)
            {
                BuscarCodigo(codigoIngresado);
            }
        }

        private void BuscarCodigo(string codigo)
        {
            if (string.IsNullOrEmpty(codigo))
            {
                MessageBox.Show("Ingrese un código de barras.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Perfume perfumeEncontrado = PerfumeControlador.getByCodigo(codigo);

            if (perfumeEncontrado != null)
            {
                _perfumeParaVer = perfumeEncontrado; // ✅ ASIGNÁS EL PERFUME AQUÍ

                lbl_codigo_erroneo.Visible = false; // Ocultamos error anterior si lo había

                _perfumeParaVer = perfumeEncontrado;
                _cerradoPorExito = true;

                // cerramos la ventana "pregunta" si existe
                this.Owner?.Close();

                using (var detalleForm = new FormVerDetallePerfume(perfumeEncontrado))
                {
                    ModalHelper.MostrarModalSinAgregarNuevoFondo(detalleForm);
                }

                this.Close();
                return;
            }
            else
            {
                // Código inválido: mostrar opciones
                lbl_codigo_erroneo.Text = "¿Qué desea hacer?";
                lbl_codigo_erroneo.Visible = true;
                lbl_numero_codigo.Visible = false;

                txt_codigo_barras.Visible = false;
                btnIngresarManual.Visible = true;
                btnVolverEscanear.Visible = true;
            }
        }

        private void btnIngresarManual_Click(object sender, EventArgs e)
        {
            lbl_codigo_erroneo.Visible = false;
            btnIngresarManual.Visible = false;
            btnVolverEscanear.Visible = false;
            lbl_numero_codigo.Visible = true;

            txt_codigo_barras.Clear();
            txt_codigo_barras.Visible = true;
            txt_codigo_barras.Focus();
        }

        private void btnVolverEscanear_Click(object sender, EventArgs e)
        {
            this.Close();

            // Llama al método del invocador para reiniciar escaneo
            var metodo = _formInvocador?.GetType().GetMethod("ResetAutoConsulta");
            metodo?.Invoke(_formInvocador, null);
        }

      
    }
}
