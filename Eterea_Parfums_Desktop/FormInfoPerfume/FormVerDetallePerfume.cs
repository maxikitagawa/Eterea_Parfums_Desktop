using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;

namespace Eterea_Parfums_Desktop
{
    public partial class FormVerDetallePerfume : Form
    {
        //public static txt_porciento_rec fv = new txt_porciento_rec();


        //RECARGOS DE CUOTAS PARA LAS TARJETAS DE CREDITO
        private static string recargoUnPagoTarjeta = "0";
        private static string recargoTresCuotas = "0";
        private static string recargoSeisCuotas = "10";
        private static string recargoNueveCuotas = "15";
        private static string recargoDoceCuotas = "20";


        private Perfume perfume;

        private bool promoSoloSinPromo;     // id = 1 o ninguna
        private bool promo10;               // descuento == 10
        private Promocion promo2x;          // segunda unidad (descuento > 10, p.ej. 25/35/40/etc.)

        private bool _updatingPromo = false;

        private bool _isInitializing = false;

        private void CheckBox10_CheckedChanged(object sender, EventArgs e) => AplicarEleccionPromo();
        private void CheckBox2x_CheckedChanged(object sender, EventArgs e) => AplicarEleccionPromo();


        public FormVerDetallePerfume(Perfume perfumeSeleccionado)
        {
            InitializeComponent();

            _isInitializing = true; // ⬅️ evita recálculos prematuros

            checkBox_10.AutoCheck = true;
            checkBox_2x.AutoCheck = true;

            // Evitar handlers duplicados (por si el diseñador los re-adjunta)
            checkBox_10.CheckedChanged -= CheckBox10_CheckedChanged;
            checkBox_2x.CheckedChanged -= CheckBox2x_CheckedChanged;
            checkBox_10.CheckedChanged += CheckBox10_CheckedChanged;
            checkBox_2x.CheckedChanged += CheckBox2x_CheckedChanged;

            foreach (DataGridViewColumn col in dataGridViewTipoNota.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;

            foreach (DataGridViewColumn col in dataGridViewAromas.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;

            this.perfume = perfumeSeleccionado;

            txt_nombre_perfume.Text = perfumeSeleccionado.nombre;

            // Estado de activo/inactivo
            if (perfumeSeleccionado.activo == false)
            {
                txt_precio_lista.Text = "Sin Stock";
                txt_recargo.Text = "0,00";
                txt_valor_cuota.Text = "0,00";
                txt_valor_recargo.Text = "0,00";
                txt_precio_final.Text = "0,00";

                label3.Visible = false;
                txt_precio_lista.ForeColor = Color.Red;

                combo_medios_pago.Enabled = false;
                combo_cuotas.Enabled = false;
                combo_descuento.Enabled = false;

                lbl_medios_pago.ForeColor = Color.LightGray;
                lbl_cuotas.ForeColor = Color.LightGray;
            }
            else
            {
                txt_precio_lista.Text = perfumeSeleccionado.precio_en_pesos.ToString("N2");
                label3.Visible = true;

                combo_medios_pago.Enabled = true;
                combo_cuotas.Enabled = true;
                combo_descuento.Enabled = true;

                lbl_medios_pago.ForeColor = Color.Brown;
                lbl_cuotas.ForeColor = Color.Brown;
            }

            richTextBox_descripcion.Text = perfumeSeleccionado.descripcion;

            txt_marca.Text = MarcaControlador.getById(perfumeSeleccionado.marca.id).nombre;
            txt_genero.Text = GeneroControlador.getById(perfumeSeleccionado.genero.id).genero;
            txt_pais.Text = PaisControlador.getById(perfumeSeleccionado.pais.id).nombre;
            txt_fecha.Text = perfumeSeleccionado.anio_de_lanzamiento.ToString();
            txt_ml.Text = perfumeSeleccionado.presentacion_ml.ToString() + " ml";
            txt_tipo.Text = TipoDePerfumeControlador.getById(perfumeSeleccionado.tipo_de_perfume.id).tipo_de_perfume;
            txt_codigo.Text = perfumeSeleccionado.codigo.ToString();
            txt_spray.Text = (perfumeSeleccionado.spray == true) ? "Sí" : "No";
            txt_recargable.Text = (perfumeSeleccionado.recargable == true) ? "Sí" : "No";

            // Combos base
            combo_medios_pago.Items.Clear();
            combo_medios_pago.Items.AddRange(new object[] { "Efectivo", "Visa Débito", "Visa Crédito", "Mastercard", "Amex", "Mercado Pago" });
            combo_medios_pago.SelectedIndex = 0;

            combo_cuotas.Items.Clear();
            combo_cuotas.Items.Add("1");
            combo_cuotas.SelectedIndex = 0;

            combo_descuento.Items.Clear();
            combo_descuento.Items.Add("0");
            combo_descuento.Items.Add("10");
            combo_descuento.Items.Add("15");
            combo_descuento.SelectedIndex = 0;

            CargarImagenPerfume(perfumeSeleccionado);

            // Estilos de combos (tu código)
            combo_medios_pago.DrawMode = DrawMode.OwnerDrawFixed;
            combo_medios_pago.DropDownStyle = ComboBoxStyle.DropDownList;
            combo_medios_pago.BackColor = Color.FromArgb(161, 136, 127);
            combo_medios_pago.ForeColor = Color.White;
            combo_medios_pago.DrawItem += comboBoxdiseño_DrawItem;

            combo_cuotas.DrawMode = DrawMode.OwnerDrawFixed;
            combo_cuotas.DropDownStyle = ComboBoxStyle.DropDownList;
            combo_cuotas.BackColor = Color.FromArgb(161, 136, 127);
            combo_cuotas.ForeColor = Color.White;
            combo_cuotas.DrawItem += comboBoxdiseño_DrawItem;

            combo_descuento.DrawMode = DrawMode.OwnerDrawFixed;
            combo_descuento.DropDownStyle = ComboBoxStyle.DropDownList;
            combo_descuento.DrawItem += comboBoxdiseño_DrawItem;

            ConfigurarDescuentos();
            cargarDataGridViewNotasDePerfume();
            CargarDataGridViewAromas();

            // --- Importante: configurar promos y recalcular con la base correcta ---
            if (perfumeSeleccionado.activo == true)
                ConfigurarPromosYPrecioBase();  // acá adentro, si corresponde, ya se llama a RecalcularTodo()

            _isInitializing = false; // ⬅️ a partir de acá, los handlers sí recalculan
        }


        private static string ObtenerRutaFallback()
        {
            return "https://drive.google.com/file/d/1XQJ3E1nk4AY2cdwPbQhbXndIkqQGRlP5/view?usp=drive_link";
        }

        private void CargarImagenPerfume(Perfume perfumeSeleccionado)
        {
            if (perfumeSeleccionado == null)
            {
                img_perfume.ImageLocation = ObtenerRutaFallback();
                return;
            }

            // 1) Normalizar URL (trim + escape espacios)
            string url = (perfumeSeleccionado.imagen1_URL ?? "").Trim();
            if (string.IsNullOrWhiteSpace(url))
            {
                img_perfume.ImageLocation = ObtenerRutaFallback();
                return;
            }

            // Escapar espacios u otros caracteres comunes
            url = url.Replace(" ", "%20");

            // 2) Si viene solo el nombre de archivo, completar con BASE_URL
            if (!url.StartsWith("http", StringComparison.OrdinalIgnoreCase) &&
                !System.IO.Path.IsPathRooted(url))
            {
                const string BASE_URL = "https://etereaparfums.com.ar/imagenes/";
                url = BASE_URL + url.TrimStart('/');
            }

            // 3) Intento 1: usar LoadAsync (rápido y simple)
            try
            {
                img_perfume.SizeMode = PictureBoxSizeMode.Zoom;
                img_perfume.ImageLocation = url; // asigno primero
                img_perfume.LoadAsync();         // dispara la carga
                return;
            }
            catch
            {
                // seguimos al intento 2
            }

            // 4) Intento 2: descargar y cargar desde memoria (más tolerante)
            try
            {
                using (var wc = new System.Net.WebClient())
                {
                    // opcional: user-agent para servers estrictos
                    wc.Headers.Add("User-Agent", "EtereaParfumsDesktop/1.0");
                    byte[] data = wc.DownloadData(url);
                    using (var ms = new System.IO.MemoryStream(data))
                    {
                        // Importante: clonar el Image para liberar el stream
                        using (var tmp = Image.FromStream(ms, useEmbeddedColorManagement: true, validateImageData: true))
                        {
                            img_perfume.Image = new Bitmap(tmp);
                            img_perfume.SizeMode = PictureBoxSizeMode.Zoom;
                            return;
                        }
                    }
                }
            }
            catch
            {
                // 5) Fallback si todo falla
                img_perfume.ImageLocation = ObtenerRutaFallback();
            }
        }




        private void cargarDataGridViewNotasDePerfume()
        {
            dataGridViewTipoNota.RowHeadersVisible = false;
            dataGridViewTipoNota.Rows.Clear();


            List<NotasDelPerfume> notas_del_perfume = NotasDelPerfumeControlador.getByIDPerfume(perfume.id);
            List<NotaConTipoDeNota> notas_con_tipo_de_nota = new List<NotaConTipoDeNota>();

            if (notas_del_perfume != null)
            {
                foreach (NotasDelPerfume nota_del_perfume in notas_del_perfume)
                {
                    var notaConTipo = NotaConTipoDeNotaControlador.getByID(nota_del_perfume.notaConTipoDeNota.id);
                    if (notaConTipo != null)
                        notas_con_tipo_de_nota.Add(notaConTipo);
                }
            }

            if (notas_con_tipo_de_nota.Any())
            {
                // Ordenar por el ID del tipo de nota (1: salida, 2: corazón, 3: fondo)
                var notasOrdenadas = notas_con_tipo_de_nota
                    .OrderBy(n => n.tipoDeNota.id)
                    .ToList();

                foreach (var notaConTipo in notasOrdenadas)
                {
                    Nota nota = NotaControlador.getById(notaConTipo.nota.id);
                    TipoDeNota tipo = TipoDeNotaControlador.getById(notaConTipo.tipoDeNota.id);

                    int rowIndex = dataGridViewTipoNota.Rows.Add();
                    dataGridViewTipoNota.Rows[rowIndex].Cells[0].Value = notaConTipo.id;
                    dataGridViewTipoNota.Rows[rowIndex].Cells[1].Value = tipo.nombre_tipo_de_nota;
                    dataGridViewTipoNota.Rows[rowIndex].Cells[2].Value = nota.nombre;

                    // 🎨 Asignar color de texto según el tipo de nota
                    switch (tipo.nombre_tipo_de_nota.ToLower().Trim())
                    {
                        case "nota de salida":
                            dataGridViewTipoNota.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DodgerBlue;
                            break;
                        case "nota de corazón":
                            dataGridViewTipoNota.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DeepPink;
                            break;
                        case "nota de fondo":
                            dataGridViewTipoNota.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.SeaGreen;
                            break;
                        default:
                            dataGridViewTipoNota.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Black;
                            break;
                    }
                    dataGridViewTipoNota.ClearSelection();
                }
            }
        }

        private void CargarDataGridViewAromas()
        {
            //Ocultas la primera columna de la tabla (es una columna de seleccion de fila)
            dataGridViewAromas.RowHeadersVisible = false;

            // Limpiar filas anteriores del DataGridView
            dataGridViewAromas.Rows.Clear();

            // Obtener la lista de aromas asociados al perfume
            List<AromaDelPerfume> aromasDelPerfume = AromaDelPerfumeControlador.getAllByIDPerfume(perfume.id);

            if (aromasDelPerfume != null)
            {
                foreach (AromaDelPerfume aromaDelPerfume in aromasDelPerfume)
                {
                    // Obtener el tipo de aroma
                    TipoDeAroma tipoDeAroma = TipoDeAromaControlador.getById(aromaDelPerfume.tipoDeAroma.id);

                    // Agregar una fila al DataGridView con la información del aroma
                    int rowIndex = dataGridViewAromas.Rows.Add();
                    dataGridViewAromas.Rows[rowIndex].Cells[0].Value = aromaDelPerfume.tipoDeAroma.id; // ID del aroma
                    dataGridViewAromas.Rows[rowIndex].Cells[1].Value = tipoDeAroma.nombre; // Nombre del tipo de aroma
                }
                dataGridViewAromas.ClearSelection();
            }
        }

        private void ConfigurarDescuentos()
        {
            if (Program.logueado != null)  // Verificamos si hay un usuario logueado
            {
                combo_descuento.Visible = true;
                txt_valor_descuento.Visible = true;
                lbl_descuento.Visible = true;
                lbl_valor_descuento.Visible = true;
                label5.Visible = true;
                label6.Visible = true;
                lbl_detalles_descuento.Visible = true;
            }
            else
            {
                combo_descuento.Visible = false;
                txt_valor_descuento.Visible = false;
                lbl_descuento.Visible = false;
                lbl_valor_descuento.Visible = false;
                label5.Visible = false;
                label6.Visible = false;
                lbl_detalles_descuento.Visible = false;
            }
        }

        private void combo_medios_pago_SelectedIndexChanged(object sender, EventArgs e)
        {
            combo_cuotas.Items.Clear();
            combo_descuento.Items.Clear();

            string formaPago = combo_medios_pago.SelectedItem.ToString();

            if (formaPago == "Efectivo")
            {
                combo_cuotas.Items.Add("1");

                combo_descuento.Items.Add("0");
                combo_descuento.Items.Add("10");
                combo_descuento.Items.Add("15");

            }
            else if (formaPago == "Visa Débito")
            {
                combo_cuotas.Items.Add("1");

                combo_descuento.Items.Add("0");
                combo_descuento.Items.Add("10");
            }
            else if (formaPago == "Visa Crédito")
            {
                combo_cuotas.Items.Add("1");
                combo_cuotas.Items.Add("3");
                combo_cuotas.Items.Add("6");
                combo_cuotas.Items.Add("9");
                combo_cuotas.Items.Add("12");

                combo_descuento.Items.Add("0");

            }
            else if (formaPago == "Mastercard")
            {
                combo_cuotas.Items.Add("1");
                combo_cuotas.Items.Add("3");
                combo_cuotas.Items.Add("6");
                combo_cuotas.Items.Add("9");
                combo_cuotas.Items.Add("12");

                combo_descuento.Items.Add("0");

            }
            else if (formaPago == "Amex")
            {
                combo_cuotas.Items.Add("1");
                combo_cuotas.Items.Add("6");
                combo_cuotas.Items.Add("12");

                combo_descuento.Items.Add("0");

            }
            else if (formaPago == "Mercado Pago")
            {
                combo_cuotas.Items.Add("1");

                combo_descuento.Items.Add("0");
                combo_descuento.Items.Add("10");
            }

            combo_cuotas.SelectedIndex = 0;
            combo_descuento.SelectedIndex = 0;

            RecalcularTodo(); 

        }

        private void CalcularRecargo(string formaPago)
        {
            int cantidadCuotas = Convert.ToInt32(combo_cuotas.SelectedItem.ToString());

            if (formaPago == "Visa Crédito" || formaPago == "Mastercard" || formaPago == "Amex")
            {
                if (cantidadCuotas == 1) txt_recargo.Text = recargoUnPagoTarjeta;
                else if (cantidadCuotas == 3) txt_recargo.Text = recargoTresCuotas;
                else if (cantidadCuotas == 6) txt_recargo.Text = recargoSeisCuotas;
                else if (cantidadCuotas == 9) txt_recargo.Text = recargoNueveCuotas;
                else if (cantidadCuotas == 12) txt_recargo.Text = recargoDoceCuotas;

                // === Cambios clave: usar precio base ===
                float precioBase = PrecioParaCalculos();
                CalcularImporteRecargo(precioBase, float.Parse(txt_recargo.Text));
                CalcularValorCuota(float.Parse(txt_recargo.Text), precioBase);
                precioFinal(precioBase, float.Parse(txt_valor_recargo.Text), float.Parse(txt_valor_descuento.Text));
               
            }
            else
            {
                txt_recargo.Text = "0,00";
            }
        }

        private void CalcularDescuento(int descuento, float preciolista)
        {
            if (preciolista > 0)
            {
                txt_valor_descuento.Text = (descuento * preciolista / 100).ToString("N2");
            }
            else
            {
                txt_valor_descuento.Text = "0,00";
            }
        }

        private void combo_descuento_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (combo_descuento.SelectedItem == null) return;

            desc();            // actualiza txt_valor_descuento
            RecalcularTodo();  // recalcula recargo, total y cuotas usando PrecioParaCalculos()
        }


        private void desc()
        {
            string descuentoStr = combo_descuento.SelectedItem?.ToString();
            if (!int.TryParse(descuentoStr, out int descuento))
            {
                txt_valor_descuento.Text = "0,00";
                return;
            }

            float precioBase = PrecioParaCalculos(); // usa txt_precio_con_descuento si está visible
            if (precioBase > 0)
                txt_valor_descuento.Text = (precioBase * descuento / 100f).ToString("N2");
            else
                txt_valor_descuento.Text = "0,00";
        }

        private void precioFinal(float preciolista, float recargo, float descuento)
        {
            txt_precio_final.Text = (preciolista + recargo - descuento).ToString("N2");
        }

        private void CalcularImporteRecargo(float precio, float recargoPorc)
        {
            txt_valor_recargo.Text = (precio * recargoPorc / 100f).ToString("N2");
        }

        private void combo_cuotas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (perfume == null || perfume.activo != true)
            {
                txt_recargo.Text = "0,00";
                txt_valor_cuota.Text = "0,00";
                txt_valor_recargo.Text = "0,00";
                txt_precio_final.Text = "0,00";

                lbl_medios_pago.ForeColor = Color.LightGray;
                lbl_cuotas.ForeColor = Color.LightGray;
                return;
            }

            string formaPago = combo_medios_pago.SelectedItem.ToString();

            CalcularRecargo(formaPago);

            // === Cambios clave: usar precio base ===
            float precioBase = PrecioParaCalculos();

            CalcularImporteRecargo(precioBase, float.Parse(txt_recargo.Text)); // mismo orden de params
            CalcularValorCuota(float.Parse(txt_recargo.Text), precioBase);
            lbl_medios_pago.ForeColor = Color.Brown;
            lbl_cuotas.ForeColor = Color.Brown;
        }



