using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop
{
    public partial class FormStockPorSucursal : Form
    {
        private string nombrePerfume;
        private int? perfumeIdSeleccionado;

        public FormStockPorSucursal(string nombrePerfume, int? perfumeId = null)
        {
            InitializeComponent();
            this.nombrePerfume = nombrePerfume;
            this.perfumeIdSeleccionado = perfumeId;
            CargarDatos();
        }

        private void CargarDatos()
        {
            //Se oculta la primera columna de la tabla (es una columna de seleccion de fila)
            dataGridViewStock.RowHeadersVisible = false;

            // Obtener todas las sucursales menos la "WEB"
            List<Sucursal> sucursales = SucursalControlador.getAll()
             .Where(s => s.id != 0 && s.id !=3)
             .ToList();


            // Configurar DataGridView
            dataGridViewStock.Columns.Clear();
            dataGridViewStock.Rows.Clear();

            // Modo de ajuste general
            dataGridViewStock.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Agregar columnas principales
            dataGridViewStock.Columns.Add("Nombre", "Nombre");
            dataGridViewStock.Columns.Add("Presentacion", "Presentación");
            dataGridViewStock.Columns.Add("Tipo", "Tipo");
            dataGridViewStock.Columns.Add("Genero", "Género");
            dataGridViewStock.Columns.Add("Precio", "Precio");

            foreach (var suc in sucursales)
            {
                dataGridViewStock.Columns.Add($"Sucursal_{suc.id}", suc.nombre);
            }

            // Asignar pesos a las columnas principales
            dataGridViewStock.Columns["Nombre"].FillWeight = 35;
            dataGridViewStock.Columns["Presentacion"].FillWeight = 8;
            dataGridViewStock.Columns["Tipo"].FillWeight = 10;
            dataGridViewStock.Columns["Genero"].FillWeight = 8;
            dataGridViewStock.Columns["Precio"].FillWeight = 14;

            // Calcular peso restante para las columnas de sucursal
            int cantidadSucursales = sucursales.Count;
            float pesoRestante = 100 - (35 + 8 + 10 + 8 + 14); // Ojo: usé los valores correctos
            float pesoPorSucursal = (cantidadSucursales > 0) ? (pesoRestante / cantidadSucursales) : 0;

            foreach (var suc in sucursales)
            {
                dataGridViewStock.Columns[$"Sucursal_{suc.id}"].FillWeight = pesoPorSucursal;
            }

            // Obtener perfumes con ese nombre
            List<Perfume> perfumes = PerfumeControlador.getByNombre(nombrePerfume)
                                            .OrderBy(p => p.presentacion_ml)
                                            .ToList();

            var stockPorPerfumeSucursal = StockControlador.ObtenerTodosLosStocksPorSucursal();

            int? filaSeleccionada = null;  // Variable para guardar el índice a seleccionar

            for (int i = 0; i < perfumes.Count; i++)
            {
                var perfume = perfumes[i];
                int rowIndex = dataGridViewStock.Rows.Add();
                var row = dataGridViewStock.Rows[rowIndex];

                row.Tag = perfume; // Guardar el perfume en el Tag para referencia

                row.Cells["Nombre"].Value = perfume.nombre;
                row.Cells["Presentacion"].Value = perfume.presentacion_ml + " ml";
                row.Cells["Tipo"].Value = TipoDePerfumeControlador.getById(perfume.tipo_de_perfume.id).tipo_de_perfume;
                row.Cells["Genero"].Value = GeneroControlador.getById(perfume.genero.id).genero;
                row.Cells["Precio"].Value = perfume.precio_en_pesos.ToString("C", CultureInfo.CurrentCulture);

                foreach (var suc in sucursales)
                {
                    int cantidad = 0;
                    if (stockPorPerfumeSucursal.TryGetValue((perfume.id, suc.id), out int stock))
                    {
                        cantidad = stock;
                    }
                    row.Cells[$"Sucursal_{suc.id}"].Value = cantidad;
                }

                // Almacenar la fila si coincide con el perfume del click (ID)
                if (perfumeIdSeleccionado.HasValue && perfume.id == perfumeIdSeleccionado.Value)
                {
                    filaSeleccionada = rowIndex;
                }
            }

            // Finalmente marcar la fila seleccionada
            if (filaSeleccionada.HasValue)
            {
                dataGridViewStock.ClearSelection(); // Limpiar selección previa
                dataGridViewStock.Rows[filaSeleccionada.Value].Selected = true;
                dataGridViewStock.FirstDisplayedScrollingRowIndex = filaSeleccionada.Value; // Opcional: desplazar a la fila
                dataGridViewStock.Rows[filaSeleccionada.Value].DefaultCellStyle.BackColor = Color.LightGreen; // Color destacado
            }
            dataGridViewStock.ClearSelection();
        }


        private void AplicarEstiloDataGridView()
        {
            // Fondo general
            dataGridViewStock.BackgroundColor = Color.FromArgb(250, 236, 239);  // Igual que Consultas
            dataGridViewStock.GridColor = Color.FromArgb(195, 156, 164);        // Color de línea

            // Fuente general
            dataGridViewStock.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dataGridViewStock.DefaultCellStyle.ForeColor = Color.FromArgb(195, 156, 164);
            dataGridViewStock.DefaultCellStyle.BackColor = Color.White;
            dataGridViewStock.DefaultCellStyle.SelectionBackColor = Color.FromArgb(195, 156, 164);
            dataGridViewStock.DefaultCellStyle.SelectionForeColor = Color.White;

            // Encabezados
            dataGridViewStock.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(228, 137, 164);
            dataGridViewStock.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridViewStock.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dataGridViewStock.EnableHeadersVisualStyles = false;

            // Bordes y estilos
            dataGridViewStock.BorderStyle = BorderStyle.None;
            dataGridViewStock.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dataGridViewStock.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridViewStock.RowHeadersVisible = false;

        }



        private void btn_close_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
