using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Helpers;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop.ControlesDeUsuario
{
    public partial class Perfumes_UC : UserControl
    {

        private List<Perfume> perfumes = new List<Perfume>();
        private List<Perfume> perfumesFiltrados = new List<Perfume>();
        private Dictionary<int, int> stockPorPerfume = new Dictionary<int, int>();


        // Paginación
        private int pageSize = 15;    // podemos cambiarlo a 20, 50, etc.
        private int currentPage = 0;

        public Perfumes_UC()
        {
            InitializeComponent();

            this.Scale(new SizeF(Program.ScaleFactor, Program.ScaleFactor));

            txt_buscar_codigo.MaxLength = 13;
            txt_buscar_codigo.KeyPress += txt_buscar_codigo_KeyPress;
            txt_buscar_codigo.TextChanged += txt_buscar_codigo_TextChanged;

            dataGridViewPerfumes.Cursor = Cursors.Default;
            dataGridViewPerfumes.RowHeadersVisible = false;

            // Evento de botones “Editar / Eliminar” pintados
            dataGridViewPerfumes.CellPainting += dataGridView1_CellPainting;

            // 1) Cargar desde BD una sola vez
            CargarDatosDesdeBD();

            // 2) Aplicar filtro vacío y mostrar primera página
            AplicarFiltroYRefrescar();


            dataGridViewPerfumes.Cursor = Cursors.Default;
        }

        private void btn_crear_perfume_Click_1(object sender, EventArgs e)
        {
            FormCrearPerfume1 productos = new FormCrearPerfume1(this);

            DialogResult dr = ModalHelper.MostrarModalConFondoOscuro(productos);



            if (dr == DialogResult.OK)
            {
                Trace.WriteLine("OK");

                //Recargar datos y refrescar grilla respetando el filtro actual
                CargarDatosDesdeBD();
                AplicarFiltroYRefrescar(txt_buscar_codigo.Text.Trim());
            }
        }

        /*internal void cargarPerfumes(string filtroPerfume = "")
        {
            //Ocultas la primera columna de la tabla (es una columna de seleccion de fila)
            dataGridViewPerfumes.RowHeadersVisible = false;

            perfumes = PerfumeControlador.getAll();
            List<Stock> stocks = StockControlador.getAll();

            dataGridViewPerfumes.Rows.Clear();
            foreach (Perfume perfume in perfumes)
            {
                if (string.IsNullOrEmpty(filtroPerfume) || perfume.codigo.Contains(filtroPerfume))
                {
                    int rowIndex = dataGridViewPerfumes.Rows.Add();

                    dataGridViewPerfumes.Rows[rowIndex].Cells[0].Value = perfume.id.ToString();
                    dataGridViewPerfumes.Rows[rowIndex].Cells[1].Value = perfume.codigo;
                    dataGridViewPerfumes.Rows[rowIndex].Cells[2].Value = (MarcaControlador.getById(perfume.marca.id)).nombre;
                    dataGridViewPerfumes.Rows[rowIndex].Cells[3].Value = perfume.nombre.ToString();
                    dataGridViewPerfumes.Rows[rowIndex].Cells[4].Value = (TipoDePerfumeControlador.getById(perfume.tipo_de_perfume.id)).tipo_de_perfume;
                    dataGridViewPerfumes.Rows[rowIndex].Cells[5].Value = (GeneroControlador.getById(perfume.genero.id)).genero;
                    dataGridViewPerfumes.Rows[rowIndex].Cells[6].Value = perfume.presentacion_ml.ToString();
                    dataGridViewPerfumes.Rows[rowIndex].Cells[7].Value = (PaisControlador.getById(perfume.pais.id)).nombre;



                    if (perfume.spray)
                    {
                        dataGridViewPerfumes.Rows[rowIndex].Cells[8].Value = "Si";
                    }
                    else
                    {
                        dataGridViewPerfumes.Rows[rowIndex].Cells[8].Value = "No";
                    }

                    if (perfume.recargable)
                    {
                        dataGridViewPerfumes.Rows[rowIndex].Cells[9].Value = "Si";
                    }
                    else
                    {
                        dataGridViewPerfumes.Rows[rowIndex].Cells[9].Value = "No";
                    }

                    dataGridViewPerfumes.Rows[rowIndex].Cells[10].Value = perfume.precio_en_pesos.ToString();


                    // Agrupar por id de perfume y sumar las cantidades
                    var stockTotal = stocks
                        .Where(s => s.perfume.id == perfume.id)
                        .Sum(p => p.cantidad);
                   

                    dataGridViewPerfumes.Rows[rowIndex].Cells[11].Value = stockTotal.ToString();

                    // Insertar "Activo" después de Stock
                    int indexActivo = dataGridViewPerfumes.Columns["Activo"].Index;

                    bool? activo = perfume.activo;

                    // Mostrar texto según valor
                    string estadoActivo = activo.HasValue
                        ? (activo.Value ? "Activo" : "Inactivo")
                        : "No especificado";

                    dataGridViewPerfumes.Rows[rowIndex].Cells[indexActivo].Value = estadoActivo;

                    // Aplicar color solo si tiene valor
                    if (activo.HasValue)
                    {
                        var color = activo.Value ? Color.Green : Color.Red;
                        dataGridViewPerfumes.Rows[rowIndex].Cells[indexActivo].Style.ForeColor = color;
                    }
                    else
                    {
                        // Color por defecto si está sin especificar
                        dataGridViewPerfumes.Rows[rowIndex].Cells[indexActivo].Style.ForeColor = Color.Gray;
                    }


                    dataGridViewPerfumes.Rows[rowIndex].Cells[13].Value = "Editar";
                    dataGridViewPerfumes.Rows[rowIndex].Cells[14].Value = "Eliminar";
                }
                dataGridViewPerfumes.ClearSelection();

                dataGridViewPerfumes.CellPainting += dataGridView1_CellPainting;

            }
        }*/

        private void dataGridViewPerfumes_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex].Name == "Editar")
            {
                //EDITAMOS

                int id = int.Parse(dataGridViewPerfumes.Rows[e.RowIndex].Cells[0].Value.ToString());

                Trace.WriteLine("El id es: " + id);

                Perfume perfume_editar = PerfumeControlador.getByID(id);

                Marca marca = MarcaControlador.getById(perfume_editar.marca.id);
                TipoDePerfume tipo_de_perfume = TipoDePerfumeControlador.getById(perfume_editar.tipo_de_perfume.id);
                Genero genero = GeneroControlador.getById(perfume_editar.genero.id);
                Pais pais = PaisControlador.getById(perfume_editar.pais.id);

                perfume_editar = new Perfume(
                    perfume_editar.id,
                    perfume_editar.codigo,
                    marca,
                    perfume_editar.nombre,
                    tipo_de_perfume,
                    genero,
                    perfume_editar.presentacion_ml,
                    pais,
                    perfume_editar.spray,
                    perfume_editar.recargable,
                    perfume_editar.descripcion,
                    perfume_editar.anio_de_lanzamiento,
                    perfume_editar.precio_en_pesos,
                    perfume_editar.activo,
                    perfume_editar.imagen1,
                    perfume_editar.imagen2,
                    perfume_editar.fecha_baja,
                    perfume_editar.imagen1_URL,
                    perfume_editar.imagen2_URL

                );

                FormEditarPerfume1 formEditarProductoABM = new FormEditarPerfume1(perfume_editar, this);


                

                // ✅ Mostrar con fondo oscuro
                DialogResult dr = ModalHelper.MostrarModalConFondoOscuro(formEditarProductoABM);

                if (dr == DialogResult.OK)
                {
                    Trace.WriteLine("OK");
                    CargarDatosDesdeBD();
                    AplicarFiltroYRefrescar(txt_buscar_codigo.Text.Trim());
                }
            }


            else if (senderGrid.Columns[e.ColumnIndex].Name == "Eliminar")
            {
                //ELIMINAMOS
                int id = int.Parse(dataGridViewPerfumes.Rows[e.RowIndex].Cells[0].Value.ToString());
                Perfume perfume = PerfumeControlador.getByID(id);

                FormEliminarPerfume formEliminarProductoABM = new FormEliminarPerfume(perfume, this);

                // ✅ Mostrar con fondo oscuro
                DialogResult dr = ModalHelper.MostrarModalConFondoOscuro(formEliminarProductoABM);

                if (dr == DialogResult.OK)
                {
                    Trace.WriteLine("OK");
                    CargarDatosDesdeBD();
                    AplicarFiltroYRefrescar(txt_buscar_codigo.Text.Trim());
                }
            }
        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && dataGridViewPerfumes.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
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

        private void txt_buscar_codigo_TextChanged(object sender, EventArgs e)
        {
            string filtroCodigo = txt_buscar_codigo.Text.Trim();
            AplicarFiltroYRefrescar(filtroCodigo);

        }

        private void txt_buscar_codigo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Ignorar entrada no válida
            }
        }

        private void CargarDatosDesdeBD()
        {
            // OJO: acá PerfumeControlador.getAll() ya viene con Include de marca, tipo, etc.
            perfumes = PerfumeControlador.getAll();

            var stocks = StockControlador.getAll();
            stockPorPerfume = stocks
                .GroupBy(s => s.perfume.id)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.cantidad));
        }

        private void AplicarFiltroYRefrescar(string filtroPerfume = "")
        {
            if (string.IsNullOrWhiteSpace(filtroPerfume))
            {
                perfumesFiltrados = perfumes.ToList();
            }
            else
            {
                perfumesFiltrados = perfumes
                    .Where(p => !string.IsNullOrEmpty(p.codigo) &&
                                p.codigo.Contains(filtroPerfume))
                    .ToList();
            }

            currentPage = 0;
            PintarPaginaActual();
        }

        private void PintarPaginaActual()
        {
            dataGridViewPerfumes.SuspendLayout();
            dataGridViewPerfumes.Rows.Clear();

            var pagina = perfumesFiltrados
                .Skip(currentPage * pageSize)
                .Take(pageSize)
                .ToList();

            foreach (Perfume perfume in pagina)
            {
                int rowIndex = dataGridViewPerfumes.Rows.Add();

                dataGridViewPerfumes.Rows[rowIndex].Cells[0].Value = perfume.id;
                dataGridViewPerfumes.Rows[rowIndex].Cells[1].Value = perfume.codigo;
                dataGridViewPerfumes.Rows[rowIndex].Cells[2].Value = perfume.marca?.nombre;
                dataGridViewPerfumes.Rows[rowIndex].Cells[3].Value = perfume.nombre;
                dataGridViewPerfumes.Rows[rowIndex].Cells[4].Value = perfume.tipo_de_perfume?.tipo_de_perfume;
                dataGridViewPerfumes.Rows[rowIndex].Cells[5].Value = perfume.genero?.genero;
                dataGridViewPerfumes.Rows[rowIndex].Cells[6].Value = perfume.presentacion_ml;
                dataGridViewPerfumes.Rows[rowIndex].Cells[7].Value = perfume.pais?.nombre;

                dataGridViewPerfumes.Rows[rowIndex].Cells[8].Value = perfume.spray ? "Si" : "No";
                dataGridViewPerfumes.Rows[rowIndex].Cells[9].Value = perfume.recargable ? "Si" : "No";

                dataGridViewPerfumes.Rows[rowIndex].Cells[10].Value =
                    perfume.precio_en_pesos.ToString("N2");

                int stockTotal = stockPorPerfume.TryGetValue(perfume.id, out var st) ? st : 0;
                dataGridViewPerfumes.Rows[rowIndex].Cells[11].Value = stockTotal;

                int indexActivo = dataGridViewPerfumes.Columns["Activo"].Index;
                bool? activo = perfume.activo;

                string estadoActivo = activo.HasValue
                    ? (activo.Value ? "Activo" : "Inactivo")
                    : "No especificado";

                dataGridViewPerfumes.Rows[rowIndex].Cells[indexActivo].Value = estadoActivo;

                if (activo.HasValue)
                {
                    dataGridViewPerfumes.Rows[rowIndex].Cells[indexActivo].Style.ForeColor =
                        activo.Value ? Color.Green : Color.Red;
                }
                else
                {
                    dataGridViewPerfumes.Rows[rowIndex].Cells[indexActivo].Style.ForeColor = Color.Gray;
                }

                dataGridViewPerfumes.Rows[rowIndex].Cells[13].Value = "Editar";
                dataGridViewPerfumes.Rows[rowIndex].Cells[14].Value = "Eliminar";
            }

            dataGridViewPerfumes.ClearSelection();
            dataGridViewPerfumes.ResumeLayout();

            // label de página:
            int totalPages = (int)Math.Ceiling((double)perfumesFiltrados.Count / pageSize);
            lbl_pagina.Text = $"Página {currentPage + 1} de {Math.Max(1, totalPages)}";
        }

        private void btn_izq_Click(object sender, EventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                PintarPaginaActual();
            }

        }

        private void btn_der_Click(object sender, EventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)perfumesFiltrados.Count / pageSize);

            if (currentPage < totalPages - 1)
            {
                currentPage++;
                PintarPaginaActual();
            }
        }
    }
}