        private void CalcularValorCuota(float recargo, float precio)
        {
            // Calcular el precio final con el recargo
            float precioFinal = precio + (precio * recargo / 100);

            // Calcular el valor de cada cuota y mostrarlo
            int cantidadCuotas = int.Parse(combo_cuotas.SelectedItem.ToString());
            float valorCuota = precioFinal / cantidadCuotas;

            // Mostrar el valor de cada cuota
            txt_valor_cuota.Text = valorCuota.ToString("N2");
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
                backgroundColor = Color.FromArgb(250, 236, 239);
                textColor = Color.FromArgb(195, 156, 164);
            }
            else
            {
                // Color cuando el ítem NO está seleccionado
                backgroundColor = Color.FromArgb(195, 156, 164); // Color personalizado
                textColor = Color.White;
            }

            // Pintar el fondo del ítem
            using (SolidBrush brush = new SolidBrush(backgroundColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            // Medir el texto
            Size textSize = TextRenderer.MeasureText(text, e.Font);

            // Calcular posición centrada
            int x = e.Bounds.Left + (e.Bounds.Width - textSize.Width) / 2;
            int y = e.Bounds.Top + (e.Bounds.Height - textSize.Height) / 2;

            // Dibujar el texto
            TextRenderer.DrawText(e.Graphics, text, e.Font, new Point(x, y), textColor);


            // Evitar problemas visuales
            e.DrawFocusRectangle();
        }


        private void btn_buscar_perfumes_simi_Click(object sender, EventArgs e)
        {
            // Intentar abrir el formulario de perfumes similares
            FormVerPerfumesSimilares verPerfumesSimilares = new FormVerPerfumesSimilares(perfume);

            // Si el formulario no se cerró por falta de datos, mostrarlo
            if (!verPerfumesSimilares.IsDisposed)
            {
                verPerfumesSimilares.ShowDialog(this);
                this.Close(); // Cerrar el formulario actual solo si el otro se abrió con éxito
            }
        }


        private void btn_ver_promociones_Click(object sender, EventArgs e)
        {
            // Obtener todas las promociones del perfume
            List<Promocion> promociones = PerfumeEnPromoControlador.getByIDPerfume(perfume.id);

            // Filtrar las promociones válidas (excluir la promo con id 1)
            List<Promocion> promocionesValidas = promociones
                .Where(p => p.id != 1) // Excluir la promo con id = 1 ("sin promo")
                .ToList();

            if (promocionesValidas.Count == 0)
            {
                MessageBox.Show("Este perfume no tiene promociones disponibles.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }



            // Si tiene promociones reales, abrir el formulario
            FormVerPromociones verPromociones = new FormVerPromociones(perfume);
            verPromociones.ShowDialog(this);
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SetPrecioConDescVisible(bool visible)
        {
            txt_precio_con_desc.Visible = visible;
            label9.Visible = visible;
        }

        // Aplica/quita tachado al precio de lista
        private void SetPrecioListaTachado(bool tachar)
        {
            txt_precio_lista.Font = new Font(txt_precio_lista.Font, tachar ? FontStyle.Strikeout : FontStyle.Regular);
        }


        private float ParsearFloat(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return 0f;

            // Limpio espacios
            s = s.Trim();

            // 1) Intento cultura es-AR (coma decimal)
            if (float.TryParse(s, NumberStyles.Any, new CultureInfo("es-AR"), out var v1)) return v1;

            // 2) Intento cultura en-US (punto decimal)
            if (float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var v2)) return v2;

            // 3) Plan C: saco separador de miles común y reintento con coma decimal
            var s2 = s.Replace(".", "");
            if (float.TryParse(s2, NumberStyles.Any, new CultureInfo("es-AR"), out var v3)) return v3;

            // 4) Plan D: saco separador de miles y uso punto decimal
            var s3 = s.Replace(",", "");
            if (float.TryParse(s3, NumberStyles.Any, CultureInfo.InvariantCulture, out var v4)) return v4;

            return 0f;
        }


        // Devuelve el precio base para todos los cálculos (si hay promo visible usa txt_precio_con_descuento)
        private float PrecioParaCalculos()
        {
            // Si hay precio con descuento visible y válido, usarlo
            if (txt_precio_con_desc.Visible)
            {
                var v = ParsearFloat(txt_precio_con_desc.Text);
                if (v > 0f) return v;
            }
            // Si no, precio de lista
            return ParsearFloat(txt_precio_lista.Text);
        }

        // Aplica/quita tachado al precio de lista
        /*private void SetPrecioListaTachado(bool tachar)
        {
            txt_precio_lista.Font = new Font(txt_precio_lista.Font,
                tachar ? FontStyle.Strikeout : FontStyle.Regular);
        }*/

        // Recalcular todo usando el precio base actual (llamalo tras cualquier cambio de selección)
        private void RecalcularTodo()
        {
            string formaPago = combo_medios_pago.SelectedItem?.ToString() ?? "Efectivo";

            // Recalcular recargo según medio de pago / cuotas (tu método ya setea algunos campos)
            CalcularRecargo(formaPago);

            // Luego rehacemos bien con el precio base correcto
            float precioBase = PrecioParaCalculos();
            float recargoPorc = ParsearFloat(txt_recargo.Text);

            // Importe recargo (en $)
            CalcularImporteRecargo(precioBase, recargoPorc);

            // Valor cuota
            CalcularValorCuota(recargoPorc, precioBase);

            // Precio final = base + recargo($) - descuento($)
            precioFinal(precioBase, ParsearFloat(txt_valor_recargo.Text), ParsearFloat(txt_valor_descuento.Text));


        }


        private void ConfigurarPromosYPrecioBase()
        {
            // Baseline de visibilidad (antes de hacer nada)
            SetPrecioConDescVisible(false);    // oculta txt_precio_con_desc y label9
            SetPrecioListaTachado(false);
            lbl_desc_10.Visible = false;
            checkBox_10.Visible = false;
            checkBox_2x.Visible = false;
            checkBox_10.Enabled = false;
            checkBox_2x.Enabled = false;

            try
            {
                var promos = PerfumeEnPromoControlador.getByIDPerfume(perfume.id) ?? new List<Promocion>();

                // LOG temporal (podés mirar en Output de VS)
                System.Diagnostics.Debug.WriteLine($"[Promos] Count={promos.Count}");
                foreach (var p in promos)
                    System.Diagnostics.Debug.WriteLine($"[Promos] id={p.id} descuento={p.descuento}");

                promoSoloSinPromo = promos.All(p => p.id == 1) || promos.Count == 0;
                promo10 = promos.Any(p => p.descuento == 10 && p.id != 1);
                promo2x = promos.FirstOrDefault(p => p.id != 1 && p.descuento > 10);

                float precioLista = ParsearFloat(txt_precio_lista.Text);

                if (promoSoloSinPromo)
                {
                    // CASO 1: sin promoción real
                    lbl_desc_10.Text = "Perfume sin promoción";
                    lbl_desc_10.Visible = true;

                    SetPrecioConDescVisible(false);
                    SetPrecioListaTachado(false);

                    RecalcularTodo();
                    return;
                }

                if (promo10 && promo2x == null)
                {
                    // CASO 2: solo 10%
                    lbl_desc_10.Text = "Descuento 10%";
                    lbl_desc_10.Visible = true;

                    float precio10 = (float)Math.Round(precioLista * 0.90f, 2, MidpointRounding.AwayFromZero);
                    txt_precio_con_desc.Text = precio10.ToString("N2");
                    SetPrecioConDescVisible(true);
                    SetPrecioListaTachado(true);

                    RecalcularTodo(); // Valor cuota + Precio final con base descontada
                    return;
                }

                if (promo10 && promo2x != null)
                {
                    // CASO 3: 10% y 2x → checkboxes
                    lbl_desc_10.Visible = false;

                    checkBox_10.Visible = checkBox_2x.Visible = true;
                    checkBox_10.Enabled = checkBox_2x.Enabled = true;

                    // Default: 10% seleccionado
                    _updatingPromo = true;
                    try
                    {
                        checkBox_10.Checked = true;
                        checkBox_2x.Checked = false;
                    }
                    finally { _updatingPromo = false; }

                    AplicarEleccionPromo(); // esto recalcula todo
                    return;
                }

                // Fallback
                lbl_desc_10.Text = "Perfume sin promoción";
                lbl_desc_10.Visible = true;
                SetPrecioConDescVisible(false);
                SetPrecioListaTachado(false);
                RecalcularTodo();
            }
            catch (Exception ex)
            {
                // Para detectar si está explotando por un null o algo raro
                System.Diagnostics.Debug.WriteLine("[Promos][ERROR] " + ex);
                // Mostrá algo corto (puede sacarse luego)
                // MessageBox.Show("Error al configurar promos: " + ex.Message);
                // Dejamos un estado neutro:
                lbl_desc_10.Text = "Perfume sin promoción";
                lbl_desc_10.Visible = true;
                SetPrecioConDescVisible(false);
                SetPrecioListaTachado(false);
                RecalcularTodo();
            }
        }




        private void AplicarEleccionPromo()
        {
            if (_updatingPromo || _isInitializing) return; // ⬅️ agregado
            _updatingPromo = true;
            try
            {
                float precioLista = ParsearFloat(txt_precio_lista.Text);

                // Exclusión mutua clara
                if (checkBox_10.Checked && checkBox_2x.Checked)
                {
                    if (checkBox_10.Focused)
                        checkBox_2x.Checked = false;
                    else if (checkBox_2x.Focused)
                        checkBox_10.Checked = false;
                    else
                        checkBox_2x.Checked = false;
                }
                else if (!checkBox_10.Checked && !checkBox_2x.Checked && promo10)
                {
                    checkBox_10.Checked = true;
                }

                if (checkBox_10.Checked && !checkBox_2x.Checked)
                {
                    // 10% OFF
                    float precio10 = (float)Math.Round(precioLista * 0.90f, 2, MidpointRounding.AwayFromZero);
                    txt_precio_con_desc.Text = precio10.ToString("N2");
                    SetPrecioConDescVisible(true);
                    SetPrecioListaTachado(true);
                }
                else if (!checkBox_10.Checked && checkBox_2x.Checked && promo2x != null)
                {
                    // 2x: total de DOS unidades (tal como ya lo mostrabas)
                    float d = promo2x.descuento;
                    float precioPar = (float)Math.Round((precioLista * 2f) * (1f - d / 100f), 2, MidpointRounding.AwayFromZero);
                    txt_precio_con_desc.Text = precioPar.ToString("N2");
                    SetPrecioConDescVisible(true);
                    SetPrecioListaTachado(true);
                }
                else
                {
                    SetPrecioConDescVisible(false);
                    SetPrecioListaTachado(false);
                }
            }
            finally
            {
                _updatingPromo = false;
            }

            // Ahora sí, con el txt_precio_con_desc visible y seteado, recalculamos todo
            RecalcularTodo();
        }


        private void Promo_CheckedChanged(object sender, EventArgs e)
        {
            if (_updatingPromo) return;
            _updatingPromo = true;
            try
            {
                // Mutua exclusión explícita según quién disparó
                if (sender == checkBox_10 && checkBox_10.Checked)
                    checkBox_2x.Checked = false;

                if (sender == checkBox_2x && checkBox_2x.Checked)
                    checkBox_10.Checked = false;

                AplicarEleccionPromo(); // calcula base y llama a RecalcularTodo
            }
            finally
            {
                _updatingPromo = false;
            }
        }




    }
}
