using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.ControlesDeUsuario;
using Eterea_Parfums_Desktop.Helpers;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Eterea_Parfums_Desktop
{
    public partial class FormCrearPerfume1 : Form
    {
        public List<Marca> marcas;
        public List<TipoDePerfume> tiposDePerfume;
        public List<Genero> generos;
        public List<Pais> paises;
        private Image imagen1;
        private Image imagen2;

        private string nombre_foto_uno;
        private string nombre_foto_dos;

        private string pathLocalImg1;
        private string pathLocalImg2;

        public string urlImagen1Actual;
        public string urlImagen2Actual;

        private static readonly Random rnd = new Random(); //genero una sola instancia
        private Perfumes_UC perfumesUC;
        public FormCrearPerfume1(Perfumes_UC perfumesUC)
        {
            InitializeComponent();
            //relaciono el form de productos con el PerfumesUC
            this.perfumesUC = perfumesUC;
            LblErrorSetVisibleFalse();
            CargarMarcas();
            CargarTiposDePerfume();
            CargarGeneros();
            CargarPaises();
            CargarOpciones(combo_spray);
            CargarOpciones(combo_recargable);
            //CargarOpciones(combo_activo);

            // ===== LIMITES Y VALIDACIONES DE ENTRADA =====

            // Longitudes máximas coherentes con tus validaciones
            txt_codigo.MaxLength = 13;          // EAN-13
            txt_nombre.MaxLength = 80;          // tu validación ya usa 80
            richTextBox_descripcion.MaxLength = 1100; // tu validación ya usa 1100
            txt_anio_de_lanzamiento.MaxLength = 4;    // año (YYYY)
            txt_presentacion.MaxLength = 4;           // ej. 30, 50, 100
                                                      // txt_precio: lo dejamos sin MaxLength estricto por si necesitás precios grandes

            // === Teclado: KeyPress ===
            // Solo dígitos
            txt_codigo.KeyPress += OnlyDigits_KeyPress;
            txt_presentacion.KeyPress += OnlyDigits_KeyPress;
            txt_anio_de_lanzamiento.KeyPress += OnlyDigits_KeyPress;

            // Letras, números y los caracteres especiales permitidos: ", #, &, (, ), !, ¡, ?, ¿, *, =
            txt_nombre.KeyPress += NameAllowed_KeyPress;
            richTextBox_descripcion.KeyPress += NameAllowed_KeyPress;

            // Precio: dígitos + un único separador decimal (',' o '.')
            txt_precio.KeyPress += Price_KeyPress;

            // === Pegado / Limpieza: TextChanged ===
            // (si pegan contenido inválido, se limpia)
            txt_codigo.TextChanged += (s, e) => SanitizeDigitsOnly(txt_codigo);
            txt_presentacion.TextChanged += (s, e) => SanitizeDigitsOnly(txt_presentacion);
            txt_anio_de_lanzamiento.TextChanged += (s, e) => SanitizeDigitsOnly(txt_anio_de_lanzamiento);

            txt_nombre.TextChanged += (s, e) => SanitizeWithPredicate(txt_nombre, IsAllowedNameChar);
            richTextBox_descripcion.TextChanged += (s, e) => SanitizeWithPredicate(richTextBox_descripcion, IsAllowedNameChar);

            txt_precio.TextChanged += (s, e) => SanitizePrice(txt_precio);

            //Diseño del combo box
            /*combo_activo.DrawMode = DrawMode.OwnerDrawFixed;
            combo_activo.DrawItem += comboBoxdiseño_DrawItem;
            combo_activo.DropDownStyle = ComboBoxStyle.DropDownList;*/

            combo_marca.DrawMode = DrawMode.OwnerDrawFixed;
            combo_marca.DrawItem += comboBoxdiseño_DrawItem;
            combo_marca.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_genero.DrawMode = DrawMode.OwnerDrawFixed;
            combo_genero.DrawItem += comboBoxdiseño_DrawItem;
            combo_genero.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_tipo_de_perfume.DrawMode = DrawMode.OwnerDrawFixed;
            combo_tipo_de_perfume.DrawItem += comboBoxdiseño_DrawItem;
            combo_tipo_de_perfume.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_spray.DrawMode = DrawMode.OwnerDrawFixed;
            combo_spray.DrawItem += comboBoxdiseño_DrawItem;
            combo_spray.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_recargable.DrawMode = DrawMode.OwnerDrawFixed;
            combo_recargable.DrawItem += comboBoxdiseño_DrawItem;
            combo_recargable.DropDownStyle = ComboBoxStyle.DropDownList;

            combo_pais.DrawMode = DrawMode.OwnerDrawFixed;
            combo_pais.DrawItem += comboBoxdiseño_DrawItem;
            combo_pais.DropDownStyle = ComboBoxStyle.DropDownList;

            //Limito la cantidad de digitos que se pueden ingresar en el txt_codigo
            txt_codigo.MaxLength = 13;

            // === Auto-clear de errores ===
            // Entradas de texto
            HookTextHideError(txt_codigo, lbl_error_codigo);
            HookTextHideError(txt_nombre, lbl_error_nombre);
            HookTextHideError(txt_presentacion, lbl_error_presentacion);
            HookTextHideError(txt_anio_de_lanzamiento, lbl_error_anio);
            HookTextHideError(txt_precio, lbl_error_precio);
            HookTextHideError(richTextBox_descripcion, lbl_error_descripcion);

            // Combos
            HookComboHideError(combo_marca, lbl_error_marca);
            HookComboHideError(combo_tipo_de_perfume, lbl_error_tipo);
            HookComboHideError(combo_genero, lbl_error_genero);
            HookComboHideError(combo_pais, lbl_error_pais);
            HookComboHideError(combo_spray, lbl_error_spray);
            HookComboHideError(combo_recargable, lbl_error_recargable);

        }

        private void LblErrorSetVisibleFalse()
        {
            lbl_error_codigo.Visible = false;
            lbl_error_nombre.Visible = false;
            lbl_error_marca.Visible = false;
            lbl_error_tipo.Visible = false;
            lbl_error_genero.Visible = false;
            lbl_error_presentacion.Visible = false;
            lbl_error_pais.Visible = false;
            lbl_error_spray.Visible = false;
            lbl_error_recargable.Visible = false;
            lbl_error_descripcion.Visible = false;
            lbl_error_anio.Visible = false;
            lbl_error_precio.Visible = false;
            //lbl_error_activo.Visible = false;
            lbl_error_img1.Visible = false;
            lbl_error_img2.Visible = false;

        }

        // Oculta error cuando cambia texto (TextBox o RichTextBox)
        private void HookTextHideError(System.Windows.Forms.TextBoxBase tb, Label errorLabel)
        {
            if (tb == null || errorLabel == null) return;
            tb.TextChanged += (s, e) => errorLabel.Hide();
        }

        // Oculta error cuando seleccionan algo en ComboBox o cuando el texto coincide con un ítem
        private void HookComboHideError(ComboBox combo, Label errorLabel)
        {
            if (combo == null || errorLabel == null) return;

            combo.SelectionChangeCommitted += (s, e) => errorLabel.Hide();

            combo.TextChanged += (s, e) =>
            {
                var txt = combo.Text?.Trim() ?? "";
                if (string.IsNullOrEmpty(txt)) return;

                bool match = combo.Items
                    .Cast<object>()
                    .Select(i => i?.ToString())
                    .Any(it => string.Equals(it ?? "", txt, StringComparison.OrdinalIgnoreCase));

                if (match) errorLabel.Hide();
            };
        }


        private void CargarMarcas()
        {
            var marcas = MarcaControlador.getAll();
            combo_marca.Items.Clear();
            foreach (Marca marca in marcas)
            {
                combo_marca.Items.Add(marca.nombre.ToString());
            }
        }

        private void CargarTiposDePerfume()
        {
            var tiposDePerfume = TipoDePerfumeControlador.getAll();
            combo_tipo_de_perfume.Items.Clear();
            foreach (TipoDePerfume tipo in tiposDePerfume)
            {
                combo_tipo_de_perfume.Items.Add(tipo.tipo_de_perfume.ToString());
            }
        }

        private void CargarGeneros()
        {
            var generos = GeneroControlador.getAll();
            combo_genero.Items.Clear();
            foreach (Genero genero in generos)
            {
                combo_genero.Items.Add(genero.genero.ToString());
            }
        }

        private void CargarPaises()
        {
            var paises = PaisControlador.getAll();
            combo_pais.Items.Clear();
            foreach (Pais pais in paises)
            {
                if (pais.id != 1)
                    combo_pais.Items.Add(pais.nombre.ToString());
            }
        }

        private void CargarOpciones(ComboBox combo)
        {
            combo.Items.Clear();
            combo.Items.Add("Si");
            combo.Items.Add("No");
        }





        internal async Task guardarNuevaImg()
        {
            try
            {
                // IMAGEN 1
                if (imagen1 != null)
                {
                    nombre_foto_uno = AsignarNombreImagenHelper.BuildNombreImagen(txt_nombre.Text, "envase", compactSuffix: false);
                    string desiredFileName1 = nombre_foto_uno + ".jpg";

                    string temp1 = GuardarComoJpegTemporal(imagen1, desiredFileName1);
                    try
                    {
                        var result1 = await ApiImageUploader.UploadAsync(temp1, desiredFileName1);
                        urlImagen1Actual = result1.url; // la API te devuelve la URL final
                    }
                    finally { try { File.Delete(temp1); } catch { } }
                }

                // IMAGEN 2
                if (imagen2 != null)
                {
                    nombre_foto_dos = AsignarNombreImagenHelper.BuildNombreImagen(txt_nombre.Text, "envase y caja", compactSuffix: false);
                    string desiredFileName2 = nombre_foto_dos + ".jpg";

                    string temp2 = GuardarComoJpegTemporal(imagen2, desiredFileName2);
                    try
                    {
                        var result2 = await ApiImageUploader.UploadAsync(temp2, desiredFileName2);
                        urlImagen2Actual = result2.url;
                    }
                    finally { try { File.Delete(temp2); } catch { } }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error subiendo imagen: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw; // si querés abortar el guardado del perfume
            }
        }


        private void btn_siguiente_Click(object sender, EventArgs e)
        {

            //Validar datos del perfume
            bool validacionDatosPerfume = ValidarPerfume();

            if (validacionDatosPerfume)
            {
                Perfume perfume = crear();
                Console.WriteLine(perfume.id);
                FormCrearPerfume2 aromaNota = new FormCrearPerfume2(perfume, this, perfumesUC);
                //aromaNota.ShowDialog();

                DialogResult dr = aromaNota.ShowDialog(this);

                if (dr == DialogResult.OK)
                {
                    Trace.WriteLine("OK");

                }


            }

        }

        /*private void saveImagenResources(out string nombreFoto, Image imagen, string sufijo)
        {
            try
            {
                int numero_aleatorio = numeroAleatorio();
                Console.WriteLine(numero_aleatorio);
                nombreFoto = txt_nombre.Text + " - " + numero_aleatorio + " - " + sufijo;
                imagen.Save(Program.Ruta_Base + nombreFoto + ".jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }*/
        private void buildNombreImagen(out string nombreArchivoSinExtension, string sufijo)
        {
            int numero_aleatorio = numeroAleatorio();
            string baseNombre = (txt_nombre.Text ?? "").Trim();

            // Sanitizar (quitar caracteres inválidos de nombre de archivo)
            string inval = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (var c in inval) baseNombre = baseNombre.Replace(c.ToString(), "");

            nombreArchivoSinExtension = $"{baseNombre} - {numero_aleatorio} - {sufijo}";
        }


        // Guarda la imagen como JPEG a un archivo temporal y devuelve la ruta
        private string GuardarComoJpegTemporal(Image imagen, string nombreDeseadoConExtension)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), nombreDeseadoConExtension);

            // Si querés controlar calidad JPEG, podés usar ImageCodecInfo; aquí simple:
            imagen.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);

            return tempPath;
        }


        private int numeroAleatorio()
        {
            return rnd.Next(1000, 9999);
        }


        internal Perfume crear()
        {
            bool spray = false;
            if (combo_spray.SelectedItem.ToString() == "Si")
            {
                spray = true;
            }

            bool recargable = false;
            if (combo_recargable.SelectedItem.ToString() == "Si")
            {
                recargable = true;
            }

            /*bool activo = true;
            if (combo_activo.SelectedItem.ToString() == "No")
            {
                activo = false;
            }*/

            bool activo = false;

            Marca marca = MarcaControlador.getByName(combo_marca.SelectedItem.ToString());
            TipoDePerfume tipo_de_perfume = TipoDePerfumeControlador.getByName(combo_tipo_de_perfume.SelectedItem.ToString());
            Genero genero = GeneroControlador.getByName(combo_genero.SelectedItem.ToString());
            Console.WriteLine("Genero: " + genero.id);
            Pais pais = PaisControlador.getByName(combo_pais.SelectedItem.ToString());
            Console.WriteLine("Marca: " + marca.nombre);
            int id_Perfume = PerfumeControlador.GetByMaxId();
            Console.WriteLine("ID: " + id_Perfume);
            Perfume perfume = new Perfume(id_Perfume + 1, txt_codigo.Text, marca, txt_nombre.Text, tipo_de_perfume,
                genero, int.Parse(txt_presentacion.Text), pais, spray, recargable, richTextBox_descripcion.Text,
                int.Parse(txt_anio_de_lanzamiento.Text), Double.Parse(txt_precio.Text), activo, nombre_foto_uno, nombre_foto_dos, null, urlImagen1Actual, urlImagen2Actual);

            return perfume;

        }


        private string ValidarCodigoDeBarra()
        {
            if (string.IsNullOrEmpty(txt_codigo.Text))
            {
                lbl_error_codigo.Text = "El código no puede estar vacío.";
                lbl_error_codigo.Show();
                return "El código no puede estar vacío.";
            }

            if (txt_codigo.Text.Length != 13 || !txt_codigo.Text.All(char.IsDigit))
            {
                lbl_error_codigo.Text = "El código no es válido. Debe tener 13 dígitos numéricos.";
                lbl_error_codigo.Show();
                return "El código no es válido. Debe tener 13 dígitos numéricos.";
            }

            if (PerfumeControlador.getByCodigo(txt_codigo.Text) != null)
            {
                lbl_error_codigo.Text = "El código ya está registrado.";
                lbl_error_codigo.Show();
                return "El código ya está registrado.";
            }

            if (!ValidarEAN13(txt_codigo.Text))
            {
                lbl_error_codigo.Text = "El código no es válido. No cumple con EAN-13.";
                lbl_error_codigo.Show();
                return "El código no es válido. No cumple con EAN-13.";
            }

            lbl_error_codigo.Visible = false;
            return string.Empty; // No hay error
        }


        private bool ValidarEAN13(string codigo)
        {
            int suma = 0;
            for (int i = 0; i < 12; i++)
            {
                int digito = codigo[i] - '0';
                suma += (i % 2 == 0) ? digito : digito * 3;
            }
            int digitoControlEsperado = (10 - (suma % 10)) % 10;
            int digitoControlReal = codigo[12] - '0';

            return digitoControlEsperado == digitoControlReal;
        }

        private void txt_codigo_TextChanged(object sender, EventArgs e)
        {
            string errorCodigo = ValidarCodigoDeBarra();
            lbl_error_codigo.Visible = !string.IsNullOrEmpty(errorCodigo);
        }

        private bool ValidarPerfume()
        {

            string errorMsg = "";
            string errorCodigo = ValidarCodigoDeBarra();
            if (!string.IsNullOrEmpty(errorCodigo))
            {
                errorMsg += errorCodigo + Environment.NewLine;
            }
            else
            {
                lbl_error_codigo.Visible = false;
            }


            if (combo_marca.SelectedItem == null || string.IsNullOrEmpty(combo_marca.Text))
            {
                errorMsg += "Debes seleccionar la marca del perfume" + Environment.NewLine;
                lbl_error_marca.Text = "Debes seleccionar la marca del perfume";
                lbl_error_marca.Show();
            }
            else
            {
                lbl_error_marca.Visible = false;
            }


            if (string.IsNullOrEmpty(txt_nombre.Text))
            {
                errorMsg += "Debes ingresar el nombre del perfume" + Environment.NewLine;
                lbl_error_nombre.Text = "Debes ingresar el nombre del perfume";
                lbl_error_nombre.Show();

            }
            else if (txt_nombre.Text.Length > 80)
            {
                errorMsg += "El nombre no puede exceder los 80 caracteres" + Environment.NewLine;
                lbl_error_nombre.Text = "El nombre no puede exceder los 80 caracteres";
                lbl_error_nombre.Show();
            }
            else
            {

                lbl_error_nombre.Visible = false;

            }

            if (combo_tipo_de_perfume.SelectedItem == null || string.IsNullOrEmpty(combo_tipo_de_perfume.Text))
            {
                errorMsg += "Debes seleccionar un tipo de perfume" + Environment.NewLine;
                lbl_error_tipo.Text = "Debes seleccionar un tipo de perfume";
                lbl_error_tipo.Show();
            }
            else
            {
                lbl_error_tipo.Visible = false;
            }

            if (combo_genero.SelectedItem == null || string.IsNullOrEmpty(combo_genero.Text))
            {
                errorMsg += "Debes seleccionar un género" + Environment.NewLine;
                lbl_error_genero.Text = "Debes seleccionar un género";
                lbl_error_genero.Show();
            }
            else
            {
                lbl_error_genero.Visible = false;
            }

            if (string.IsNullOrEmpty(txt_presentacion.Text))
            {
                errorMsg += "Debes ingresar los ml en numero" + Environment.NewLine;
                lbl_error_presentacion.Text = "Debes ingresar los ml en numero";
                lbl_error_presentacion.Show();

            }
            else
            {
                if (!int.TryParse(txt_presentacion.Text, out int result))

                {
                    errorMsg += "Debes ingresar un número entero. Sólo números" + Environment.NewLine;
                    lbl_error_presentacion.Text = "Debes ingresar un número entero. Sólo números";
                    lbl_error_presentacion.Show();
                }
                else
                {
                    lbl_error_presentacion.Visible = false;
                }
            }

            if (combo_pais.SelectedItem == null || string.IsNullOrEmpty(combo_pais.Text))
            {
                errorMsg += "Debes ingresar el nombre del perfume" + Environment.NewLine;
                lbl_error_pais.Text = "Debes ingresar el nombre del perfume";
                lbl_error_pais.Show();
            }
            else
            {
                lbl_error_pais.Visible = false;
            }

            if (combo_spray.SelectedItem == null || string.IsNullOrEmpty(combo_spray.Text))
            {
                errorMsg += "Debes indicar si viene en formato spray o no" + Environment.NewLine;
                lbl_error_spray.Text = "Debes indicar si viene en formato spray o no";
                lbl_error_spray.Show();
            }
            else
            {
                lbl_error_spray.Visible = false;
            }

            if (combo_recargable.SelectedItem == null || string.IsNullOrEmpty(combo_recargable.Text))
            {
                errorMsg += "Debes indicar si es o no recargable" + Environment.NewLine;
                lbl_error_recargable.Text = "Debes indicar si es o no recargable";
                lbl_error_recargable.Show();
            }
            else
            {
                lbl_error_recargable.Visible = false;
            }

            if (string.IsNullOrEmpty(richTextBox_descripcion.Text))
            {
                errorMsg += "Debes ingresar la descripción del perfume" + Environment.NewLine;
                lbl_error_descripcion.Text = "Debes ingresar la descripción del perfume";
                lbl_error_descripcion.Show();

            }
            else if (richTextBox_descripcion.Text.Length > 1100)
            {
                errorMsg += "La descripción del perfume no puede exceder los 1100 caracteres" + Environment.NewLine;
                lbl_error_descripcion.Text = "La descripción del perfume no puede exceder los 1100 caracteres";
                lbl_error_descripcion.Show();
            }
            else
            {
                {
                    lbl_error_descripcion.Visible = false;
                }
            }

            if (string.IsNullOrEmpty(txt_anio_de_lanzamiento.Text))
            {
                errorMsg += "Debes ingresar el año de lanzamiento del perfume" + Environment.NewLine;
                lbl_error_anio.Text = "Debes ingresar el año de lanzamiento del perfume";
                lbl_error_anio.Show();

            }
            else
            {
                if (!int.TryParse(txt_anio_de_lanzamiento.Text, out int year) || year < 1900 || year > DateTime.Now.Year)
                {
                    errorMsg += "Debes ingresar un año válido" + Environment.NewLine;
                    lbl_error_anio.Text = "Debes ingresar un año válido";
                    lbl_error_anio.Show();
                }
                else
                {
                    lbl_error_anio.Visible = false;
                }
            }
            if (string.IsNullOrEmpty(txt_precio.Text))
            {
                errorMsg += "Debes ingresar un precio" + Environment.NewLine;
                lbl_error_precio.Text = "Debes ingresar un precio";
                lbl_error_precio.Show();

            }
            else
            {
                if (!double.TryParse(txt_precio.Text.Replace(",", "."), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double price) || price < 0)
                {
                    errorMsg += "Debes ingresar un precio válido" + Environment.NewLine;
                    lbl_error_precio.Text = "Debes ingresar un precio válido";
                    lbl_error_precio.Show();
                }
                else
                {
                    lbl_error_precio.Visible = false;
                }
            }

            /*if (combo_activo.SelectedItem == null || string.IsNullOrEmpty(combo_activo.Text))
            {
                errorMsg += "Debes indicar si el producto ingresa como activo o no" + Environment.NewLine;
                lbl_error_activo.Text = "Debes indicar si el producto ingresa como activo o no";
                lbl_error_activo.Show();
            }
            else
            {
                lbl_error_activo.Visible = false;
            }*/

            if (pictureBoxProducto1.Image == null)
            {
                errorMsg += "Debes cargar una Imagen" + Environment.NewLine;
                lbl_error_img1.Text = "Debes cargar una Imagen 1";
                lbl_error_img1.Show();

            }
            else
            {

                lbl_error_img1.Visible = false;

            }

            if (pictureBoxProducto2.Image == null)
            {
                errorMsg += "Debes cargar una Imagen" + Environment.NewLine;
                lbl_error_img2.Text = "Debes cargar una Imagen 2";
                lbl_error_img2.Show();

            }
            else
            {
                lbl_error_img2.Visible = false;
            }


            if (string.IsNullOrEmpty(errorMsg))
            {
                LblErrorSetVisibleFalse();
            }

            return string.IsNullOrEmpty(errorMsg);


        }

        private void btn_cargar_img1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JPG(*.JPG)|*.JPG|PNG(*.png)|*.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imagen1 = Image.FromFile(ofd.FileName);
                pictureBoxProducto1.Image = imagen1;

                lbl_error_img1.Hide();

                // ✅ Guardar la ruta local original
                pathLocalImg1 = ofd.FileName;
            }
        }

        private void btn_cargar_img2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JPG(*.JPG)|*.JPG|PNG(*.png)|*.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imagen2 = Image.FromFile(ofd.FileName);
                pictureBoxProducto2.Image = imagen2;

                lbl_error_img2.Hide();

                // ✅ Guardar la ruta local original
                pathLocalImg2 = ofd.FileName;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
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

        // Flag para evitar recursión al sanitizar
        private bool _sanitizing = false;

        // ====== PREDICADO DE CARACTERES PERMITIDOS PARA NOMBRE/DESCRIPCIÓN ======
        private static readonly HashSet<char> _allowedSpecials = new HashSet<char>
{
    '"', '#', '&', '(', ')', '!', '¡', '?', '¿', '*', '=', ';', ' '  // incluye espacio
};

        private static bool IsAllowedNameChar(char c)
        {
            // Letras (incluye acentos), números o caracteres especiales definidos
            return char.IsLetterOrDigit(c) || _allowedSpecials.Contains(c);
        }

        // ====== KEYPRESS HANDLERS ======
        private void OnlyDigits_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Permitir teclas de control (Backspace, etc.)
            if (char.IsControl(e.KeyChar)) return;

            // Solo dígitos
            if (!char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        private void NameAllowed_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            if (!IsAllowedNameChar(e.KeyChar))
                e.Handled = true;
        }

        private void Price_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;

            var tb = sender as TextBox;
            if (tb == null) return;

            // Aceptar dígitos
            if (char.IsDigit(e.KeyChar)) return;

            // Aceptar un único separador decimal (coma o punto)
            if (e.KeyChar == ',' || e.KeyChar == '.')
            {
                if (tb.Text.Contains(",") || tb.Text.Contains("."))
                    e.Handled = true; // ya hay separador
                return;
            }

            // Bloquear todo lo demás
            e.Handled = true;
        }

        // ====== SANITIZADORES (para pegado / set programático) ======
        private void SanitizeDigitsOnly(TextBox tb)
        {
            if (_sanitizing) return;
            _sanitizing = true;
            try
            {
                if (tb == null) return;
                var digits = new string(tb.Text.Where(char.IsDigit).ToArray());
                if (digits != tb.Text)
                {
                    int sel = tb.SelectionStart;
                    tb.Text = digits;
                    tb.SelectionStart = Math.Min(sel, tb.Text.Length);
                }
            }
            finally { _sanitizing = false; }
        }

        private void SanitizeWithPredicate(TextBoxBase tb, Func<char, bool> predicate)
        {
            if (_sanitizing) return;
            _sanitizing = true;
            try
            {
                if (tb == null) return;
                string original = tb.Text ?? "";
                string cleaned = new string(original.Where(predicate).ToArray());
                if (cleaned != original)
                {
                    int sel = (tb as TextBox)?.SelectionStart ?? (tb as RichTextBox)?.SelectionStart ?? cleaned.Length;
                    tb.Text = cleaned;

                    if (tb is TextBox t1)
                        t1.SelectionStart = Math.Min(sel, t1.Text.Length);
                    else if (tb is RichTextBox r1)
                        r1.SelectionStart = Math.Min(sel, r1.Text.Length);
                }
            }
            finally { _sanitizing = false; }
        }

        private void SanitizePrice(TextBox tb)
        {
            if (_sanitizing) return;
            _sanitizing = true;
            try
            {
                if (tb == null) return;

                // Mantener solo dígitos y separadores ',' '.'
                string onlyAllowed = new string(tb.Text.Where(ch => char.IsDigit(ch) || ch == ',' || ch == '.').ToArray());

                // Si hay más de un separador, conservar el primero y eliminar los restantes
                int firstSep = -1;
                var chars = new List<char>(onlyAllowed.Length);
                for (int i = 0; i < onlyAllowed.Length; i++)
                {
                    char c = onlyAllowed[i];
                    if (c == ',' || c == '.')
                    {
                        if (firstSep == -1)
                        {
                            firstSep = i;
                            chars.Add(c);
                        }
                        // else: omitir separadores extra
                    }
                    else
                    {
                        chars.Add(c);
                    }
                }

                string cleaned = new string(chars.ToArray());

                // Normalizamos: si preferís, podés dejar coma; tu validador acepta reemplazo de coma por punto
                // Acá no reemplazo nada, solo limpio múltiples separadores.

                if (cleaned != tb.Text)
                {
                    int sel = tb.SelectionStart;
                    tb.Text = cleaned;
                    tb.SelectionStart = Math.Min(sel, tb.Text.Length);
                }
            }
            finally { _sanitizing = false; }
        }


    }
}
