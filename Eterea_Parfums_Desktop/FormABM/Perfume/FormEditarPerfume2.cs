using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.ControlesDeUsuario;
using Eterea_Parfums_Desktop.Helpers;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop
{
    public partial class FormEditarPerfume2 : Form
    {
        private List<TipoDeAroma> tipo_de_aromas;
        private List<TipoDeNota> tipo_de_notas;
        private string filtro = "";
        private Perfume perfume;
        private List<NotasDelPerfume> notas_del_perfume;
        private List<NotaConTipoDeNota> notas_con_tipo_de_nota;
        private NotasDelPerfume notasDelPerfume;
        private FormEditarPerfume1 formEditarProducto;
        private Perfumes_UC perfumesUC;

        public FormEditarPerfume2()
        {
            InitializeComponent();
        }

        public FormEditarPerfume2(Perfume perfume, FormEditarPerfume1 formEditarProducto, Perfumes_UC perfumesUC)
        {
            InitializeComponent();
            this.formEditarProducto = formEditarProducto;
            this.perfumesUC = perfumesUC;
            this.perfume = perfume;

            cargarTipoDeAromas();
            cargarTipoDeNotas();
            txt_nombre_perfume.Text = perfume.nombre;

            CargarDatosCheckBoxListAromas();
            cargarDataGridViewNotasDePerfume();

            lbl_error_seleccion_aroma.Visible = false;
            lbl_error_seleccion_nota.Visible = false;

            this.Load += new System.EventHandler(this.FormEditarPerfume2_Load);
            checkedListBoxAroma.ItemCheck += checkedListBoxAroma_ItemCheck;
        }

        // ==================== Carga de catálogos ====================
        private void cargarTipoDeAromas()
        {
            tipo_de_aromas = TipoDeAromaControlador.getAll();
            if (tipo_de_aromas != null)
            {
                foreach (TipoDeAroma tipo_de_aroma in tipo_de_aromas)
                    checkedListBoxAroma.Items.Add(tipo_de_aroma.nombre);
            }
        }

        private void cargarTipoDeNotas()
        {
            tipo_de_notas = TipoDeNotaControlador.getAll();
            if (tipo_de_notas != null)
            {
                foreach (TipoDeNota tipo_de_nota in tipo_de_notas)
                    checkedListBoxNota.Items.Add(tipo_de_nota.nombre_tipo_de_nota);
            }
        }

        private void CargarDatosCheckBoxListAromas()
        {
            var aromasDelPerfume = AromaDelPerfumeControlador.getAllByIDPerfume(perfume.id) ?? new List<AromaDelPerfume>();
            foreach (AromaDelPerfume aromaDelPerfume in aromasDelPerfume)
            {
                var tipoDeAroma = TipoDeAromaControlador.getById(aromaDelPerfume.tipoDeAroma.id);
                if (tipoDeAroma == null) continue;

                for (int index = 0; index < checkedListBoxAroma.Items.Count; index++)
                {
                    if (string.Equals(checkedListBoxAroma.Items[index].ToString(), tipoDeAroma.nombre, StringComparison.Ordinal))
                    {
                        checkedListBoxAroma.SetItemChecked(index, true);
                        break;
                    }
                }
            }
        }

        // ==================== Búsqueda de notas ====================
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txt_nota.Text))
            {
                filtro = null;
                filtrar();
            }
            else
            {
                filtro = txt_nota.Text.ToLowerInvariant();
                filtrar();
            }
        }

        private void filtrar()
        {
            var notas = NotaControlador.getAll() ?? new List<Nota>();
            string textoFiltro = txt_nota.Text;

            if (!string.IsNullOrWhiteSpace(textoFiltro))
            {
                string match = NotaFiltroHelper.MejorCoincidencia(textoFiltro, notas);

                if (match != null)
                {
                    lbl_nota.Text = match;
                    lbl_buscar_nota.Text = match; // si usás este label como sugerencia
                }
                else
                {
                    MessageBox.Show("No se encontró ninguna nota con ese nombre");
                    lbl_buscar_nota.Text = "";
                    lbl_nota.Text = "Nota";
                    txt_nota.Clear();
                }
            }
            else
            {
                lbl_buscar_nota.Text = "";
                lbl_nota.Text = "Nota";
            }
        }

        // ==================== Grilla de Notas del Perfume ====================
        private void cargarDataGridViewNotasDePerfume()
        {
            notas_del_perfume = NotasDelPerfumeControlador.getByIDPerfume(perfume.id) ?? new List<NotasDelPerfume>();
            notas_con_tipo_de_nota = new List<NotaConTipoDeNota>();

            dataGridViewNotasDelPerfume.RowHeadersVisible = false;
            dataGridViewNotasDelPerfume.Rows.Clear();
            dataGridViewNotasDelPerfume.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            foreach (var ndp in notas_del_perfume)
            {
                var nctdn = NotaConTipoDeNotaControlador.getByID(ndp.notaConTipoDeNota.id);
                if (nctdn != null) notas_con_tipo_de_nota.Add(nctdn);
            }

            RenderNotasEnGrilla(notas_con_tipo_de_nota);
        }

        private void cargarDataGridViewNotasDePerfume(List<NotaConTipoDeNota> notaConTipoDeNotas)
        {
            dataGridViewNotasDelPerfume.RowHeadersVisible = false;
            dataGridViewNotasDelPerfume.Rows.Clear();
            dataGridViewNotasDelPerfume.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            RenderNotasEnGrilla(notaConTipoDeNotas ?? new List<NotaConTipoDeNota>());
        }

        private void RenderNotasEnGrilla(List<NotaConTipoDeNota> lista)
        {
            // Orden: salida(1), corazón(2), fondo(3) si así están en tu BD
            var notasOrdenadas = lista.OrderBy(n => n.tipoDeNota.id).ToList();

            foreach (var item in notasOrdenadas)
            {
                var nota = NotaControlador.getById(item.nota.id);
                var tipo = TipoDeNotaControlador.getById(item.tipoDeNota.id);
                if (nota == null || tipo == null) continue;

                int rowIndex = dataGridViewNotasDelPerfume.Rows.Add();
                dataGridViewNotasDelPerfume.Rows[rowIndex].Cells[0].Value = item.id;
                dataGridViewNotasDelPerfume.Rows[rowIndex].Cells[1].Value = tipo.nombre_tipo_de_nota;
                dataGridViewNotasDelPerfume.Rows[rowIndex].Cells[2].Value = nota.nombre;
                dataGridViewNotasDelPerfume.Rows[rowIndex].Cells[3].Value = "Eliminar";

                switch (tipo.nombre_tipo_de_nota.ToLower().Trim())
                {
                    case "nota de salida":
                        dataGridViewNotasDelPerfume.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DodgerBlue;
                        break;
                    case "nota de corazón":
                        dataGridViewNotasDelPerfume.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DeepPink;
                        break;
                    case "nota de fondo":
                        dataGridViewNotasDelPerfume.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.SeaGreen;
                        break;
                    default:
                        dataGridViewNotasDelPerfume.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Black;
                        break;
                }
            }

            dataGridViewNotasDelPerfume.ClearSelection();
            dataGridViewNotasDelPerfume.CellPainting -= dataGridViewNotasDelPerfume_CellPainting;
            dataGridViewNotasDelPerfume.CellPainting += dataGridViewNotasDelPerfume_CellPainting;
        }

        // ==================== Agregar una nota ====================
        private void btn_agregar_Click(object sender, EventArgs e)
        {
            if (checkedListBoxNota.CheckedItems.Count == 0)
            {
                lbl_error_seleccion_nota.Text = "Debe marcar un tipo de nota";
                lbl_error_seleccion_nota.Visible = true;
                return;
            }

            if (string.IsNullOrEmpty(lbl_nota.Text) || lbl_nota.Text == "Nota")
            {
                lbl_error_seleccion_nota.Text = "Debe ingresar una nota";
                lbl_error_seleccion_nota.Visible = true;
                return;
            }

            var nombreTipoNota = checkedListBoxNota.CheckedItems[0].ToString();
            var nota = NotaControlador.getByNombre(lbl_nota.Text);
            var tipoDeNota = TipoDeNotaControlador.getByNombre(nombreTipoNota);

            if (nota == null || tipoDeNota == null)
            {
                MessageBox.Show("No se pudo resolver la nota o el tipo de nota.");
                return;
            }

            var notaConTipo = NotaConTipoDeNotaControlador.getByNotaAndTipoDeNota(nota, tipoDeNota);
            if (notaConTipo == null)
            {
                MessageBox.Show("La combinación Nota/Tipo de Nota no existe en la base.");
                return;
            }

            notasDelPerfume = new NotasDelPerfume(perfume, notaConTipo);

            if (notas_con_tipo_de_nota == null)
                notas_con_tipo_de_nota = new List<NotaConTipoDeNota>();

            if (notas_del_perfume == null)
                notas_del_perfume = new List<NotasDelPerfume>();

            if (notas_con_tipo_de_nota.Any(x => x.id == notaConTipo.id))
            {
                lbl_error_seleccion_nota.Text = "Esta combinación ya fue agregada";
                lbl_error_seleccion_nota.Visible = true;
                return;
            }

            lbl_error_seleccion_nota.Visible = false;
            notas_con_tipo_de_nota.Add(notaConTipo);
            notas_del_perfume.Add(notasDelPerfume);

            // limpiar sin perder el tilde del tipo
            txt_nota.Clear();
            lbl_nota.Text = "Nota";
            checkedListBoxNota.ClearSelected(); // solo quita la selección visual

            cargarDataGridViewNotasDePerfume(notas_con_tipo_de_nota);
        }

        // ==================== Guardar todo ====================
        private async void btn_finalizar_Click(object sender, EventArgs e)
        {
            if (checkedListBoxAroma.CheckedItems.Count == 0)
            {
                lbl_error_seleccion_aroma.Text = "Debe marcar al menos un tipo de aroma";
                lbl_error_seleccion_aroma.Visible = true;
                return;
            }

            try
            {
                // Deshabilito UI mientras guardo
                Cursor = Cursors.WaitCursor;
                btn_finalizar.Enabled = false;

                // 1) Subir imágenes si se cambiaron (REPLACE en servidor)
                await formEditarProducto.SubirSiCambioImagenAsync();

                // 2) Actualizar perfume con datos de la pestaña 1
                perfume = formEditarProducto.editar();
                PerfumeControlador.update(perfume);

                // 3) Aromas: sincronización (borra los que ya no están, agrega los nuevos)
                var marcados = checkedListBoxAroma.CheckedItems.Cast<string>().ToList();
                var existentes = AromaDelPerfumeControlador.getAllByIDPerfume(perfume.id) ?? new List<AromaDelPerfume>();

                // borrar los que no están marcados
                foreach (var aromaExistente in existentes)
                {
                    string nombreAromaExistente = TipoDeAromaControlador.getById(aromaExistente.tipoDeAroma.id)?.nombre;
                    if (string.IsNullOrEmpty(nombreAromaExistente)) continue;

                    if (!marcados.Contains(nombreAromaExistente))
                        AromaDelPerfumeControlador.deleteBYTipoDePerfume(aromaExistente.tipoDeAroma.id);
                }

                // agregar los nuevos
                foreach (var nombreAroma in marcados)
                {
                    var tipo = TipoDeAromaControlador.getByNombre(nombreAroma);
                    if (tipo == null) continue;

                    bool yaExiste = existentes.Any(a => a.tipoDeAroma.id == tipo.id);
                    if (!yaExiste)
                    {
                        var aromaDelPerfume = new AromaDelPerfume(perfume, tipo);
                        AromaDelPerfumeControlador.create(aromaDelPerfume);
                    }
                }

                // 4) Notas: reemplazo completo (más simple y seguro)
                NotasDelPerfumeControlador.delete(perfume.id);
                if (notas_del_perfume != null)
                {
                    foreach (var ndp in notas_del_perfume)
                        NotasDelPerfumeControlador.create(ndp);
                }

                MessageBox.Show("Se han guardado los cambios del perfume correctamente.");
                this.Close();
                formEditarProducto.Close();
                perfumesUC.cargarPerfumes();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btn_finalizar.Enabled = true;
                Cursor = Cursors.Default;
            }
        }

        // ==================== UI/Handlers ====================
        private void checkedListBoxNota_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Seguridad
            if (e.Index < 0) return;

            // 1) Forzar selección única (destildar todos menos el clickeado)
            for (int i = 0; i < checkedListBoxNota.Items.Count; i++)
            {
                if (i != e.Index && checkedListBoxNota.GetItemChecked(i))
                    checkedListBoxNota.SetItemChecked(i, false);
            }

            // 2) Actualizar etiqueta con el estado final y quitar resaltado azul
            this.BeginInvoke(new Action(() =>
            {
                bool quedoChequeado = checkedListBoxNota.GetItemChecked(e.Index);
                lbl_tipo_de_nota.Text = quedoChequeado
                    ? checkedListBoxNota.Items[e.Index].ToString()
                    : "";

                checkedListBoxNota.ClearSelected(); // sin azul
            }));
        }


        private void btn_x_cerrar_Click(object sender, EventArgs e)
        {
            this.Close();
            formEditarProducto.Show();
        }

        private void dataGridViewNotasDelPerfume_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var senderGrid = (DataGridView)sender;
            if (!(senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn)) return;

            // Seguridad: verificar que haya ID en la celda 0
            var cellVal = dataGridViewNotasDelPerfume.Rows[e.RowIndex].Cells[0].Value;
            if (cellVal == null) return;

            if (!int.TryParse(cellVal.ToString(), out int id)) return;

            // Eliminar de ambas listas en memoria
            if (notas_con_tipo_de_nota != null)
                notas_con_tipo_de_nota = notas_con_tipo_de_nota.Where(x => x.id != id).ToList();
            if (notas_del_perfume != null)
                notas_del_perfume = notas_del_perfume.Where(x => x.notaConTipoDeNota.id != id).ToList();

            cargarDataGridViewNotasDePerfume(notas_con_tipo_de_nota);
            MessageBox.Show("Se ha eliminado la nota con el tipo de nota del perfume correctamente");
        }

        // Botón estilizado en la grilla
        private void dataGridViewNotasDelPerfume_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && dataGridViewNotasDelPerfume.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
            {
                e.Handled = true;
                e.PaintBackground(e.CellBounds, true);

                Rectangle buttonRect = e.CellBounds;
                buttonRect.Inflate(-2, -2);

                Color buttonColor = Color.FromArgb(228, 137, 164);
                Color textColor = Color.FromArgb(250, 236, 239);

                using (SolidBrush brush = new SolidBrush(buttonColor))
                    e.Graphics.FillRectangle(brush, buttonRect);

                TextRenderer.DrawText(e.Graphics, (string)e.Value, e.CellStyle.Font, buttonRect, textColor,
                                      TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        private void checkedListBoxAroma_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // (opcional) feedback visual
        }

        private void CheckedListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            var clb = (CheckedListBox)sender;

            if (e.Index < 0 || e.Index >= clb.Items.Count)
                return;

            bool isChecked = clb.GetItemChecked(e.Index);

            // Ignoramos "Selected" para NO usar el azul, pintamos según esté tildado
            Color backgroundColor = isChecked ? Color.FromArgb(232, 186, 197) : Color.White;
            Color textColor = isChecked ? Color.White : Color.Black;

            using (var bg = new SolidBrush(backgroundColor))
            using (var fg = new SolidBrush(textColor))
            {
                e.Graphics.FillRectangle(bg, e.Bounds);
                string text = clb.Items[e.Index].ToString();
                e.Graphics.DrawString(text, e.Font, fg, e.Bounds);
            }

            
        }


        private void FormEditarPerfume2_Load(object sender, EventArgs e)
        {
            // OwnerDraw en ambos
            checkedListBoxAroma.DrawMode = DrawMode.OwnerDrawFixed;
            checkedListBoxAroma.DrawItem -= CheckedListBox_DrawItem;
            checkedListBoxAroma.DrawItem += CheckedListBox_DrawItem;

            checkedListBoxNota.DrawMode = DrawMode.OwnerDrawFixed;
            checkedListBoxNota.DrawItem -= CheckedListBox_DrawItem;
            checkedListBoxNota.DrawItem += CheckedListBox_DrawItem;

            // Tilde al hacer click
            checkedListBoxAroma.CheckOnClick = true;
            checkedListBoxNota.CheckOnClick = true;

            // Limpiar selección para que no quede azul
            checkedListBoxAroma.SelectedIndexChanged += (s, ev) => checkedListBoxAroma.ClearSelected();
            checkedListBoxNota.SelectedIndexChanged += (s, ev) => checkedListBoxNota.ClearSelected();

            // Blindaje extra (mouse/teclado)
            checkedListBoxAroma.MouseUp += (s, ev) => checkedListBoxAroma.ClearSelected();
            checkedListBoxNota.MouseUp += (s, ev) => checkedListBoxNota.ClearSelected();
            checkedListBoxAroma.KeyUp += (s, ev) => checkedListBoxAroma.ClearSelected();
            checkedListBoxNota.KeyUp += (s, ev) => checkedListBoxNota.ClearSelected();
        }
    
    }
}
