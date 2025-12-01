using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Modelos;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using Eterea_Parfums_Desktop.Helpers;
using iTextSharp.tool.xml.html;
using System.Globalization;


namespace Eterea_Parfums_Desktop.ControlesDeUsuario
{
    public partial class Facturar_UC : UserControl
    {



        Cliente clientefactura = new Cliente();
        public string numeroCaja;

        public int IdHistorialCaja { get; set; }

        private StringBuilder codigoBarrasBuffer = new StringBuilder();
        private DateTime ultimaLectura = DateTime.Now;
        private const int TIEMPO_ENTRE_LECTURAS_MS = 100;

        private bool yaMostroAdvertenciaCaja = false;  // lo ponés como campo en la clase


        private static readonly CultureInfo Ar = CultureInfo.GetCultureInfo("es-AR");
        private static string Mon(decimal v) => "$ " + v.ToString("N2", Ar);
        private static string Num(int v) => v.ToString(Ar);

        private Point lblCuotasPosOriginal;

        private double totalDescuentoPromoFactura = 0.0;

        private static string ObtenerCarpetaFacturas()
        {
            // Mis Documentos\Eterea Parfums\Facturas
            string documentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string carpeta = Path.Combine(documentos, "Eterea Parfums", "Facturas");

            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            return carpeta;
        }


        public Facturar_UC()
        {
            InitializeComponent();

            // Guardamos la posición original del label de cuotas
            lblCuotasPosOriginal = lbl_cuotas.Location;

            // Estado inicial para la funcion de ingreso manual del codigo de barras
            btn_ing_manual.Visible = true;
            txt_ing_manual.Visible = false;
            txt_ing_manual.MaxLength = 13;

            //EStado inicial para la funcion de ingresar pago en efectivo
            txt_ing_pago.Visible = false;
            lbl_pesos_1.Visible = false;
            lbl_pesos_2.Visible = false;
            txt_vuelto.Visible = false;
            lbl_vuelto.Visible = false;
            btn_ok.Visible = false;
            btn_imprimir.Visible = false;

            // Eventos
            btn_ing_manual.Click += btn_ing_manual_Click;
            txt_ing_manual.KeyPress += txt_ing_manual_KeyPress;
            txt_ing_manual.TextChanged += txt_ing_manual_TextChanged;
            txt_ing_manual.Leave += txt_ing_manual_Leave;


            //Evento pago en efectivo
            txt_ing_pago.KeyPress += txt_ing_pago_KeyPress;
            txt_ing_pago.TextChanged += txt_ing_pago_TextChanged;
            btn_ok.Click += btn_ok_Click;

            //Evento agregar fila al dataGridView
            Factura.RowsAdded += Factura_RowsAdded;


            txt_nombre_empleado.Text = Program.logueado.nombre + " " + Program.logueado.apellido;

            this.Load += FormFacturacion_Load;
            txt_scan_factura.Leave += Txt_scan_factura_Leave;
            txt_scan_factura.TextChanged += Txt_scan_factura_TextChanged;
            Factura.CellContentClick += DataGridViewFactura_CellContentClick;
            Factura.CellValidating += Factura_CellValidating;
            Factura.CellEndEdit += Factura_CellEndEdit;
            btn_pago.Visible = false;

           

            combo_forma_pago.SelectedIndexChanged -= combo_forma_pago_SelectedIndexChanged;

            txt_nombre_cliente.Text = "";
            txt_condicion_iva.Text = "";
            txt_numero_factura.Text = "";
            btn_imprimir_habilitado = true;

            combo_forma_pago.Items.Clear();
            combo_forma_pago.Items.Add("Efectivo");
            combo_forma_pago.Items.Add("Visa Débito");
            combo_forma_pago.Items.Add("Visa Crédito");
            combo_forma_pago.Items.Add("Mastercard");
            combo_forma_pago.Items.Add("Amex");
            combo_forma_pago.Items.Add("Mercado Pago");
            combo_forma_pago.SelectedIndex = 0;


            combo_descuento.Items.Clear();
            combo_descuento.Items.AddRange(new object[] { 0, 10, 15 });
            combo_descuento.SelectedItem = 0;


            combo_cuotas.Items.Clear();
            combo_cuotas.Items.AddRange(new object[] { 1, 3, 6, 9, 12 });
            combo_cuotas.SelectedIndex = 0;

            //txt_recargo.Hide();

            txt_total.Text = "0,00";
            txt_subtotal.Text = "0,00";
            txt_monto_recargo.Text = "0,00";
            txt_monto_descuento.Text = "0,00";
            txt_iva.Text = "0,00";

            combo_forma_pago.SelectedIndexChanged += combo_forma_pago_SelectedIndexChanged;
            ActualizarDescuentosYCuotas();
            ActualizarUIFormaPago();

            lbl_dniE.Hide();
            txt_scan_factura.Hide();

            this.Cursor = Cursors.Default;
            this.UseWaitCursor = false;

            ActualizarEstadoCaja();

            //Diseño del combo box
            combo_forma_pago.DrawMode = DrawMode.OwnerDrawFixed;
            combo_forma_pago.DrawItem += comboBoxdiseño_DrawItem;
            combo_forma_pago.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_cuotas.DrawMode = DrawMode.OwnerDrawFixed;
            combo_cuotas.DrawItem += comboBoxdiseño_DrawItem;
            combo_cuotas.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_descuento.DrawMode = DrawMode.OwnerDrawFixed;
            combo_descuento.DrawItem += comboBoxdiseño_DrawItem;
            combo_descuento.DropDownStyle = ComboBoxStyle.DropDownList;

            this.VisibleChanged += Facturar_UC_VisibleChanged;
        }

        private void txt_ing_manual_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Permitir teclas de control (Backspace, Delete, flechas, etc.)
            if (char.IsControl(e.KeyChar))
                return;

