using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.ControlesDeUsuario;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using Eterea_Parfums_Desktop.Helpers;


namespace Eterea_Parfums_Desktop
{
    public partial class FormInicioAutoconsulta : Form
    {
        private bool escaneoHabilitado = false; // ✅ Inicialmente deshabilitado

        private static Perfume filtro = new Perfume();

        private List<Perfume> Perfumes_Completo = new List<Perfume>();
        private List<Perfume> Perfumes_Filtrado = new List<Perfume>();
        private List<Perfume> Perfumes_Paginados = new List<Perfume>();

        public List<Marca> marcas;
        public List<Genero> generos;

        private List<TipoDeAroma> aromas;

        //LA PAGINA ACTUAL
        private static int current = 0;
        private static int paginador = 10;

        //TOTAL DE PRODUCTOS
        private static int total = 0;
        private static int last_pag = 0;
        private static int current_pag = 1;

        private int? aromaIdSeleccionado = null;


        public FormInicioAutoconsulta()
        {
            InitializeComponent();

            this.VisibleChanged += FormInicioAutoconsulta_VisibleChanged;


            foreach (DataGridViewColumn col in dataGridViewConsultas.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            this.TopMost = false;

            /*ESCALAR TAMAÑO DEL FORM
            float scaleFactor = 0.8f; // 80% del tamaño original
            this.Scale(new SizeF(scaleFactor, scaleFactor));
            this.Scale(new SizeF(Program.ScaleFactor, Program.ScaleFactor));*/


            //Ocultar campos de escaneo 
            lbl_codigoBarras.Visible = false;
            txt_scan.Visible = false;
            txt_scan.Enabled = false;

            // Configurar evento Click en todos los controles del formulario excepto en txt_scan y lbl_codigoBarras
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl != txt_scan && ctrl != lbl_codigoBarras && ctrl != btn_escanear)
                {
                    ctrl.Click += Form_Click;
                }
            }


            var fileName = "Diseño Logo2.png";  // que coincida EXACTO con el nombre en disco
            var logoPath = Path.Combine(Program.Ruta_Base, fileName);

            if (!File.Exists(logoPath))
            {
                MessageBox.Show("No se encontró la imagen:\n" + logoPath, "Archivo faltante");
                // Opcional: fallback a otro nombre o a Ruta_Web
                // var fallback = Path.Combine(Program.Ruta_Base, "Diseno Logo2.png");
                // ...
            }
            else
            {
                img_logo.Image = Image.FromFile(logoPath);
            }


            CargarMarcas();
            CargarGeneros();
            CargarAromas();
            //CargarStock();
            CargarArticulos();

            //Diseño del combo box
            combo_filtro_genero.DrawMode = DrawMode.OwnerDrawFixed;
            combo_filtro_genero.DrawItem += comboBoxdiseño_DrawItem;
            combo_filtro_genero.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_filtro_marca.DrawMode = DrawMode.OwnerDrawFixed;
            combo_filtro_marca.DrawItem += comboBoxdiseño_DrawItem;
            combo_filtro_marca.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_filtro_articulos.DrawMode = DrawMode.OwnerDrawFixed;
            combo_filtro_articulos.DrawItem += comboBoxdiseño_DrawItem;
            combo_filtro_articulos.DropDownStyle = ComboBoxStyle.DropDownList;

            //combo_filtro_stock.DrawMode = DrawMode.OwnerDrawFixed;
            //combo_filtro_stock.DrawItem += comboBoxdiseño_DrawItem;
            //combo_filtro_stock.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_filtro_aroma.DrawMode = DrawMode.OwnerDrawFixed;
            combo_filtro_aroma.DrawItem += comboBoxdiseño_DrawItem;
            combo_filtro_aroma.DropDownStyle = ComboBoxStyle.DropDownList;

            this.KeyPreview = true;

            //Perfumes_Completo = PerfumeControlador.getAll();
            //Perfumes_Filtrado = PerfumeControlador.filtrarPorNombre(filtro.nombre);
            Perfumes_Completo = PerfumeControlador.getAll();
            filtro.activo = true; // Esto asegura que al inicio solo se muestren los perfumes activos
            filtrar();


            dataGridViewConsultas.CellPainting += dataGridViewConsultas_CellPainting;



        }

        private void FormInicioAutoconsulta_VisibleChanged(object sender, EventArgs e)
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
            if (InvokeRequired)
            {
                Invoke(new Action(() => ProcesarCodigoLeido(codigo)));
                return;
            }


