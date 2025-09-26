using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop
{
    public partial class FormVerPromociones : Form
    {
        private Perfume perfume;

        public FormVerPromociones(Perfume perfumeSeleccionado)
        {
            InitializeComponent();
            txt_nombre_perfume.Text = perfumeSeleccionado.nombre;
            this.perfume = perfumeSeleccionado;

            string nombreImagen = perfumeSeleccionado.imagen1_URL.ToString();
            //string rutaCompletaImagen = Program.Ruta_Base + nombreImagen + ".jpg";
            //img_perfume.Image = Image.FromFile(rutaCompletaImagen);
            img_perfume.SizeMode = PictureBoxSizeMode.Zoom;
            img_perfume.ImageLocation = nombreImagen;

            /*string nombreImagen2 = perfumeSeleccionado.imagen1.ToString();
            string rutaCompletaImagen2 = Program.Ruta_Base + nombreImagen + ".jpg";
            pictureBox9.Image = Image.FromFile(rutaCompletaImagen);*/

            string rutaCompletaImagen1 = Program.Ruta_Base + @"Diseño Promo.png";
            pictureBox9.Image = Image.FromFile(rutaCompletaImagen1);

            string rutaCompletaImagen2 = Program.Ruta_Base + @"promo_no_acumulable_2.png";
            promos_no_acumulables.Image = Image.FromFile(rutaCompletaImagen2);

            //perfume = perfumeSeleccionado;
            CargarDataGridViewPromociones();
        }

        private void btn_ver_detalles_Click(object sender, EventArgs e)
        {
            FormVerDetallePerfume verDetallePerfume = new FormVerDetallePerfume(perfume);
            verDetallePerfume.ShowDialog(this);
            this.Close();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CargarDataGridViewPromociones()
        {
            //Ocultas la primera columna de la tabla (es una columna de seleccion de fila)
            dataGridViewpromociones.RowHeadersVisible = false;

            // Limpiar DataGridView
            dataGridViewpromociones.Rows.Clear();

            // Obtener promociones asociadas al perfume
            List<Promocion> promociones = PerfumeEnPromoControlador.getByIDPerfume(perfume.id)
                 .Where(p => p.id != 1) // Excluir promo "sin promo"
                 .ToList();

            if (promociones != null && promociones.Count > 0)
            {
                foreach (Promocion promo in promociones)
                {
                    int rowIndex = dataGridViewpromociones.Rows.Add();
                    dataGridViewpromociones.Rows[rowIndex].Cells[0].Value = promo.id;
                    dataGridViewpromociones.Rows[rowIndex].Cells[1].Value = promo.nombre;
                    dataGridViewpromociones.Rows[rowIndex].Cells[2].Value = "Ver";
                }
                // 👉 Limpiar la selección de filas al terminar de cargar
                dataGridViewpromociones.ClearSelection();

                dataGridViewpromociones.CellPainting += dataGridViewpromociones_CellPainting;
            }
            else
            {
                MessageBox.Show("Este perfume no tiene promociones disponibles.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void dataGridViewpromociones_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Verifica que no se haga clic en el encabezado de la columna
            if (e.RowIndex >= 0 && e.ColumnIndex == 2) // La columna del botón "Ver"
            {
                // Obtiene la fila seleccionada
                DataGridViewRow filaSeleccionada = dataGridViewpromociones.Rows[e.RowIndex];

                // Obtiene los valores de la fila
                int idPromo = Convert.ToInt32(filaSeleccionada.Cells[0].Value);
                string nombre = filaSeleccionada.Cells[1].Value.ToString();

                // Buscar la promoción en la lista
                Promocion promocionSeleccionada = PerfumeEnPromoControlador.getByIDPerfume(perfume.id)
                    .FirstOrDefault(p => p.id == idPromo);

                if (promocionSeleccionada != null)
                {
                    // Obtener precio de lista del perfume
                    double precioLista = perfume.precio_en_pesos;

                    // Obtener porcentaje de descuento
                    double porcentajeDescuento = promocionSeleccionada.descuento;

                    // Calcular el valor del descuento
                    double valorDescuento = precioLista * (porcentajeDescuento / 100);

                    // Calcular el precio final con descuento
                    double precioFinal = precioLista - valorDescuento;

                    // Mostrar los datos en los TextBox
                    txt_nombre.Text = promocionSeleccionada.nombre;
                    txt_fecha_inicio.Text = promocionSeleccionada.fecha_inicio.ToShortDateString();
                    txt_fecha_fin.Text = promocionSeleccionada.fecha_fin.ToShortDateString();
                    richTextBox_descripcion.Text = promocionSeleccionada.descripcion;

                    pictureBox9.Visible = false;

                    // Cargar imagen de la promoción en foto_promo
                    if (!string.IsNullOrEmpty(promocionSeleccionada.banner))
                    {
                        string rutaCompletaBanner = Program.Ruta_Base + promocionSeleccionada.banner + ".jpg";

                        if (System.IO.File.Exists(rutaCompletaBanner))
                        {
                            foto_promo.Image = Image.FromFile(rutaCompletaBanner);
                            foto_promo.Visible = true;
                        }
                        else
                        {
                            foto_promo.Image = null;
                            MessageBox.Show("La imagen del banner no se encontró.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        foto_promo.Image = null;
                    }
                }
            }
        }

        private void dataGridViewpromociones_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && dataGridViewpromociones.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
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

        private void label6_Click(object sender, EventArgs e)
        {

        }

    }
}
