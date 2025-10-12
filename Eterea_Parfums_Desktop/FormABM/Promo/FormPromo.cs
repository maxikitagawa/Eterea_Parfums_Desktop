using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.DTOs;
using Eterea_Parfums_Desktop.Helpers;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Eterea_Parfums_Desktop
{
    public partial class FormPromo : Form
    {

        string situacion;

        int idPromo;

        Image banner;
        //int num = 0;

        private string promoBannerStemActual = null; // p.ej. "banner-black-friday"
        private string promoImagenUrlActual = null;  // URL completa devuelta por la API



        Dictionary<int, string> textosDescuento = new Dictionary<int, string>
        {
            { 50, "2 x 1" },
            { 40, "2da Unidad 80% Dto." },
            { 35, "2da Unidad 70% Dto." },
            { 25, "2da Unidad 50% Dto." },
            { 10, "Descuento especial del 10%" },
            { 0, "Sin descuento" }
        };

        private List<DataGridViewRow> backupRows = new List<DataGridViewRow>();


        // Declarar el ToolTip a nivel de la clase
        private ToolTip toolTipBorrar;

        //Crear variable local para guardar la promocion que se esta editando y obtener el nombre de la imagen
        Promocion nombBanner = new Promocion();

        public FormPromo()
        {

            InitializeComponent();

            //txt_nomb_promo.KeyPress += txt_nomb_promo_KeyPress;
            //txt_descripcion_promo.KeyPress += txt_descripcion_promo_KeyPress;

            // Ocultar etiquetas de error
            lbl_error_tipo_promo.Visible = false;
            lbl_error_nombP.Visible = false;
            lbl_error_desc_promo.Visible = false;
            lbl_error_fecha_iniP.Visible = false;
            lbl_error_fecha_finP.Visible = false;
            lbl_error_promo_act.Visible = false;
            lbl_error_banner.Visible = false;

            //Ocultar el boton para borrar el texto ingresado en la busqueda de promo por nombre
            lbl_borrar_texto.Visible = false;

            // Inicializar y configurar el ToolTip
            toolTipBorrar = new ToolTip();
            toolTipBorrar.SetToolTip(lbl_borrar_texto, "Borrar texto ingresado");

            //Cargar los combo_box, inicializar los DatePickers y cargar los perfumes al dataGridView de busqueda de perfumes
            cargarComboBoxDescuentos();
            cargarComboBoxMarcas();
            cargarComboBoxGeneros();
            inicializarDatePickers();
            cargarPerfumes();

            combo_activo_promo.Items.Clear();
            combo_activo_promo.Items.Add("Si");
            combo_activo_promo.Items.Add("No");
            combo_activo_promo.SelectedIndex = -1;

            situacion = "Creacion";

            //Diseño del combo box
            combo_tipo_promo.DrawMode = DrawMode.OwnerDrawFixed;
            combo_tipo_promo.DrawItem += comboBoxdiseño_DrawItem;
            combo_tipo_promo.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_activo_promo.DrawMode = DrawMode.OwnerDrawFixed;
            combo_activo_promo.DrawItem += comboBoxdiseño1_DrawItem;
            combo_activo_promo.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_buscar_marcaP.DrawMode = DrawMode.OwnerDrawFixed;
            combo_buscar_marcaP.DrawItem += comboBoxdiseño_DrawItem;
            combo_buscar_marcaP.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_buscar_generoP.DrawMode = DrawMode.OwnerDrawFixed;
            combo_buscar_generoP.DrawItem += comboBoxdiseño_DrawItem;
            combo_buscar_generoP.DropDownStyle = ComboBoxStyle.DropDownList;
        }










        //Cargar marcas en el combo_box

        private void cargarComboBoxMarcas()
        {
            // Obtener marcas desde la base de datos
            List<Marca> marcas = MarcaControlador.getAll();

            // Limpiar el ComboBox
            combo_buscar_marcaP.Items.Clear();

            // Agregar una opción para "todas las marcas"
            combo_buscar_marcaP.Items.Add(new KeyValuePair<int, string>(0, "Todas las marcas"));

            // Llenar el ComboBox con las marcas
            foreach (Marca marca in marcas)
            {
                combo_buscar_marcaP.Items.Add(new KeyValuePair<int, string>(marca.id, marca.nombre));
            }

            // Configurar DisplayMember y ValueMember
            combo_buscar_marcaP.DisplayMember = "Value";
            combo_buscar_marcaP.ValueMember = "Key";

            // Seleccionar "Todas las Marcas" por defecto
            combo_buscar_marcaP.SelectedIndex = 0;
        }









        //Cargar descuentos en el combo_box

        public void cargarComboBoxDescuentos()
        {
            combo_tipo_promo.Items.Clear(); // Limpia cualquier ítem anterior
            foreach (var item in textosDescuento)
            {
                combo_tipo_promo.Items.Add(new KeyValuePair<int, string>(item.Key, item.Value));
            }

            combo_tipo_promo.DisplayMember = "Value"; // Texto visible
            combo_tipo_promo.ValueMember = "Key";    // Valor asociado


        }









        //Cargar generos en el combo_box

        public void cargarComboBoxGeneros()
        {
            // Obtener marcas desde la base de datos
            List<Genero> generos = GeneroControlador.getAll();

            // Limpiar el ComboBox
            combo_buscar_generoP.Items.Clear();

            // Agregar una opción para "todos los géneros"
            combo_buscar_generoP.Items.Add(new KeyValuePair<int, string>(0, "Todos los géneros"));

            // Llenar el ComboBox con los generos

            foreach (Genero genero in generos)
            {
                combo_buscar_generoP.Items.Add(new KeyValuePair<int, string>(genero.id, genero.genero));
            }

            // Configurar DisplayMember y ValueMember
            combo_buscar_generoP.DisplayMember = "Value";
            combo_buscar_generoP.ValueMember = "Key";

            // Seleccionar "Todos los géneros" por defecto
            combo_buscar_generoP.SelectedIndex = 0;
        }








        //Inicializar los DatePickers

        private void inicializarDatePickers()
        {
            


            // Configura las fechas mínimas para los DateTimePicker
            dateTime_inicio_promo.MinDate = DateTime.Now; // No permite elegir fechas pasadas
            dateTime_inicio_promo.Value = DateTime.Now;   // Fecha predeterminada de inicio

            dateTime_fin_promo.MinDate = DateTime.Now.AddDays(1); // Fin debe ser al menos un día después de hoy
            dateTime_fin_promo.Value = DateTime.Now.AddDays(1);   // Fecha predeterminada de fin

            // Configura los valores personalizados de formato
            dateTime_inicio_promo.CustomFormat = " ";
            dateTime_inicio_promo.Format = DateTimePickerFormat.Custom;
            dateTime_inicio_promo.ValueChanged += dateTime_inicio_promo_ValueChanged;

            dateTime_fin_promo.CustomFormat = " ";
            dateTime_fin_promo.Format = DateTimePickerFormat.Custom;
            dateTime_fin_promo.ValueChanged += dateTime_fin_promo_ValueChanged;
        }










        //Método para activar restricciones en el datePicker de la fecha de inicio

        private void dateTime_inicio_promo_ValueChanged(object sender, EventArgs e)
        {

            // Establece el formato estándar para mostrar la fecha
            dateTime_inicio_promo.Format = DateTimePickerFormat.Short;

            // Establece la nueva fecha mínima para el DateTimePicker de fin
            DateTime nuevaFechaMinima = dateTime_inicio_promo.Value.AddDays(1);
            dateTime_fin_promo.MinDate = nuevaFechaMinima;

            if (dateTime_fin_promo.Value < nuevaFechaMinima)
            {
                MessageBox.Show(
                    "La fecha de fin seleccionada es inválida. Por favor, elija una fecha posterior a la fecha de inicio.",
                    "Fecha inválida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                // Restablece la fecha de fin a la nueva fecha mínima
                dateTime_fin_promo.Value = nuevaFechaMinima;
            }
        }








        //Método para activar restricciones en el datePicker de la fecha de fin

        private void dateTime_fin_promo_ValueChanged(object sender, EventArgs e)
        {

            // Establece el formato estándar para mostrar la fecha
            dateTime_fin_promo.Format = DateTimePickerFormat.Short;
        }


        //Método para cargar los datos en el dataGridView de la busqueda de perfumes


        private void cargarPerfumes(int filtroMarcaP = 0, string filtroNombreP = "", int filtroGeneroP = 0)
        {
            List<Perfume> perfumes = PerfumeControlador.getAll();

            //Se oculta la primera columna de la tabla (es una columna de seleccion de fila)
            dataGrid_resultado_busqueda_perfumes.RowHeadersVisible = false;

            //Se oculta la primera columna de la tabla (es una columna de seleccion de fila)
            dataGrid_perfumes_agregados_a_promo.RowHeadersVisible = false;

            dataGrid_resultado_busqueda_perfumes.Rows.Clear();

            foreach (Perfume perfume in perfumes)
            {
                // Aplica los filtros dinámicamente

                bool coincideMarca = filtroMarcaP == 0 || perfume.marca.id == filtroMarcaP;

                bool coincideNombre = string.IsNullOrEmpty(filtroNombreP) ||
                            perfume.nombre.IndexOf(filtroNombreP, StringComparison.OrdinalIgnoreCase) >= 0;


                bool coincideGenero = filtroGeneroP == 0 || perfume.genero.id == filtroGeneroP;

                if (coincideNombre && coincideMarca && coincideGenero)

                {
                    int rowIndex = dataGrid_resultado_busqueda_perfumes.Rows.Add();

                    dataGrid_resultado_busqueda_perfumes.Rows[rowIndex].Cells[0].Value = (MarcaControlador.getById(perfume.marca.id)).nombre;
                    dataGrid_resultado_busqueda_perfumes.Rows[rowIndex].Cells[1].Value = perfume.nombre.ToString();
                    dataGrid_resultado_busqueda_perfumes.Rows[rowIndex].Cells[2].Value = perfume.presentacion_ml.ToString();
                    dataGrid_resultado_busqueda_perfumes.Rows[rowIndex].Cells[3].Value = (GeneroControlador.getById(perfume.genero.id)).genero;
                    dataGrid_resultado_busqueda_perfumes.Rows[rowIndex].Cells[4].Value = "Agregar";
                    dataGrid_resultado_busqueda_perfumes.Rows[rowIndex].Cells[5].Value = perfume.id.ToString();
                }


                dataGrid_resultado_busqueda_perfumes.CellPainting += dataGridViewConsultas_CellPainting;

                dataGrid_perfumes_agregados_a_promo.CellPainting += dataGridViewConsultas1_CellPainting;
            }

        }



        //Método para aplicar el filtro por marca a la busqueda de perfumes cada vez que se detecte un cambio en el combo_box

        private void comboBox_Marcas_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Obtener el ID de la marca seleccionada (si hay una selección)
            int idMarcaSeleccionada = combo_buscar_marcaP.SelectedItem != null
                ? ((KeyValuePair<int, string>)combo_buscar_marcaP.SelectedItem).Key
                : 0; // Si no hay selección, usa 0 (todas las marcas)

            // Obtener el ID del género seleccionado (si hay una selección)
            int idGeneroSeleccionado = combo_buscar_generoP.SelectedItem != null
                ? ((KeyValuePair<int, string>)combo_buscar_generoP.SelectedItem).Key
                : 0; // Si no hay selección, usa 0 (todos los géneros)

            // Mantener el texto ingresado en el filtro de nombre
            string filtroNombre = txt_buscar_nombP.Text.Trim();

            // Recargar los perfumes con los valores actuales
            cargarPerfumes(filtroNombreP: filtroNombre, filtroMarcaP: idMarcaSeleccionada, filtroGeneroP: idGeneroSeleccionado);
        }
















        //Método para aplicar el filtro por nombre a la busqueda de perfumes cada vez que se detecte un cambio en el text_box

        private void txt_buscar_perfume_x_nombre_TextChanged(object sender, EventArgs e)
        {
            // Mostrar el label solo si hay texto ingresado
            lbl_borrar_texto.Visible = !string.IsNullOrWhiteSpace(txt_buscar_nombP.Text);

            // Obtén los valores de los filtros
            int filtroMarcaP = combo_buscar_marcaP.SelectedItem != null
              ? ((KeyValuePair<int, string>)combo_buscar_marcaP.SelectedItem).Key
              : 0; // Si no hay selección, el filtro es 0 (sin filtro)

            string filtroNombreP = txt_buscar_nombP.Text.Trim();

            int filtroGeneroP = combo_buscar_generoP.SelectedItem != null
                ? ((KeyValuePair<int, string>)combo_buscar_generoP.SelectedItem).Key
                : 0; // Si no hay selección, el filtro es 0 (sin filtro)



            // Actualiza el DataGridView
            cargarPerfumes(filtroMarcaP, filtroNombreP, filtroGeneroP);
        }










        //Acción del botón para borrar el texto del text_box de busqueda por nombre de perfume

        private void lbl_borrar_texto_Click(object sender, EventArgs e)
        {
            // Borra el texto en el TextBox de filtro por nombre
            txt_buscar_nombP.Text = "";

            // Ocultar el label al borrar el texto
            lbl_borrar_texto.Visible = false;

            // Llama al método de carga de perfumes, manteniendo los filtros actuales de marca y género
            int filtroMarcaP = (combo_buscar_marcaP.SelectedItem != null)
                ? ((KeyValuePair<int, string>)combo_buscar_marcaP.SelectedItem).Key
                : 0; // Si no hay selección de marca, se usa el filtro predeterminado (0 = todas las marcas)

            int filtroGeneroP = combo_buscar_generoP.SelectedItem != null
                ? ((KeyValuePair<int, string>)combo_buscar_generoP.SelectedItem).Key
                : 0; // Si no hay selección de género, se usa el filtro predeterminado (0 = todos los géneros)

            // Llama a cargarPerfumes con los filtros actuales de marca y género, y el filtro de nombre vacío
            cargarPerfumes(filtroMarcaP, "", filtroGeneroP);
        }









        //Método para aplicar el filtro por genero a la busqueda de perfumes cada vez que se detecte un cambio en el combo_box

        private void combo_genero_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Obtener el ID de la marca seleccionada (si hay una selección)
            int idMarcaSeleccionada = combo_buscar_marcaP.SelectedItem != null
                ? ((KeyValuePair<int, string>)combo_buscar_marcaP.SelectedItem).Key
                : 0; // Si no hay selección, usa 0 (todas las marcas)

            // Obtener el ID del género seleccionado (si hay una selección)
            int idGeneroSeleccionado = combo_buscar_generoP.SelectedItem != null
                ? ((KeyValuePair<int, string>)combo_buscar_generoP.SelectedItem).Key
                : 0; // Si no hay selección, usa 0 (todos los géneros)

            // Mantener el texto ingresado en el filtro de nombre
            string filtroNombre = txt_buscar_nombP.Text.Trim();

            // Recargar los perfumes con los valores actuales
            cargarPerfumes(filtroNombreP: filtroNombre, filtroMarcaP: idMarcaSeleccionada, filtroGeneroP: idGeneroSeleccionado);
        }









        //Acción del botón para quitar todos los filtros de la busqueda

        private void btn_quitar_filtros_Click(object sender, EventArgs e)
        {
            // Restablecer los filtros a sus valores predeterminados
            combo_buscar_marcaP.SelectedIndex = 0;  // "Todas las Marcas"
            txt_buscar_nombP.Text = "";             // Nombre vacío
            combo_buscar_generoP.SelectedIndex = 0; // "Todos los Géneros"

            // Recargar el DataGridView con todos los perfumes sin aplicar filtros
            cargarPerfumes(0, "", 0);
        }











        //SOBRECARGAR EL CONSTRUCTOR PARA INICIAR EL FORM CON LA INFO CARGADA, PARA EDITAR
        public FormPromo(Promocion promo)
        {
            InitializeComponent();

            //txt_nomb_promo.KeyPress += txt_nomb_promo_KeyPress;
            //txt_descripcion_promo.KeyPress += txt_descripcion_promo_KeyPress;

            // Ocultar etiquetas de error
            lbl_error_tipo_promo.Visible = false;
            lbl_error_nombP.Visible = false;
            lbl_error_desc_promo.Visible = false;
            lbl_error_fecha_iniP.Visible = false;
            lbl_error_fecha_finP.Visible = false;
            lbl_error_promo_act.Visible = false;
            lbl_error_banner.Visible = false;

            //Diseño del combo box
            combo_tipo_promo.DrawMode = DrawMode.OwnerDrawFixed;
            combo_tipo_promo.DrawItem += comboBoxdiseño_DrawItem;
            combo_tipo_promo.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_activo_promo.DrawMode = DrawMode.OwnerDrawFixed;
            combo_activo_promo.DrawItem += comboBoxdiseño1_DrawItem;
            combo_activo_promo.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_buscar_marcaP.DrawMode = DrawMode.OwnerDrawFixed;
            combo_buscar_marcaP.DrawItem += comboBoxdiseño_DrawItem;
            combo_buscar_marcaP.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_buscar_generoP.DrawMode = DrawMode.OwnerDrawFixed;
            combo_buscar_generoP.DrawItem += comboBoxdiseño_DrawItem;
            combo_buscar_generoP.DropDownStyle = ComboBoxStyle.DropDownList;

            // Llamar a los métodos de carga de datos
            cargarComboBoxDescuentos();
            cargarComboBoxMarcas();
            cargarComboBoxGeneros();
            cargarPerfumes();


            // Asignar el id de la promo al campo interno id_editar
            idPromo = promo.id;


            //Creamos la lista de perfumes utilizando PerfumeDTO
            List<PerfumeDTO> perfumes = new List<PerfumeDTO>();


            //Ocultar el boton para borrar el texto ingresado en la busqueda de promo por nombre
            lbl_borrar_texto.Visible = false;

            // Inicializar y configurar el ToolTip
            toolTipBorrar = new ToolTip();
            toolTipBorrar.SetToolTip(lbl_borrar_texto, "Borrar texto ingresado");


            // Llamada al método que carga los perfumes por idPromo
            PerfumeEnPromoControlador controladorPerfume = new PerfumeEnPromoControlador();
            perfumes = controladorPerfume.CargarPerfumesPorIdPromocion(idPromo);

            cargarPerfumesDePromo(perfumes);

            // Cargar el combo box para activo
            combo_activo_promo.Items.Clear();
            combo_activo_promo.Items.Add("Si");
            combo_activo_promo.Items.Add("No");
            combo_activo_promo.SelectedIndex = -1;

            // Asignar el descuento correspondiente al texto
            int descuento = promo.descuento; // Valor numérico del descuento obtenido de la BD

            if (textosDescuento.TryGetValue(descuento, out string textoDescuento))
            {
                // Encontramos el KeyValuePair correspondiente
                var selectedItem = combo_tipo_promo.Items
                    .Cast<KeyValuePair<int, string>>()
                    .FirstOrDefault(x => x.Key == descuento);

                if (!selectedItem.Equals(default(KeyValuePair<int, string>)))
                {
                    combo_tipo_promo.SelectedItem = selectedItem; // Asignamos el KeyValuePair completo
                }
                else
                {
                    MessageBox.Show("No se encontró un elemento coincidente en el ComboBox.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("El valor del descuento no tiene un texto asociado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            // Asignar los valores de la promoción a los controles del formulario
            txt_nomb_promo.Text = promo.nombre;
            
            txt_descripcion_promo.Text = promo.descripcion;

            // Primero, permitir cualquier fecha (evita restricciones previas)
            dateTime_inicio_promo.MinDate = DateTimePicker.MinimumDateTime;
            dateTime_fin_promo.MinDate = DateTimePicker.MinimumDateTime;

            // Asignar las fechas recuperadas de la BD
            dateTime_inicio_promo.Value = promo.fecha_inicio;
            dateTime_fin_promo.Value = promo.fecha_fin;

            // Establecer si la promoción está activa
            if (promo.activo == true)
            {
                combo_activo_promo.SelectedItem = "Si";
            }
            else
            {
                combo_activo_promo.SelectedItem = "No";
            }


            // Guardar en campos para uso posterior si querés
            promoBannerStemActual = promo.banner;         // ej. "banner-black-friday"
            promoImagenUrlActual = promo.imagen_URL;     // ej. "https://.../imagenes/banner-black-friday.jpg"

            // Preferí URL; si no hay, caé al archivo local por stem
            _ = CargarImagenPromoPreferUrlAsync(promoImagenUrlActual, promoBannerStemActual);

            situacion = "Edicion";
            lbl_crear_promo.Text = "Editar Promoción";
            btn_crear_promo.Text = "Editar Promoción";

        }













        //Método para cargar los datos en el dataGridView de los perfumes agregados a la promo

        private void cargarPerfumesDePromo(List<PerfumeDTO> perfumes)
        {    

            try
            {

                // Verificar que la lista de perfumes tiene datos
                if (perfumes != null && perfumes.Count > 0)
                {

                    //Se oculta la primera columna de la tabla (es una columna de seleccion de fila)
                    dataGrid_resultado_busqueda_perfumes.RowHeadersVisible = false;

                    // Limpiar las filas previas del DataGridView
                    dataGrid_perfumes_agregados_a_promo.Rows.Clear();

                    // Recorrer la lista de perfumes
                    foreach (var perfume in perfumes)
                    {
                        // Crear una nueva fila
                        int rowIndex = dataGrid_perfumes_agregados_a_promo.Rows.Add();

                        // Asignar valores a las celdas de la fila
                        dataGrid_perfumes_agregados_a_promo.Rows[rowIndex].Cells[0].Value = perfume.marca;
                        dataGrid_perfumes_agregados_a_promo.Rows[rowIndex].Cells[1].Value = perfume.nombre;
                        dataGrid_perfumes_agregados_a_promo.Rows[rowIndex].Cells[2].Value = perfume.mililitros;
                        dataGrid_perfumes_agregados_a_promo.Rows[rowIndex].Cells[3].Value = perfume.genero;
                        dataGrid_perfumes_agregados_a_promo.Rows[rowIndex].Cells[4].Value = "Eliminar"; // Texto del botón
                        dataGrid_perfumes_agregados_a_promo.Rows[rowIndex].Cells[5].Value = perfume.id; // ID del perfume
                    }
                }
                else
                {
                    // Si no hay perfumes, mostrar mensaje
                    MessageBox.Show("No se han encontrado perfumes para esta promoción.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los perfumes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


        }








        //Método para detectar cambios en el combo_box de tipo de promoción

        private void combo_tipo_promo_edit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (combo_tipo_promo.SelectedItem != null)
            {
                var selectedItem = (KeyValuePair<int, string>)combo_tipo_promo.SelectedItem;
                int descuentoSeleccionado = selectedItem.Key;
                // Aquí puedes trabajar con el descuentoSeleccionado, como guardarlo en una base de datos o usarlo en la lógica de tu aplicación
            }
        }









        //Acción del botón "Agregar todos" para agregar todos los perfumes del resultado de la busqueda a la promo que se esta creando/editando

        private void btn_agregar_todos_Click_1(object sender, EventArgs e)
        {
            int perfumesAgregados = 0;

            foreach (DataGridViewRow sourceRow in dataGrid_resultado_busqueda_perfumes.Rows)
            {
                int idPerfume = Convert.ToInt32(sourceRow.Cells[5].Value);

                // Si el perfume no está agregado, lo agregamos
                if (!EsPerfumeDuplicado(idPerfume))
                {
                    AgregarPerfumeAPromocion(sourceRow, idPerfume);
                    perfumesAgregados++;
                }
            }

            // 🔹 Mostrar mensaje único al final
            if (perfumesAgregados > 0)
            {
                MessageBox.Show($"Se han agregado {perfumesAgregados} perfumes correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No se agregó ningún perfume porque ya estaban en la promoción.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }










        // Acción del botón "Agregar" en el DataGridView de resultados de búsqueda de perfumes
        private void dataGrid_perfumes_agregados_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGrid_resultado_busqueda_perfumes.Columns["Agregar"].Index && e.RowIndex >= 0)
            {
                // Obtener el ID del perfume seleccionado
                DataGridViewRow selectedRow = dataGrid_resultado_busqueda_perfumes.Rows[e.RowIndex];
                int idPerfume = Convert.ToInt32(selectedRow.Cells["Id"].Value);

                // Verificar si ya fue agregado
                if (EsPerfumeDuplicado(idPerfume))
                {
                    MessageBox.Show("El perfume ya ha sido agregado a la promoción.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Agregar el perfume a la lista de perfumes agregados a la promoción
                AgregarPerfumeAPromocion(selectedRow, idPerfume);
            }
        }

        // Método para verificar si el perfume ya ha sido agregado
        private bool EsPerfumeDuplicado(int idPerfume)
        {
            foreach (DataGridViewRow row in dataGrid_perfumes_agregados_a_promo.Rows)
            {
                if (Convert.ToInt32(row.Cells["IdOK"].Value) == idPerfume)
                {
                    return true;
                }
            }
            return false;
        }

        // Método para agregar el perfume al DataGridView de perfumes agregados
        private void AgregarPerfumeAPromocion(DataGridViewRow sourceRow, int idPerfume)
        {
            int newRowIndex = dataGrid_perfumes_agregados_a_promo.Rows.Add();
            dataGrid_perfumes_agregados_a_promo.Rows[newRowIndex].Cells[0].Value = sourceRow.Cells[0].Value;
            dataGrid_perfumes_agregados_a_promo.Rows[newRowIndex].Cells[1].Value = sourceRow.Cells[1].Value;
            dataGrid_perfumes_agregados_a_promo.Rows[newRowIndex].Cells[2].Value = sourceRow.Cells[2].Value;
            dataGrid_perfumes_agregados_a_promo.Rows[newRowIndex].Cells[3].Value = sourceRow.Cells[3].Value;
            dataGrid_perfumes_agregados_a_promo.Rows[newRowIndex].Cells[4].Value = "Eliminar";
            dataGrid_perfumes_agregados_a_promo.Rows[newRowIndex].Cells[5].Value = idPerfume;
        }







        // ✅ Acción del botón "Eliminar" en el DataGridView de perfumes en la promoción
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!EsCeldaEliminar(e)) return;

            string nombrePerfume = dataGrid_perfumes_agregados_a_promo.Rows[e.RowIndex].Cells[1].Value.ToString();
            string mililitrosPerfume = dataGrid_perfumes_agregados_a_promo.Rows[e.RowIndex].Cells[2].Value.ToString();

            if (ConfirmarEliminacionPerfume(nombrePerfume, mililitrosPerfume))
            {
                dataGrid_perfumes_agregados_a_promo.Rows.RemoveAt(e.RowIndex);
            }
        }

        // ✅ Método para verificar si la celda clickeada es la de "Eliminar"
        private bool EsCeldaEliminar(DataGridViewCellEventArgs e)
        {
            return e.ColumnIndex == 4 && e.RowIndex >= 0;
        }

        // ✅ Método para confirmar la eliminación del perfume
        private bool ConfirmarEliminacionPerfume(string nombrePerfume, string mililitrosPerfume)
        {
            DialogResult resultado = MessageBox.Show(
                $"¿Estás seguro de que deseas eliminar el perfume '{nombrePerfume}' de '{mililitrosPerfume}'ml del listado?",
                "Confirmar eliminación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            return resultado == DialogResult.Yes;
        }








        //Acción del botón "Eliminar todos" para quitar todos los perfumes agregados a la lista de perfumes a los que aplica la promo

        private void btn_eliminar_todos_Click(object sender, EventArgs e)
        {
            if (!ConfirmarEliminacion()) return;

            // Respaldar antes de eliminar
            RespaldarPerfumesAgregados();

            // Eliminar todos los perfumes del DataGridView
            EliminarTodosLosPerfumes();

            MessageBox.Show("Todos los perfumes han sido eliminados correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Método para confirmar la eliminación con el usuario
        private bool ConfirmarEliminacion()
        {
            DialogResult resultado = MessageBox.Show(
                "¿Estás seguro de que deseas eliminar todos los perfumes agregados?",
                "Confirmación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            return resultado == DialogResult.Yes;
        }

        // Método para respaldar los perfumes antes de eliminarlos
        private void RespaldarPerfumesAgregados()
        {
            backupRows.Clear(); // Limpiar el respaldo anterior

            foreach (DataGridViewRow row in dataGrid_perfumes_agregados_a_promo.Rows)
            {
                if (!row.IsNewRow)
                {
                    backupRows.Add(row);
                }
            }
        }

        // Método para eliminar todas las filas del DataGridView
        private void EliminarTodosLosPerfumes()
        {
            dataGrid_perfumes_agregados_a_promo.Rows.Clear();
        }







        //Acción del botón para volver atras si se eliminaron todos los perfumes de la promo por error


        private void btrn_deshacer_eliminacion_Click(object sender, EventArgs e)
        {
            if (!HayDatosParaRestaurar()) return;

            RestaurarPerfumesEliminados();

            MessageBox.Show("Se ha deshecho la eliminación de los perfumes.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Método para verificar si hay datos en el respaldo
        private bool HayDatosParaRestaurar()
        {
            if (backupRows.Count == 0)
            {
                MessageBox.Show("No hay datos para deshacer la eliminación.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        // Método para restaurar los perfumes eliminados desde el respaldo
        private void RestaurarPerfumesEliminados()
        {
            foreach (DataGridViewRow row in backupRows)
            {
                int rowIndex = dataGrid_perfumes_agregados_a_promo.Rows.Add();
                for (int i = 0; i < row.Cells.Count; i++)
                {
                    dataGrid_perfumes_agregados_a_promo.Rows[rowIndex].Cells[i].Value = row.Cells[i].Value;
                }
            }

            // Limpiar el respaldo una vez restaurado
            backupRows.Clear();
        }







        /*private int numeroAleatorio()
        {
            Random rnd = new Random();
            int numero = rnd.Next(1000, 9999);
            return numero;
        }*/






        //Método para obtener los datos del formulario

        private Promocion ObtenerDatosPromo()
        {
            KeyValuePair<int, string> tipoPromo = (KeyValuePair<int, string>)combo_tipo_promo.SelectedItem;
            return new Promocion
            {
                descuento = tipoPromo.Key,
                nombre = txt_nomb_promo.Text,
                fecha_inicio = dateTime_inicio_promo.Value,
                fecha_fin = dateTime_fin_promo.Value,
                activo = combo_activo_promo.SelectedIndex == 0,
                descripcion = txt_descripcion_promo.Text,
                //banner = $"{txt_nomb_promo.Text}-{num}-banner"
            };
        }





        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

        private async Task<(bool ok, string stem, string publicUrl, string error)>
    SubirBannerPromoAsync(Image imagen, string nombrePromo)
        {
            try
            {
                if (imagen == null) return (false, null, null, "No hay imagen para subir.");

                string stem = AsignarNombreImagenHelper.BuildPromoFileStem(nombrePromo); // "banner-<slug>"
                string fileName = stem + ".jpg";
                string tempPath = Path.Combine(Path.GetTempPath(), fileName);

                // GUARDAR SIEMPRE UN CLON (evita locks/problemas con streams)
                using (var bmp = new Bitmap(imagen))
                {
                    bmp.Save(tempPath, ImageFormat.Jpeg);
                }

                try
                {
                    var result = await ApiImageUploader.UploadAsync(tempPath, fileName);
                    if (result == null || string.IsNullOrWhiteSpace(result.url))
                        return (false, null, null, "La API no devolvió 'url'.");

                    return (true, stem, result.url, null);
                }
                finally
                {
                    if (File.Exists(tempPath)) File.Delete(tempPath);
                }
            }
            catch (Exception ex)
            {
                return (false, null, null, ex.Message);
            }
        }

        private async Task CargarImagenPromoPreferUrlAsync(string url, string bannerStem)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(url))
                {
                    await CargarImagenDesdeUrlAsync(url);
                    return;
                }

                // Fallback: disco por stem
                CargarImagenDesdeDisco(bannerStem);
            }
            catch
            {
                pictBox_banner.Image?.Dispose();
                pictBox_banner.Image = new Bitmap(Properties.Resources.imagen_por_defecto);
            }
        }

        private async Task CargarImagenDesdeUrlAsync(string url)
        {
            using (var http = new HttpClient())
            using (var resp = await http.GetAsync(url))
            {
                resp.EnsureSuccessStatusCode();
                var bytes = await resp.Content.ReadAsByteArrayAsync();

                using (var ms = new MemoryStream(bytes))
                using (var img = Image.FromStream(ms))
                {
                    pictBox_banner.Image?.Dispose();
                    // Clonamos a Bitmap para evitar “error genérico en GDI+”
                    pictBox_banner.Image = new Bitmap(img);
                }
            }
        }

        private void CargarImagenDesdeDisco(string bannerStem)
        {
            try
            {
                var ruta = ObtenerRutaImagen(bannerStem); // Program.Ruta_Base + stem + ".jpg"
                if (File.Exists(ruta))
                {
                    using (var fs = new FileStream(ruta, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var img = Image.FromStream(fs))
                    {
                        pictBox_banner.Image?.Dispose();
                        pictBox_banner.Image = new Bitmap(img);
                    }
                }
                else
                {
                    pictBox_banner.Image?.Dispose();
                    pictBox_banner.Image = new Bitmap(Properties.Resources.imagen_por_defecto);
                }
            }
            catch
            {
                pictBox_banner.Image?.Dispose();
                pictBox_banner.Image = new Bitmap(Properties.Resources.imagen_por_defecto);
            }
        }


        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX


        //Método para guardar la imagen de la promoción

        /* private string GuardarImagenPromo(string nombrePromoSanitizado)
         {
             if (nuevaImagenCargada && pictBox_banner.Image != null)
             {
                 try
                 {
                     num = numeroAleatorio();
                     string nuevaRuta = Path.Combine(Program.Ruta_Base, $"{nombrePromoSanitizado}-{num}-banner.jpg");

                     pictBox_banner.Image.Save(nuevaRuta, ImageFormat.Jpeg);
                     return $"{nombrePromoSanitizado}-{num}-banner";
                 }
                 catch (Exception ex)
                 {
                     MessageBox.Show("No se pudo guardar la nueva foto: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                 }
             }
             return null;
         }
        */







        //Método para eliminar una imagen

        private bool EliminarImagen(string rutaImagen)
        {
            try
            {
                if (!ExisteImagen(rutaImagen))
                {
                    return false;
                }

                // 🔹 Liberar el PictureBox antes de eliminar la imagen
                pictBox_banner.Image?.Dispose();
                pictBox_banner.Image = null;

                // 🔹 Forzar la recolección de basura para asegurarnos de que el archivo se libere
                GC.Collect();
                GC.WaitForPendingFinalizers();

                // 🔹 Intentar eliminar la imagen
                File.Delete(rutaImagen);
                Console.WriteLine($"Imagen eliminada correctamente: {rutaImagen}");

                return true;
            }
            catch (IOException ex)
            {
                MessageBox.Show($"No se pudo eliminar la imagen porque está en uso: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocurrió un error inesperado al eliminar la imagen: {ex.Message}",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        // ✅ Método para verificar si la imagen existe
        private bool ExisteImagen(string rutaImagen)
        {
            return !string.IsNullOrEmpty(rutaImagen) && File.Exists(rutaImagen);
        }









        //Método para confirmar la edición
        private bool ConfirmarEdicion()
        {
            DialogResult resultado = MessageBox.Show(
                "¿Estás seguro de que deseas editar esta promoción?",
                "Confirmación",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (resultado == DialogResult.No)
            {
                MessageBox.Show("Edición cancelada.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
                return false;
            }
            return true;
        }








        /*//Método para renombrar una imagen

        private string RenombrarImagen(string nombreBannerAnterior, string nombrePromoSanitizado)
        {
            if (!string.IsNullOrEmpty(nombreBannerAnterior))
            {
                string rutaAnterior = Path.Combine(Program.Ruta_Base, nombreBannerAnterior + ".jpg");

                if (File.Exists(rutaAnterior))
                {
                    pictBox_banner.Image?.Dispose();
                    pictBox_banner.Image = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    string[] partesNombre = Path.GetFileNameWithoutExtension(rutaAnterior).Split('-');
                    string numAleatorio = partesNombre.Reverse().FirstOrDefault(p => int.TryParse(p, out _)) ?? new Random().Next(1000, 9999).ToString();

                    string nuevoNombreBanner = $"{nombrePromoSanitizado}-{numAleatorio}-banner";
                    string rutaNuevaEdit = Path.Combine(Path.GetDirectoryName(rutaAnterior), nuevoNombreBanner + ".jpg");

                    File.Move(rutaAnterior, rutaNuevaEdit);
                    Console.WriteLine($"Imagen renombrada: {rutaAnterior} -> {rutaNuevaEdit}");

                    pictBox_banner.Image = Image.FromFile(rutaNuevaEdit);
                    return nuevoNombreBanner;
                }
            }
            return nombreBannerAnterior;
        }*/








        //Metodo para actualizar el nombre de una imagen al haberse editado el nombre de la promocion

        private string ActualizarNombreImagenPromo(string nombreBannerAnterior, string nombrePromoSanitizado)
        {
            if (!string.IsNullOrEmpty(nombreBannerAnterior))
            {
                string rutaAnterior = Path.Combine(Program.Ruta_Base, nombreBannerAnterior + ".jpg");

                if (File.Exists(rutaAnterior))
                {
                    pictBox_banner.Image?.Dispose();
                    pictBox_banner.Image = null;
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // Extraer el número aleatorio del nombre anterior
                    string[] partesNombre = Path.GetFileNameWithoutExtension(rutaAnterior).Split('-');
                    string numAleatorio = partesNombre.Reverse().FirstOrDefault(p => int.TryParse(p, out _)) ?? new Random().Next(1000, 9999).ToString();

                    // Nuevo nombre con el mismo número aleatorio
                    string nuevoNombreBanner = $"{nombrePromoSanitizado}-{numAleatorio}-banner";
                    string rutaNueva = Path.Combine(Path.GetDirectoryName(rutaAnterior), nuevoNombreBanner + ".jpg");

                    try
                    {
                        File.Move(rutaAnterior, rutaNueva);
                        Console.WriteLine($"Imagen renombrada: {rutaAnterior} -> {rutaNueva}");

                        pictBox_banner.Image = Image.FromFile(rutaNueva);
                        return nuevoNombreBanner;
                    }
                    catch (IOException ex)
                    {
                        MessageBox.Show($"No se pudo renombrar la imagen: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            return nombreBannerAnterior;
        }





        //Método que ejecuta las acciones para crear la promoción (llama a PromoControlador.crearPromo(promoEditada))

        /*private void crearPromo()
        {
            // Obtener datos del formulario
            Promocion nuevaPromo = ObtenerDatosPromo();

            // Generar un nombre de imagen si se cargó una
            string nombrePromoSanitizado = string.Join("_", txt_nomb_promo.Text.Split(Path.GetInvalidFileNameChars()));
            string bannerPromo = GuardarImagenPromo(nombrePromoSanitizado);

            // Asignar el nombre de la imagen a la nueva promoción
            nuevaPromo.banner = bannerPromo ?? $"{nombrePromoSanitizado}-{num}-banner";

            // Crear la promoción en la base de datos
            if (PromoControlador.crearPromocion(nuevaPromo))
            {
                this.DialogResult = DialogResult.OK;
                MessageBox.Show("La promoción se creó exitosamente.", "Promoción creada", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }*/





        //Método editarPromo

        /*private bool editarPromo()
        {
            if (!ConfirmarEdicion()) return false;


            int idPromoEdit = idPromo;
            var seleccionTipoPromo = (KeyValuePair<int, string>)combo_tipo_promo.SelectedItem;
            int descuentoClave = seleccionTipoPromo.Key;
            bool activo = combo_activo_promo.SelectedIndex == 0;

            Promocion promoActual = PromoControlador.obtenerPorId(idPromo);
            string nombreBannerAnterior = promoActual.banner;

            string nombrePromoSanitizado = string.Join("_", txt_nomb_promo.Text.Split(Path.GetInvalidFileNameChars()));

            if (string.IsNullOrWhiteSpace(nombrePromoSanitizado))
            {
                MessageBox.Show("El nombre de la promoción no puede estar vacío.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }


            // Si se cargó una nueva imagen, se guarda y se actualiza el banner
            string nombreBannerNuevo = GuardarImagenPromo(nombrePromoSanitizado) ?? nombreBannerAnterior;

            // Si no se cargó una nueva imagen pero se cambió el nombre de la promo, renombrar la imagen anterior
            if (nombreBannerNuevo == nombreBannerAnterior && promoActual.nombre != txt_nomb_promo.Text)
            {
                nombreBannerNuevo = ActualizarNombreImagenPromo(nombreBannerAnterior, nombrePromoSanitizado);
            }

            // Si se guardó una nueva imagen, eliminar la anterior
            if (nombreBannerNuevo != nombreBannerAnterior)
            {
                EliminarImagen(Path.Combine(Program.Ruta_Base, nombreBannerAnterior + ".jpg"));
            }

            // Actualizar la promoción en la base de datos
            Promocion promoEditada = new Promocion(idPromo, txt_nomb_promo.Text, dateTime_inicio_promo.Value, dateTime_fin_promo.Value, descuentoClave, activo, txt_descripcion_promo.Text, nombreBannerNuevo);

            if (PromoControlador.editarPromo(promoEditada))
            {
                this.DialogResult = DialogResult.OK;
                CargarImagenPromo(nombreBannerNuevo);
                return true;
            }

            return false;
        }*/






        //Método para limpiar los mensajes de error

        private void limpiarMensajesError()
        {
            lbl_error_tipo_promo.Visible = false;
            lbl_error_nombP.Visible = false;
            lbl_error_desc_promo.Visible = false;
            lbl_error_fecha_iniP.Visible = false;
            lbl_error_fecha_finP.Visible = false;
            lbl_error_promo_act.Visible = false;
            lbl_error_banner.Visible = false;

        }









        private bool validarPromo(out string errorMsg)
        {
            errorMsg = "";
            limpiarMensajesError();

            bool esValido = true;

            if (!ValidarTipoPromo(ref errorMsg)) esValido = false;
            if (!ValidarNombrePromo(ref errorMsg)) esValido = false;
            if (!ValidarDescripcionPromo(ref errorMsg)) esValido = false;
            if (!ValidarFechasPromo(ref errorMsg)) esValido = false;
            if (!ValidarEstadoPromo(ref errorMsg)) esValido = false;
            if (!ValidarImagenPromo(ref errorMsg)) esValido = false;

            return esValido;
        }

        // ✅ MÉTODOS DE VALIDACIÓN INDIVIDUAL

        private bool ValidarTipoPromo(ref string errorMsg)
        {
            if (combo_tipo_promo.SelectedItem == null || string.IsNullOrEmpty(combo_tipo_promo.Text))
            {
                MostrarError(lbl_error_tipo_promo, "Debes seleccionar el tipo de promoción", ref errorMsg);
                return false;
            }
            OcultarError(lbl_error_tipo_promo);
            return true;
        }

        private bool ValidarNombrePromo(ref string errorMsg)
        {
            string caracteresPermitidos = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÁÉÍÓÚÑáéíóúñ0123456789 %.,-:()\r\n";

            string nombre = txt_nomb_promo.Text;
            bool esValido = true;

            if (string.IsNullOrEmpty(nombre))
            {
                MostrarError(lbl_error_nombP, "Debes ingresar el nombre de la promoción", ref errorMsg);
                esValido = false;
            }
            if (nombre.Length < 4 || nombre.Length > 80)
            {
                MostrarError(lbl_error_nombP, "El nombre de la promoción debe tener entre 4 y 80 caracteres.", ref errorMsg);
                esValido = false;
            }
            if (!EsTextoValido(nombre, caracteresPermitidos))
            {
                MostrarError(lbl_error_nombP, "Solo letras, números, espacios y los caracteres: %, ., ,, -, :, (, ).", ref errorMsg);
                esValido = false;
            }
            if (nombre.Count(c => c == '%') > 1)
            {
                MostrarError(lbl_error_nombP, "Solo se permite un símbolo % en el nombre de la promoción.", ref errorMsg);
                esValido = false;
            }

            if (esValido) OcultarError(lbl_error_nombP);
            return esValido;
        }

        private bool ValidarDescripcionPromo(ref string errorMsg)
        {
            string caracteresPermitidos = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzÁÉÍÓÚÑáéíóúñ0123456789 %.,-:()¡!\r\n";

            string descripcion = txt_descripcion_promo.Text;
            bool esValido = true;

            if (string.IsNullOrEmpty(descripcion))
            {
                MostrarError(lbl_error_desc_promo, "Debes ingresar una descripción para la promoción", ref errorMsg);
                esValido = false;
            }
            if (descripcion.Length < 4 || descripcion.Length > 180)
            {
                MostrarError(lbl_error_desc_promo, "La descripción debe tener entre 4 y 180 caracteres.", ref errorMsg);
                esValido = false;
            }
            if (!EsTextoValido(descripcion, caracteresPermitidos))
            {
                MostrarError(lbl_error_desc_promo, "Solo letras, números, espacios y los caracteres: %, ., ,, -, :, (, ), ¡, !.", ref errorMsg);
                esValido = false;
            }
            if (descripcion.Count(c => c == '%') > 1 || new[] { '.', ',', '-', ':', '(', ')', '¡', '!' }.Any(c => descripcion.Count(ch => ch == c) > 2))
            {
                MostrarError(lbl_error_desc_promo, "Solo un símbolo '%' y hasta 2 de c/u de: ., ,, -, :, (, ).", ref errorMsg);
                esValido = false;
            }

            if (esValido) OcultarError(lbl_error_desc_promo);
            return esValido;
        }

        private bool ValidarFechasPromo(ref string errorMsg)
        {
            bool esValido = true;

            if (dateTime_inicio_promo.Format == DateTimePickerFormat.Custom && dateTime_inicio_promo.CustomFormat == " ")
            {
                MostrarError(lbl_error_fecha_iniP, "Debes seleccionar una fecha de inicio", ref errorMsg);
                esValido = false;
            }
            else
            {
                OcultarError(lbl_error_fecha_iniP);
            }

            if (dateTime_fin_promo.Format == DateTimePickerFormat.Custom && dateTime_fin_promo.CustomFormat == " ")
            {
                MostrarError(lbl_error_fecha_finP, "Debes seleccionar una fecha de finalización", ref errorMsg);
                esValido = false;
            }
            else if (dateTime_fin_promo.Value <= dateTime_inicio_promo.Value)
            {
                MostrarError(lbl_error_fecha_finP, "La fecha de finalización debe ser posterior a la fecha de inicio", ref errorMsg);
                esValido = false;
            }
            else
            {
                OcultarError(lbl_error_fecha_finP);
            }

            return esValido;
        }

        private bool ValidarEstadoPromo(ref string errorMsg)
        {
            if (combo_activo_promo.SelectedItem == null || string.IsNullOrEmpty(combo_activo_promo.Text))
            {
                MostrarError(lbl_error_promo_act, "Debes seleccionar si la promoción está activa", ref errorMsg);
                return false;
            }
            OcultarError(lbl_error_promo_act);
            return true;
        }

        private bool ValidarImagenPromo(ref string errorMsg)
        {
            if (pictBox_banner.Image == null)
            {
                MostrarError(lbl_error_banner, "Debes cargar una Imagen", ref errorMsg);
                return false;
            }
            OcultarError(lbl_error_banner);
            return true;
        }

        // ✅ MÉTODO PARA MOSTRAR ERRORES DE VALIDACIÓN

        private void MostrarError(Label lbl, string mensaje, ref string errorMsg)
        {
            if (!errorMsg.Contains(mensaje))
            {
                errorMsg += mensaje + Environment.NewLine;
            }
            lbl.Text = mensaje;
            lbl.Show();
        }

        // ✅ MÉTODO PARA OCULTAR ERRORES DE VALIDACIÓN

        private void OcultarError(Label lbl)
        {
            lbl.Visible = false;
        }

        // ✅ MÉTODO AUXILIAR PARA VALIDAR CARACTERES PERMITIDOS

        private bool EsTextoValido(string texto, string caracteresPermitidos)
        {
            return texto.All(c => char.IsLetterOrDigit(c) || caracteresPermitidos.Contains(c));
        }








        // Método principal para asignar perfumes a la promoción
        private void asignarPerfumesAPromo(int idPromo)
        {
            List<int> idsPerfumes = ObtenerIdsPerfumesEnPromo();

            GuardarRelacionPromoPerfume(idPromo, idsPerfumes);
        }

        // ✅ Método para obtener los IDs de los perfumes en la promoción
        private List<int> ObtenerIdsPerfumesEnPromo()
        {
            List<int> idsPerfumes = new List<int>();

            foreach (DataGridViewRow fila in dataGrid_perfumes_agregados_a_promo.Rows)
            {
                if (fila.Cells[5].Value != null && int.TryParse(fila.Cells[5].Value.ToString(), out int idPerfume))
                {
                    idsPerfumes.Add(idPerfume);
                }
            }

            return idsPerfumes;
        }

        // ✅ Método para guardar la relación entre la promoción y los perfumes
        private void GuardarRelacionPromoPerfume(int idPromo, List<int> idsPerfumes)
        {
            foreach (int idPerfume in idsPerfumes)
            {
                PerfumeEnPromoControlador.guardarRelacionPromoPerfume(idPerfume, idPromo);
            }
        }








        //Método para limpiar el formulario

        /*  private void limpiarFormulario()
          {
              combo_tipo_promo.SelectedIndex = -1; // Deseleccionar tipo de promoción
              txt_nomb_promo.Clear(); // Limpiar nombre
              // Limpia fecha de inicio
              dateTime_inicio_promo.CustomFormat = " ";
              dateTime_inicio_promo.Format = DateTimePickerFormat.Custom;
              // Limpia fecha de fin
              dateTime_fin_promo.CustomFormat = " ";
              dateTime_fin_promo.Format = DateTimePickerFormat.Custom;
              combo_activo_promo.SelectedIndex = -1; // Para que el combo_box vuelva a quedar vacío
              inicializarDatePickers();
          }*/










        //Acción del botón crear/editar

        // Acción del botón crear/editar
        private async void btn_crear_promo_Click(object sender, EventArgs e)
        {
            bool esCreacion = (situacion == "Creacion");

            if (!ValidarYVerificarPromo(out string errorMsg, esCreacion))
            {
                return;
            }

            if (esCreacion)
            {
                ProcesarCreacionPromo();
            }
            else
            {
                await editarPromoAsync();
            }
        }

        // ✅ Método para validar la promoción y verificar si el nombre ya existe
        // Reemplazá tu método por este
        private bool ValidarYVerificarPromo(out string errorMsg, bool esCreacion)
        {
            bool promoValidada = validarPromo(out errorMsg);
            if (!promoValidada) return false;

            string nombrePromo = txt_nomb_promo.Text.Trim();

            if (esCreacion)
            {
                if (PromoControlador.ExisteNombrePromo(nombrePromo))
                {
                    MessageBox.Show("Ya existe una promoción con este nombre. Elija otro.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }
            else
            {
                // <<— evita conflicto contra sí misma
                if (PromoControlador.ExisteNombrePromo(nombrePromo, idPromo))
                {
                    MessageBox.Show("Ya existe otra promoción con este nombre. Elija otro.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }



        // ✅ Método para procesar la creación de una promoción
        private async void ProcesarCreacionPromo()
        {
            try
            {
                // 1) Subir la imagen a la API con nombre fijo banner-<slug>.jpg
                var (ok, stem, publicUrl, error) = await SubirBannerPromoAsync(pictBox_banner.Image, txt_nomb_promo.Text.Trim());

                if (!ok)
                {
                    MessageBox.Show("No se pudo subir el banner: " + error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 2) Armar el objeto Promocion con banner y URL
                var nuevaPromo = ObtenerDatosPromo();
                nuevaPromo.banner = stem;           // ej: "banner-black-friday"
                nuevaPromo.imagen_URL = publicUrl;  // URL pública devuelta por API

                // 3) Crear en BD
                if (!PromoControlador.crearPromocion(nuevaPromo))
                {
                    MessageBox.Show("No se pudo crear la promoción en la base de datos.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 4) Asignar perfumes a la promo creada
                int idPromoCreada = PromoControlador.obtenerMaxId();
                asignarPerfumesAPromo(idPromoCreada);

                this.DialogResult = DialogResult.OK;
                MessageBox.Show("La promoción se creó exitosamente.", "Promoción creada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al crear la promoción: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // ✅ Método para procesar la edición de una promoción
        private async Task<bool> editarPromoAsync()
        {
            if (!ConfirmarEdicion()) return false;

            // Promo actual desde BD
            var promoActual = PromoControlador.obtenerPorId(idPromo);
            string bannerAnterior = promoActual.banner;          // p.ej. "banner-black-friday"
            string urlAnterior = promoActual.imagen_URL;      // p.ej. https://.../imagenes/banner-black-friday.jpg

            // Valores por defecto (si no cambia nada)
            string nombreBannerNuevo = bannerAnterior;
            string urlNuevaOAnterior = urlAnterior;

            // ¿cambió el nombre?
            string nombreNuevo = txt_nomb_promo.Text.Trim();
            bool cambioNombre = !string.Equals(
                promoActual.nombre?.Trim(),
                nombreNuevo,
                StringComparison.Ordinal
            );

            // CASO 1: Se cargó una NUEVA imagen en el formulario
            if (nuevaImagenCargada && pictBox_banner.Image != null)
            {
                // Borrar archivo anterior del server para evitar sufijos -1, -2, ...
                try { await ApiImageUploader.DeleteAsync(bannerAnterior + ".jpg"); } catch { /* opcional: log/warn */ }

                // Subir la nueva imagen con el NUEVO stem (derivado del nombre actual del form)
                var (ok, stem, publicUrl, error) = await SubirBannerPromoAsync(pictBox_banner.Image, nombreNuevo);
                if (!ok)
                {
                    MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                nombreBannerNuevo = stem;      // ej: "banner-nuevo-slug"
                urlNuevaOAnterior = publicUrl; // ej: https://.../imagenes/banner-nuevo-slug.jpg
            }
            // CASO 2: NO se cargó imagen nueva, pero SÍ cambió el NOMBRE de la promo
            else if (cambioNombre)
            {
                // Necesitamos re-subir la MISMA imagen que se ve en el PictureBox con el NUEVO nombre (stem),
                // y luego borrar el archivo viejo del servidor para mantener consistencia.
                if (pictBox_banner.Image == null)
                {
                    MessageBox.Show(
                        "No hay imagen cargada en el banner para re-nombrar. Cargue la imagen o cancele el cambio de nombre.",
                        "Atención",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return false;
                }

                // Subimos la MISMA imagen (del PictureBox) pero con el NUEVO stem (derivado del nombre nuevo)
                var (ok, stem, publicUrl, error) = await SubirBannerPromoAsync(pictBox_banner.Image, nombreNuevo);
                if (!ok)
                {
                    MessageBox.Show(error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Si subió bien con el nuevo nombre, borramos el archivo anterior del server
                try { await ApiImageUploader.DeleteAsync(bannerAnterior + ".jpg"); } catch { /* opcional: log/warn */ }

                nombreBannerNuevo = stem;      // ej: "banner-nuevo-slug"
                urlNuevaOAnterior = publicUrl; // ej: https://.../imagenes/banner-nuevo-slug.jpg
            }
            // CASO 3: Ni imagen nueva ni cambio de nombre => se conservan banner/URL anteriores.

            // Descuento seleccionado
            var seleccionTipoPromo = (KeyValuePair<int, string>)combo_tipo_promo.SelectedItem;

            // Armar objeto actualizado
            var promoEditada = new Promocion(
                idPromo,
                nombreNuevo,
                dateTime_inicio_promo.Value,
                dateTime_fin_promo.Value,
                seleccionTipoPromo.Key,
                (combo_activo_promo.SelectedIndex == 0),
                txt_descripcion_promo.Text.Trim(),
                nombreBannerNuevo,
                urlNuevaOAnterior
            );

            // Persistir cambios
            if (PromoControlador.editarPromoYRelaciones(promoEditada, ObtenerIdsPerfumesEnPromo()))
            {
                this.DialogResult = DialogResult.OK;
                // (opcional) refrescar preview:
                _ = CargarImagenPromoPreferUrlAsync(urlNuevaOAnterior, nombreBannerNuevo);
                return true;
            }

            MessageBox.Show("No se pudo actualizar la promoción en BD.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }






        //Acción para el botón cerrar pantalla

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close(); // Cierra el formulario
        }








        // Variable booleana para controlar si se cargó una nueva imagen
        private bool nuevaImagenCargada = false;


        //Acción del botón "Seleccionar imagen"
        private void btn_selec_banner_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.webp" })
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    // soltar anteriores
                    banner?.Dispose();
                    pictBox_banner.Image?.Dispose();

                    using (var tmp = Image.FromFile(ofd.FileName)) // FromFile bloquea: clonamos
                    {
                        banner = new Bitmap(tmp);                   // banner “independiente”
                    }
                    pictBox_banner.Image = new Bitmap(banner);       // y clon para el PictureBox

                    nuevaImagenCargada = true;
                }
            }
        }



        //Método para cargar la imagen al formPromo "editar"

        private bool CargarImagenPromo(string nombreImagen)
        {
            try
            {
                string rutaImagen = ObtenerRutaImagen(nombreImagen);

                if (File.Exists(rutaImagen))
                {
                    using (var fs = new FileStream(rutaImagen, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var imgTemp = Image.FromStream(fs))
                    {
                        // liberar la anterior si existía
                        pictBox_banner.Image?.Dispose();
                        // clonar a Bitmap para que NO dependa del stream
                        pictBox_banner.Image = new Bitmap(imgTemp);
                    }
                    return true;
                }
                else
                {
                    MessageBox.Show(
                        "No se encontró la imagen: " + nombreImagen + ". Se cargará una imagen por defecto.",
                        "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    pictBox_banner.Image?.Dispose();
                    pictBox_banner.Image = new Bitmap(Properties.Resources.imagen_por_defecto);
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar la imagen: " + ex.Message, "Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);

                pictBox_banner.Image?.Dispose();
                pictBox_banner.Image = new Bitmap(Properties.Resources.imagen_por_defecto);
                return false;
            }
        }


        // ✅ Método para obtener la ruta de la imagen basada en el nombre
        private string ObtenerRutaImagen(string nombreImagen)
        {
            return Path.Combine(Program.Ruta_Base, nombreImagen + ".jpg");
        }


        //Diseño del boton del datagridview
        private void dataGridViewConsultas_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && dataGrid_perfumes_agregados_a_promo.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
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

        //Diseño del boton del datagridview
        private void dataGridViewConsultas1_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0 && dataGrid_resultado_busqueda_perfumes.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
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

            // Obtener el ítem como KeyValuePair
            var item = (KeyValuePair<int, string>)combo.Items[e.Index];
            string text = item.Value; // Mostrar solo el texto, no el número

            //string text = combo.Items[e.Index].ToString();

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

        //Diseño del combo box
        private void comboBoxdiseño1_DrawItem(object sender, DrawItemEventArgs e)
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
    }
}