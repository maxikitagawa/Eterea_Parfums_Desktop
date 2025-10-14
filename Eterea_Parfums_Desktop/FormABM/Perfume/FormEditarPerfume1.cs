using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.ControlesDeUsuario;
using Eterea_Parfums_Desktop.Helpers;
using Eterea_Parfums_Desktop.Modelos;
using System.Configuration;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop
{
    public partial class FormEditarPerfume1 : Form
    {
        private Image imagen1;
        private Image imagen2;

        private string nombre_foto_uno;
        private string nombre_foto_dos;

        private string pathLocalImg1;
        private string pathLocalImg2;

        private string urlImagen1Actual;
        private string urlImagen2Actual;

        private Perfume perfume;
        private static readonly Random rnd = new Random();
        private Perfumes_UC perfumesUC;

        private const string SufijoImg1 = "envase";
        private const string SufijoImg2 = "envaseycaja";

        public FormEditarPerfume1()
        {
            InitializeComponent();
        }

        public FormEditarPerfume1(Perfume perfume, Perfumes_UC perfumesUC)
        {
            InitializeComponent();

            this.perfumesUC = perfumesUC;
            LblErrorSetVisibleFalse();
            this.perfume = perfume;
            CargarMarcas();
            CargarTiposDePerfume();
            CargarGeneros();
            CargarPaises();
            CargarOpciones(combo_spray);
            CargarOpciones(combo_recargable);
            CargarOpciones(combo_activo);
            cargarDatos(perfume);

            this.Shown += (_, __) =>
            {
                // Asegura datos consistentes (por si vino incompleto)
                if (string.IsNullOrWhiteSpace(perfume.imagen1_URL) || string.IsNullOrWhiteSpace(perfume.imagen2_URL)
                    || string.IsNullOrWhiteSpace(nombre_foto_uno) || string.IsNullOrWhiteSpace(nombre_foto_dos))
                {
                    var p = PerfumeControlador.getByID(perfume.id);
                    if (p != null)
                    {
                        perfume.imagen1_URL = string.IsNullOrWhiteSpace(perfume.imagen1_URL) ? p.imagen1_URL : perfume.imagen1_URL;
                        perfume.imagen2_URL = string.IsNullOrWhiteSpace(perfume.imagen2_URL) ? p.imagen2_URL : perfume.imagen2_URL;
                        nombre_foto_uno = string.IsNullOrWhiteSpace(nombre_foto_uno) ? p.imagen1 : nombre_foto_uno;
                        nombre_foto_dos = string.IsNullOrWhiteSpace(nombre_foto_dos) ? p.imagen2 : nombre_foto_dos;
                    }
                }

                var basePublica = GetPublicImagesBase();

                // --- URL 1 ---
                var url1 = !string.IsNullOrWhiteSpace(perfume.imagen1_URL)
                    ? AsignarNombreImagenHelper.EncodeLight(perfume.imagen1_URL)
                    : (string.IsNullOrWhiteSpace(nombre_foto_uno) || string.IsNullOrWhiteSpace(basePublica)
                        ? null
                        : AsignarNombreImagenHelper.ToPublicUrl(basePublica, nombre_foto_uno));

                pictureBoxProducto1.InitialImage = Properties.Resources.sinImagen;
                pictureBoxProducto1.ErrorImage = Properties.Resources.sinImagen;
                pictureBoxProducto1.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBoxProducto1.ImageLocation = url1;
                pictureBoxProducto1.LoadAsync();

                // --- URL 2 ---
                var url2 = !string.IsNullOrWhiteSpace(perfume.imagen2_URL)
                    ? AsignarNombreImagenHelper.EncodeLight(perfume.imagen2_URL)
                    : (string.IsNullOrWhiteSpace(nombre_foto_dos) || string.IsNullOrWhiteSpace(basePublica)
                        ? null
                        : AsignarNombreImagenHelper.ToPublicUrl(basePublica, nombre_foto_dos));

                pictureBoxProducto2.InitialImage = Properties.Resources.sinImagen;
                pictureBoxProducto2.ErrorImage = Properties.Resources.sinImagen;
                pictureBoxProducto2.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBoxProducto2.ImageLocation = url2;
                pictureBoxProducto2.LoadAsync();
            };

            // Diseño combos
            combo_activo.DrawMode = DrawMode.OwnerDrawFixed;
            combo_activo.DrawItem += comboBoxdiseño_DrawItem;
            combo_activo.DropDownStyle = ComboBoxStyle.DropDownList;

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

            txt_codigo.MaxLength = 13;
        }

        private void cargarDatos(Perfume perfume)
        {
            txt_codigo.Text = perfume.codigo;
            combo_marca.Text = perfume.marca.nombre;
            txt_nombre.Text = perfume.nombre;
            combo_tipo_de_perfume.Text = perfume.tipo_de_perfume.tipo_de_perfume;
            combo_genero.SelectedItem = perfume.genero.genero;
            txt_presentacion.Text = perfume.presentacion_ml.ToString();
            combo_pais.Text = perfume.pais.nombre;

            combo_spray.Text = perfume.spray ? "Si" : "No";
            combo_recargable.Text = perfume.recargable ? "Si" : "No";

            richTextBox_descripcion.Text = perfume.descripcion;
            txt_anio_de_lanzamiento.Text = perfume.anio_de_lanzamiento.ToString();
            txt_precio.Text = perfume.precio_en_pesos.ToString();

            if (perfume.activo.HasValue)
                combo_activo.SelectedItem = perfume.activo.Value ? "Si" : "No";
            else
                combo_activo.SelectedItem = "No especificado";

            nombre_foto_uno = perfume.imagen1;
            nombre_foto_dos = perfume.imagen2;

            urlImagen1Actual = perfume.imagen1_URL?.Trim();
            urlImagen2Actual = perfume.imagen2_URL?.Trim();

            Debug.WriteLine("URL1: " + urlImagen1Actual);
            Debug.WriteLine("URL2: " + urlImagen2Actual);
        }

        private static string GetPublicImagesBase()
        {
            var baseUrl = (ConfigurationManager.AppSettings["PublicImagesBaseUrl"]
                           ?? ConfigurationManager.AppSettings["ApiBaseUrl"])
                          ?.TrimEnd('/');
            var folder = ConfigurationManager.AppSettings["PublicImagesFolder"] ?? "/imagenes";
            var folderClean = folder.Trim().Trim('/');

            return string.IsNullOrWhiteSpace(baseUrl) ? null : $"{baseUrl}/{folderClean}";
        }

        // ========================= NUEVO: helpers de imagen/nombre =========================

        /*private static string ExtraerNombreArchivoDesdeUrl(string urlCompleta)
        {
            if (string.IsNullOrWhiteSpace(urlCompleta)) return null;
            try { return Path.GetFileName(new Uri(urlCompleta).AbsolutePath); }
            catch { return Path.GetFileName(urlCompleta); }
        }

        private string GetNombreFinalImg1() => $"perfume-{perfume.id}-envase.jpg";
        private string GetNombreFinalImg2() => $"perfume-{perfume.id}-envase-y-caja.jpg";

        // Guarda la Image en un .jpg temporal y devuelve la ruta
        private string GuardarComoJpegTemporal(Image imagen, string nombreDeseadoConExtension)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), nombreDeseadoConExtension);
            imagen.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            return tempPath;
        }*/

        // ======================= FIN helpers =======================

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
            lbl_error_activo.Visible = false;
            lbl_error_img1.Visible = false;
            lbl_error_img2.Visible = false;
        }

        private void CargarMarcas()
        {
            var marcas = MarcaControlador.getAll();
            combo_marca.Items.Clear();
            foreach (Marca marca in marcas)
                combo_marca.Items.Add(marca.nombre.ToString());
        }

        private void CargarTiposDePerfume()
        {
            var tiposDePerfume = TipoDePerfumeControlador.getAll();
            combo_tipo_de_perfume.Items.Clear();
            foreach (TipoDePerfume tipo in tiposDePerfume)
                combo_tipo_de_perfume.Items.Add(tipo.tipo_de_perfume.ToString());
        }

        private void CargarGeneros()
        {
            var generos = GeneroControlador.getAll();
            combo_genero.Items.Clear();
            foreach (Genero genero in generos)
                combo_genero.Items.Add(genero.genero.ToString());
        }

        private void CargarPaises()
        {
            var paises = PaisControlador.getAll();
            combo_pais.Items.Clear();
            foreach (Pais pais in paises)
                if (pais.id != 1) combo_pais.Items.Add(pais.nombre.ToString());
        }

        private void CargarOpciones(ComboBox combo)
        {
            combo.Items.Clear();
            combo.Items.Add("Si");
            combo.Items.Add("No");
        }

        private void btn_cargar_img1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JPG(*.JPG)|*.JPG|PNG(*.png)|*.png";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imagen1 = Image.FromFile(ofd.FileName);
                pictureBoxProducto1.Image = imagen1;
                pathLocalImg1 = ofd.FileName; // ruta local
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
                pathLocalImg2 = ofd.FileName; // ruta local
            }
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

            var perfumeExistente = PerfumeControlador.getByCodigo(txt_codigo.Text);
            if (perfumeExistente != null && perfumeExistente.codigo != perfume.codigo)
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
            return string.Empty;
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
                errorMsg += errorCodigo + Environment.NewLine;
            else
                lbl_error_codigo.Visible = false;

            if (combo_genero.SelectedItem == null || string.IsNullOrEmpty(combo_marca.Text))
            {
                errorMsg += "Debes seleccionar la marca del perfume" + Environment.NewLine;
                lbl_error_marca.Text = "Debes seleccionar la marca del perfume";
                lbl_error_marca.Show();
            }
            else lbl_error_marca.Visible = false;

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
            else lbl_error_nombre.Visible = false;

            if (combo_tipo_de_perfume.SelectedItem == null || string.IsNullOrEmpty(combo_tipo_de_perfume.Text))
            {
                errorMsg += "Debes seleccionar un tipo de perfume" + Environment.NewLine;
                lbl_error_tipo.Text = "Debes seleccionar un tipo de perfume";
                lbl_error_tipo.Show();
            }
            else lbl_error_tipo.Visible = false;

            if (combo_genero.SelectedItem == null || string.IsNullOrEmpty(combo_genero.Text))
            {
                errorMsg += "Debes seleccionar un género" + Environment.NewLine;
                lbl_error_genero.Text = "Debes seleccionar un género";
                lbl_error_genero.Show();
            }
            else lbl_error_genero.Visible = false;

            if (string.IsNullOrEmpty(txt_presentacion.Text))
            {
                errorMsg += "Debes ingresar los ml en numero" + Environment.NewLine;
                lbl_error_presentacion.Text = "Debes ingresar los ml en numero";
                lbl_error_presentacion.Show();
            }
            else
            {
                if (!int.TryParse(txt_presentacion.Text, out int _))
                {
                    errorMsg += "Debes ingresar un número entero. Sólo números" + Environment.NewLine;
                    lbl_error_presentacion.Text = "Debes ingresar un número entero. Sólo números";
                    lbl_error_presentacion.Show();
                }
                else lbl_error_presentacion.Visible = false;
            }

            if (combo_pais.SelectedItem == null || string.IsNullOrEmpty(combo_pais.Text))
            {
                errorMsg += "Debes ingresar el nombre del perfume" + Environment.NewLine;
                lbl_error_pais.Text = "Debes ingresar el nombre del perfume";
                lbl_error_pais.Show();
            }
            else lbl_error_pais.Visible = false;

            if (combo_spray.SelectedItem == null || string.IsNullOrEmpty(combo_spray.Text))
            {
                errorMsg += "Debes indicar si viene en formato spray o no" + Environment.NewLine;
                lbl_error_spray.Text = "Debes indicar si viene en formato spray o no";
                lbl_error_spray.Show();
            }
            else lbl_error_spray.Visible = false;

            if (combo_recargable.SelectedItem == null || string.IsNullOrEmpty(combo_recargable.Text))
            {
                errorMsg += "Debes indicar si es o no recargable" + Environment.NewLine;
                lbl_error_recargable.Text = "Debes indicar si es o no recargable";
                lbl_error_recargable.Show();
            }
            else lbl_error_recargable.Visible = false;

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
            else lbl_error_descripcion.Visible = false;

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
                else lbl_error_anio.Visible = false;
            }

            if (string.IsNullOrEmpty(txt_precio.Text))
            {
                errorMsg += "Debes ingresar un precio" + Environment.NewLine;
                lbl_error_precio.Text = "Debes ingresar un precio";
                lbl_error_precio.Show();
            }
            else
            {
                if (!double.TryParse(txt_precio.Text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out double price) || price < 0)
                {
                    errorMsg += "Debes ingresar un precio válido" + Environment.NewLine;
                    lbl_error_precio.Text = "Debes ingresar un precio válido";
                    lbl_error_precio.Show();
                }
                else lbl_error_precio.Visible = false;
            }

            if (combo_activo.SelectedItem == null || string.IsNullOrEmpty(combo_activo.Text))
            {
                errorMsg += "Debes indicar si el producto ingresa como activo o no" + Environment.NewLine;
                lbl_error_activo.Text = "Debes indicar si el producto ingresa como activo o no";
                lbl_error_activo.Show();
            }
            else lbl_error_activo.Visible = false;

            if (pictureBoxProducto1.Image == null)
            {
                errorMsg += "Debes cargar una Imagen" + Environment.NewLine;
                lbl_error_img1.Text = "Debes cargar una Imagen 1";
                lbl_error_img1.Show();
            }
            else lbl_error_img1.Visible = false;

            if (pictureBoxProducto2.Image == null)
            {
                errorMsg += "Debes cargar una Imagen" + Environment.NewLine;
                lbl_error_img2.Text = "Debes cargar una Imagen 2";
                lbl_error_img2.Show();
            }
            else lbl_error_img2.Visible = false;

            if (string.IsNullOrEmpty(errorMsg))
                LblErrorSetVisibleFalse();

            return string.IsNullOrEmpty(errorMsg);
        }

        // ========================= ACTUALIZADO: usa REPLACE para editar =========================
        internal async Task SubirSiCambioImagenAsync()
        {
            try
            {
                // ------------ IMAGEN 1 ------------
                string oldName1 = FileNameFromUrl(perfume.imagen1_URL) ?? (string.IsNullOrWhiteSpace(perfume.imagen1) ? null : perfume.imagen1 + ".jpg");
                string oldStem1 = string.IsNullOrWhiteSpace(perfume.imagen1)
                                    ? NombreSinExtFromUrl(perfume.imagen1_URL)
                                    : perfume.imagen1;                            // ej "212-vip-black-5555-envase"

                // ¿cambió el nombre del perfume? => comparar bases (sin número ni sufijo)
                var oldBase1 = ExtraerBaseSlug(oldStem1, SufijoImg1);            // ej "212-vip-black"
                var newBase = AsignarNombreImagenHelper.Slugify(txt_nombre.Text);
                bool nombreCambio = !string.Equals(oldBase1, newBase, StringComparison.OrdinalIgnoreCase);

                // si cambió el nombre, forzamos número NUEVO
                string newStem1 = BuildNombrePerfumeConNumero(txt_nombre.Text, SufijoImg1, oldStem1,
                                                              forceNewRandom: nombreCambio);
                string newFile1 = newStem1 + ".jpg";
                if (imagen1 != null) // CASO A: subió archivo nuevo
                {
                    var temp1 = GuardarComoJpegTemporal(imagen1, newFile1);
                    try
                    {
                        var r = await ApiImageUploader.ReplaceAsync(
                            localFilePath: temp1,
                            oldNameOnServerOrNull: oldName1,
                            newNameOnServer: newFile1,
                            _: true
                        );
                        urlImagen1Actual = r.url;
                        nombre_foto_uno = newStem1;
                    }
                    finally { try { File.Delete(temp1); } catch { } }
                }
                else // CASO B/C: no subió archivo
                {
                    if (!string.IsNullOrWhiteSpace(oldName1) &&
                        !string.Equals(oldStem1, newStem1, StringComparison.OrdinalIgnoreCase))
                    {
                        // B: cambió nombre: renombrá archivo físico y URL
                        var (returnedName, returnedUrl) = await ApiImageUploader.RenameAsync(oldName1, newFile1);
                        urlImagen1Actual = returnedUrl;
                        nombre_foto_uno = Path.GetFileNameWithoutExtension(returnedName);
                    }
                    else
                    {
                        // C: no cambió nada
                        urlImagen1Actual = perfume.imagen1_URL;
                        nombre_foto_uno = string.IsNullOrWhiteSpace(perfume.imagen1) ? newStem1 : perfume.imagen1;
                    }
                }

                // ------------ IMAGEN 2 ------------
                string oldName2 = FileNameFromUrl(perfume.imagen2_URL) ?? (string.IsNullOrWhiteSpace(perfume.imagen2) ? null : perfume.imagen2 + ".jpg"); ;
                string oldStem2 = string.IsNullOrWhiteSpace(perfume.imagen2)
                                    ? NombreSinExtFromUrl(perfume.imagen2_URL)
                                    : perfume.imagen2;

                var oldBase2 = ExtraerBaseSlug(oldStem2, SufijoImg2);
                bool nombreCambio2 = !string.Equals(oldBase2, newBase, StringComparison.OrdinalIgnoreCase);

                string newStem2 = BuildNombrePerfumeConNumero(txt_nombre.Text, SufijoImg2, oldStem2,
                                                              forceNewRandom: nombreCambio2);
                string newFile2 = newStem2 + ".jpg";

                if (imagen2 != null) // CASO A: subió archivo nuevo
                {
                    var temp2 = GuardarComoJpegTemporal(imagen2, newFile2);
                    try
                    {
                        var r = await ApiImageUploader.ReplaceAsync(
                            localFilePath: temp2,
                            oldNameOnServerOrNull: oldName2,
                            newNameOnServer: newFile2,
                            _: true
                        );
                        urlImagen2Actual = r.url;
                        nombre_foto_dos = newStem2;
                    }
                    finally { try { File.Delete(temp2); } catch { } }
                }
                else // CASO B/C: no subió archivo
                {
                    if (!string.IsNullOrWhiteSpace(oldName2) &&
                        !string.Equals(oldStem2, newStem2, StringComparison.OrdinalIgnoreCase))
                    {
                        // B: cambió nombre: renombrá archivo físico y URL
                        var (returnedName, returnedUrl) = await ApiImageUploader.RenameAsync(oldName2, newFile2);
                        urlImagen2Actual = returnedUrl;
                        nombre_foto_dos = Path.GetFileNameWithoutExtension(returnedName);
                    }
                    else
                    {
                        // C: no cambió nada
                        urlImagen2Actual = perfume.imagen2_URL;
                        nombre_foto_dos = string.IsNullOrWhiteSpace(perfume.imagen2) ? newStem2 : perfume.imagen2;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error subiendo/renombrando imágenes: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        // ========================================================================================

        private int numeroAleatorio()
        {
            return rnd.Next(1000, 9999);
        }

        internal Perfume editar()
        {
            bool spray = (combo_spray.SelectedItem?.ToString() == "Si");
            bool recargable = (combo_recargable.SelectedItem?.ToString() == "Si");
            bool activo = (combo_activo.SelectedItem?.ToString() != "No");

            Marca marca = MarcaControlador.getByName(combo_marca.Text);
            TipoDePerfume tipo_de_perfume = TipoDePerfumeControlador.getByName(combo_tipo_de_perfume.Text);
            Genero genero = GeneroControlador.getByName(combo_genero.Text);
            Pais pais = PaisControlador.getByName(combo_pais.Text);

            int presentacionMl = int.Parse(txt_presentacion.Text);
            int anio = int.Parse(txt_anio_de_lanzamiento.Text);
            double precio = double.Parse(
                txt_precio.Text.Replace(",", "."),
                NumberStyles.Any,
                CultureInfo.InvariantCulture
            );

            DateTime? fechaBaja = perfume.fecha_baja;

            // Si no se cambiaron las imágenes, aseguramos no pisar con null
            var finalUrl1 = string.IsNullOrWhiteSpace(urlImagen1Actual) ? perfume.imagen1_URL : urlImagen1Actual;
            var finalUrl2 = string.IsNullOrWhiteSpace(urlImagen2Actual) ? perfume.imagen2_URL : urlImagen2Actual;

            // También mantenemos nombre_foto_* si no se generó (para compatibilidad)
            if (string.IsNullOrWhiteSpace(nombre_foto_uno))
                nombre_foto_uno = perfume.imagen1;
            if (string.IsNullOrWhiteSpace(nombre_foto_dos))
                nombre_foto_dos = perfume.imagen2;

            return new Perfume(
                perfume.id,
                txt_codigo.Text,
                marca,
                txt_nombre.Text,
                tipo_de_perfume,
                genero,
                presentacionMl,
                pais,
                spray,
                recargable,
                richTextBox_descripcion.Text,
                anio,
                precio,
                activo,
                nombre_foto_uno,
                nombre_foto_dos,
                fechaBaja,
                finalUrl1,
                finalUrl2
            );
        }

        private void btn_siguiente_Click(object sender, EventArgs e)
        {
            bool validacionDatosPerfume = ValidarPerfume();
            if (validacionDatosPerfume)
            {
                Perfume perfume = editar();

                var editarAromaNota = new FormEditarPerfume2(perfume, this, perfumesUC);
                DialogResult dr = editarAromaNota.ShowDialog(this);

                if (dr == DialogResult.OK)
                {
                    Trace.WriteLine("OK");
                }
            }
        }

        private void btn_x_cerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //Diseño del combo box
        private void comboBoxdiseño_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;

            ComboBox combo = sender as ComboBox;
            string text = combo.Items[e.Index].ToString();

            Color backgroundColor;
            Color textColor;

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                backgroundColor = Color.FromArgb(195, 156, 164);
                textColor = Color.White;
            }
            else
            {
                backgroundColor = Color.FromArgb(250, 236, 239);
                textColor = Color.FromArgb(195, 156, 164);
            }

            using (SolidBrush brush = new SolidBrush(backgroundColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            TextRenderer.DrawText(e.Graphics, text, e.Font, e.Bounds, textColor, TextFormatFlags.Left);
            e.DrawFocusRectangle();
        }

        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
        //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX

        // Devuelve fileName (con extensión) desde una URL pública
        private static string FileNameFromUrl(string urlCompleta)
         {
             if (string.IsNullOrWhiteSpace(urlCompleta)) return null;
             try { return Path.GetFileName(new Uri(urlCompleta).AbsolutePath); }
             catch { return Path.GetFileName(urlCompleta); }
         }

         // Devuelve nombre sin extensión desde URL (o null)
         private static string NombreSinExtFromUrl(string urlCompleta)
         {
             var fn = FileNameFromUrl(urlCompleta);
             return string.IsNullOrWhiteSpace(fn) ? null : Path.GetFileNameWithoutExtension(fn);
         }

         // Extrae el NÚMERO aleatorio si el nombre tiene patrón slug-####-sufijo
         // Ej: "212-vip-black-5555-envase" -> 5555
         private static int? TryGetRandomFromNombre(string nombreSinExtension)
         {
             if (string.IsNullOrWhiteSpace(nombreSinExtension)) return null;
             // buscamos "-####-" en el medio
             var match = System.Text.RegularExpressions.Regex.Match(nombreSinExtension, @"-(\d{3,6})-");
             if (match.Success && int.TryParse(match.Groups[1].Value, out int n)) return n;
             return null;
         }

         // Construye el nombre final respetando tu formato y conservando el N° si existe
         // Si nombreAnteriorSinExt trae un número aleatorio, se conserva. Si no, genera uno nuevo.
         /*private string BuildNombrePerfumeConNumero(string nombrePerfumeActual, string sufijo, string nombreAnteriorSinExt)
         {
             var slug = Eterea_Parfums_Desktop.Helpers.AsignarNombreImagenHelper.Slugify(nombrePerfumeActual);
             int num = TryGetRandomFromNombre(nombreAnteriorSinExt) ?? new Random().Next(1000, 9999);
             return $"{slug}-{num}-{sufijo}";
         }*/

         // Guarda la Image a JPG temporal y devuelve la ruta
         private string GuardarComoJpegTemporal(Image imagen, string nombreDeseadoConExtension)
         {
             string tempPath = Path.Combine(Path.GetTempPath(), nombreDeseadoConExtension);
             imagen.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);
             return tempPath;
         }

        // Extrae el slug base (sin número ni sufijo) del stem actual.
        // Ej: "212-vip-black-5555-envase" + "envase" -> "212-vip-black"
        private static string ExtraerBaseSlug(string stem, string sufijo)
        {
            if (string.IsNullOrWhiteSpace(stem)) return null;
            var rx = new System.Text.RegularExpressions.Regex(
                @"^(?<base>.+)-\d{3,6}-" + System.Text.RegularExpressions.Regex.Escape(sufijo) + "$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var m = rx.Match(stem);
            return m.Success ? m.Groups["base"].Value : null;
        }

        // Construye el nombre final respetando formato y permite forzar NUEVO número.
        // - Si forceNewRandom = true => SIEMPRE genera número nuevo.
        // - Si false, intenta conservar el número si el stem anterior lo tenía.
        private string BuildNombrePerfumeConNumero(string nombrePerfumeActual, string sufijo,
                                                   string nombreAnteriorSinExt, bool forceNewRandom)
        {
            var slug = Eterea_Parfums_Desktop.Helpers.AsignarNombreImagenHelper.Slugify(nombrePerfumeActual);

            int num;
            if (!forceNewRandom)
            {
                // Intentar conservar número previo
                var match = System.Text.RegularExpressions.Regex.Match(
                    nombreAnteriorSinExt ?? "", @"-(\d{3,6})-");
                num = (match.Success && int.TryParse(match.Groups[1].Value, out var n)) ? n
                                                                                        : new Random().Next(1000, 9999);
            }
            else
            {
                // Forzar nuevo
                num = new Random().Next(1000, 9999);
            }

            return $"{slug}-{num}-{sufijo}";
        }


    }
}
