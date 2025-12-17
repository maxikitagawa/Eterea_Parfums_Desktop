using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Helpers;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop.ControlesDeUsuario
{

    public partial class Promos_UC : UserControl
    {
        int idPromo;

        // listas en memoria
        private List<Promocion> Promos_Completo = new List<Promocion>();
        private List<Promocion> Promos_Filtrado = new List<Promocion>();

        // paginación
        private int _page = 1;
        private const int _pageSize = 12;

        List<Promocion> promociones;
        public Promos_UC()
        {
            InitializeComponent();
         
            dataGridViewPromos.Cursor = Cursors.Default;
            // enganchar painting una sola vez
            dataGridViewPromos.CellPainting += dataGridView1_CellPainting;

            // cargar una sola vez
            RecargarDesdeBD();

            //Ocultas la primera columna de la tabla (es una columna de seleccion de fila)
            dataGridViewPromos.RowHeadersVisible = false;
        }

        private void RecargarDesdeBD()
        {
            Promos_Completo = PromoControlador.obtenerTodos();

            // Ignorar promo id=1 desde el arranque (así no la filtrás siempre)
            Promos_Completo.RemoveAll(p => p.id == 1);

            _page = 1;
            AplicarFiltrosYPaginarYRender();
        }

        private void AplicarFiltrosYPaginarYRender()
        {
            string filtroNombre = txt_buscar_nombre.Text.Trim();

            // ✅ FILTRO EN MEMORIA
            if (string.IsNullOrWhiteSpace(filtroNombre))
            {
                Promos_Filtrado = new List<Promocion>(Promos_Completo);
            }
            else
            {
                Promos_Filtrado = Promos_Completo.FindAll(p =>
                    (p.nombre ?? "").IndexOf(filtroNombre, StringComparison.OrdinalIgnoreCase) >= 0
                );
            }

            // ✅ PAGINACIÓN
            int total = Promos_Filtrado.Count;
            int totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)_pageSize));

            if (_page > totalPages) _page = totalPages;
            if (_page < 1) _page = 1;

            var pagina = Promos_Filtrado
                .GetRange((_page - 1) * _pageSize, Math.Min(_pageSize, total - ((_page - 1) * _pageSize)));

            // ✅ RENDER GRID
            dataGridViewPromos.Rows.Clear();

            foreach (var promocion in pagina)
            {
                int rowIndex = dataGridViewPromos.Rows.Add();

                dataGridViewPromos.Rows[rowIndex].Cells[0].Value = promocion.id.ToString();
                dataGridViewPromos.Rows[rowIndex].Cells[1].Value = promocion.nombre;

                dataGridViewPromos.Rows[rowIndex].Cells[3].Value = promocion.fecha_inicio.ToString("dd-MM-yyyy");
                dataGridViewPromos.Rows[rowIndex].Cells[4].Value = promocion.fecha_fin.ToString("dd-MM-yyyy");

                dataGridViewPromos.Rows[rowIndex].Cells[2].Value = GetTextoPromocion(promocion.descuento);

                string estadoActivo = promocion.activo ? "Activo" : "Inactivo";
                dataGridViewPromos.Rows[rowIndex].Cells[5].Value = estadoActivo;
                dataGridViewPromos.Rows[rowIndex].Cells[5].Style.ForeColor = promocion.activo ? Color.Green : Color.Red;

                dataGridViewPromos.Rows[rowIndex].Cells[6].Value = "Editar";
                dataGridViewPromos.Rows[rowIndex].Cells[7].Value = "Eliminar";
            }

            dataGridViewPromos.ClearSelection();

            // ✅ UI paginado
            lbl_pagina.Text = $"Página {_page}/{totalPages}";
            btn_izq.Enabled = _page > 1;
            btn_der.Enabled = _page < totalPages;
        }

        private void cargarPromociones(string filtroNombre = "")
        {
           

            promociones = PromoControlador.obtenerTodos();

            dataGridViewPromos.Rows.Clear();
            foreach (Promocion promocion in promociones)
            {
                if (promocion.id == 1)
                    continue; // 👉 Ignorar la promo con id = 1

                if (string.IsNullOrEmpty(filtroNombre) || promocion.nombre.ToString().IndexOf(filtroNombre, StringComparison.OrdinalIgnoreCase) >= 0)

                {
                    int rowIndex = dataGridViewPromos.Rows.Add();

                    dataGridViewPromos.Rows[rowIndex].Cells[0].Value = promocion.id.ToString();
                    dataGridViewPromos.Rows[rowIndex].Cells[1].Value = promocion.nombre.ToString();
                    dataGridViewPromos.Rows[rowIndex].Cells[3].Value = promocion.fecha_inicio.ToString("dd-MM-yyyy");
                    dataGridViewPromos.Rows[rowIndex].Cells[4].Value = promocion.fecha_fin.ToString("dd-MM-yyyy");
                    //dataGV_Promos.Rows[rowIndex].Cells[2].Value = promocion.descuento.ToString();
                    int descuento = promocion.descuento; // Obtén el valor del descuento como entero

                    //string textoPromocion;
                    // Asignar texto basado en el descuento
                    string textoPromocion = GetTextoPromocion(promocion.descuento);
                    dataGridViewPromos.Rows[rowIndex].Cells[2].Value = textoPromocion;

                    string estadoActivo = promocion.activo ? "Activo" : "Inactivo";
                    dataGridViewPromos.Rows[rowIndex].Cells[5].Value = estadoActivo;

                    // Colorear el texto según el estado
                    dataGridViewPromos.Rows[rowIndex].Cells[5].Style.ForeColor = promocion.activo ? Color.Green : Color.Red;


                    dataGridViewPromos.Rows[rowIndex].Cells[6].Value = "Editar";
                    dataGridViewPromos.Rows[rowIndex].Cells[7].Value = "Eliminar";
                }
                dataGridViewPromos.ClearSelection();
            }
        }

        private string GetTextoPromocion(int descuento)
        {
            // Devuelve el texto adecuado según el descuento
            switch (descuento)
            {
                case 50:
                    return "2 x 1";
                case 25:
                    return "2da Unidad 50% Dto.";
                case 35:
                    return "2da Unidad 70% Dto.";
                case 40:
                    return "2da Unidad 80% Dto.";
                case 10:
                    return "Descuento especial del 10%";
                default:
                    return "Sin promoción";
            }
        }

        private void btn_izq_Click(object sender, EventArgs e)
        {
            _page--;
            AplicarFiltrosYPaginarYRender();
        }

        private void btn_der_Click(object sender, EventArgs e)
        {
            _page++;
            AplicarFiltrosYPaginarYRender();
        }


        private void txt_buscar_nombre_TextChanged(object sender, EventArgs e)
        {
            _page = 1; // ✅ al filtrar, vuelve a página 1
            AplicarFiltrosYPaginarYRender();
        }

        private void btn_crear_promo_Click(object sender, EventArgs e)
        {
            FormPromo crearPromo = new FormPromo();

            DialogResult dr = ModalHelper.MostrarModalConFondoOscuro(crearPromo);

            if (dr == DialogResult.OK)
            {
                Trace.WriteLine("OK");

                //Actualizar la lista
                RecargarDesdeBD();

            }
        }


        private void dataGridViewPromos_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {
            // Verificar si el clic fue en la columna de editar (puedes verificar el índice de la columna)
            if (e.ColumnIndex == dataGridViewPromos.Columns[6].Index && e.RowIndex >= 0)
            {
                // Obtener el ID de la promoción de la fila seleccionada
                var idPromoCell = dataGridViewPromos.Rows[e.RowIndex].Cells[0].Value;

                // Verificar si el ID de la promoción es válido
                if (idPromoCell != null && int.TryParse(idPromoCell.ToString(), out idPromo))
                {

                    // Crear una nueva instancia del formulario de edición
                    Promocion promocion_editar = PromoControlador.obtenerPorId(idPromo);
                    FormPromo formEditarPromo = new FormPromo(promocion_editar);
                    // ✅ Mostrar con fondo oscuro
                    DialogResult dr = ModalHelper.MostrarModalConFondoOscuro(formEditarPromo);

                    if (dr == DialogResult.OK)
                    {
                        Trace.WriteLine("OK");

                        //ACTUALIZAR LA LISTA
                        RecargarDesdeBD();

                    }

                }
            }
            // Verificar si el clic fue en la columna de "Eliminar"
            if (e.ColumnIndex == dataGridViewPromos.Columns[7].Index && e.RowIndex >= 0)
            {
                // Obtener el ID y el nombre de la promoción de la fila seleccionada
                var idPromoCellEliminar = dataGridViewPromos.Rows[e.RowIndex].Cells[0].Value;
                var nombrePromoCellEliminar = dataGridViewPromos.Rows[e.RowIndex].Cells[1].Value;

                // Verificar si el ID de la promoción es válido
                if (idPromoCellEliminar != null && int.TryParse(idPromoCellEliminar.ToString(), out int idPromoEliminar))
                {
                    string nombrePromoEliminar = nombrePromoCellEliminar?.ToString() ?? "Sin nombre";

                    // Crear una nueva instancia del formulario de eliminación y pasar datos
                    FormEliminarPromo formEliminarPromo = new FormEliminarPromo(idPromoEliminar, nombrePromoEliminar);

                    // Mostrar el formulario
                    // ✅ Mostrar con fondo oscuro
                    DialogResult dr = ModalHelper.MostrarModalConFondoOscuro(formEliminarPromo);
                    RecargarDesdeBD();
                }
                else
                {
                    // Mostrar un mensaje de error si el ID no es válido
                    MessageBox.Show("No se pudo obtener el ID de la promoción.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dataGridView1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && dataGridViewPromos.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
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
    }
}

