using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Helpers;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop.ControlesDeUsuario.PrepararEnvios
{
    public partial class PrepararEnvios_UC : UserControl
    {
        private int? numeroOrdenFiltrada = null;
        private int? estadoOrdenFiltrada = null;
        private bool volverAFormBuscarPedidos = false;
        private string nombreSucursalActual;
        private bool mostrarBotonVolver;

        private static string ObtenerCarpetaEtiquetas()
        {
            // Ej: C:\Users\TUUSUARIO\Documents\Eterea Parfums\Etiquetas
            string documentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string carpeta = Path.Combine(documentos, "Eterea Parfums", "Etiquetas");

            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            return carpeta;
        }




        public PrepararEnvios_UC(int? numeroOrden = null, int? estado = null, bool volverABuscarPedidos = false, bool mostrarBotonVolver = false)
        {
            InitializeComponent();

            numeroOrdenFiltrada = numeroOrden;
            estadoOrdenFiltrada = estado;
            this.volverAFormBuscarPedidos = volverABuscarPedidos;
            this.mostrarBotonVolver = mostrarBotonVolver;


            OrdenControlador controlador = new OrdenControlador();
            int cantidad = controlador.ObtenerCantidadOrdenesActivasEnRango19a19();

            if (cantidad == 0 && !numeroOrden.HasValue)
            {
                MessageBox.Show("En este momento no hay órdenes activas para despachar.", "Sin órdenes activas", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            txt_nombre_empleado.Text = Program.logueado.nombre + " " + Program.logueado.apellido;
            txt_nombre_empleado.Visible = false;

            txt_cantidad_ordenes.Text = cantidad.ToString();

            this.lbl_pedido_buscado.Visible = false; // oculto por defecto
            this.Controls.Add(this.lbl_pedido_buscado);

            

            
            nombreSucursalActual = SucursalControlador.ObtenerNombreSucursalPorId(Program.sucursal);

            this.Load += PrepararEnvios_UC_Load;

            this.Cursor = Cursors.Default;
            this.UseWaitCursor = false;
            this.Dock = DockStyle.Fill;
            this.BringToFront();

        }


        private void PrepararEnvios_UC_Load(object sender, EventArgs e)
        {
            btn_volver.Visible = mostrarBotonVolver;
            CargarOrdenes();


        }

        private void CargarOrdenes()
        {
            OrdenControlador controlador = new OrdenControlador();

            DataTable dtOrdenes;

            if (numeroOrdenFiltrada.HasValue)
            {
                // Es un pedido buscado → ocultar cantidad total y mostrar leyenda personalizada
                lbl_cantidad_ordenes.Visible = false;
                txt_cantidad_ordenes.Visible = false;

                lbl_pedido_buscado.Visible = true;
                lbl_pedido_buscado.Text = $"Mostrando detalles de la Orden N° {numeroOrdenFiltrada.Value}";
                dtOrdenes = new OrdenControlador().BuscarOrdenPorNumero(numeroOrdenFiltrada.Value);
            }
            else
            {
                // Es la lista completa → mostrar total
                lbl_cantidad_ordenes.Visible = true;
                txt_cantidad_ordenes.Visible = true;
                lbl_pedido_buscado.Visible = false;
                //dtOrdenes = new OrdenControlador().ObtenerOrdenes();
                dtOrdenes = controlador.ObtenerOrdenesDeUltimas24HorasHasta19hs();

            }

            //MessageBox.Show("Órdenes activas encontradas: " + dtOrdenes.Rows.Count);
            flowLayoutPanel1.Controls.Clear();

            foreach (DataRow orden in dtOrdenes.Rows)
            {
                int numeroOrden = Convert.ToInt32(orden["numero_de_orden"]);
                int numFactura = Convert.ToInt32(orden["factura_id"]);

                Panel panelOrden = new Panel
                {
                    Width = flowLayoutPanel1.Width - 30,
                    Height = 250, // Aumentá un poco la altura para que entre el botón
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(5)
                };

                Label lblOrden = new Label
                {
                    Text = $"Orden Nº {numeroOrden} - Cliente: {orden["nombre_cliente"]} {orden["apellido_cliente"]} - DNI: {orden["dni"]} - Fecha: {Convert.ToDateTime(orden["fecha_creacion"]).ToShortDateString()}",
                    Location = new Point(5, 5),
                    Font = new Font("Segoe UI", 16, FontStyle.Bold),
                    ForeColor = Color.FromArgb(195, 156, 164),
                    AutoSize = true
                };

                DataGridView dgvPerfumes = new DataGridView
                {
                    DataSource = controlador.ObtenerPerfumesDeFactura(numFactura),
                    Location = new Point(5, 40),
                    Width = panelOrden.Width - 10,
                    Height = 160,
                    ReadOnly = true,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false
                };

                PersonalizarDataGridView(dgvPerfumes);

                // 🔽 BOTÓN "Imprimir Etiqueta"
                Button btnImprimir = new Button
                {
                    Text = "Despachar la orden y crear su etiqueta de envío.",
                    Location = new Point(500, 205),
                    Width = 450,
                    Height = 40,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold),
                    BackColor = Color.FromArgb(228, 137, 164),     // Color de fondo
                    ForeColor = Color.FromArgb(250, 236, 239),                       // Color del texto
                    FlatStyle = FlatStyle.Flat,
                    Tag = numeroOrden
                };

                // 🔽 Si la orden buscada ya fue despachada (estado = 0), ocultar botón
                if (numeroOrdenFiltrada.HasValue && estadoOrdenFiltrada.HasValue && estadoOrdenFiltrada.Value == 0)
                {
                    btnImprimir.Visible = false;
                }


                btnImprimir.Click += (s, e) =>
                {

                    if (!int.TryParse(orden["numero_de_orden"]?.ToString(), out
                        numeroOrden))
                    {
                        MessageBox.Show("Número de orden inválido o no encontrado.");
                        return;
                    }

                    // Verificar si ya tiene código de despacho asignado
                    object valorCodigo = orden["codigo_despacho"];
                    bool yaTieneCodigo = valorCodigo != DBNull.Value && !string.IsNullOrEmpty(valorCodigo.ToString());

                    string codigoDespacho;

                    if (yaTieneCodigo)
                    {
                        // Ya tiene código → usar el que ya está en la BD
                        codigoDespacho = valorCodigo.ToString();
                    }
                    else
                    {
                        // Generar código único nuevo
                        codigoDespacho = controlador.GenerarCodigoDespachoUnico();
                        controlador.GuardarCodigoDespacho(numeroOrden, codigoDespacho);

                    }
                    controlador.DesactivarOrden(numeroOrden);

                    // Email de cliente
                    string emailCliente = orden["e_mail_cliente"]?.ToString();
                    string nombreCliente = $"{orden["nombre_cliente"]} {orden["apellido_cliente"]}";

                    // Validar y enviar notificación
                    if (!string.IsNullOrWhiteSpace(emailCliente))
                    {
                        CorreoHelper.EnviarCorreoNotificacionDespacho(emailCliente, nombreCliente, codigoDespacho);
                    }

                    string celularSinFormatear = orden["celular_cliente"]?.ToString();

                    string celularFormateado = FormatearCelular(celularSinFormatear);

                    string contenidoQR = $"Orden N {numeroOrden}\n" +
                                         $"Cliente: {orden["nombre_cliente"]} {orden["apellido_cliente"]}\n" +
                                         $"DNI: {orden["dni"]}\n" +
                                         $"Celular: {celularFormateado}\n" +
                                         $"Email: {orden["e_mail_cliente"]}\n" +
                                         $"Domicilio: {orden["domicilio_de_envio"]}\n\n" +
                                         $"Codigo verificacion de despacho: {codigoDespacho}\n" +
                                         $"Orden despachada por: {txt_nombre_empleado.Text}";





                    // 4. Generar QR
                    using (var qrGenerator = new QRCoder.QRCodeGenerator())
                    using (var qrData = qrGenerator.CreateQrCode(contenidoQR, QRCoder.QRCodeGenerator.ECCLevel.Q))
                    using (var qrCode = new QRCoder.QRCode(qrData))
                    {
                        Bitmap qrImage = qrCode.GetGraphic(20);
                        //qrBitmapParaImprimir = qrCode.GetGraphic(20);
                        // 3. Crear PDF
                        CrearPdfEtiqueta(contenidoQR, qrImage, numeroOrden);
                    }



                    //  Guardar datos en variable para impresión
                    //datosOrdenParaImprimir = contenidoQR;

                    //  Imprimir
                    //ImprimirOrden();


                    // Verificar si se debe regresar al BuscarPedidos_UC
                    if (volverAFormBuscarPedidos)
                    {
                        // Mostrar mensaje de confirmación
                        MessageBox.Show("La orden buscada fue despachada correctamente.", "Orden despachada", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Navegar de regreso al BuscarPedidos_UC
                        Control parent = this.Parent;
                        if (parent != null)
                        {
                            parent.Controls.Clear();
                            var buscarPedidosUC = new BuscarPedidos_UC();
                            parent.Controls.Add(buscarPedidosUC);
                            buscarPedidosUC.Dock = DockStyle.Fill;
                        }
                    }
                    else
                    {
                        // Si no se accedió desde BuscarPedidos_UC, simplemente recargar las órdenes
                        CargarOrdenes();
                        int cantidad = controlador.ObtenerCantidadOrdenesActivasEnRango19a19();
                        txt_cantidad_ordenes.Text = cantidad.ToString();

                        // Si ya no quedan órdenes, mostrar mensaje
                        if (cantidad == 0)
                        {
                            MessageBox.Show("Ya fueron despachadas todas las órdenes.", "Despacho completo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                };


                // Agregar controles al panel
                panelOrden.Controls.Add(lblOrden);
                panelOrden.Controls.Add(dgvPerfumes);
                panelOrden.Controls.Add(btnImprimir);

                // Agregar panel al FlowLayoutPanel
                flowLayoutPanel1.Controls.Add(panelOrden);
            }
        }


        private void PersonalizarDataGridView(DataGridView dgv)
        {
            // Fuente y colores
            dgv.Font = new Font("Segoe UI", 10);
            dgv.BackgroundColor = Color.FromArgb(235, 199, 206);  // Color personalizado
            dgv.BorderStyle = BorderStyle.None;

            // Encabezado de columnas
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(195, 156, 164);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersHeight = 40; // Aumentar el tamaño del encabezado
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // Celdas normales
            dgv.DefaultCellStyle.BackColor = Color.FromArgb(235, 199, 206);
            dgv.DefaultCellStyle.ForeColor = Color.Black;
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(242, 217, 222);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Bordes
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = Color.FromArgb(195, 156, 164);

            // Ajuste de filas
            dgv.RowTemplate.Height = 30;
            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Otros
            dgv.AllowUserToResizeRows = false;
            dgv.RowHeadersVisible = false;

            // no cambiar tamaño de columnas y filas
            dgv.AllowUserToResizeColumns = false;
            dgv.AllowUserToResizeRows = false;

            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(235, 199, 206);
        }



        private void CrearPdfEtiqueta(string contenidoTexto, Bitmap qrImage, int numeroOrden)
        {
            // Carpeta segura para guardar etiquetas (en Mis Documentos del usuario)
            string carpetaEtiquetas = ObtenerCarpetaEtiquetas();

            string rutaArchivo = Path.Combine(carpetaEtiquetas, $"Etiqueta_Orden_{numeroOrden}.pdf");

            // Crear el PDF
            PdfDocument documento = new PdfDocument();
            documento.Info.Title = $"Etiqueta Orden N° {numeroOrden}";
            PdfPage pagina = documento.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(pagina);

            // Fuentes
            XFont fontTitulo = new XFont("Segoe UI", 16, XFontStyleEx.Bold);
            XFont fontSubtitulo = new XFont("Segoe UI", 12, XFontStyleEx.Bold);
            XFont fontTexto = new XFont("Segoe UI", 10, XFontStyleEx.Regular);

            double x = 40;
            double y = 40;

            gfx.DrawString("ETIQUETA DE ENVÍO", fontTitulo, XBrushes.Black, x, y);
            y += 35;

            string ordenStr = contenidoTexto.Split('\n')[0]; // "Orden N° XYZ"
            gfx.DrawString(ordenStr, fontSubtitulo, XBrushes.Black, x, y);
            y += 25;

            string[] lineas = contenidoTexto.Split('\n');
            for (int i = 1; i < lineas.Length; i++)
            {
                gfx.DrawString(lineas[i], fontTexto, XBrushes.Black, x, y);
                y += 18;
            }

            y += 10;

            if (qrImage != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    qrImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    ms.Position = 0;
                    XImage qrXImage = XImage.FromStream(ms);
                    gfx.DrawImage(qrXImage, x, y, 150, 150);
                }
            }

            documento.Save(rutaArchivo);
            documento.Close();

            // Mostrar el PDF y preguntar
            MostrarDialogoDeEtiqueta(rutaArchivo);
        }


        private void MostrarDialogoDeEtiqueta(string rutaArchivo)
        {
            try
            {
                var visor = new FormVisorPDF(rutaArchivo)
                {
                    StartPosition = FormStartPosition.CenterScreen,
                    TopMost = true // 🔥 Forza que esté al frente
                };

                visor.ShowDialog(); // ShowDialog lo hace modal (bloquea hasta que se cierra)
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir el visor PDF: " + ex.Message);
            }

            DialogResult resultado = MessageBox.Show("¿Desea imprimir la etiqueta ahora?", "Etiqueta generada",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                ImprimirPdf(rutaArchivo);
            }
        }



        private void ImprimirPdf(string rutaPdf)
        {
            try
            {
                System.Diagnostics.Process printProcess = new System.Diagnostics.Process();
                printProcess.StartInfo.FileName = rutaPdf;
                printProcess.StartInfo.Verb = "print";
                printProcess.StartInfo.CreateNoWindow = true;
                printProcess.StartInfo.UseShellExecute = true;
                printProcess.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al imprimir el PDF: " + ex.Message);
            }
        }

        private void btn_volver_Click(object sender, EventArgs e)
        {
            // Obtener el contenedor principal (FormInicioVendedor)
            Control parent = this.Parent;
            if (parent != null)
            {
                // Limpiar los controles actuales
                parent.Controls.Clear();

                // Crear una nueva instancia de BuscarPedidos_UC
                var buscarPedidosUC = new BuscarPedidos_UC();

                // Agregar el nuevo control al contenedor
                parent.Controls.Add(buscarPedidosUC);
                buscarPedidosUC.Dock = DockStyle.Fill;
            }
        }

        private string FormatearCelular(string celular)
        {
            if (string.IsNullOrWhiteSpace(celular))
                return "No disponible";

            string soloNumeros = new string(celular.Where(char.IsDigit).ToArray());

            if (soloNumeros.Length < 8)
                return celular; // No se puede formatear

            string ultimos8 = soloNumeros.Substring(soloNumeros.Length - 8);
            string codigoArea = soloNumeros.Substring(0, soloNumeros.Length - 8);

            string grupo1 = ultimos8.Substring(0, 4);
            string grupo2 = ultimos8.Substring(4, 4);

            return $"{codigoArea}-{grupo1}-{grupo2}";
        }


    }
}