            escaneoHabilitado = true;
            txt_scan.Text = codigo;
            // ✅ Se fuerza la llamada al evento:
            txt_scan_TextChanged(txt_scan, EventArgs.Empty);
        }




        public void FormInicioAutoconsulta_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.L)
            {
                FormStart formStart = Application.OpenForms.OfType<FormStart>().FirstOrDefault();

                if (formStart != null)
                {
                    this.Hide();

                    formStart.Show();
                    formStart.BringToFront();
                    formStart.SendToBack();

                    FormLogin login = new FormLogin();
                    login.Owner = formStart;
                    login.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Error: No se encontró FormStart en ejecución.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (e.Control && e.KeyCode == Keys.X)
            {
                DialogResult result = MessageBox.Show(
                    "¿Está seguro que desea cerrar la aplicación?",
                    "Confirmar salida",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!txt_scan.Focused)
                txt_scan.Focus();
        }


        private void txt_scan_TextChanged(object sender, EventArgs e)
        {
            if (!escaneoHabilitado || !txt_scan.Visible || !txt_scan.Enabled)
            {
                txt_scan.Text = "";
                return;
            }

            // ✅ Quedarse sólo con dígitos
            string codigo = new string(txt_scan.Text.Where(char.IsDigit).ToArray());

            if (codigo.Length < 13)
                codigo = codigo.PadLeft(13, '0');//Normalizar a 13 dígitos (EAN-13): agrega ceros a la izquierda si faltan

            if (string.IsNullOrEmpty(codigo))
                return;

            Perfume perfumeEncontrado = PerfumeControlador.getByCodigo(codigo);

            if (perfumeEncontrado != null)
            {
                var detalleForm = new FormVerDetallePerfume(perfumeEncontrado);
                detalleForm.FormClosed += (s, args) => ResetAutoConsulta();
                ModalHelper.MostrarModalConFondoOscuro(detalleForm);
            }
            else
            {
                ResetAutoConsulta();
                var cartel = new FormCartelCodigoNoEncontrado(this);
                ModalHelper.MostrarModalConFondoOscuro(cartel);
            }
        }

        public void ResetAutoConsulta()
        {
            escaneoHabilitado = false;
            btn_escanear.Visible = true;
            txt_scan.Visible = false;
            txt_scan.Enabled = false;
            lbl_codigoBarras.Visible = false;
            txt_scan.Text = ""; // ✅ Vacía el textbox
        }

       /* private void GuardarTextoEnArchivo(string texto)
        {
            string rutaArchivo = @"C:\Users\intersan\Desktop\TESIS\Eterea_Parfums_Desktop\Eterea_Parfums_Desktop\txt_scan.txt";
            File.WriteAllText(rutaArchivo, texto);
        }*/

        private void InicioAutoConsultas_KeyDown_1(object sender, KeyEventArgs e)
        {
            // Detectar si se presionan las teclas Ctrl + L
            if (e.Control && e.KeyCode == Keys.L)
            {
                // Obtener la referencia de FormStart (debe estar siempre abierto)
                FormStart formStart = Application.OpenForms.OfType<FormStart>().FirstOrDefault();

                if (formStart != null)
                {
                    // Ocultar FormInicioAutoconsulta antes de abrir FormLogin
                    this.Hide();

                    // Traer FormStart al fondo pero asegurando que esté visible
                    formStart.Show();
                    formStart.BringToFront();
                    formStart.SendToBack(); // Se asegura de que no esté minimizado ni cubierto

                    // Crear y mostrar FormLogin asegurando que FormStart siga visible en el fondo
                    FormLogin login = new FormLogin();
                    login.Owner = formStart; // Asigna FormStart como dueño de FormLogin
                    login.ShowDialog(); // Mostrar de forma modal

                    // Restaurar FormInicioAutoconsulta si es necesario al cerrar el login
                    //this.Show(this);


                }
                else
                {
                    MessageBox.Show("Error: No se encontró FormStart en ejecución.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                return; // Evitar que se sigan evaluando otras teclas
            }


            // Detectar si se presionan las teclas Ctrl + X
            if (e.Control && e.KeyCode == Keys.X)
            {
                DialogResult result = MessageBox.Show(
                    "¿Está seguro que desea cerrar la aplicación?",
                    "Confirmar salida",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    Application.Exit();
                }
            }
        }


        private void CargarMarcas()
        {
            marcas = MarcaControlador.getAll();
            combo_filtro_marca.Items.Clear();
            combo_filtro_marca.Items.Add("Todas las Marcas");
            foreach (Marca marca in marcas)
            {
                combo_filtro_marca.Items.Add(marca.nombre);
            }
            combo_filtro_marca.SelectedIndex = 0;
        }

        private void CargarGeneros()
        {
            generos = GeneroControlador.getAll();
            combo_filtro_genero.Items.Clear();
            combo_filtro_genero.Items.Add("Todos los Géneros");
            foreach (Genero genero in generos)
            {
                combo_filtro_genero.Items.Add(genero.genero);
            }
            combo_filtro_genero.SelectedIndex = 0;
        }

        private void CargarAromas()
        {
            aromas = TipoDeAromaControlador.getAll();
            combo_filtro_aroma.Items.Clear();
            combo_filtro_aroma.Items.Add("Todos los Aromas");
            foreach (TipoDeAroma aroma in aromas)
            {
                combo_filtro_aroma.Items.Add(aroma.nombre);
            }
            combo_filtro_aroma.SelectedIndex = 0;
        }

        /*private void CargarStock()
        {
            combo_filtro_stock.Items.Clear();
            combo_filtro_stock.Items.Add("Todos los Perfumes");
            combo_filtro_stock.Items.Add("Disponible");
            combo_filtro_stock.Items.Add("Sin stock");
            combo_filtro_stock.SelectedIndex = 0;  // Establece la opción por defecto
        }*/

        private void CargarArticulos()
        {
            combo_filtro_articulos.Items.Clear();
            combo_filtro_articulos.Items.Add("Perfumes a la venta");
            combo_filtro_articulos.Items.Add("Todos los Perfumes");
            combo_filtro_articulos.Items.Add("Perfumes sin stock");
            combo_filtro_articulos.SelectedIndex = 0;  // Establece la opción por defecto
        }



        private void paginar(List<Perfume> listaDefinitiva)
        {
            Perfumes_Paginados = listaDefinitiva.Skip(current).Take(paginador).ToList();
            VisualizarPerfumes(Perfumes_Paginados);

            lbl_paginacion_Info.Text = $"Mostrando: {(total == 0 ? 0 : current + 1)} a {current + Perfumes_Paginados.Count} de {total}";

            btn_anterior.Visible = current_pag > 1;
            btn_posterior.Visible = current_pag < last_pag;
        }

        private void VisualizarPerfumes(List<Perfume> perfumesPagina)
        {
            dataGridViewConsultas.RowHeadersVisible = false;
            dataGridViewConsultas.Rows.Clear();

            foreach (var perfume in perfumesPagina)
            {
                int rowIndex = dataGridViewConsultas.Rows.Add();
                var row = dataGridViewConsultas.Rows[rowIndex];
                row.Tag = perfume;

                // Precio “Sin stock” si corresponde (solo a modo visual)
                string precioMostrado = (perfume.activo == true)
                  ? perfume.precio_en_pesos.ToString("C", CultureInfo.CurrentCulture)
                  : "Sin Stock";

                dataGridViewConsultas.Rows[rowIndex].Cells[0].Value = perfume.nombre;
                dataGridViewConsultas.Rows[rowIndex].Cells[1].Value = perfume.presentacion_ml + "ml";
                dataGridViewConsultas.Rows[rowIndex].Cells[2].Value = MarcaControlador.getById(perfume.marca.id).nombre;
                dataGridViewConsultas.Rows[rowIndex].Cells[3].Value = GeneroControlador.getById(perfume.genero.id).genero;
                dataGridViewConsultas.Rows[rowIndex].Cells[4].Value = precioMostrado;

                if (precioMostrado == "Sin Stock")
                {
                    var c = dataGridViewConsultas.Rows[rowIndex].Cells[4];
                    c.Style.ForeColor = Color.Red;
                    c.Style.Font = new Font(dataGridViewConsultas.DefaultCellStyle.Font, FontStyle.Bold);
                }

                dataGridViewConsultas.Rows[rowIndex].Cells[5].Value = "Ver";
            }

            dataGridViewConsultas.ClearSelection();
        }

        private void btn_anterior_Click(object sender, EventArgs e)
        {
            if (current >= paginador)
            {
                current = current - paginador;
                current_pag = current_pag - 1;
                lbl_numero_pagina.Text = current_pag.ToString();
                paginar(Perfumes_Filtrado);
            }
        }

        private void btn_posterior_Click(object sender, EventArgs e)
        {
            if (current + paginador < total)
            {
                current += paginador;
                current_pag++;
                lbl_numero_pagina.Text = current_pag.ToString();
                paginar(Perfumes_Filtrado);
            }
        }

        private void txt_filtro_nombre_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txt_filtro_nombre.Text))
            {
                //limpiamos el filtro
                filtro.nombre = null;
                filtrar();
            }
            else
            {
                string nombreFiltrar = txt_filtro_nombre.Text.ToString().ToLower();

                filtro.nombre = nombreFiltrar;
                filtrar();
            }
        }

        private void combo_filtro_marca_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combo_filtro_marca.SelectedIndex > 0)
            {
                string marcaSeleccionada = combo_filtro_marca.SelectedItem.ToString();
                Marca marca = MarcaControlador.getByName(marcaSeleccionada);
                if (marca != null)
                {
                    filtro.marca = marca;
                    filtrar();
                }
            }
            else
            {
                filtro.marca = null;
                filtrar();
            }
        }

        private void combo_filtro_genero_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combo_filtro_genero.SelectedIndex > 0)
            {
                string generoSeleccionado = combo_filtro_genero.SelectedItem.ToString();
                Genero genero = GeneroControlador.getByName(generoSeleccionado);
                if (genero != null)
                {
                    filtro.genero = genero;
                    filtrar();
                }
            }
            else
            {
                filtro.genero = null;
                filtrar();
            }
        }

        private void combo_filtro_articulos_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combo_filtro_articulos.SelectedIndex == 0) // Perfumes a la venta
            {
                filtro.activo = true;
            }
            else if (combo_filtro_articulos.SelectedIndex == 1) // Todos los perfumes
            {
                filtro.activo = null; // No filtramos por activo
            }
            else if (combo_filtro_articulos.SelectedIndex == 2) // Perfumes sin stock
            {
                filtro.activo = null; // No filtramos por activo, el control lo hace VisualizarPerfumes
            }

            filtrar();
        }


        private void combo_filtro_aroma_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combo_filtro_aroma.SelectedIndex > 0)
            {
                string aromaSeleccionado = combo_filtro_aroma.SelectedItem.ToString();
                TipoDeAroma aroma = TipoDeAromaControlador.getByNombre(aromaSeleccionado);
                if (aroma != null)
                {
                    aromaIdSeleccionado = aroma.id;
                    filtrar();
                }
            }
            else
            {
                aromaIdSeleccionado = null;
                filtrar();
            }

        }






        private void filtrar()
        {
            // 1) Base
            Perfumes_Filtrado = Perfumes_Completo;

            // 2) Filtros por marca / genero
            if (filtro.marca != null)
                Perfumes_Filtrado = Perfumes_Filtrado.Where(x => x.marca.id == filtro.marca.id).ToList();

            if (filtro.genero != null)
                Perfumes_Filtrado = Perfumes_Filtrado.Where(x => x.genero.id == filtro.genero.id).ToList();

            // 3) Filtro por activo (solo cuando corresponde)
            if (filtro.activo.HasValue)
                Perfumes_Filtrado = Perfumes_Filtrado.Where(x => x.activo == filtro.activo.Value).ToList();

            // 4) Filtro por nombre
            if (!string.IsNullOrWhiteSpace(filtro.nombre))
                Perfumes_Filtrado = Perfumes_Filtrado
                    .Where(x => x.nombre != null && x.nombre.ToLower().Contains(filtro.nombre))
                    .ToList();

            // 5) Filtro por aroma (si corresponde)
            if (aromaIdSeleccionado != null)
            {
                List<int> perfumesConAroma = AromaDelPerfumeControlador.getPerfumeIdsPorAroma(aromaIdSeleccionado.Value);
                Perfumes_Filtrado = Perfumes_Filtrado.Where(p => perfumesConAroma.Contains(p.id)).ToList();
            }

            // 6) **Filtro final “a la venta / todos / sin stock” (usa stock)**
            var visibles = AplicarFiltroStockYActivo(Perfumes_Filtrado);

            // 7) Paginar sobre la lista ya DEFINITIVA
            total = visibles.Count;
            last_pag = (int)Math.Ceiling((double)total / paginador);
            current = 0;
            current_pag = 1;
            lbl_numero_pagina.Text = current_pag.ToString();

            paginar(visibles);
        }

        private void dataGridViewConsultas_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 5) // Verifica que se haga clic en la columna correcta
            {
                DataGridViewRow row = dataGridViewConsultas.Rows[e.RowIndex];
                Perfume perfumeSeleccionado = row.Tag as Perfume;

                if (perfumeSeleccionado == null)
                    return;

                // ✅ Mostrar fondo oscuro
                FormFondoOscuro fondoOscuro = new FormFondoOscuro();
                fondoOscuro.Show();

                // ✅ Crear formulario detalle
                FormVerDetallePerfume detallesForm = new FormVerDetallePerfume(perfumeSeleccionado);
                detallesForm.StartPosition = FormStartPosition.CenterScreen;
                detallesForm.TopMost = true;

                // ✅ Deshabilitar este formulario
                this.Enabled = false;

                // ✅ Al cerrar: cerrar fondo y reactivar este form
                detallesForm.FormClosing += (s, args) =>
                {
                    this.Enabled = true;
                    fondoOscuro.Close();
                    this.BringToFront();
                };

                // ✅ Mostrar detalle
                detallesForm.Show();

                // ✅ Después de mostrar, enviar fondo al fondo
                FormFondoOscuro.EnviarAlFondo();
            }
        }



        //Diseño del boton del datagridview
        private void dataGridViewConsultas_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && dataGridViewConsultas.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                // Crear un rectángulo para el botón
                Rectangle buttonRect = e.CellBounds;
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

  


        private void btn_escanear_Click(object sender, EventArgs e)
        {
            /* Escanear escanear = new Escanear();
             escanear.Show();
             this.Hide();*/
            escaneoHabilitado = true;



            // Ocultar el botón y mostrar el TextBox
            btn_escanear.Visible = false;
            txt_scan.Visible = true;
            txt_scan.Enabled = true;
            txt_scan.Focus(); // Poner el cursor en el TextBox
            lbl_codigoBarras.Visible = true;
            this.TopMost = false;  // Restaurar el estado normal de TopMost
                                   // (Muy importante) Registrar nuevamente el listener
           
        }

        public void PrepararParaNuevoEscaneo()
        {
            escaneoHabilitado = false;  // Primero aseguro que está limpio
           
            txt_scan.Text = "";
            txt_scan.Enabled = false;
            txt_scan.Visible = false;
            lbl_codigoBarras.Visible = false;
            btn_escanear.Visible = true;
            btn_escanear.Focus(); // Opcional, si querés que quede seleccionado el botón
        }

        private void MostrarCartelCodigoNoEncontrado()
        {
            using (var cartel = new FormCartelCodigoNoEncontrado())
            {
                if (cartel.ShowDialog() == DialogResult.OK)
                {
                    if (cartel.Eleccion == FormCartelCodigoNoEncontrado.Resultado.IngresarManual)
                    {
                        // Abro FormEscanear
                        FormIngresoCodigoManual formEscanear = new FormIngresoCodigoManual(this);
                        formEscanear.ShowDialog();
                    }
                    else if (cartel.Eleccion == FormCartelCodigoNoEncontrado.Resultado.ReintentarEscaneo)
                    {
                        PrepararParaNuevoEscaneo(); // Método que limpia y habilita nuevamente escaneo
                    }
                }
            }
        }



     

        private void Form_Click(object sender, EventArgs e)
        {
            // Si txt_scan está visible y se hace clic fuera de él, oculta los controles de escaneo
            if (txt_scan.Visible)
            {
                RestaurarUI();
            }
        }



        public void MostrarErrorCodigo()
        {
            MessageBox.Show("Error al leer el código de barras", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void RestaurarUI()
        {
            // ✅ Desactivar escaneo
            escaneoHabilitado = false;

            btn_escanear.Visible = true;  // Mostrar botón Escanear
            txt_scan.Visible = false;     // Ocultar txt_scan
            lbl_codigoBarras.Visible = false;  // Ocultar lbl_codigoBarras
        }

        private List<Perfume> AplicarFiltroStockYActivo(List<Perfume> listaBase)
        {
            // Mapa de stock por perfume (id -> total)
            var stockPorPerfume = StockControlador
                .ObtenerTodosLosStocksPorSucursal()
                .GroupBy(k => k.Key.perfumeId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Value));

            bool Mostrar(Perfume p)
            {
                int stock = stockPorPerfume.TryGetValue(p.id, out var s) ? s : 0;

                switch (combo_filtro_articulos.SelectedIndex)
                {
                    case 0: // Perfumes a la venta: activos y con stock
                        return p.activo == true && stock > 0;

                    case 1: // Todos los perfumes
                        return true;

                    case 2: // Perfumes sin stock: inactivos o stock <= 0
                        return p.activo == false || stock <= 0;

                    default:
                        return true;
                }
            }

            return listaBase.Where(Mostrar).ToList();
        }


    }
}
