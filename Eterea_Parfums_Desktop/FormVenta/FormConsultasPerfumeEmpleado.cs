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

namespace Eterea_Parfums_Desktop
{
    public partial class FormConsultasPerfumeEmpleado : Form
    {
        private bool escaneoHabilitado = false; // ✅ Inicialmente deshabilitado

        private Facturar_UC facturacionForm;

        private Perfume filtro = new Perfume();

        private List<Perfume> Perfumes_Completo = new List<Perfume>();
        private List<Perfume> Perfumes_Filtrado = new List<Perfume>();
        private List<Perfume> Perfumes_Paginados = new List<Perfume>();

        public List<Marca> marcas;
        public List<Genero> generos;

        private List<TipoDeAroma> aromas;

        private Dictionary<int, int> stockTotalPorPerfume = new Dictionary<int, int>();
        private Dictionary<int, int> stockSucursalPorPerfume = new Dictionary<int, int>();


        //LA PAGINA ACTUAL
        private static int current = 0;
        private static int paginador = 9;

        //TOTAL DE PRODUCTOS
        private static int total = 0;
        private static int last_pag = 0;
        private static int current_pag = 1;

        private int? aromaIdSeleccionado = null;

        public string NumeroCaja { get; set; }

        private readonly Facturar_UC _parent;

        public FormConsultasPerfumeEmpleado(Facturar_UC parent)
        {
            InitializeComponent();
            _parent = parent;


            RegistrarClicks(this);

            ConfigurarDataGridView();  // 👈 Configurar columnas

            this.VisibleChanged += FormConsultasPerfumeEmpleado_VisibleChanged;

            facturacionForm = parent;

            facturacionForm.DesactivarEscaner(); // ✅ Esto evita que el escáner de Facturar_UC interfiera


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

           


            // Ruta completa (segura)
            string rutaCompletaImagen = Path.Combine(Program.Ruta_Base, "Diseño Logo2.png");

            // Verificar que el archivo exista antes de abrirlo
            if (File.Exists(rutaCompletaImagen))
            {
                // Usamos FromStream para evitar que el archivo quede bloqueado
                using (var fs = new FileStream(rutaCompletaImagen, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    img_logo.Image = Image.FromStream(fs);
                }
            }
            else
            {
                MessageBox.Show("No se encontró la imagen:\n" + rutaCompletaImagen,
                                "Archivo faltante", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            CargarStockGlobal();           // 🔹 NUEVO: cachear stock una vez

            filtro.activo = true;          // solo activos al inicio
            filtrar();                     // aplica filtros + paginación

            ResetearFiltros();

            // Enganchar CellPainting UNA sola vez
            dataGridViewConsultas.CellPainting += dataGridViewConsultas_CellPainting;

            this.FormClosed += (s, e) => _parent?.ActivarEscaner(); // ✅ reactivar escáner del padre al cerrar

        }

        private void CargarStockGlobal()
        {
            // Stock global por perfume (todas las sucursales)
            var stockGlobal = StockControlador.ObtenerTodosLosStocksPorSucursal();

            // stockGlobal seguramente sea algo tipo Dictionary<(perfumeId, sucursalId), int>
            stockTotalPorPerfume = stockGlobal
                .GroupBy(kvp => kvp.Key.perfumeId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(x => x.Value)
                );

            // Stock de la sucursal actual (este ya es por perfume)
            stockSucursalPorPerfume = StockControlador.ObtenerTodosLosStocksPorSucursal(Program.sucursal);
        }

        private void ConfigurarDataGridView()
        {
            dataGridViewConsultas.Columns.Clear();
            dataGridViewConsultas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridViewConsultas.RowHeadersVisible = false;
            dataGridViewConsultas.AllowUserToAddRows = false;

            // Columnas principales
            dataGridViewConsultas.Columns.Add("Nombre", "Nombre");
            dataGridViewConsultas.Columns.Add("Presentacion", "Presentación");
            dataGridViewConsultas.Columns.Add("Marca", "Marca");
            dataGridViewConsultas.Columns.Add("Genero", "Género");
            dataGridViewConsultas.Columns.Add("Precio", "Precio");

            // Botón Detalles
            DataGridViewButtonColumn btnDetalles = new DataGridViewButtonColumn();
            btnDetalles.Name = "Detalles";
            btnDetalles.HeaderText = "";
            btnDetalles.Text = "Detalles";
            btnDetalles.UseColumnTextForButtonValue = true;
            dataGridViewConsultas.Columns.Add(btnDetalles);

            // Botón Ver Stock
            DataGridViewButtonColumn btnStock = new DataGridViewButtonColumn();
            btnStock.Name = "VerStock";
            btnStock.HeaderText = "";
            btnStock.Text = "Ver Stock";
            btnStock.UseColumnTextForButtonValue = true;
            dataGridViewConsultas.Columns.Add(btnStock);

            // Botón Agregar
            DataGridViewButtonColumn btnAgregar = new DataGridViewButtonColumn();
            btnAgregar.Name = "Agregar";
            btnAgregar.HeaderText = "";
            btnAgregar.Text = "Agregar";
            btnAgregar.UseColumnTextForButtonValue = true;
            dataGridViewConsultas.Columns.Add(btnAgregar);
        }

        private void ResetearFiltros()
        {
            filtro = new Perfume(); // Resetea el objeto filtro
            aromaIdSeleccionado = null; // Resetea aroma
            txt_filtro_nombre.Text = ""; // Limpia el textbox
            combo_filtro_marca.SelectedIndex = 0;
            combo_filtro_genero.SelectedIndex = 0;
            combo_filtro_articulos.SelectedIndex = 0;
            combo_filtro_aroma.SelectedIndex = 0;
        }

        private void FormConsultasPerfumeEmpleado_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                facturacionForm.DesactivarEscaner();

                BarcodeReceiver.OnCodigoLeido -= ProcesarCodigoLeido;
                BarcodeReceiver.OnCodigoLeido += ProcesarCodigoLeido;
            }
            else
            {
                BarcodeReceiver.OnCodigoLeido -= ProcesarCodigoLeido;
            }
        }

   


        private void RegistrarClicks(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl != txt_scan && ctrl != lbl_codigoBarras && ctrl != btn_escanear)
                {
                    ctrl.Click += Form_Click;
                }

                // Llamada recursiva si el control tiene hijos
                if (ctrl.HasChildren)
                {
                    RegistrarClicks(ctrl);
                }
            }
        }




      



