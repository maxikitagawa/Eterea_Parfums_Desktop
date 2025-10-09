using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.ControlesDeUsuario;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

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
            cargarTipoDeAromas();
            cargarTipoDeNotas();
            txt_nombre_perfume.Text = perfume.nombre;
            this.perfume = perfume;
            CargarDatosCheckBoxListAromas();
            cargarDataGridViewNotasDePerfume();
            lbl_error_seleccion_aroma.Visible = false;
            lbl_error_seleccion_nota.Visible = false;

            this.Load += new System.EventHandler(this.FormEditarPerfume2_Load);
            checkedListBoxAroma.ItemCheck += checkedListBoxAroma_ItemCheck;

        }

        private void cargarTipoDeAromas()
        {
            tipo_de_aromas = TipoDeAromaControlador.getAll();
            if (tipo_de_aromas != null)
            {
                foreach (TipoDeAroma tipo_de_aroma in tipo_de_aromas)
                {
                    checkedListBoxAroma.Items.Add(tipo_de_aroma.nombre);
                }
            }
        }

        private void cargarTipoDeNotas()
        {
            tipo_de_notas = TipoDeNotaControlador.getAll();
            if (tipo_de_notas != null)
            {
                foreach (TipoDeNota tipo_de_nota in tipo_de_notas)
                {
                    checkedListBoxNota.Items.Add(tipo_de_nota.nombre_tipo_de_nota);
                }
            }
        }

        private void CargarDatosCheckBoxListAromas()
        {
            List<AromaDelPerfume> aromasDelPerdume = AromaDelPerfumeControlador.getAllByIDPerfume(perfume.id);

            foreach (AromaDelPerfume aromaDelPerfume in aromasDelPerdume)
            {
                TipoDeAroma tipoDeAroma = TipoDeAromaControlador.getById(aromaDelPerfume.tipoDeAroma.id);
                for (int index = 0; index < checkedListBoxAroma.Items.Count; index++)
                {
                    if (checkedListBoxAroma.Items[index].ToString() == tipoDeAroma.nombre)
                    {
                        checkedListBoxAroma.SetItemChecked(index, true);
                        break; // Optimización: salir del bucle una vez encontrado
                    }
                }
            }
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //FILTRAR POR NOMBRE
            if (string.IsNullOrEmpty(txt_nota.Text))
            {
                //lIMPIAMOS EL FILTRO
                filtro = null;
                filtrar();
            }
            else
            {
                String nombreFiltrar = txt_nota.Text.ToString().ToLower();
                filtro = nombreFiltrar;
                filtrar();
            }

        }

        private void filtrar()
        {
            List<Nota> notas = NotaControlador.getAll();
            List<Nota> notas_filtradas;
            if (filtro != null)
            {
                // Filtrar las notas según el nombre
                notas_filtradas = notas.Where(x => x.nombre != null && x.nombre.ToLower().StartsWith(filtro)).ToList();

                if (notas_filtradas.Count > 0)
                {
                    lbl_nota.Text = notas_filtradas.First().nombre;
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

            }
        }

        private void cargarDataGridViewNotasDePerfume()
        {
            //CARGAR DATAGRIDVIEW DE NOTAS DE PERFUME
            notas_del_perfume = NotasDelPerfumeControlador.getByIDPerfume(perfume.id);
            notas_con_tipo_de_nota = new List<NotaConTipoDeNota>();

            Nota nota = null;
            TipoDeNota tipo_de_nota = null;

            //Ocultas la primera columna de la tabla (es una columna de seleccion de fila)
            dataGridViewNotasDelPerfume.RowHeadersVisible = false;
            dataGridViewNotasDelPerfume.Rows.Clear();
            dataGridViewNotasDelPerfume.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            if (notas_del_perfume != null)
            {
                //dataGridViewNotasDelPerfume.DataSource = notas;
                foreach (NotasDelPerfume nota_del_perfume in notas_del_perfume)
                {
                    notas_con_tipo_de_nota.Add(NotaConTipoDeNotaControlador.getByID(nota_del_perfume.notaConTipoDeNota.id));

                }
            }

            if (notas_con_tipo_de_nota != null)
            {
                //dataGridViewNotasDelPerfume.Rows.Clear();

                // Ordenar por ID del tipo de nota (1: salida, 2: corazón, 3: fondo)
                var notasOrdenadas = notas_con_tipo_de_nota
                    .OrderBy(n => n.tipoDeNota.id)
                    .ToList();

                foreach (NotaConTipoDeNota nota_con_tipo_de_nota_ in notasOrdenadas)
                {
                    nota = NotaControlador.getById(nota_con_tipo_de_nota_.nota.id);
                    tipo_de_nota = TipoDeNotaControlador.getById(nota_con_tipo_de_nota_.tipoDeNota.id);

                    int rowIndex = dataGridViewNotasDelPerfume.Rows.Add();
                    dataGridViewNotasDelPerfume.Rows[rowIndex].Cells[0].Value = nota_con_tipo_de_nota_.id;
                    dataGridViewNotasDelPerfume.Rows[rowIndex].Cells[1].Value = tipo_de_nota.nombre_tipo_de_nota;
                    dataGridViewNotasDelPerfume.Rows[rowIndex].Cells[2].Value = nota.nombre;
                    dataGridViewNotasDelPerfume.Rows[rowIndex].Cells[3].Value = "Eliminar";

                    // 🎨 Color de texto según el tipo de nota
                    switch (tipo_de_nota.nombre_tipo_de_nota.ToLower().Trim())
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

                // Quitar selección automática
                dataGridViewNotasDelPerfume.ClearSelection();

                // Asegurar que no se duplique el evento
                dataGridViewNotasDelPerfume.CellPainting -= dataGridViewNotasDelPerfume_CellPainting;
                dataGridViewNotasDelPerfume.CellPainting += dataGridViewNotasDelPerfume_CellPainting;

            }
        }

        private void cargarDataGridViewNotasDePerfume(List<NotaConTipoDeNota> notaConTipoDeNotas)
        {
            Nota nota = null;
            TipoDeNota tipo_de_nota = null;

            //Ocultas la primera columna de la tabla (es una columna de seleccion de fila)
            dataGridViewNotasDelPerfume.RowHeadersVisible = false;
            dataGridViewNotasDelPerfume.Rows.Clear();
            dataGridViewNotasDelPerfume.DefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Regular);

            if (notaConTipoDeNotas != null)
            {
                //dataGridViewNotasDelPerfume.Rows.Clear();

                // Ordenar por ID del tipo de nota (1: salida, 2: corazón, 3: fondo)
                var notasOrdenadas = notas_con_tipo_de_nota
                    .OrderBy(n => n.tipoDeNota.id)
                    .ToList();

                foreach (NotaConTipoDeNota nota_con_tipo_de_nota_ in notasOrdenadas)
                {
                    nota = NotaControlador.getById(nota_con_tipo_de_nota_.nota.id);
                    tipo_de_nota = TipoDeNotaControlador.getById(nota_con_tipo_de_nota_.tipoDeNota.id);

                    int rowIndex = dataGridViewNotasDelPerfume.Rows.Add();
                    dataGridViewNotasDelPerfume.Rows[rowIndex].Cells[0].Value = nota_con_tipo_de_nota_.id;
                    dataGridViewNotasDelPerfume.Rows[rowIndex].Cells[1].Value = tipo_de_nota.nombre_tipo_de_nota;
                    dataGridViewNotasDelPerfume.Rows[rowIndex].Cells[2].Value = nota.nombre;
                    dataGridViewNotasDelPerfume.Rows[rowIndex].Cells[3].Value = "Eliminar";

                    // 🎨 Color de texto según el tipo de nota
                    switch (tipo_de_nota.nombre_tipo_de_nota.ToLower().Trim())
                    {
                        case "nota de salida":
                            dataGridViewNotasDelPerfume.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Green;
                            break;
                        case "nota de corazón":
                            dataGridViewNotasDelPerfume.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Red;
                            break;
                        case "nota de fondo":
                            dataGridViewNotasDelPerfume.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.MediumPurple;
                            break;
                        default:
                            dataGridViewNotasDelPerfume.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.Black;
                            break;

                    }

                }

                // Quitar selección automática
                dataGridViewNotasDelPerfume.ClearSelection();

                // Asegurar que no se duplique el evento
                dataGridViewNotasDelPerfume.CellPainting -= dataGridViewNotasDelPerfume_CellPainting;
                dataGridViewNotasDelPerfume.CellPainting += dataGridViewNotasDelPerfume_CellPainting;
            }
        }
        private void btn_agregar_Click(object sender, EventArgs e)
        {
            string tipoDeNotaMarcado = null;

            if (checkedListBoxNota.CheckedItems.Count == 0)
            {
                lbl_error_seleccion_nota.Text = "Debe marcar un tipo de nota";
                lbl_error_seleccion_nota.Visible = true;
                return;
            }
            else if (!(string.IsNullOrEmpty(lbl_nota.Text) || lbl_nota.Text == "Nota"))
            {

                Nota nota = NotaControlador.getByNombre(lbl_nota.Text);
                tipoDeNotaMarcado = checkedListBoxNota.CheckedItems[0].ToString();
                TipoDeNota tipoDeNota = TipoDeNotaControlador.getByNombre(tipoDeNotaMarcado);
                NotaConTipoDeNota notaConTipoDeNota = NotaConTipoDeNotaControlador.getByNotaAndTipoDeNota(nota, tipoDeNota);
                notasDelPerfume = new NotasDelPerfume(perfume, notaConTipoDeNota);


                if (notas_con_tipo_de_nota == null)
                {
                    notas_con_tipo_de_nota = new List<NotaConTipoDeNota>();

                }
                if (notas_del_perfume == null)
                {
                    notas_del_perfume = new List<NotasDelPerfume>();
                }

                if (notas_con_tipo_de_nota.Any(x => x.id == notaConTipoDeNota.id))
                {
                    Console.WriteLine("Nota con tipo de nota id: " + notaConTipoDeNota.id);
                    lbl_error_seleccion_nota.Text = "Esta convinacion ya fue agregada";
                    lbl_error_seleccion_nota.Visible = true;

                }
                else
                {
                    lbl_error_seleccion_nota.Visible = false;
                    notas_con_tipo_de_nota.Add(notaConTipoDeNota);
                    notas_del_perfume.Add(notasDelPerfume);
                    //uncheckear el checkedlistbox
                    checkedListBoxNota.SetItemChecked(checkedListBoxNota.SelectedIndex, false);
                    //Limpiar el textbox de la nota
                    txt_nota.Clear();
                    //Resetea el label de la nota
                    lbl_nota.Text = "Nota";
                    cargarDataGridViewNotasDePerfume(notas_con_tipo_de_nota);
                }

            }
            else
            {
                lbl_error_seleccion_nota.Text = "Debe ingresar una nota";
                lbl_error_seleccion_nota.Visible = true;
                return;
            }

        }


        private async void btn_finalizar_Click(object sender, EventArgs e)
        {
            if (checkedListBoxAroma.CheckedItems.Count == 0)
            {
                lbl_error_seleccion_aroma.Text = "Debe marcar al menos un tipo de aroma";
                lbl_error_seleccion_aroma.Visible = true;
                return;
            }

            //Subir imagen si cambió
            await formEditarProducto.SubirSiCambioImagenAsync();
            //Actualizar el perfume con los datos que se han modificado
            perfume = formEditarProducto.editar();
            //Actualizar el perfume
            PerfumeControlador.update(perfume);

            // Lista de aromas marcados
            var listaDeAromasMarcados = checkedListBoxAroma.CheckedItems.Cast<string>().ToList();


            List<AromaDelPerfume> aromasDelPerfumeExistentes = AromaDelPerfumeControlador.getAllByIDPerfume(perfume.id);


            foreach (var aromaExistente in aromasDelPerfumeExistentes)
            {
                string nombreAromaExistente = TipoDeAromaControlador.getById(aromaExistente.tipoDeAroma.id).nombre;

                // Si el aroma existe en la base de datos pero no está en la lista de marcados, eliminarlo
                if (!listaDeAromasMarcados.Contains(nombreAromaExistente))
                {
                    AromaDelPerfumeControlador.deleteBYTipoDePerfume(aromaExistente.tipoDeAroma.id);
                }
            }

            // Paso 2: Agregar aromas que están marcados y no existen en la base de datos
            foreach (var tipoDeAromaMarcado in listaDeAromasMarcados)
            {
                TipoDeAroma tipoDeAroma = TipoDeAromaControlador.getByNombre(tipoDeAromaMarcado);

                // Verificar si el aroma ya está asociado
                bool yaExiste = aromasDelPerfumeExistentes.Any(aroma =>
                    aroma.tipoDeAroma.id == tipoDeAroma.id);

                if (!yaExiste)
                {
                    // Crear la relación entre el perfume y el aroma
                    AromaDelPerfume aromaDelPerfume = new AromaDelPerfume(perfume, tipoDeAroma);
                    AromaDelPerfumeControlador.create(aromaDelPerfume);
                }
            }

            // Eliminar todas las notas del perfume anteriores

            NotasDelPerfumeControlador.delete(perfume.id);

            // Insertar nuevas notas del perfume
            if (notas_del_perfume != null)
            {
                foreach (NotasDelPerfume notasDelPerfume in notas_del_perfume)
                {
                    NotasDelPerfumeControlador.create(notasDelPerfume);
                }
            }

            MessageBox.Show("Se han guardado los cambios del perfume correctamente.");
            this.Close();
            formEditarProducto.Close();
            //Actualizar la lista de perfumes en el perfumesUC
            perfumesUC.cargarPerfumes();
        }


        private void checkedListBoxNota_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                // Verifica si hay al menos un elemento marcado
                if (checkedListBoxNota.CheckedItems.Count == 0)
                {
                    lbl_tipo_de_nota.Text = "";
                }
                else
                {
                    // Desmarcar todos los elementos antes de marcar el nuevo
                    for (int i = 0; i < checkedListBoxNota.Items.Count; i++)
                    {
                        if (i != checkedListBoxNota.SelectedIndex) // Evita desmarcar el que acaba de ser seleccionado
                        {
                            checkedListBoxNota.SetItemChecked(i, false);
                        }
                    }
                    // Obtener el último elemento marcado
                    string ultimoMarcado = checkedListBoxNota.SelectedItem.ToString();
                    lbl_tipo_de_nota.Text = ultimoMarcado;
                }
            }));
        }

        private void btn_x_cerrar_Click(object sender, EventArgs e)
        {
            this.Close();
            formEditarProducto.Show();
        }


        private void dataGridViewNotasDelPerfume_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;
            int id = int.Parse(dataGridViewNotasDelPerfume.Rows[e.RowIndex].Cells[0].Value.ToString());
            if (senderGrid.Columns[e.ColumnIndex].Name == "Eliminar")
            {
                //ELIMINAMOS
                notas_con_tipo_de_nota = notas_con_tipo_de_nota.Where(x => x.id != id).ToList();
                notas_del_perfume = notas_del_perfume.Where(x => x.notaConTipoDeNota.id != id).ToList();
                cargarDataGridViewNotasDePerfume(notas_con_tipo_de_nota);
                MessageBox.Show("Se ha eliminado la nota con el tipo de nota del perfume correctamente");
            }
        }



        //Diseño del boton del datagridview
        private void dataGridViewNotasDelPerfume_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && dataGridViewNotasDelPerfume.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
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

        private void checkedListBoxAroma_ItemCheck(object sender, ItemCheckEventArgs e)
        {

        }

        //Verificar rayooo
        private void checkedListBoxAroma_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Evita el fondo por defecto
            e.DrawBackground();

            // Ítem actual
            string itemText = checkedListBoxAroma.Items[e.Index].ToString();

            // Determina si el ítem está seleccionado o chequeado
            bool isChecked = checkedListBoxAroma.GetItemChecked(e.Index);

            // Configuración de colores
            Color textColor = Color.Black;
            Color backgroundColor = Color.White;

            // Si está seleccionado o chequeado, cambia colores
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected || isChecked)
            {
                backgroundColor = Color.FromArgb(232, 186, 197); // Color personalizado
                textColor = Color.White;
            }

            // Pintar fondo y texto
            using (SolidBrush backgroundBrush = new SolidBrush(backgroundColor))
            using (SolidBrush textBrush = new SolidBrush(textColor))
            {
                e.Graphics.FillRectangle(backgroundBrush, e.Bounds);
                e.Graphics.DrawString(itemText, e.Font, textBrush, e.Bounds);
            }

            e.DrawFocusRectangle();
        }

        // Configuración del CheckedListBox
        private void FormCrearPerfume2_Load(object sender, EventArgs e)
        {
            checkedListBoxAroma.DrawMode = DrawMode.OwnerDrawFixed;
            checkedListBoxAroma.DrawItem += checkedListBoxAroma_DrawItem;
        }




        // Configuración del CheckedListBox
        private void FormEditarPerfume2_Load(object sender, EventArgs e)
        {
            checkedListBoxAroma.DrawMode = DrawMode.OwnerDrawFixed;
            checkedListBoxAroma.DrawItem += checkedListBoxAroma_DrawItem;
        }


    }
}