            // Solo dígitos
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Bloquea el carácter
            }
        }

        private void txt_ing_manual_Leave(object sender, EventArgs e)
        {
            // Si ya no está visible (por ejemplo, porque se ocultó en ProcesarCodigoBarrasManual),
            // no hacemos nada.
            if (!txt_ing_manual.Visible)
                return;

            // Si el usuario hizo clic en otro lado y abandonó el ingreso manual,
            // cancelamos el proceso y volvemos al botón.
            txt_ing_manual.Clear();
            txt_ing_manual.Visible = false;
            btn_ing_manual.Visible = true;
            btn_ing_manual.Focus();
        }


        private void txt_ing_pago_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Permitir teclas de control (Backspace, Delete, etc.)
            if (char.IsControl(e.KeyChar))
                return;

            // Solo dígitos
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txt_ing_pago_TextChanged(object sender, EventArgs e)
        {
            // Si no hay total o no hay pago, ponemos 0,00
            if (string.IsNullOrWhiteSpace(txt_total.Text) ||
                string.IsNullOrWhiteSpace(txt_ing_pago.Text))
            {
                txt_vuelto.Text = "0,00";
                return;
            }

            if (!decimal.TryParse(txt_total.Text, NumberStyles.Any, Ar, out decimal total))
            {
                txt_vuelto.Text = "0,00";
                return;
            }

            // txt_ing_pago son solo dígitos, así que esto es seguro
            if (!decimal.TryParse(txt_ing_pago.Text, NumberStyles.Any, Ar, out decimal pagado))
            {
                txt_vuelto.Text = "0,00";
                return;
            }

            decimal vuelto = pagado - total;

            // Si no querés mostrar negativo cuando falta dinero, podés descomentar:
            // if (vuelto < 0) vuelto = 0;

            txt_vuelto.Text = vuelto.ToString("N2", Ar);
        }

        private void Factura_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            descuentoUnitario();
            ActualizarTotales();
        }


        private void btn_ing_manual_Click(object sender, EventArgs e)
        {
            // Verificamos que haya caja abierta, igual que con el escáner
            if (string.IsNullOrEmpty(numeroCaja) || numeroCaja == "Caja sin asignar")
            {
                MessageBox.Show(
                    "No se puede ingresar productos sin una caja abierta.\nHaz clic en 'Abrir Caja'.",
                    "Caja no asignada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            btn_ing_manual.Visible = false;
            txt_ing_manual.Visible = true;
            txt_ing_manual.Clear();
            txt_ing_manual.Focus();
        }

        private bool EsEan13Valido(string ean)
        {
            if (string.IsNullOrWhiteSpace(ean) || ean.Length != 13 || !ean.All(char.IsDigit))
                return false;

            int sumaImpares = 0; // posiciones 1,3,5,... (índices 0,2,4,...)
            int sumaPares = 0;   // posiciones 2,4,6,... (índices 1,3,5,...)

            for (int i = 0; i < 12; i++)
            {
                int dig = ean[i] - '0';
                if (i % 2 == 0)
                    sumaImpares += dig; // index par = posición impar
                else
                    sumaPares += dig;   // index impar = posición par
            }

            int total = sumaImpares + (sumaPares * 3);
            int resto = total % 10;
            int digitoCalculado = (10 - resto) % 10;

            int digitoReal = ean[12] - '0';

            return digitoCalculado == digitoReal;
        }

        private void txt_ing_manual_TextChanged(object sender, EventArgs e)
        {
            string codigo = txt_ing_manual.Text.Trim();

            // Hasta que no haya 13 dígitos, no hacemos nada
            if (codigo.Length < 13)
                return;

            if (codigo.Length > 13)
            {
                // Por seguridad, si se pasa de largo
                codigo = codigo.Substring(0, 13);
                txt_ing_manual.Text = codigo;
                txt_ing_manual.SelectionStart = txt_ing_manual.Text.Length;
            }

            // En este punto tiene exactamente 13 caracteres
            ProcesarCodigoBarrasManual(codigo);
        }

        private void ProcesarCodigoBarrasManual(string codigo)
        {
            try
            {
                // Validaciones básicas
                if (string.IsNullOrEmpty(numeroCaja) || numeroCaja == "Caja sin asignar")
                {
                    MessageBox.Show(
                        "No se puede ingresar productos sin una caja abierta.\nHaz clic en 'Abrir Caja'.",
                        "Caja no asignada",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                if (codigo.Length != 13 || !codigo.All(char.IsDigit))
                {
                    MessageBox.Show(
                        "El código de barras debe contener exactamente 13 dígitos numéricos.",
                        "Código inválido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                if (!EsEan13Valido(codigo))
                {
                    MessageBox.Show(
                        "El código EAN-13 ingresado no es válido.",
                        "Código inválido",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return;
                }

                //    Esto ya se encarga de:
                //    - Buscar el Perfume
                //    - Verificar stock
                //    - Agregarlo a la factura con AddOrIncrementPerfume
                ProcesarCodigoBarras(codigo);
            }
            finally
            {
                // Siempre volvemos al estado inicial de los controles
                txt_ing_manual.Clear();
                txt_ing_manual.Visible = false;
                btn_ing_manual.Visible = true;
                btn_ing_manual.Focus();
            }
        }


        private void Facturar_UC_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                BarcodeReceiver.OnCodigoLeido -= ProcesarCodigoLeido;
                BarcodeReceiver.OnCodigoLeido += ProcesarCodigoLeido;
            }
            else
            {
                BarcodeReceiver.OnCodigoLeido -= ProcesarCodigoLeido;
            }
        }

        private void ProcesarCodigoLeido(string codigo)
        {
            if (!this.Visible || string.IsNullOrEmpty(numeroCaja) || numeroCaja == "Caja sin asignar")
            {
                if (!yaMostroAdvertenciaCaja)
                {
                    yaMostroAdvertenciaCaja = true;
                    MessageBox.Show("No se puede escanear productos sin una caja abierta.", "Caja no asignada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return;
            }

            if (InvokeRequired)
            {
                Invoke(new Action(() => ProcesarCodigoLeido(codigo)));
                return;
            }

            if (codigo.Length == 12)
            {
                codigo = "0" + codigo;
            }


            txt_scan_factura.Text = codigo;
            ProcesarCodigoBarras(codigo);
            txt_scan_factura.Clear();
        }

    
        public void DesactivarEscaner()
        {
            BarcodeReceiver.OnCodigoLeido -= ProcesarCodigoLeido;
        }

        public void ActivarEscaner()
        {
            BarcodeReceiver.OnCodigoLeido -= ProcesarCodigoLeido;
            BarcodeReceiver.OnCodigoLeido += ProcesarCodigoLeido;
        }

        private bool btn_imprimir_habilitado = true;
        private void txt_dni_TextChanged(object sender, EventArgs e)
        {
            // Si el campo DNI está vacío, permitimos facturar como "Consumidor Final".
            // Si el usuario escribe algo, pedimos que primero busque el cliente.
            if (string.IsNullOrWhiteSpace(txt_dni.Text))
            {
                btn_imprimir_habilitado = true;
            }
            else
            {
                btn_imprimir_habilitado = false;
            }
        }

        private void FormFacturacion_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Program.NumeroCajaActual) || Program.NumeroCajaActual == "Caja sin asignar")
            {
                FormNumeroDeCaja formNumero = new FormNumeroDeCaja();
                formNumero.AutoTomarCaja = true;

                formNumero.ConfirmarNumeroCaja += (s, cajaElegida) =>
                {
                    numeroCaja = cajaElegida;
                    IdHistorialCaja = Program.IdHistorialCajaActual;
                    txt_numero_caja.Text = numeroCaja;
                    ActualizarEstadoCaja();


                    // 👉 Reiniciamos el flag aquí
                    yaMostroAdvertenciaCaja = false;


                    // 🔥 ACTIVAR EL ESCANER AQUÍ EXPLÍCITAMENTE
                    BarcodeReceiver.OnCodigoLeido -= ProcesarCodigoLeido;
                    BarcodeReceiver.OnCodigoLeido += ProcesarCodigoLeido;

                    // ✅ Asignar "Consumidor Final"
                    txt_nombre_cliente.Text = "Consumidor Final";
                    txt_condicion_iva.Text = "Consumidor Final";

                    // ✅ Obtener y mostrar número de factura
                    txt_numero_factura.Text = Num_factura_máximo();
                };

                ModalHelper.MostrarModalConFondoOscuro(formNumero);
            }
            else
            {
                numeroCaja = Program.NumeroCajaActual;
                IdHistorialCaja = Program.IdHistorialCajaActual;
                txt_numero_caja.Text = numeroCaja;
            }


           
            txt_scan_factura.Focus();
            ActualizarEstadoCaja();
        }


        private string Num_factura_máximo()
        {
            int puntoDeVenta = Program.sucursal;
            string puntoDeVentaString = puntoDeVenta.ToString("D4");
            string numeroDeFacturaString = FacturaControlador.ObtenerProximoNumFactura(tipo_de_factura(), puntoDeVentaString);
            txt_numero_factura.Text = numeroDeFacturaString;
            return numeroDeFacturaString;
        }

        private void ProcesarCodigoBarras(string codigo)
        {
            if (string.IsNullOrEmpty(codigo)) return;

            Perfume perfume = PerfumeControlador.getByCodigo(codigo);

            if (perfume == null)
            {
                MessageBox.Show("Producto no encontrado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //Ocultas la primera columna de la tabla (es una columna de seleccion de fila)
            Factura.RowHeadersVisible = false;

            Factura.CellPainting -= Factura_CellPainting;
            Factura.CellPainting += Factura_CellPainting;

            // 👉 Usamos el método único
            AddOrIncrementPerfume(perfume, 1);

            txt_scan_factura.Clear();
        }

        private void Txt_scan_factura_Leave(object sender, EventArgs e)
        {
            txt_scan_factura.Focus(); // Si pierde el foco, volver a asignárselo automáticamente
        }

        private void Txt_scan_factura_TextChanged(object sender, EventArgs e)
        {

        }


        private void button2_Click_1(object sender, EventArgs e)
        {
            CajaManager.IntentarAbrirCajaDesdeBoton(this, (numeroCajaAsignada) =>
            {
                numeroCaja = numeroCajaAsignada;
                IdHistorialCaja = Program.IdHistorialCajaActual;
                txt_numero_caja.Text = numeroCaja;
                txt_estadoCaja.Text = "Abierta";
                txt_estadoCaja.ForeColor = Color.Green;
                ReiniciarFormulario();
            });



        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!CajaManager.HayCajaAsignada())
            {
                MessageBox.Show("No hay ninguna caja en uso.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Cerrar la caja correctamente y registrar en la base
            CajaManager.CerrarCaja();

            // Mostrar mensaje de confirmación
            MessageBox.Show("Caja cerrada correctamente.", "Cierre de Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Actualizar interfaz
            txt_numero_caja.Text = "Caja no asignada";
            txt_estadoCaja.Text = "Cerrada";
            txt_estadoCaja.ForeColor = Color.Red;

            // Reset variables locales del UC también
            numeroCaja = "Caja sin asignar";
            IdHistorialCaja = 0;

            ReiniciarFormulario();

            //Limpiar consumidor y num factura
            txt_nombre_cliente.Text = "";
            txt_condicion_iva.Text = "";
            txt_numero_factura.Text = "";

            // Limpiar perfumes de la factura si hay alguno cargado
            if (Factura.Rows.Count > 0)
            {
                Factura.Rows.Clear();
            }




        }



        private void ActualizarEstadoCaja()
        {
            string numero = Program.NumeroCajaActual;


            if (!string.IsNullOrEmpty(numero) && numero != "Caja sin asignar")
            {
                txt_estadoCaja.Text = "Abierta";
                txt_estadoCaja.ForeColor = Color.Green;
            }
            else
            {
                txt_estadoCaja.Text = "Cerrada";
                txt_estadoCaja.ForeColor = Color.Red;
            }
        }

        /*private void MostrarCajaSinAsignar()
        {
            numeroCaja = null;

            txt_numero_caja.Text = "Caja sin asignar";
        }*/

        private static string NombreConPresentacion(string nombre, object presentacionVal)
        {
            var nom = nombre?.Trim() ?? "";
            int ml = 0;
            if (presentacionVal != null) int.TryParse(presentacionVal.ToString(), out ml);
            return ml > 0 ? $"{nom} {ml} ml" : nom;
        }


        private void BuscarYSeleccionarClientePorDocumento(long documento)
        {
            var cli = ClienteControlador.obtenerPorDni(documento); // si tenés otro método que admite CUIT, podés reemplazar acá

            if (cli == null)
            {
                // si no está, no habilitamos nada
                btn_imprimir_habilitado = false;
                return;
            }

            // guardo el cliente activo en el campo que ya usás en CrearFactura()
            clientefactura = cli;

            // reflejo datos en la UI
            txt_dni.Text = documento.ToString();
            txt_nombre_cliente.Text = $"{cli.nombre} {cli.apellido}".Trim();
            txt_condicion_iva.Text = cli.condicion_frente_al_iva ?? "Consumidor Final";
            txt_email.Text = cli.e_mail ?? "";

            // habilito imprimir (esto es lo que hoy controlás para permitir facturar)
            btn_imprimir_habilitado = true;

            // actualizo numeración y totales, como cuando termina tu btn_buscar_Click
            Num_factura_máximo();
            ActualizarTotales();
        }


        private void btn_buscar_Click(object sender, EventArgs e)
        {
            string numero = Program.NumeroCajaActual;


            if (numero != null && numero != "Caja sin asignar")
            {
                // Si hay caja asignada
                if (string.IsNullOrWhiteSpace(txt_dni.Text))
                {
                    MessageBox.Show("Ingrese un número de DNI o CUIT antes de buscar un cliente.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_dni.Text = "";
                    return;
                }
                // Validar DNI o CUIT
                if (!txt_dni.Text.All(char.IsDigit))
                {
                    MessageBox.Show("El DNI o CUIT solo puede contener números, sin guiones.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_dni.Text = "";
                    return;
                }

                if (txt_dni.Text.Length == 11)
                {
                    if (!ValidarCuit(txt_dni.Text))
                    {
                        MessageBox.Show("El CUIT ingresado no es válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txt_dni.Text = "";
                        return;
                    }
                }
                else if (txt_dni.Text.Length != 8)
                {
                    MessageBox.Show("El número ingresado debe tener 8 o 11 dígitos.", "Mensaje", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txt_dni.Text = "";
                    return;
                }

                long doc = long.Parse(txt_dni.Text);

                // 1) intento buscar en BD
                var cliente = ClienteControlador.obtenerPorDni(doc);
                if (cliente != null)
                {
                    // ✅ simulo "Buscar encontrado"
                    BuscarYSeleccionarClientePorDocumento(doc);
                    return;
                }

                // 2) no existe → abrir alta abreviada
                using (var formCrearClienteFactura = new FormCrearClienteFactura(doc))
                {
                    // si usás el helper con fondo (ModalHelper), mantenelo:
                    var dr = ModalHelper.MostrarModalConFondoOscuro(formCrearClienteFactura);

                    if (dr == DialogResult.OK)
                    {
                        // ✅ si tu FormCrearClienteFactura expone ClienteCreado, usalo:
                        var creado = formCrearClienteFactura.ClienteCreado ?? ClienteControlador.obtenerPorDni(doc);

                        if (creado != null)
                        {
                            // ✅ simulo "Buscar encontrado"
                            BuscarYSeleccionarClientePorDocumento(doc);
                            return;
                        }

                        // Si por algún motivo no vuelve creado, re-verifico igual:
                        BuscarYSeleccionarClientePorDocumento(doc);
                    }
                }
            }
            else
            {
                MessageBox.Show("Debes ingresar un número de caja.\n Haz click en 'Abrir Caja' ", "Número de Caja", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }


        private void txt_dni_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true; // evita el sonido 'ding'
                btn_buscar.PerformClick(); // llama al botón como si hicieras clic
            }
        }

        private bool ValidarCuit(string cuit)
        {
            if (cuit.Length != 11 || !cuit.All(char.IsDigit))
                return false;

            int[] pesos = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };
            int suma = 0;

            for (int i = 0; i < 10; i++)
            {
                suma += int.Parse(cuit[i].ToString()) * pesos[i];
            }

            int resto = suma % 11;
            int verificador = resto == 0 ? 0 : resto == 1 ? 9 : 11 - resto;

            return verificador == int.Parse(cuit[10].ToString());
        }



        private void btn_consultas_Click(object sender, EventArgs e)
        {
            string numero = Program.NumeroCajaActual;

            if (numero != null && numero != "Caja sin asignar")
            {
                FormConsultasPerfumeEmpleado consultasPerfumeEmpleado = new FormConsultasPerfumeEmpleado(this);
                ModalHelper.MostrarModalConFondoOscuro(consultasPerfumeEmpleado);


            }
            else
            {
                // No hay caja asignada, mostrar FormNumeroDeCaja para elegirla
                MessageBox.Show("Debes ingresar un número de caja.\n Haz click en 'Abrir Caja' ", "Número de Caja", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }


        private void DataGridViewFactura_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 8)
            {
                if (Factura.Rows.Count > 0 && e.RowIndex < Factura.Rows.Count)
                {
                    Factura.Rows.RemoveAt(e.RowIndex);
                    ActualizarTotales();
                }
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex == 2) // Botón +
            {
                DataGridViewRow fila = Factura.Rows[e.RowIndex];

                int perfumeId = Convert.ToInt32(fila.Cells["Id_Perfume"].Value);
                int stockDisponible = ObtenerStockDisponible(perfumeId);

                int cantActual = Convert.ToInt32(fila.Cells["Cantidad"].Value);

                if (cantActual + 1 > stockDisponible)
                {
                    MessageBox.Show(
                        $"No hay stock suficiente.\n" +
                        $"Stock disponible: {stockDisponible}",
                        "Stock insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                fila.Cells["Cantidad"].Value = cantActual + 1;

                int valorMultiplicador = Convert.ToInt32(fila.Cells["Cantidad"].Value);
                double precio = Convert.ToDouble(fila.Cells["Precio_Unitario"].Value);

                fila.Cells["Tot"].Value = (precio * valorMultiplicador).ToString();
                descuentoUnitario();
                ActualizarTotales();
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex == 3)
            {
                int cant = int.Parse(Factura.Rows[e.RowIndex].Cells[1].Value.ToString());
                if (int.Parse(Factura.Rows[e.RowIndex].Cells[1].Value.ToString()) > 1)
                {
                    Factura.Rows[e.RowIndex].Cells[1].Value = cant - 1;

                    int rowIndex = e.RowIndex;

                    int valorMultiplicador = Convert.ToInt32(Factura.Rows[rowIndex].Cells[1].Value);
                    double precio = Convert.ToDouble(Factura.Rows[rowIndex].Cells[5].Value);

                    Factura.Rows[e.RowIndex].Cells[7].Value = (precio * valorMultiplicador).ToString();
                    descuentoUnitario();
                    ActualizarTotales();

                }
                else if (Factura.Rows.Count > 0 && e.RowIndex < Factura.Rows.Count)
                {
                    Factura.Rows.RemoveAt(e.RowIndex);
                    descuentoUnitario();
                    ActualizarTotales();

                }

            }
        }

        public void descuentoUnitario()
        {
            DataGridView dgv = this.GetFacturaDataGrid();
            PerfumeEnPromoControlador promoController = new PerfumeEnPromoControlador();
            int descuentoPorcentaje = 0;

            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.Cells["Nombre_Perfume"].Value != null) // Verifica que la fila no esté vacía
                {
                    int perfumeId = Convert.ToInt32(row.Cells[0].Value); // ID del perfume
                    int descuentoUnitario = 2;
                    decimal descuentoMonto = 0;
                    decimal totalConDescuento = 0;
                    // Obtener precio unitario
                    decimal precioUnitario = Convert.ToDecimal(row.Cells["Precio_Unitario"].Value);

                    if (row.Cells["Cantidad"].Value != null && int.TryParse(row.Cells["Cantidad"].Value.ToString(), out int cantidad))
                    {
                        if (cantidad % 2 == 0)
                        {
                            descuentoUnitario = 0; //Cambiamos el valor para que no se aplique el descuento unitario porque la cantidad es par

                            // Obtener el descuento del perfume (en porcentaje)
                            descuentoPorcentaje = promoController.obtenerMayorDescuentoPorPerfume(perfumeId) ?? 0; // CAMBIAR METODO TIENE QUE SER MAYOR A 20%
                            Console.WriteLine($"Perfume ID: {perfumeId}, Descuento Porcentaje Obtenido aca: {descuentoPorcentaje}%");

                            if (descuentoPorcentaje > 20)
                            {
                                Console.WriteLine($"Perfume ID: {perfumeId}, Descuento Porcentaje Obtenido first: {descuentoPorcentaje}%");


                                // Obtener cantidad
                                cantidad = Convert.ToInt32(row.Cells["Cantidad"].Value);

                                // Calcular el monto de descuento
                                descuentoMonto = ((precioUnitario * descuentoPorcentaje) / 100) * cantidad;

                                // Mostrar el monto de descuento en la celda "Descuento" (valor nominal)
                                row.Cells["Descuento"].Value = descuentoMonto;

                                // Calcular el total con descuento
                                totalConDescuento = ((precioUnitario * cantidad) - descuentoMonto);

                                // Actualizar el total en el DataGridView
                                row.Cells["Tot"].Value = totalConDescuento;

                                // Mostrar en consola para depuración
                                Console.WriteLine($"Perfume ID: {perfumeId}, Descuento Aplicado: {descuentoMonto} (Monto), Total con Descuento: {totalConDescuento}");
                            }
                            else
                            {
                                descuentoUnitario = 2;
                            }
                        }
                        else
                        {
                            // Obtener el descuento del perfume (en porcentaje) solo si es mayor a 20%
                            descuentoPorcentaje = promoController.obtenerMayorDescuentoPorPerfume(perfumeId) ?? 0;

                            // Solo aplicar el descuento si es mayor a 20%
                            if (descuentoPorcentaje > 20)
                            {
                                Console.WriteLine($"Perfume ID: {perfumeId}, Descuento Porcentaje Obtenido: {descuentoPorcentaje}%");

                                // Obtener cantidad
                                cantidad = Convert.ToInt32(row.Cells["Cantidad"].Value);

                                // Verificar si la cantidad es mayor que 1 antes de aplicar descuento
                                if (cantidad > 1)
                                {
                                    // Calcular cuántas veces se aplicará el descuento (por cada par de unidades)
                                    int cantidadDescuentos = cantidad / 2;  // Dividir entre 2 para saber cuántos pares de unidades hay

                                    cantidadDescuentos = cantidadDescuentos * 2;  // multiplicar por 2 para saber cuantos descuentos aplicar

                                    // Calcular el monto de descuento por cada par de unidades
                                    decimal descuentoMontoPorPar = (precioUnitario * descuentoPorcentaje) / 100;

                                    // Calcular el descuento total
                                    descuentoMonto = descuentoMontoPorPar * cantidadDescuentos;
                                    Console.WriteLine($"descuentoMonto : {descuentoMonto}, cantidadDescuentos: {cantidadDescuentos} , descuentoMontoPorPar: {descuentoMontoPorPar}");

                                    // Mostrar el monto de descuento en la celda "Descuento" (valor nominal)
                                    row.Cells["Descuento"].Value = descuentoMonto;

                                    // Calcular el total con descuento
                                    totalConDescuento = ((precioUnitario * cantidad) - descuentoMonto);

                                    // Actualizar el total en el DataGridView
                                    row.Cells["Tot"].Value = totalConDescuento;

                                    descuentoUnitario = 1; //Se utiliza para verificar si se debe aplicar algun descuento unitario ya que la cantidad es impar y mayor a 3

                                    // Mostrar en consola para depuración
                                    Console.WriteLine($"Perfume ID: {perfumeId}, Descuento Aplicado: {descuentoMonto} (Monto), Total con Descuento: {totalConDescuento}");
                                }

                            }
                            if (descuentoUnitario == 1) //Descuento del 10% cuando es impar mayor a 1
                            {
                                // Obtener el descuento del perfume (en porcentaje)
                                descuentoPorcentaje = promoController.obtenerPromocionPorPerfumeConDescuento10(perfumeId) ?? 0;


                                Console.WriteLine($"Perfume ID: {perfumeId}, Descuento Porcentaje Obtenido 1: {descuentoPorcentaje}%");


                                // Obtener cantidad
                                cantidad = Convert.ToInt32(row.Cells["Cantidad"].Value);

                                // Calcular el monto de descuento
                                descuentoMonto += (((precioUnitario * descuentoPorcentaje) / 100));

                                // Mostrar el monto de descuento en la celda "Descuento" (valor nominal)
                                row.Cells["Descuento"].Value = descuentoMonto;

                                // Calcular el total con descuento
                                totalConDescuento = ((precioUnitario * cantidad) - descuentoMonto);

                                // Actualizar el total en el DataGridView
                                row.Cells["Tot"].Value = totalConDescuento;

                                // Mostrar en consola para depuración
                                Console.WriteLine($"Perfume ID: {perfumeId}, Descuento Aplicado: {descuentoMonto} (Monto), Total con Descuento: {totalConDescuento}");
                            }
                        }
                        if (descuentoUnitario == 2) //Descuento cuando no tiene descuento mayor a 20%
                            {
                                // Obtener el descuento del perfume (en porcentaje)
                                descuentoPorcentaje = promoController.obtenerPromocionPorPerfumeConDescuento10(perfumeId) ?? 0;

                                Console.WriteLine($"Perfume ID: {perfumeId}, Descuento Porcentaje Obtenido 2: {descuentoPorcentaje}%");

                                // Obtener cantidad
                                cantidad = Convert.ToInt32(row.Cells["Cantidad"].Value);

                                // Calcular el monto de descuento
                                descuentoMonto = (((precioUnitario * descuentoPorcentaje) / 100) * cantidad);

                                // Mostrar el monto de descuento en la celda "Descuento" (valor nominal)
                                row.Cells["Descuento"].Value = descuentoMonto;

                                // Calcular el total con descuento
                                totalConDescuento = ((precioUnitario * cantidad) - descuentoMonto);

                                // Actualizar el total en el DataGridView
                                row.Cells["Tot"].Value = totalConDescuento;

                                // Mostrar en consola para depuración
                                Console.WriteLine($"Perfume ID: {perfumeId}, Descuento Aplicado: {descuentoMonto} (Monto), Total con Descuento: {totalConDescuento}");
                            }
                        

                    }
                }
            }
        }

        private void totalFactura()
        {
            decimal sumaPrecios = 0m;

            foreach (DataGridViewRow fila in Factura.Rows)
            {
                if (fila.IsNewRow)
                    continue;

                // "Tot" es la columna total de la fila
                var celdaTot = fila.Cells["Tot"].Value;
                if (celdaTot == null)
                    continue;

                decimal precioFila;
                if (decimal.TryParse(celdaTot.ToString(), NumberStyles.Any, Ar, out precioFila))
                {
                    sumaPrecios += precioFila;
                }
            }

            // txt_subtotal = suma de la columna Tot del DGV Factura
            txt_subtotal.Text = sumaPrecios.ToString("N2", Ar);
        }

   

        private void CalcularImporteRecargo(float subtotal, float recargo)
        {
            txt_monto_recargo.Text = (subtotal * recargo / 100).ToString("N2");
        }

        private void desc()
        {
            string formaPago = combo_forma_pago.SelectedItem?.ToString() ?? "";

            // Descuento SOLO si la forma de pago es EFECTIVO
            if (formaPago != "Efectivo")
            {
                txt_desc.Text = "0";
                txt_monto_descuento.Text = "0,00";
                return;
            }

            if (combo_descuento.SelectedItem == null)
            {
                txt_desc.Text = "0";
                txt_monto_descuento.Text = "0,00";
                return;
            }

            string descuentoStr = combo_descuento.SelectedItem.ToString();

            if (int.TryParse(descuentoStr, out int descuento))
            {
                txt_desc.Text = descuento.ToString(); // Mostrar el descuento en txt_desc

                if (float.TryParse(txt_subtotal.Text, out float subtotal))
                {
                    CalcularDescuento(descuento, subtotal);
                }
                else
                {
                    txt_monto_descuento.Text = "0,00";
                }
            }
            else
            {
                MessageBox.Show("El valor de descuento no es válido.");
            }
        }



        private void CalcularDescuento(int desc, float subtotal)
        {
            if (subtotal > 0)
            {
                txt_monto_descuento.Text = (desc * subtotal / 100).ToString("N2");
            }
            else
            {
                txt_monto_descuento.Text = "0,00";
            }
        }

        private void sumaFinal(float subtotal, float recargo, float descuento)
        {
            string condicionCliente = txt_condicion_iva.Text.Trim();

            // 🔹 Total con IVA incluido (lo que realmente se cobra)
            float totalConIva = subtotal + recargo - descuento;

            // Mostrar el total en el textbox usando la cultura AR
            txt_total.Text = totalConIva.ToString("N2", Ar);

            // Solo discriminamos IVA para Responsable Inscripto
            if (condicionCliente.Contains("Responsable Inscripto"))
            {
                decimal totalDec = (decimal)totalConIva;

                // Base imponible = Total / 1.21
                decimal baseImponibleDec = Math.Round(totalDec / 1.21m, 2, MidpointRounding.AwayFromZero);

                // IVA = Total - Base imponible
                decimal ivaDec = Math.Round(totalDec - baseImponibleDec, 2, MidpointRounding.AwayFromZero);

                // ❗ YA NO TOCAMOS txt_subtotal: queda como suma de Tot
                txt_iva.Text = ivaDec.ToString("N2", Ar);
            }
            else
            {
                // Consumidor Final / Exento / Monotributista: no se discrimina IVA
                txt_iva.Text = 0m.ToString("N2", Ar);
            }
        }






        public void ActualizarTotales()
        {
            // 1) Recalcular subtotal (suma de filas)
            totalFactura();
            // 2) Recalcular descuento general según combo_descuento
            desc();
            // 3) Recalcular recargo según forma de pago + cuotas seleccionadas
            string forma = combo_forma_pago.SelectedItem?.ToString() ?? "";
            if (forma == "Efectivo")
            {
                // En efectivo no hay recargo
                txt_monto_recargo.Text = "0,00";
                txt_rec.Text = "0";
            }
            else
            {
                // Tarjetas / otros medios → recargo según cuotas
                CalcularRecargo();
            }
            // 4) Tomar valores de los textbox y recalcular total + IVA
            float subtotal, recargo, descuento;
            if (!float.TryParse(txt_subtotal.Text, out subtotal)) subtotal = 0;
            if (!float.TryParse(txt_monto_recargo.Text, out recargo)) recargo = 0;
            if (!float.TryParse(txt_monto_descuento.Text, out descuento)) descuento = 0;
            sumaFinal(subtotal, recargo, descuento);

            // 🔹 Si había un pago en efectivo cargado, lo limpiamos porque cambió el total
            LimpiarPagoEfectivoSiHayDatos();
        }


        public DataGridView GetFacturaDataGrid()
        {
            //Ocultas la primera columna de la tabla (es una columna de seleccion de fila)
            Factura.RowHeadersVisible = false;

            Factura.CellPainting += Factura_CellPainting;

            return Factura;
        }

        private void ActualizarDescuentosYCuotas()
        {
            string formaPago = combo_forma_pago.SelectedItem.ToString();

            // Reiniciamos combos
            combo_cuotas.Items.Clear();
            combo_descuento.Items.Clear();
            combo_descuento.Items.AddRange(new object[] { 0, 10, 15 });

            if (formaPago == "Efectivo")
            {
                // Efectivo: sin cuotas, permite descuentos
                txt_desc.Text = "0";
                combo_descuento.SelectedIndex = 0;
                combo_cuotas.Items.Add(1);
                combo_cuotas.SelectedIndex = 0;
                combo_cuotas.Enabled = false;
                combo_descuento.Enabled = true;
            }
            else if (formaPago == "Mercado Pago" || formaPago == "Visa Débito")
            {
                // Débito / Mercado Pago: solo 1 cuota, sin descuentos
                txt_desc.Text = "0";
                combo_descuento.SelectedIndex = 0;
                combo_cuotas.Items.Add(1);
                combo_cuotas.SelectedIndex = 0;
                combo_cuotas.Enabled = false;
                combo_descuento.Enabled = false;
            }
            else if (formaPago == "Amex")
            {
                // Amex: solo 1, 6 y 12 cuotas
                txt_desc.Text = "0";
                combo_descuento.SelectedIndex = 0;
                combo_cuotas.Items.AddRange(new object[] { 1, 6, 12 });
                combo_cuotas.SelectedIndex = 0;
                combo_cuotas.Enabled = true;
                combo_descuento.Enabled = false;
            }
            else if (formaPago == "Visa Crédito" || formaPago == "Mastercard")
            {
                // Visa crédito o Mastercard: todas las cuotas
                txt_desc.Text = "0";
                combo_descuento.SelectedIndex = 0;
                combo_cuotas.Items.AddRange(new object[] { 1, 3, 6, 9, 12 });
                combo_cuotas.SelectedIndex = 0;
                combo_cuotas.Enabled = true;
                combo_descuento.Enabled = false;
            }
            else
            {
                // Otros medios (por seguridad)
                txt_desc.Text = "0";
                combo_descuento.SelectedIndex = 0;
                combo_cuotas.Items.Add(1);
                combo_cuotas.SelectedIndex = 0;
                combo_cuotas.Enabled = false;
                combo_descuento.Enabled = false;
            }
        }

        private void ActualizarUIFormaPago()
        {
            string forma = combo_forma_pago.SelectedItem?.ToString() ?? "";
            bool esEfectivo = forma == "Efectivo";

            if (esEfectivo)
            {
                // Siempre que entro a EFECTIVO, muestro los controles y los dejo limpios
                txt_ing_pago.Visible = true;
                txt_vuelto.Visible = true;
                lbl_vuelto.Visible = true;
                lbl_pesos_1.Visible = true;
                lbl_pesos_2.Visible = true;
                btn_ok.Visible = true;

                // Limpiar valores al volver a efectivo
                txt_ing_pago.Text = "";
                txt_vuelto.Text = "0,00";

                // Ocultar cuotas (no tiene sentido en efectivo)
                combo_cuotas.Visible = false;
                lbl_cuotas.Text = "Ing. Pago";
                lbl_cuotas.Location = new Point(1211, 390);

                // En efectivo no usamos btn_pago de tarjeta
                btn_pago.Visible = false;
                btn_imprimir.Visible = false;
            }
            else
            {
                // Al salir de EFECTIVO, oculto y LIMPIO el importe y el vuelto
                txt_ing_pago.Visible = false;
                txt_vuelto.Visible = false;
                lbl_vuelto.Visible = false;
                lbl_pesos_1.Visible = false;
                lbl_pesos_2.Visible = false;
                btn_ok.Visible = false;

                txt_ing_pago.Text = "";
                txt_vuelto.Text = "0,00";

                // Mostrar cuotas de nuevo
                combo_cuotas.Visible = true;
                lbl_cuotas.Text = "Cuotas";
                lbl_cuotas.Location = lblCuotasPosOriginal;

                // Lógica de botón de pago/imprimir para tarjetas u otros medios
                btn_imprimir.Visible = false;
                btn_pago.Visible = true;
            }
        }




        // Método para calcular el recargo según las cuotas seleccionadas
        private void CalcularRecargo()
        {
            if (combo_cuotas.SelectedItem == null)
            {
                txt_monto_recargo.Text = "0,00";
                txt_rec.Text = "0";
                return;
            }

            string formaPago = combo_forma_pago.SelectedItem?.ToString() ?? "";
            string cuotasStr = combo_cuotas.SelectedItem.ToString().Trim();
            int cuotas;

            if (!int.TryParse(cuotasStr, out cuotas))
            {
                MessageBox.Show("El valor de cuotas no es válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal recargoPct = 0m; // valor por defecto

            // Solo aplicamos recargo a tarjetas de crédito
            bool esTarjetaCredito = formaPago == "Visa Crédito" || formaPago == "Mastercard" || formaPago == "Amex";

            if (esTarjetaCredito)
            {
                switch (cuotas)
                {
                    case 1:
                    case 3:
                        recargoPct = 0m;
                        break;
                    case 6:
                        recargoPct = 10m;
                        break;
                    case 9:
                        recargoPct = 15m;
                        break;
                    case 12:
                        recargoPct = 20m;
                        break;
                    default:
                        recargoPct = 0m;
                        break;
                }
            }
            else
            {
                // Efectivo, débito, Mercado Pago, etc. => sin recargo
                recargoPct = 0m;
            }

            // Mostrar el porcentaje en el textbox
            txt_rec.Text = recargoPct.ToString("0.##");

            // Obtener el subtotal desde el textbox
            float subtotal;
            if (float.TryParse(txt_subtotal.Text, out subtotal))
            {
                CalcularImporteRecargo(subtotal, (float)recargoPct);
            }
            else
            {
                txt_monto_recargo.Text = "0,00";
            }
        }


        private string tipo_de_factura()
        {
            string cond = (txt_condicion_iva.Text ?? "").Trim().ToLowerInvariant();
            // Factura A solo para Responsable Inscripto
            if (cond.Contains("responsable inscripto"))
                return "A";
            return "B"; // monotributista, exento, consumidor final => B
        }


        private void combo_forma_pago_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarDescuentosYCuotas();
            ActualizarTotales();

            string forma = combo_forma_pago.SelectedItem.ToString();

            if (forma != "Efectivo")
            {
                btn_imprimir.Visible = false;
                btn_pago.Visible = true;
            }
            else
            {
                btn_imprimir.Visible = true;
                btn_pago.Visible = false;
            }

            // 🔹 Ajustar visibilidad de los nuevos controles según la forma de pago
            ActualizarUIFormaPago();
        }

        private void btn_pago_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Pago realizado exitosamente", "Confirmación", MessageBoxButtons.OK, MessageBoxIcon.Information);
            btn_imprimir.Visible = true;
            btn_pago.Visible = false;
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            // 0) Verificar que haya al menos un artículo cargado
            if (Factura.Rows.Count == 0)
            {
                MessageBox.Show("Debe agregar al menos un artículo para facturar.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                // Aseguramos que quede EFECTIVO seleccionado y visible
                combo_forma_pago.SelectedItem = "Efectivo";       
                ActualizarDescuentosYCuotas();
                ActualizarUIFormaPago();

                // Limpiamos por si quedó algo en el pago/vuelto
                txt_ing_pago.Text = "";
                txt_vuelto.Text = "0,00";

                return;
            }

            // 1) Validar que haya un importe ingresado
            if (string.IsNullOrWhiteSpace(txt_ing_pago.Text))
            {
                MessageBox.Show("Ingrese el monto recibido en efectivo.",
                                "Pago en efectivo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2) Parsear el total de la factura
            if (!decimal.TryParse(txt_total.Text, NumberStyles.Any, Ar, out decimal total))
            {
                MessageBox.Show("El total de la factura no es válido.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 3) Parsear el monto ingresado
            if (!decimal.TryParse(txt_ing_pago.Text, NumberStyles.Any, Ar, out decimal pagado))
            {
                MessageBox.Show("El monto ingresado no es válido.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 4) Validar que el pago alcance el total
            if (pagado < total)
            {
                MessageBox.Show("El monto ingresado es menor al total de la factura.",
                                "Pago insuficiente",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                return;
            }

            // 5) Si todo está correcto, hacemos lo mismo que "Imprimir"
            btn_imprimir_Click(sender, e);

        }


        private void combo_cuotas_SelectedIndexChanged(object sender, EventArgs e)
        {
            CalcularRecargo();
            ActualizarTotales();
        }

        private void combo_descuento_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combo_descuento.SelectedItem != null && int.TryParse(combo_descuento.SelectedItem.ToString(), out int descuento))
            {
                txt_desc.Text = descuento.ToString(); // Establece el valor en txt_desc
            }
            else
            {
                // Si el valor seleccionado no es un número entero, establece el texto en 0
                txt_desc.Text = "0";
            }
            ActualizarTotales();
        }


        private void CrearFactura()
        {
            try
            {
                Empleado empleadoAFacturar = new Empleado();

                int id = FacturaControlador.ObtenerProximoIdFactura();
                DateTime fecha = DateTime.Now;
                int sucursalId = Program.sucursal;
                int vendedorId = Program.logueado.id;

                int clienteId = clientefactura.id;
                if (clienteId == 0)
                {
                    clienteId = 1; // Cliente "Consumidor Final" por defecto
                }

                string formaDePago = combo_forma_pago.SelectedItem.ToString();

                // Usamos decimal + CultureInfo Ar para evitar problemas de comas/puntos
                decimal precioTotal = decimal.Parse(txt_total.Text, Ar);
                decimal recargoTarjeta = decimal.Parse(txt_monto_recargo.Text, Ar);

                // Bonificación por forma de pago (efectivo)
                decimal bonificacion = decimal.Parse(txt_monto_descuento.Text, Ar);

                // 🔹 Descuento por PROMOCIÓN (acumulado en Factura A/B)
                // Asegurate de tener esto declarado arriba en la clase:
                // private double totalDescuentoPromoFactura = 0.0;
                decimal descuentoPromo = Math.Round(
                                              (decimal)totalDescuentoPromoFactura,
                                              2,
                                              MidpointRounding.AwayFromZero);

                // 🔹 Descuento TOTAL = promo + bonificación (redondeado a 2 decimales)
                decimal descuentoTotal = Math.Round(
                                              bonificacion + descuentoPromo,
                                              2,
                                              MidpointRounding.AwayFromZero);

                int numeroDeCaja = int.Parse(txt_numero_caja.Text);

                string tipoConsumidor = clientefactura.condicion_frente_al_iva;
                if (string.IsNullOrEmpty(tipoConsumidor))
                {
                    tipoConsumidor = "Consumidor Final";
                }

                string origen = "Local";
                string facturaPdf = "";
                string numFactura = txt_numero_factura.Text;
                string tipoDeFactura = tipo_de_factura();

                // Llamar al método crearFactura desde FacturaControlador
                bool exito = FacturaControlador.crearFactura(
                    id,
                    fecha,
                    sucursalId,
                    vendedorId,
                    clienteId,
                    formaDePago,
                    (double)precioTotal,
                    (double)recargoTarjeta,
                    (double)descuentoTotal,  // ⬅️ AQUÍ VA PROMO + BONIFICACIÓN
                    numeroDeCaja,
                    tipoConsumidor,
                    origen,
                    facturaPdf,
                    numFactura,
                    tipoDeFactura
                );

                if (exito)
                {
                    MessageBox.Show("Factura creada exitosamente.");
                }
                else
                {
                    MessageBox.Show("Hubo un problema al crear la factura.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear la factura: " + ex.Message);
            }
        }


        private void CrearDetalleFactura()
        {
            try
            {
                // Verificar si numFactura es un número entero válido
                int numFactura = 0;
                if (!int.TryParse(txt_numero_factura.Text, out numFactura))
                {
                    MessageBox.Show("El número de factura no es válido.");
                    return;
                }
                PerfumeEnPromoControlador promoController = new PerfumeEnPromoControlador();
                // Recorrer las filas del DataGridView
                foreach (DataGridViewRow row in Factura.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        // Verificar si perfume_id es un número entero válido
                        int perfume_id = 0;
                        if (!int.TryParse(row.Cells["Id_Perfume"].Value.ToString(), out perfume_id))
                        {
                            MessageBox.Show($"El ID del perfume en la fila no es válido.");
                            return;
                        }

                        // Verificar si cantidad es un número entero válido
                        int cantidad = 0;
                        if (!int.TryParse(row.Cells["cantidad"].Value.ToString(), out cantidad) || cantidad <= 0)
                        {
                            MessageBox.Show($"La cantidad en la fila del perfume ID {perfume_id} no es un número válido o es menor o igual a cero.");
                            return;
                        }

                        // Verificar si el precio_unitario es un valor float válido
                        float precio_unitario = 0f;
                        if (!float.TryParse(row.Cells["precio_unitario"].Value.ToString(), out precio_unitario) || precio_unitario <= 0)
                        {
                            MessageBox.Show($"El precio unitario en la fila del perfume ID {perfume_id} no es un número válido o es menor o igual a cero.");
                            return;
                        }
                        int? promocion_id = promoController.obtenerPromocionIdPorPerfume(perfume_id);
                        int? promocion2_id = promoController.obtenerPromocionIdPorPerfumeConDescuento10(perfume_id);
                        //int? promocion_id10 = promoController.obtenerPromocionIdPorPerfumeConDescuento10(perfume_id);

                        //ACA AGREGAR LOGICA PARA GUARDAR SI HAY UNA SEGUNDA PROMOCION!!!! MAXI

                        if (cantidad > 1 && promocion_id != null)
                        {
                            if (cantidad % 2 == 0)
                            {
                                promocion2_id = 1;
                               // MessageBox.Show($"PromocionID: {promocion_id} y PromocionID2: {promocion2_id}");
                            }
                        }
                        else if (cantidad > 1 && cantidad % 2 == 0 && promocion2_id != null)
                        {
                                promocion_id = 1;
                            
                        }
                        else if (cantidad == 1 && promocion2_id != null)
                        {
                            promocion_id = promoController.obtenerPromocionIdPorPerfumeConDescuento10(perfume_id);
                            promocion2_id = 1;
                            //MessageBox.Show($"PromocionID: {promocion_id} y PromocionID2: {promocion2_id}");
                        }
                        else if (promocion_id != null && promocion2_id != null)
                        {
                            promocion_id = 1;
                            promocion2_id = 1;
                           // MessageBox.Show($"PromocionID: {promocion_id} y PromocionID2: {promocion2_id}");
                        }
                        else
                        {
                            promocion_id = 1;
                            promocion2_id = 1;
                        }

                        int id_factura = FacturaControlador.ObtenerMaxIdFactura();
                        //MessageBox.Show("Numero de factura:" + id_factura);

                        //MessageBox.Show($"Enviando datos2: NumFactura: {id_factura}, PerfumeID: {perfume_id}, Cantidad: {cantidad}, PrecioUnitario: {precio_unitario}, PromocionID: {promocion_id}");

                        bool exito = DetalleFacturaControlador.crearDetalleFactura(id_factura, perfume_id, cantidad, precio_unitario, promocion_id, promocion2_id);

                        if (!exito)
                        {
                            MessageBox.Show($"Error al agregar el perfume ID {perfume_id} al detalle de la factura.");
                            return;
                        }

                        int sucursalId = Program.sucursal;
                        StockControlador.updateStock(perfume_id, sucursalId, -cantidad);
                        //MessageBox.Show($"Actualizando stock de perfume {perfume_id} en sucursal {sucursalId} con cantidad {-cantidad}");

                    }
                }

                //MessageBox.Show("Detalle de factura creado exitosamente.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear el detalle de la factura: " + ex.Message);
            }
        }

        private static string FormatearDomicilio(Cliente c)
        {
            // Nombre de calle (si existe y no es “sin dato”)
            var calle = c?.calle_id?.nombre?.Trim();
            var tieneCalle = !string.IsNullOrWhiteSpace(calle) &&
                             !calle.Equals("sin dato", StringComparison.OrdinalIgnoreCase) &&
                             !calle.Equals("sin calle", StringComparison.OrdinalIgnoreCase);

            // Numeración válida (> 0)
            int? num = c?.numeracion_calle;
            var tieneNumero = num.HasValue && num.Value > 0;

            if (tieneCalle && tieneNumero) return $"{calle} {num.Value}";
            if (tieneCalle) return calle;               // hay calle pero número vacío/0
            return "SIN DATO";                                     // no hay datos
        }

        private static void RellenarDatosSucursalEnEncabezado(ref string html)
        {
            // Sucursal configurada en el config.json y cargada en Program.sucursal
            int sucursalId = Program.sucursal;

            Sucursal suc = SucursalControlador.getById(sucursalId);

            string dirSucursal = "SIN DOMICILIO";
            string locPaisSuc = "SIN LOCALIDAD";

            if (suc != null)
            {
                // ==== Calle + número ====
                string calle = suc.calle_id?.nombre?.Trim();

                // Si numeracion_calle es int "normal"
                int numero = suc.numeracion_calle;

                if (!string.IsNullOrWhiteSpace(calle))
                {
                    dirSucursal = numero > 0 ? $"{calle} {numero}" : calle;
                }

                // ==== Localidad y país ====
                string localidad = suc.localidad_id?.nombre?.Trim();
                string pais = suc.pais_id?.nombre?.Trim();

                if (!string.IsNullOrWhiteSpace(localidad) || !string.IsNullOrWhiteSpace(pais))
                {
                    if (!string.IsNullOrWhiteSpace(localidad) && !string.IsNullOrWhiteSpace(pais))
                        locPaisSuc = $"{localidad}, {pais}";
                    else
                        locPaisSuc = !string.IsNullOrWhiteSpace(localidad) ? localidad : pais;
                }
            }

            // Reemplazar los placeholders en la plantilla HTML
            html = html.Replace("@DIR_SUCURSAL", dirSucursal);
            html = html.Replace("@LOC_PAIS_SUC", locPaisSuc);
        }


        private void btn_imprimir_Click(object sender, EventArgs e)
        {
            string numero = Program.NumeroCajaActual;

            if (!btn_imprimir_habilitado)
            {
                MessageBox.Show("Debe buscar un cliente por su dni antes de imprimir.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (Factura.Rows.Count == 0)
            {
                MessageBox.Show("Debe agregar al menos un artículo para facturar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (numero == null || numero == "Caja sin asignar")
            {
                MessageBox.Show("\"Debes ingresar un número de caja. \n Haz click en 'Abrir Caja' ", "Número de Caja", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string condicionCliente = txt_condicion_iva.Text.Trim();
            string PaginaHTML_Texto = "";

            // Carpeta segura para guardar facturas (carpeta de usuario)
            string carpetaFacturas = ObtenerCarpetaFacturas();

            string rutaFactura = Path.Combine(carpetaFacturas, $"Factura_Orden_{txt_numero_factura.Text}.pdf");
            string filePath = rutaFactura;


            // ---------------------------
            // FACTURA B (Consumidor Final / Exento / Monotributista)
            // ---------------------------
            if (condicionCliente == "Consumidor Final" || condicionCliente == "Exento" || condicionCliente == "Monotributista")
            {
                PaginaHTML_Texto = Properties.Resources.PlantillaFactura.ToString();

                string dni = txt_dni.Text;
                string localidad = "SIN DATO";
                string domicilio = "SIN DATO";
                string numeracion_calle = "SIN DATO";
                string domicilioEntero = "SIN DATO";

                if (string.IsNullOrWhiteSpace(dni))
                {
                    dni = "SIN DATO";
                }
                else
                {
                    long dniNumerico;
                    localidad = "SIN DATO";
                    domicilio = "SIN DATO";
                    numeracion_calle = "SIN DATO";
                    if (!long.TryParse(dni, out dniNumerico))
                    {
                        MessageBox.Show("El DNI ingresado no es válido.");
                        return;
                    }
                    dniNumerico = long.Parse(dni);
                    Cliente cliente = ClienteControlador.obtenerPorDni(dniNumerico);
                    if (cliente?.localidad_id != null) localidad = cliente.localidad_id.nombre;

                    domicilioEntero = FormatearDomicilio(cliente);
                }

                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@CLIENTE", txt_nombre_cliente.Text);
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@DOCUMENTO", dni);
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@NUMEROFACTURA", txt_numero_factura.Text);
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@FECHA", DateTime.Now.ToString("dd/MM/yyyy"));
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@CONDIVA", txt_condicion_iva.Text);
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@FORMAPAGO", combo_forma_pago.SelectedItem.ToString());
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@DOMICILIO", domicilioEntero);
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@LOCALIDAD", localidad);

                // 🔹 Completar Dirección y Localidad/Pais de la sucursal
                RellenarDatosSucursalEnEncabezado(ref PaginaHTML_Texto);

                // ===== Fila dinámica de "Forma de Pago" (+ "Cuotas" si es tarjeta) =====
                var forma = combo_forma_pago.SelectedItem?.ToString() ?? "";
                var cuotasSel = combo_cuotas.SelectedItem?.ToString() ?? "1";
                bool esTarjeta = forma == "Visa Crédito" || forma == "Mastercard" || forma == "Amex";

                string rowFormaPago;
                if (esTarjeta)
                {
                    // 2 recuadros: Forma de Pago | Cuotas
                    rowFormaPago = @"
        <tr>
          <td style='background:#F6DDE6; font-weight:bold;'>Forma de Pago:</td>
          <td style='width:40%;'>" + System.Net.WebUtility.HtmlEncode(forma) + @"</td>
          <td style='width:20%; background:#F6DDE6; font-weight:bold;'>Cuotas:</td>
          <td>" + System.Net.WebUtility.HtmlEncode(cuotasSel) + @"</td>
        </tr>";
                }
                else
                {
                    // Un solo recuadro ocupando todo el ancho a la derecha
                    rowFormaPago = @"
        <tr>
          <td style='background:#F6DDE6; font-weight:bold;'>Forma de Pago:</td>
          <td colspan='3'>" + System.Net.WebUtility.HtmlEncode(forma) + @"</td>
        </tr>";
                }
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@ROW_FORMA_PAGO", rowFormaPago);
                // ======================================================================

                // Filas del detalle
                string filas = string.Empty;
                decimal total = 0m;             // suma de Tot (precio final por fila)
                decimal totalDescPromo = 0m;    // suma de descuento PROMO de cada fila

                foreach (DataGridViewRow row in Factura.Rows)
                {
                    var cant = Convert.ToInt32(row.Cells["Cantidad"].Value);
                    var desc = row.Cells["Nombre_Perfume"].Value?.ToString() ?? "";
                    var unit = Convert.ToDecimal(row.Cells["Precio_Unitario"].Value);

                    // Descuento por promoción de ESA fila
                    var descMonto = Convert.ToDecimal(row.Cells["Descuento"].Value ?? 0m);

                    // Total de la fila ya con promo aplicada
                    var tot = Convert.ToDecimal(row.Cells["Tot"].Value);

                    filas += $@"
        <tr>
         <td class='cant'>{Num(cant)}</td>
         <td>{System.Net.WebUtility.HtmlEncode(desc)}</td>
         <td class='money'>{Mon(unit)}</td>
         <td class='money'>{Mon(descMonto)}</td>
         <td class='money'>{Mon(tot)}</td>
        </tr>";

                    total += tot;
                    totalDescPromo += descMonto;
                }

                // 🔹 Guardar la suma de DESCUENTO PROMO para la BD (se suma con bonificación en CrearFactura)
                totalDescuentoPromoFactura = (double)totalDescPromo;

                // 🔹 Recargo y bonificación (descuento por forma de pago) como decimal, sin pasar por double
                decimal recargoTarjetaDec = 0m;
                decimal bonificacionDec = 0m;   // bonificación por EFECTIVO, etc.

                decimal.TryParse(txt_monto_recargo.Text, NumberStyles.Any, Ar, out recargoTarjetaDec);
                decimal.TryParse(txt_monto_descuento.Text, NumberStyles.Any, Ar, out bonificacionDec);

                // 🔹 Total de la factura:
                //     Total = suma Tot (ya con promo)
                //           - bonificación (forma de pago)
                //           + recargo (tarjeta/cuotas)
                decimal totalFactura = total + recargoTarjetaDec - bonificacionDec;

                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@FILAS", filas);
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@RECARGO", Mon(recargoTarjetaDec));
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@DESCUENTO", Mon(bonificacionDec));
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@TOTAL", Mon(totalFactura));
            }
            // ---------------------------
            // FACTURA A (Responsable Inscripto)
            // ---------------------------
            else
            {
                PaginaHTML_Texto = Properties.Resources.FacturaA.ToString();

                string dni = txt_dni.Text;
                string localidad = "Sin localidad";
                string domicilio = "Sin calle";
                string numeracion_calle = "Sin numeración";
                string domicilioEntero = "Sin domicilio";

                if (string.IsNullOrWhiteSpace(dni))
                {
                    dni = "Sin DNI";
                }
                else
                {
                    long dniNumerico;
                    localidad = "Sin localidad";
                    domicilio = "Sin calle";
                    numeracion_calle = "Sin numeración";
                    if (!long.TryParse(dni, out dniNumerico))
                    {
                        MessageBox.Show("El DNI ingresado no es válido.");
                        return;
                    }
                    dniNumerico = long.Parse(dni);
                    Cliente cliente = ClienteControlador.obtenerPorDni(dniNumerico);
                    if (cliente?.localidad_id != null) localidad = cliente.localidad_id.nombre;

                    domicilioEntero = FormatearDomicilio(cliente);
                }

                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@CLIENTE", txt_nombre_cliente.Text);
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@DOCUMENTO", dni);
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@NUMEROFACTURA", txt_numero_factura.Text);
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@FECHA", DateTime.Now.ToString("dd/MM/yyyy"));
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@CONDIVA", txt_condicion_iva.Text);
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@FORMAPAGO", combo_forma_pago.SelectedItem.ToString());
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@DOMICILIO", domicilioEntero);
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@LOCALIDAD", localidad);

                // 🔹 Dirección y Localidad/Pais de la sucursal
                RellenarDatosSucursalEnEncabezado(ref PaginaHTML_Texto);

                // ===== Fila dinámica "Forma de Pago" (+ Cuotas si es tarjeta) =====
                var forma = combo_forma_pago.SelectedItem?.ToString() ?? "";
                var cuotasSel = combo_cuotas.SelectedItem?.ToString() ?? "1";
                bool esTarjeta = forma == "Visa Crédito" || forma == "Mastercard" || forma == "Amex";

                string rowFormaPago;
                if (esTarjeta)
                {
                    rowFormaPago = @"
        <tr>
          <td style='background:#F6DDE6; font-weight:bold;'>Forma de Pago:</td>
          <td style='width:40%;'>" + System.Net.WebUtility.HtmlEncode(forma) + @"</td>
          <td style='width:20%; background:#F6DDE6; font-weight:bold;'>Cuotas:</td>
          <td>" + System.Net.WebUtility.HtmlEncode(cuotasSel) + @"</td>
        </tr>";
                }
                else
                {
                    rowFormaPago = @"
        <tr>
          <td style='background:#F6DDE6; font-weight:bold;'>Forma de Pago:</td>
          <td colspan='3'>" + System.Net.WebUtility.HtmlEncode(forma) + @"</td>
        </tr>";
                }
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@ROW_FORMA_PAGO", rowFormaPago);
                // ======================================================================

                // ==== Detalle: Precio Unitario (c/IVA), Subtotal, Descuento Promo, Total c/IVA ====
                string filas = string.Empty;

                decimal sumaTotalConIva = 0m;   // suma de columna "Total c/IVA"
                decimal totalDescPromo = 0m;    // suma de descuentos por promoción (toda la factura)

                foreach (DataGridViewRow row in Factura.Rows)
                {
                    if (row.IsNewRow) continue;

                    var cant = Convert.ToInt32(row.Cells["Cantidad"].Value);
                    var desc = row.Cells["Nombre_Perfume"].Value?.ToString() ?? "";

                    // Precio unitario CON IVA (como está en la grilla)
                    decimal unitConIva = Convert.ToDecimal(row.Cells["Precio_Unitario"].Value);

                    // Subtotal (con IVA, sin promo)
                    decimal subtotal = unitConIva * cant;

                    // Descuento PROMO de ESA fila (valor nominal en pesos)
                    decimal descPromo = Convert.ToDecimal(row.Cells["Descuento"].Value ?? 0m);
                    totalDescPromo += descPromo;

                    // Total c/IVA para esa línea (subtotal - descuento promo)
                    decimal totalConIvaLinea = subtotal - descPromo;
                    sumaTotalConIva += totalConIvaLinea;

                    filas += $@"
        <tr>
          <td class='cant'>{Num(cant)}</td>
          <td>{System.Net.WebUtility.HtmlEncode(desc)}</td>
          <td class='money'>{Mon(unitConIva)}</td>
          <td class='money'>{Mon(subtotal)}</td>
          <td class='money'>{Mon(descPromo)}</td>
          <td class='money'>{Mon(totalConIvaLinea)}</td>
        </tr>";
                }

                // ⬅ guardamos el total de promo en el campo de la clase (para BD)
                totalDescuentoPromoFactura = Math.Round((double)totalDescPromo, 2, MidpointRounding.AwayFromZero);

                // Bonificación (descuento forma de pago) y recargo (tarjeta)
                var culture = new System.Globalization.CultureInfo("es-AR");
                decimal bonificacion = 0m;
                decimal recargoTarjeta = 0m;

                decimal.TryParse(txt_monto_descuento.Text, System.Globalization.NumberStyles.Any, culture, out bonificacion);
                decimal.TryParse(txt_monto_recargo.Text, System.Globalization.NumberStyles.Any, culture, out recargoTarjeta);

                // 👉 Precio Final c/IVA = suma Total c/IVA - bonificación + recargo
                decimal precioFinalConIva = sumaTotalConIva - bonificacion + recargoTarjeta;
                precioFinalConIva = Math.Round(precioFinalConIva, 2, MidpointRounding.AwayFromZero);

                // Importe Neto Gravado = Precio Final c/IVA / 1,21
                decimal baseImponible = 0m;
                decimal iva = 0m;

                if (precioFinalConIva != 0)
                {
                    baseImponible = Math.Round(precioFinalConIva / 1.21m, 2, MidpointRounding.AwayFromZero);
                    iva = precioFinalConIva - baseImponible;
                    iva = Math.Round(iva, 2, MidpointRounding.AwayFromZero);
                }

                // Reemplazos en plantilla
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@FILAS", filas);

                // Bonificación y recargo
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@DESCUENTO", Mon(bonificacion));
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@RECARGO", Mon(recargoTarjeta));

                // Precio Final c/IVA (nuevo row)
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@PRECIO_FINAL", Mon(precioFinalConIva));

                // Importe Neto Gravado (base imponible)
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@IMPORTE", Mon(baseImponible));

                // IVA 21%
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@IVA", Mon(iva));

                // Total a pagar (puede ser igual al Precio Final c/IVA)
                PaginaHTML_Texto = PaginaHTML_Texto.Replace("@TOTAL", Mon(precioFinalConIva));
            }


            // GUARDAR FACTURA AUTOMÁTICAMENTE EN CARPETA DE USUARIO

            string rutaArchivo = Path.Combine(carpetaFacturas, $"Factura_Orden_{txt_numero_factura.Text}.pdf");

            using (FileStream stream = new FileStream(rutaArchivo, FileMode.Create))
            {
                Document pdfDoc = new Document(PageSize.A4, 25, 25, 25, 25);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();

                // Logo
                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(
                    Properties.Resources.LogoEtereaFactura,
                    System.Drawing.Imaging.ImageFormat.Png
                );
                img.ScaleToFit(60, 60);
                img.Alignment = iTextSharp.text.Image.UNDERLYING;
                img.SetAbsolutePosition(pdfDoc.LeftMargin + 12, pdfDoc.Top - 73);
                pdfDoc.Add(img);

                // HTML
                using (StringReader sr = new StringReader(PaginaHTML_Texto))
                {
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                }

                pdfDoc.Close();
            }


            // Abrir el PDF en el visor del SO (opcional, se muestra para visualizar como queda confeccionada la factura)
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = rutaArchivo,
                UseShellExecute = true
            });

            // ==== NUEVA LÓGICA DE ENTREGA (imprimir / mail) ====
            bool hizoAccion = false;

            if (!string.IsNullOrWhiteSpace(txt_email.Text))
            {
                // Sí = enviar por mail, No = imprimir, Cancel = nada
                var elegir = MessageBox.Show(
                    $"¿Cómo querés entregar la factura?\n\nSí = Enviar por mail a {txt_email.Text}\nNo = Imprimir\nCancelar = No enviar. No imprimir.\n           Solo generarla en el sistema.",
                    "Entregar factura",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question
                );

                if (elegir == DialogResult.Yes)
                {
                    try
                    {
                        CorreoHelper.EnviarCorreoFactura(rutaArchivo, txt_email.Text.Trim());
                        MessageBox.Show("Factura enviada a " + txt_email.Text + " correctamente.");
                        hizoAccion = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("No se pudo enviar el mail: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if (elegir == DialogResult.No)
                {
                    ImprimirPdf(rutaArchivo);
                    hizoAccion = true;
                }
                // Cancel: no hace nada
            }
            else
            {
                // No hay mail: sólo ofrecer imprimir (OK = imprimir, Cancel = nada)
                var confirmar = MessageBox.Show(
                    "No hay email cargado para el cliente.\n\n¿Imprimir la factura?",
                    "Imprimir factura",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Question
                );

                if (confirmar == DialogResult.OK)
                {
                    ImprimirPdf(rutaArchivo);
                    hizoAccion = true;
                }
            }

            // Guardar en BD y limpiar SIEMPRE (independiente de la acción elegida)
            CrearFactura();
            CrearDetalleFactura();
            ReiniciarFormulario();
        }


        private void ReiniciarFormulario()
        {
            // Volver a cliente "Consumidor Final"
            clientefactura = new Cliente();

            txt_nombre_cliente.Text = "Consumidor Final";
            txt_condicion_iva.Text = "Consumidor Final";

            //Resetear el valor del total de los descuentos 
            totalDescuentoPromoFactura = 0;

            // Este flag igualmente se va a corregir con txt_dni_TextChanged
            btn_imprimir_habilitado = true;

            txt_total.Text = "0,00";
            txt_subtotal.Text = "0,00";
            txt_monto_recargo.Text = "0,00";
            txt_monto_descuento.Text = "0,00";
            txt_iva.Text = "0,00";
            txt_email.Text = "";
            txt_dni.Text = ""; // Esto dispara txt_dni_TextChanged → lo deja en true

            // Limpiar detalle
            Factura.Rows.Clear();

            // ✅ Dejar forma de pago en Efectivo como al inicio
            combo_forma_pago.SelectedIndex = 0;
            combo_descuento.SelectedIndex = 0;
            combo_cuotas.SelectedIndex = 0;

            // Recalcular reglas según forma de pago
            ActualizarDescuentosYCuotas();
            ActualizarTotales();

            // ✅ Mostrar UI correspondiente a efectivo (txt_ing_pago, txt_vuelto, etc.)
            ActualizarUIFormaPago();

            // Limpiar campos de efectivo
            txt_ing_pago.Text = "";
            txt_vuelto.Text = "0,00";

            // Volver a generar número de factura
            int puntoDeVenta = Program.sucursal;
            txt_numero_factura.Text = Num_factura_máximo();
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
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Si el textbox manual está visible, no usamos Enter para el escáner
            if (txt_ing_manual.Visible)
            {
                if (keyData == Keys.Escape)
                {
                    // Permitir cancelar el ingreso manual con ESC
                    txt_ing_manual.Clear();
                    txt_ing_manual.Visible = false;
                    btn_ing_manual.Visible = true;
                    btn_ing_manual.Focus();
                    return true;
                }

                // No queremos que Enter dispare el escaneo automático del txt_scan_factura
                if (keyData == Keys.Enter)
                    return true;

                return base.ProcessCmdKey(ref msg, keyData);
            }

            if (keyData == Keys.Enter)
            {
                if (!string.IsNullOrEmpty(txt_scan_factura.Text))
                {
                    ProcesarCodigoBarras(txt_scan_factura.Text.Trim());
                    txt_scan_factura.Clear();
                    return true; // Consumimos la tecla Enter
                }
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }


        private void btn_enviar_Click(object sender, EventArgs e)
        {
            string numero = Program.NumeroCajaActual;

            if (numero != null && numero != "Caja sin asignar")
            {

            }
            else
            {
                // No hay caja asignada, mostrar FormNumeroDeCaja para elegirla
                MessageBox.Show("\"Debes ingresar un número de caja. \n Haz click en 'Abrir Caja' ", "Número de Caja", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //Diseño del combo box
        private void comboBoxdiseño_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            // Obtener el ComboBox y el texto del ítem actual
            ComboBox combo = sender as ComboBox;
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

        //Diseño del boton del datagridview
        private void Factura_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && Factura.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                // Crear un rectángulo para el botón
                System.Drawing.Rectangle buttonRect = e.CellBounds;
                buttonRect.Inflate(-2, -2); // Reducir tamaño para dar efecto de borde

                // Definir colores personalizados
                Color buttonColor = Color.FromArgb(228, 137, 164); // Color de fondo del botón
                Color textColor = Color.FromArgb(250, 236, 239); // Color del texto

                using (SolidBrush brush = new SolidBrush(buttonColor))
                {
                    e.Graphics.FillRectangle(brush, buttonRect);
                }

                // Dibujar el texto del botón
                TextRenderer.DrawText(e.Graphics, (string)e.Value, e.CellStyle.Font, buttonRect, textColor,
                                      TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        public void AddOrIncrementPerfume(Perfume perfume, int cantidadInicial = 1)
        {
            if (perfume == null || cantidadInicial <= 0) return;

            const string colId = "Id_Perfume";
            const string colCantidad = "Cantidad";
            const string colNombre = "Nombre_Perfume"; 
            const string colPrecioUnit = "Precio_Unitario";
            const string colDesc = "Descuento";
            const string colTot = "Tot";

            int stockDisponible = ObtenerStockDisponible(perfume.id);
            if (stockDisponible <= 0)
            {
                MessageBox.Show($"No hay stock disponible para {perfume.nombre}.",
                    "Stock insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int cantidadActualEnFactura = ObtenerCantidadEnFactura(perfume.id);
            if (cantidadActualEnFactura + cantidadInicial > stockDisponible)
            {
                MessageBox.Show(
                    $"No hay stock suficiente para {perfume.nombre}.\n" +
                    $"Stock disponible: {stockDisponible}\n" +
                    $"Ya cargado en la factura: {cantidadActualEnFactura}",
                    "Stock insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 1) Si ya existe, incremento cantidad
            foreach (DataGridViewRow fila in Factura.Rows)
            {
                if (!fila.IsNewRow && fila.Cells[colId]?.Value?.ToString() == perfume.id.ToString())
                {
                    int cantActual = 0;
                    int.TryParse(fila.Cells[colCantidad].Value?.ToString(), out cantActual);
                    fila.Cells[colCantidad].Value = cantActual + cantidadInicial;

                    descuentoUnitario();
                    ActualizarTotales();
                    return;
                }
            }

            // 2) Si no existe, agrego nueva fila
            string nombreMostrar = NombreConPresentacion(perfume.nombre, perfume.presentacion_ml);

            int rowIndex = Factura.Rows.Add(
                perfume.id,                             // Id_Perfume
                cantidadInicial,                        // Cantidad
                "",                                     // +
                "",                                     // -
                nombreMostrar,                          // Nombre_Perfume
                perfume.precio_en_pesos,                // Precio_Unitario
                0m,                                     // Descuento
                perfume.precio_en_pesos * cantidadInicial, // Tot
                ""                                      // Eliminar
            );

            Factura.Rows[rowIndex].Cells[2] = new DataGridViewButtonCell() { Value = "➕" };
            Factura.Rows[rowIndex].Cells[3] = new DataGridViewButtonCell() { Value = "➖" };
            Factura.Rows[rowIndex].Cells[8] = new DataGridViewButtonCell() { Value = "Eliminar" };

            descuentoUnitario();
            ActualizarTotales();
        }

        // Devuelve el stock disponible del perfume en la sucursal actual
        private int ObtenerStockDisponible(int perfumeId)
        {
            int sucursalId = Program.sucursal;

            // Usamos tu método actual del StockControlador
            int cantidad = StockControlador.getStock(perfumeId, sucursalId);

            // Por las dudas, si devuelve -1 (no encontró registro), lo tomamos como 0
            if (cantidad < 0)
                cantidad = 0;

            return cantidad;
        }

        // Cantidad TOTAL de ese perfume ya cargada en el DataGridView
        private int ObtenerCantidadEnFactura(int perfumeId)
        {
            int total = 0;

            foreach (DataGridViewRow fila in Factura.Rows)
            {
                if (fila.IsNewRow) continue;

                if (Convert.ToInt32(fila.Cells["Id_Perfume"].Value) == perfumeId)
                {
                    int cant = 0;
                    int.TryParse(Convert.ToString(fila.Cells["Cantidad"].Value), out cant);
                    total += cant;
                }
            }

            return total;
        }


        private void Factura_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (Factura.Columns[e.ColumnIndex].Name != "Cantidad" || Factura.Rows[e.RowIndex].IsNewRow)
                return;

            int nuevaCantidad;
            if (!int.TryParse(Convert.ToString(e.FormattedValue), out nuevaCantidad) || nuevaCantidad <= 0)
            {
                e.Cancel = true;
                MessageBox.Show("La cantidad debe ser un número entero positivo.",
                    "Cantidad inválida", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int perfumeId = Convert.ToInt32(Factura.Rows[e.RowIndex].Cells["Id_Perfume"].Value);
            int stockDisponible = ObtenerStockDisponible(perfumeId);

            if (nuevaCantidad > stockDisponible)
            {
                e.Cancel = true;
                MessageBox.Show(
                    $"La cantidad no puede superar el stock disponible ({stockDisponible}).",
                    "Stock insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Factura_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (Factura.Columns[e.ColumnIndex].Name == "Cantidad")
            {
                descuentoUnitario();
                ActualizarTotales();
            }
        }

        private void OcultarControlesEfectivoYRestaurarCuotas()
        {
            // Limpiar valores
            txt_ing_pago.Text = "";
            txt_vuelto.Text = "0,00";

            // Ocultar controles de efectivo
            txt_ing_pago.Visible = false;
            txt_vuelto.Visible = false;
            lbl_vuelto.Visible = false;
            lbl_pesos_1.Visible = false;
            lbl_pesos_2.Visible = false;
            btn_ok.Visible = false;
            btn_imprimir.Visible = false;

            // Mostrar cuotas de nuevo y restaurar label
            combo_cuotas.Visible = true;
            lbl_cuotas.Text = "Cuotas";
            lbl_cuotas.Location = lblCuotasPosOriginal;

            // Garantizar estado de botones de pago
            btn_pago.Visible = false;
            btn_imprimir.Visible = true;
        }

        private void LimpiarPagoEfectivoSiHayDatos()
        {
            if (!string.IsNullOrWhiteSpace(txt_ing_pago.Text))
            {
                txt_ing_pago.Text = "";
                txt_vuelto.Text = "0,00";
            }
        }


    }
}
