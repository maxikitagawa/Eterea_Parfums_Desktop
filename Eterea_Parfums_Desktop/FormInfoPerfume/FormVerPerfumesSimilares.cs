using Eterea_Parfums_Desktop.Controladores;
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
    public partial class FormVerPerfumesSimilares : Form
    {
        private Perfume perfume;

        private static Perfume filtro = new Perfume();

        private List<Perfume> Perfumes_Completo = new List<Perfume>();
        private List<Perfume> Perfumes_Filtrado = new List<Perfume>();
        private List<Perfume> Perfumes_Paginados = new List<Perfume>();

        public List<Marca> marcas;
        public List<Genero> generos;

        //LA PAGINA ACTUAL
        private static int current = 0;
        private static int paginador = 5;

        //TOTAL DE PRODUCTOS
        private static int total = 0;
        private static int last_pag = 0;
        private static int current_pag = 1;

        private bool _inicializando = false;


        public FormVerPerfumesSimilares(Perfume perfumeSeleccionado)
        {
            InitializeComponent();

            txt_nombre_perfume.Text = perfumeSeleccionado.nombre;
            this.perfume = perfumeSeleccionado;

            // Imagen grande del perfume seleccionado (ya tenías ImageLocation)
            img_perfume.SizeMode = PictureBoxSizeMode.Zoom;
            img_perfume.ImageLocation = perfumeSeleccionado.imagen1_URL?.ToString();

            // Obtener perfumes similares
            Perfumes_Completo = PerfumeControlador.getPerfumesSimilares(perfumeSeleccionado);
            if (Perfumes_Completo.Count == 0)
            {
                MessageBox.Show("No se encontraron perfumes similares para el perfume seleccionado.",
                                "Sin coincidencias", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
                return;
            }

          

          

           

            _inicializando = true;
            // Cargar combos
            CargarMarcas();
            CargarGeneros();
            _inicializando = false;

            // Paginación
            total = Perfumes_Completo.Count;
            last_pag = (int)Math.Ceiling((double)total / paginador);
            lbl_numero_pagina.Text = current_pag.ToString();

            // DataGridView: imagen con Zoom
            if (dataGridViewConsultas.Columns[0] is DataGridViewImageColumn imgCol)
                imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;

            // Suscripción única
            dataGridViewConsultas.CellPainting -= dataGridView1_CellPainting;
            dataGridViewConsultas.CellPainting += dataGridView1_CellPainting;


            // Diseño combos (como ya lo tenías)
            combo_filtro_genero.DrawMode = DrawMode.OwnerDrawFixed;
            combo_filtro_genero.DrawItem += comboBoxdiseño_DrawItem;
            combo_filtro_genero.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_filtro_marca.DrawMode = DrawMode.OwnerDrawFixed;
            combo_filtro_marca.DrawItem += comboBoxdiseño_DrawItem;
            combo_filtro_marca.DropDownStyle = ComboBoxStyle.DropDownList;

        
            // Primera carga de la grilla
            paginar(Perfumes_Completo);
        }
      
     

        private void CargarMarcas()
        {
            marcas = MarcaControlador.getAll();
            combo_filtro_marca.Items.Clear();
            combo_filtro_marca.Items.Add("Todas las marcas");
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
            combo_filtro_genero.Items.Add("Todos los generos");
            foreach (Genero genero in generos)
            {
                combo_filtro_genero.Items.Add(genero.genero);
            }
            combo_filtro_genero.SelectedIndex = 0;
        }

        private void paginar(List<Perfume> perfumeMostrar)
        {
            Perfumes_Paginados = perfumeMostrar.Skip(current).Take(paginador).ToList();
            VisualizarPerfumesAsync(Perfumes_Paginados);
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

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_ver_detalles_Click(object sender, EventArgs e)
        {
            FormVerDetallePerfume verDetallePerfume = new FormVerDetallePerfume(perfume);
            verDetallePerfume.ShowDialog(this);
            this.Close();
        }

        private async Task VisualizarPerfumesAsync(List<Perfume> perfumeMostrar)
        {
            dataGridViewConsultas.RowHeadersVisible = false;
            dataGridViewConsultas.Rows.Clear();

            var baseUrl = (Program.Ruta_Web ?? "").TrimEnd('/') + "/";

            foreach (var p in perfumeMostrar)
            {
                if (p.activo==false) continue;

                int rowIndex = dataGridViewConsultas.Rows.Add();

                string nombreArchivo = Path.GetFileName(p.imagen1 ?? string.Empty);
                if (!Path.HasExtension(nombreArchivo)) nombreArchivo += ".jpg";

                string imageUrl = baseUrl + nombreArchivo;
                Image img = await ApiImageUploader.DownloadImageAsync(imageUrl);

                dataGridViewConsultas.Rows[rowIndex].Cells[0].Value = img;
                dataGridViewConsultas.Rows[rowIndex].Cells[1].Value = p.nombre;
                dataGridViewConsultas.Rows[rowIndex].Cells[2].Value = (MarcaControlador.getById(p.marca.id)).nombre;
                dataGridViewConsultas.Rows[rowIndex].Cells[3].Value = (GeneroControlador.getById(p.genero.id)).genero;
                dataGridViewConsultas.Rows[rowIndex].Cells[4].Value = p.precio_en_pesos.ToString("C", CultureInfo.CurrentCulture);
                dataGridViewConsultas.Rows[rowIndex].Cells[5].Value = "Ver";
            }

            dataGridViewConsultas.ClearSelection();
        }


        private void dataGridViewConsultas_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (e.RowIndex >= 0 && e.ColumnIndex == 5)
            {
                int rowIndex = e.RowIndex;
                Perfume perfumeSeleccionado = Perfumes_Paginados[rowIndex];

                FormVerDetallePerfume detallesForm = new FormVerDetallePerfume(perfumeSeleccionado);
                detallesForm.ShowDialog(this);

                this.Close();
            }
            //dataGridViewConsultas.CellPainting += dataGridView1_CellPainting;
        }



        private void btn_anterior_Click_1(object sender, EventArgs e)
        {
            if (current >= paginador)
            {
                current = paginador;
                current_pag = 1;
                lbl_numero_pagina.Text = current_pag.ToString();
                paginar(Perfumes_Completo);
            }
        }

        private void btn_posterior_Click_1(object sender, EventArgs e)
        {
            if (current >= paginador)
            {
                current = current + paginador;
                current_pag = current_pag + 1;
                lbl_numero_pagina.Text = current_pag.ToString();
                paginar(Perfumes_Completo);
            }
        }

        private void combo_filtro_marca_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_inicializando) return;   // ⛔ no filtrar durante carga

            if (combo_filtro_marca.SelectedIndex > 0)
            {
                var marca = MarcaControlador.getByName(combo_filtro_marca.SelectedItem.ToString());
                filtro.marca = marca;
            }
            else
            {
                filtro.marca = null;
            }
            filtrar();
        }

        private void combo_filtro_genero_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_inicializando) return;   // ⛔ no filtrar durante carga

            if (combo_filtro_genero.SelectedIndex > 0)
            {
                var genero = GeneroControlador.getByName(combo_filtro_genero.SelectedItem.ToString());
                filtro.genero = genero;
            }
            else
            {
                filtro.genero = null;
            }
            filtrar();
        }

        private void filtrar()
        {
            Perfumes_Filtrado = Perfumes_Completo;

            if (filtro.marca != null)
            {
                Perfumes_Filtrado = Perfumes_Filtrado.Where(x => x.marca.id == filtro.marca.id).ToList();
            }

            if (filtro.genero != null)
            {
                Perfumes_Filtrado = Perfumes_Filtrado.Where(x => x.genero.id == filtro.genero.id).ToList();
            }

            total = Perfumes_Filtrado.Count;
            last_pag = (int)Math.Ceiling((double)total / paginador);
            current = 0;
            current_pag = 1;
            paginar(Perfumes_Filtrado);
            lbl_numero_pagina.Text = current_pag.ToString();
        }


        //Diseño del boton del datagridview
        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
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
