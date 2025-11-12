using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop.ControlesDeUsuario.AdministrarStock
{
    public partial class AdministrarStock_UC : UserControl
    {
        private Perfume perfume;
        private int idSucursal;
        

        private bool limpiezaAutomatica = false;


        public AdministrarStock_UC(int idSucursal)
        {
            InitializeComponent();

            img_perfume.SizeMode = PictureBoxSizeMode.Zoom;
            img_perfume.WaitOnLoad = false; // para LoadAsync
            img_perfume.LoadCompleted += (s, e) =>
            {
                if (e.Error != null || img_perfume.Image == null)
                {
                    img_perfume.Image = Properties.Resources.sinImagen;
                }
            };

            this.idSucursal = idSucursal;
            
            Sucursal sucursal = SucursalControlador.getById(idSucursal);

            if (sucursal != null)
            {
                txt_nombre_sucursal.Text = sucursal.nombre;
            }
            else
            {
                txt_nombre_sucursal.Text = "Sucursal no encontrada";
            }


            //txt_cantidad_producto.Text = "0";
            txt_codigo_producto.MaxLength = 13;
            txt_codigo_producto.KeyPress += txt_codigo_producto_KeyPress;
            txt_codigo_producto.TextChanged += txt_codigo_producto_TextChanged;

            // Solo números positivos
            txt_cantidad_producto.KeyPress += TxtCantidad_KeyPress;       // bloquea al tipear
            txt_cantidad_producto.TextChanged += TxtCantidad_TextChanged; // sanea al pegar/IME
            txt_cantidad_producto.Leave += TxtCantidad_Leave;             // valida al salir
            txt_cantidad_producto.MaxLength = 3;                          // opcional

            // Imagen inicial por defecto
            CargarImagenPorDefecto();

            lbl_error_codigo.Visible = false;
            lbl_error_stock.Visible = false;

            this.Cursor = Cursors.Default;
            this.UseWaitCursor = false;

           

          


        }

      

       


        


        private void CargarImagenPorDefecto()
        {
            img_perfume.Image = Properties.Resources.sinImagen;

        }

        private bool validarSiExisteCodigoPerfume(string codigo)
        {
            perfume = PerfumeControlador.getByCodigo(codigo);
            // Si el perfume no existe, perfume.nombre es null
            return perfume != null;
        }


        // Validar datos de entrada del código de perfume
        private bool Validar_Datos_Codigo()
        {
            string mensaje = "";
            if (string.IsNullOrEmpty(txt_codigo_producto.Text))
            {
                mensaje = "Por favor, ingrese un código de producto.";
            }
            else if (txt_codigo_producto.Text.Length != 13)
            {
                mensaje = "Código ingresado es inexistente.";
            }
            else if (!validarSiExisteCodigoPerfume(txt_codigo_producto.Text.Trim()))
            {
                mensaje = "Código ingresado es inexistente.";
            }

            if (!string.IsNullOrEmpty(mensaje))
            {
                lbl_error_codigo.Text = mensaje;
                lbl_error_codigo.Visible = true;
            }

            return string.IsNullOrEmpty(mensaje);
        }

        // Validar datos de entrada del stock de perfume
        private bool Validar_Datos_Stock()
        {
            string mensaje = "";
            if (string.IsNullOrEmpty(txt_cantidad_producto.Text))
            {
                mensaje = "Por favor, ingrese cantidad de producto.";
            }
            else if (int.Parse(txt_cantidad_producto.Text) < 0)
            {
                mensaje = "Por favor, ingrese una cantidad valida.";
            }

            if (!string.IsNullOrEmpty(mensaje))
            {
                lbl_error_stock.Text = mensaje;
                lbl_error_stock.Visible = true;
            }

            return string.IsNullOrEmpty(mensaje);
        }



        private async void txt_codigo_producto_TextChanged(object sender, EventArgs e)
        {
            if (limpiezaAutomatica) return;

            string codigoIngresado = txt_codigo_producto.Text.Trim();

            if (codigoIngresado.Length < 13)
            {
                lbl_error_codigo.Text = "El código del producto debe tener 13 dígitos.";
                lbl_error_codigo.Visible = true;
                LimpiarCamposSinOcultarMensaje();
                return;
            }

            perfume = PerfumeControlador.getByCodigo(codigoIngresado);

            if (perfume == null)
            {
                lbl_error_codigo.Text = "El código ingresado es inexistente.";
                lbl_error_codigo.Visible = true;
                LimpiarCamposSinOcultarMensaje();
                img_perfume.Image = Properties.Resources.sinImagen;
                return;
            }

            lbl_error_codigo.Visible = false;

            txt_datos_producto.Text = perfume.nombre;
            txt_tamaño_producto.Text = perfume.presentacion_ml + " ML";

            List<Stock> stocks = StockControlador.getAll();
            int stockTotal = stocks
                .Where(s => s.perfume.id == perfume.id && s.sucursal.id == idSucursal)
                .Sum(s => s.cantidad);

            txt_cantidad_actual_producto.Text = stockTotal.ToString();

            // === NUEVO: cargar imagen desde la web/API ===
            string url = ObtenerUrlImagenPrincipal(perfume);
            await CargarImagenDesdeUrlAsync(url);
        }


        // Método auxiliar para limpiar los campos
        private void LimpiarCampos()
        {
            txt_datos_producto.Text = string.Empty;
            txt_tamaño_producto.Text = string.Empty;
            txt_cantidad_actual_producto.Text = string.Empty;

            // Cargar imagen por defecto embebida
            img_perfume.Image = Properties.Resources.sinImagen;

            lbl_error_codigo.Visible = false;

            txt_cantidad_producto.Text = string.Empty;
            txt_total_stock.Text = string.Empty;
            txt_codigo_producto.Text = string.Empty;
        }

        private void LimpiarCamposSinOcultarMensaje()
        {
            txt_datos_producto.Text = string.Empty;
            txt_tamaño_producto.Text = string.Empty;
            txt_cantidad_actual_producto.Text = string.Empty;
            img_perfume.Image = Properties.Resources.sinImagen;
            // No ocultamos lbl_error_codigo aquí
        }

        private void txt_cantidad_producto_TextChanged(object sender, EventArgs e)
        {
            if (perfume != null && int.TryParse(txt_cantidad_producto.Text, out int cantidadNueva))
            {
                int stockActual = StockControlador.getStock(perfume.id, idSucursal);
                int total = (stockActual != -1 ? stockActual : 0) + cantidadNueva;
                txt_total_stock.Text = total.ToString();
            }
            else
            {
                txt_total_stock.Text = ""; // Limpiar si no hay datos válidos
            }
        }

        private void txt_codigo_producto_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Ignorar entrada no válida
            }
        }

        private void btn_confirmar_Click(object sender, EventArgs e)
        {
            lbl_error_codigo.Text = "";
            lbl_error_codigo.Visible = false;
            lbl_error_stock.Text = "";
            lbl_error_stock.Visible = false;

            bool stockValido = Validar_Datos_Stock();
            bool codigoValido = Validar_Datos_Codigo();

            if (!stockValido || !codigoValido)
            {
                return;
            }

            int cantidad = int.Parse(txt_cantidad_producto.Text);

            if (StockControlador.getStock(perfume.id, idSucursal) != -1)
            {
                StockControlador.updateStock(perfume.id, idSucursal, cantidad);
            }
            else
            {
                StockControlador.insertStock(perfume.id, idSucursal, cantidad);
            }

            MessageBox.Show("Se ha ingresado con éxito.");

            // Si el perfume no está activo y se le está agregando stock, lo marcamos como activo
            if(perfume.activo == false)
            {
                PerfumeControlador.marcarComoActivo(perfume.id);
                perfume.activo = true; // actualizamos el objeto en memoria
                PerfumeControlador.LimpiarFechaDeBaja(perfume.id);
            }

            limpiezaAutomatica = true;  // Activar bandera para evitar validaciones al limpiar
            LimpiarCampos();
            limpiezaAutomatica = false;  // Desactivar bandera
        }

        // Helpers 
        private string ObtenerUrlImagenPrincipal(Perfume p)
        {
            if (!string.IsNullOrWhiteSpace(p.imagen1_URL))
                return p.imagen1_URL.Trim();

            if (!string.IsNullOrWhiteSpace(p.imagen1))
            {
                string baseUrl = (Program.Ruta_Web ?? "").Trim().TrimEnd('/');
                string nombre = p.imagen1.Trim().TrimStart('/');
                if (!System.IO.Path.HasExtension(nombre))
                    nombre += ".jpg";
                return $"{baseUrl}/{nombre}";
            }
            return null;
        }

        private async Task CargarImagenDesdeUrlAsync(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    img_perfume.Image = Properties.Resources.sinImagen;
                    return;
                }
                img_perfume.LoadAsync(url); // no bloquea la UI
            }
            catch
            {
                img_perfume.Image = Properties.Resources.sinImagen;
            }
        }

        private void TxtCantidad_KeyPress(object sender, KeyPressEventArgs e)
        {
            // permitir control (Backspace, etc.) y dígitos
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            // evitar que empiece con '0'
            if (!char.IsControl(e.KeyChar) && e.KeyChar == '0' && txt_cantidad_producto.SelectionStart == 0)
            {
                // si está vacío o el caret está al principio, no permitir 0 inicial
                e.Handled = true;
            }
        }

        private void TxtCantidad_TextChanged(object sender, EventArgs e)
        {
            var t = txt_cantidad_producto;
            string original = t.Text;

            // Dejar solo dígitos
            string soloDigitos = new string(original.Where(char.IsDigit).ToArray());

            // Quitar ceros a la izquierda (pero no dejar vacío si todo eran ceros)
            if (soloDigitos.Length > 1)
                soloDigitos = soloDigitos.TrimStart('0');

            if (original != soloDigitos)
            {
                int caret = t.SelectionStart;
                int delta = original.Length - soloDigitos.Length;

                t.Text = soloDigitos;
                t.SelectionStart = Math.Max(0, caret - delta);
            }
        }

        private void TxtCantidad_Leave(object sender, EventArgs e)
        {
            // Asegurar > 0 al salir del control
            if (string.IsNullOrWhiteSpace(txt_cantidad_producto.Text))
            {
                txt_cantidad_producto.Text = ""; // vacío permitido si querés forzar validación luego
                return;
            }

            if (!int.TryParse(txt_cantidad_producto.Text, out int n) || n <= 0)
            {
                // fijar valor mínimo 1; podés mostrar un mensaje si preferís
                txt_cantidad_producto.Text = "1";
            }
        }



    }



}