        public void ResetAutoConsulta()
        {
            escaneoHabilitado = false;
            txt_scan.Text = "";
            txt_scan.Enabled = false;
            txt_scan.Visible = false;
            lbl_codigoBarras.Visible = false;
            btn_escanear.Visible = true;
            btn_escanear.Focus();
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


        private void CargarArticulos()
        {
            combo_filtro_articulos.Items.Clear();
            combo_filtro_articulos.Items.Add("Perfumes a la venta");
            combo_filtro_articulos.Items.Add("Todos los Perfumes");
            combo_filtro_articulos.Items.Add("Perfumes sin stock");
            combo_filtro_articulos.SelectedIndex = 0;  // Establece la opción por defecto
        }



        private void paginar(List<Perfume> perfumeMostrar)
        {
            Perfumes_Paginados = perfumeMostrar.Skip(current).Take(paginador).ToList();
            VisualizarPerfumes(Perfumes_Paginados);
            lbl_paginacion_Info.Text = "Mostrando: " + (current + 1) + " a " + (current + Perfumes_Paginados.Count) + "  de  " + total;

            if (current_pag == 1)
            {
                btn_anterior.Hide();
            }
            else
            {
                btn_anterior.Show();
                btn_posterior.Show();
            }
            if (current_pag == last_pag)
            {
                btn_posterior.Hide();
            }
            else
            {
                btn_posterior.Show();
            }
        }



        private void VisualizarPerfumes(List<Perfume> perfumeMostrar)
        {
            dataGridViewConsultas.Rows.Clear();

            foreach (Perfume perfume in perfumeMostrar)
            {
                // Stock total (todas las sucursales)
                int stockDisponible = stockTotalPorPerfume.TryGetValue(perfume.id, out var sTot)
                    ? sTot
                    : 0;

                // Stock solo en la sucursal actual
                int stockEnSucursal = stockSucursalPorPerfume.TryGetValue(perfume.id, out var sSuc)
                    ? sSuc
                    : 0;

                string precioMostrado = (perfume.activo == false || stockDisponible <= 0)
                    ? "Sin Stock"
                    : perfume.precio_en_pesos.ToString("C", CultureInfo.CurrentCulture);

                int rowIndex = dataGridViewConsultas.Rows.Add();
                DataGridViewRow row = dataGridViewConsultas.Rows[rowIndex];
                row.Tag = perfume;

                // Marca y género desde listas ya cargadas en memoria
                string nombreMarca = marcas.FirstOrDefault(m => m.id == perfume.marca.id)?.nombre;
                string nombreGenero = generos.FirstOrDefault(g => g.id == perfume.genero.id)?.genero;

                row.Cells["Nombre"].Value = perfume.nombre;
                row.Cells["Presentacion"].Value = perfume.presentacion_ml.ToString() + " ml";
                row.Cells["Marca"].Value = nombreMarca;
                row.Cells["Genero"].Value = nombreGenero;
                row.Cells["Precio"].Value = precioMostrado;

                if (precioMostrado == "Sin Stock")
                {
                    row.Cells["Precio"].Style.ForeColor = Color.Red;
                    row.Cells["Precio"].Style.Font = new Font(dataGridViewConsultas.DefaultCellStyle.Font, FontStyle.Bold);
                }
                else
                {
                    row.Cells["Precio"].Style.ForeColor = Color.Black;
                    row.Cells["Precio"].Style.Font = dataGridViewConsultas.DefaultCellStyle.Font;
                }

                // Mostrar "Agregar" solo si hay stock en la sucursal actual
                if (stockEnSucursal > 0)
                {
                    row.Cells["Agregar"].Value = "Agregar";
                }
                else
                {
                    row.Cells["Agregar"].Value = ""; // o "Sin stock en sucursal"
                }
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
                Marca marca = marcas.FirstOrDefault(m => m.nombre == marcaSeleccionada);

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
                Genero genero = generos.FirstOrDefault(g => g.genero == generoSeleccionado);
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
                TipoDeAroma aroma = aromas.FirstOrDefault(a => a.nombre == aromaSeleccionado);
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
            Perfumes_Filtrado = Perfumes_Completo;

            // Filtros por marca
            if (filtro.marca != null)
                Perfumes_Filtrado = Perfumes_Filtrado.Where(x => x.marca.id == filtro.marca.id).ToList();

            // Filtros por género
            if (filtro.genero != null)
                Perfumes_Filtrado = Perfumes_Filtrado.Where(x => x.genero.id == filtro.genero.id).ToList();

            // Filtro por estado activo
            if (filtro.activo.HasValue)
                Perfumes_Filtrado = Perfumes_Filtrado.Where(x => x.activo == filtro.activo).ToList();

            // Filtro por nombre (filtro.nombre ya es lowercase)
            if (!string.IsNullOrEmpty(filtro.nombre))
                Perfumes_Filtrado = Perfumes_Filtrado.Where(x => x.nombre.ToLower().Contains(filtro.nombre)).ToList();

            // Filtro por aroma
            if (aromaIdSeleccionado != null)
            {
                List<int> perfumesConAroma = AromaDelPerfumeControlador.getPerfumeIdsPorAroma(aromaIdSeleccionado.Value);
                Perfumes_Filtrado = Perfumes_Filtrado.Where(p => perfumesConAroma.Contains(p.id)).ToList();
            }

            // 🔥 Filtro por stock y estado de "a la venta" según combo
            switch (combo_filtro_articulos.SelectedIndex)
            {
                case 0: // Perfumes a la venta (activos y con stock total > 0)
                    Perfumes_Filtrado = Perfumes_Filtrado.Where(p =>
                    {
                        int stockTot = stockTotalPorPerfume.TryGetValue(p.id, out var s) ? s : 0;
                        return p.activo == true && stockTot > 0;
                    }).ToList();
                    break;

                case 1: // Todos los perfumes
                        // no se filtra por stock
                    break;

                case 2: // Perfumes sin stock (inactivos o stock total <= 0)
                    Perfumes_Filtrado = Perfumes_Filtrado.Where(p =>
                    {
                        int stockTot = stockTotalPorPerfume.TryGetValue(p.id, out var s) ? s : 0;
                        return p.activo == false || stockTot <= 0;
                    }).ToList();
                    break;
            }

            // Paginación
            total = Perfumes_Filtrado.Count;
            last_pag = (int)Math.Ceiling((double)total / paginador);
            current = 0;
            current_pag = 1;
            paginar(Perfumes_Filtrado);
            lbl_numero_pagina.Text = current_pag.ToString();
        }


        private void dataGridViewConsultas_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (e.RowIndex >= 0 && e.ColumnIndex == 5) // Verifica que se haga clic en la columna correcta
            {
                DataGridViewRow row = dataGridViewConsultas.Rows[e.RowIndex];
                Perfume perfumeSeleccionado = row.Tag as Perfume;

                if (perfumeSeleccionado == null)
                    return;


                // Crear la ventana de detalles
                FormVerDetallePerfume detallesForm = new FormVerDetallePerfume(perfumeSeleccionado);
                detallesForm.Owner = this; // Hace que se muestre sobre FormInicioAutoconsulta

                // Deshabilitar FormInicioAutoconsulta para evitar interacciones
                this.Enabled = false;

                // Asegurar que FormDetallePerfume siempre quede por delante
                detallesForm.TopMost = true; // Lo pone en la parte superior
                detallesForm.Show();
                //detallesForm.TopMost = false; // Restablece su estado para evitar bloqueos
                detallesForm.BringToFront(); // Lo trae al frente

                // Restaurar FormInicioAutoconsulta al cerrar FormDetallePerfume
                detallesForm.FormClosing += (s, args) => this.Enabled = true;
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex == 6) // Columna "Stock"
            {
                DataGridViewRow row = dataGridViewConsultas.Rows[e.RowIndex];
                Perfume perfumeSeleccionado = row.Tag as Perfume;

                if (perfumeSeleccionado != null)
                {
                    FormStockPorSucursal stockForm = new FormStockPorSucursal(perfumeSeleccionado.nombre, perfumeSeleccionado.id);
                    stockForm.TopMost = true;
                    stockForm.ShowDialog(this);
                }
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex == 7) // Columna "Agregar"
            {
                int rowIndex = e.RowIndex;
                Perfume perfumeSeleccionado = Perfumes_Paginados[rowIndex];

                // ✔️ Chequeo de stock en sucursal (lo dejás tal cual)
                int stockEnSucursal = 0;
                var stockPorPerfumeSucursal = StockControlador.ObtenerTodosLosStocksPorSucursal(Program.sucursal);
                if (stockPorPerfumeSucursal.ContainsKey(perfumeSeleccionado.id))
                    stockEnSucursal = stockPorPerfumeSucursal[perfumeSeleccionado.id];

                if (stockEnSucursal <= 0)
                {
                    MessageBox.Show("No hay stock disponible en esta sucursal para agregar este perfume.", "Sin Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // ✅ Usar método centralizado del padre (agrega o incrementa y calcula totales/descuentos)
                _parent.AddOrIncrementPerfume(perfumeSeleccionado, 1);
                _parent.ActualizarTotales();

                // Opcional: reactivar escáner y cerrar
                _parent.ActivarEscaner();
                this.Close();
            }
        }

        private void completarFactura(Perfume perfumeSeleccionado)
        {
            if (perfumeSeleccionado == null) return;
            _parent.AddOrIncrementPerfume(perfumeSeleccionado, 1);
            _parent.ActualizarTotales();
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



        private void ProcesarCodigoLeido(string codigo)
        {
            if (!this.Visible || !escaneoHabilitado || string.IsNullOrWhiteSpace(codigo))
                return;

            // ✅ Solo aceptamos códigos de 12 o 13 caracteres
            if (codigo.Length == 12)
                codigo = "0" + codigo;
            else if (codigo.Length != 13)
                return;

            if (InvokeRequired)
            {
                Invoke(new Action(() => ProcesarCodigoLeido(codigo)));
                return;
            }



            // Buscar perfume directamente
            var perfume = Perfumes_Completo.FirstOrDefault(p => p.codigo == codigo);

            if (perfume != null)
            {
                escaneoHabilitado = false;
                ResetAutoConsulta(); // ✅ Oculta todo

                var detalle = new FormVerDetallePerfume(perfume);
                detalle.FormClosed += (s, e) =>
                
                ResetAutoConsulta();
                detalle.ShowDialog();

            }
            else
            {
                escaneoHabilitado = false;
                ResetAutoConsulta(); // ✅ Oculta todo

                var cartel = new FormCartelCodigoNoEncontrado(this);
                cartel.ShowDialog();
            }
        }


        private void btn_escanear_Click(object sender, EventArgs e)
        {
            /* Escanear escanear = new Escanear();
             escanear.Show();
             this.Hide();*/
            escaneoHabilitado = true;
            txt_scan.Text = "";


            // Ocultar el botón y mostrar el TextBox
            btn_escanear.Visible = false;
            txt_scan.Visible = true;
            txt_scan.Enabled = true;
            txt_scan.Focus(); // Poner el cursor en el TextBox
            lbl_codigoBarras.Visible = true;
            this.TopMost = false;  // Restaurar el estado normal de TopMost

            BarcodeReceiver.OnCodigoLeido -= ProcesarCodigoLeido;
            BarcodeReceiver.OnCodigoLeido += ProcesarCodigoLeido;


        }

       

        private void Form_Click(object sender, EventArgs e)
        {
            if (escaneoHabilitado)
            {
                ResetAutoConsulta();
            }
        }



        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
