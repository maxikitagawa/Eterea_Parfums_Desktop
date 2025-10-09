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
                // Si faltan URLs o nombres, re-lee el perfume por ID (por las dudas)
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

                // Base pública desde app.config (PublicImagesBaseUrl + PublicImagesFolder)
                var basePublica = GetPublicImagesBase(); //

                // --- URL 1 ---
                // Si la BD ya tiene URL completa, la usamos pero "arreglada" (espacios, backslashes)
                // Si no, la armamos con basePublica + nombre_foto_uno + ".jpg"
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

                // Opcional para depurar
                // Debug.WriteLine($"URL1-> {url1}");
                // Debug.WriteLine($"URL2-> {url2}");
            };


            //Diseño del combo box
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

            //Limito la cantidad de digitos que se pueden ingresar en el txt_codigo
            txt_codigo.MaxLength = 13;
        }


        private async void cargarDatos(Perfume perfume)
        {
            txt_codigo.Text = perfume.codigo;
            combo_marca.Text = perfume.marca.nombre;
            Console.WriteLine(perfume.marca.nombre);
            txt_nombre.Text = perfume.nombre;
            combo_tipo_de_perfume.Text = perfume.tipo_de_perfume.tipo_de_perfume;
            combo_genero.SelectedItem = perfume.genero.genero;
            txt_presentacion.Text = perfume.presentacion_ml.ToString();
            combo_pais.Text = perfume.pais.nombre;
            combo_spray.Text = perfume.spray.ToString();
            if (perfume.spray == true)
            {
                combo_spray.SelectedItem = "Si";
            }
            else
            {
                combo_spray.SelectedItem = "No";
            }
            combo_recargable.Text = perfume.recargable.ToString();
            if (perfume.recargable == true)
            {
                combo_recargable.SelectedItem = "Si";
            }
            else
            {
                combo_recargable.SelectedItem = "No";
            }
            richTextBox_descripcion.Text = perfume.descripcion;
            txt_anio_de_lanzamiento.Text = perfume.anio_de_lanzamiento.ToString();
            txt_precio.Text = perfume.precio_en_pesos.ToString();

            if (perfume.activo.HasValue)
            {
                combo_activo.SelectedItem = perfume.activo.Value ? "Si" : "No";
            }
            else
            {
                combo_activo.SelectedItem = "No especificado"; // O dejalo vacío si preferís
            }


            nombre_foto_uno = perfume.imagen1;
            nombre_foto_dos = perfume.imagen2;

            urlImagen1Actual = perfume.imagen1_URL?.Trim();
            urlImagen2Actual = perfume.imagen2_URL?.Trim();

            // LOG para verificar que llegan las URLs
            Debug.WriteLine("URL1: " + urlImagen1Actual);
            Debug.WriteLine("URL2: " + urlImagen2Actual);

          
            //cargarImagen(nombre_foto_uno, pictureBoxProducto1);
            //cargarImagen(nombre_foto_dos, pictureBoxProducto2);

            //Console.WriteLine(nombre_foto_dos);

        }

        private static string GetPublicImagesBase()
        {
            // Si no están, uso ApiBaseUrl + "/imagenes" por defecto
            var baseUrl = (ConfigurationManager.AppSettings["PublicImagesBaseUrl"]
                           ?? ConfigurationManager.AppSettings["ApiBaseUrl"])
                          ?.TrimEnd('/');
            var folder = ConfigurationManager.AppSettings["PublicImagesFolder"] ?? "/imagenes";
            var folderClean = folder.Trim().Trim('/');

            return string.IsNullOrWhiteSpace(baseUrl) ? null : $"{baseUrl}/{folderClean}";
        }


       /* private string PrepararUrl(string urlCruda)
        {
            if (string.IsNullOrWhiteSpace(urlCruda)) return null;

            // limpiar espacios / comillas accidentales
            var u = urlCruda.Trim().Trim('"', '\'');

            // si te guardaron la ruta relativa (ej: "/imagenes/archivo.jpg")
            if (!u.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !u.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                // base pública configurable (App.config)
                // <add key="PublicImagesBaseUrl" value="https://etereaparfums.com.ar" />
                var baseUrl = System.Configuration.ConfigurationManager.AppSettings["PublicImagesBaseUrl"]?.TrimEnd('/');
                if (!string.IsNullOrWhiteSpace(baseUrl))
                    u = $"{baseUrl}/{u.TrimStart('/')}";
            }

            // normalizá segmentos (espacios/acentos/ñ)
            return NormalizarUrl(u);
        }*/



        /*private void EnsureUrls()
        {
            // si vino todo desde BD, no hago nada
            if (!string.IsNullOrWhiteSpace(perfume.imagen1_URL) || !string.IsNullOrWhiteSpace(perfume.imagen2_URL))
                return;

            // recargo desde BD por si el objeto vino incompleto (MUY común)
            var p = PerfumeControlador.getByID(perfume.id);
            if (p != null)
            {
                perfume.imagen1_URL = p.imagen1_URL?.Trim();
                perfume.imagen2_URL = p.imagen2_URL?.Trim();
                if (string.IsNullOrWhiteSpace(perfume.imagen1)) perfume.imagen1 = p.imagen1;
                if (string.IsNullOrWhiteSpace(perfume.imagen2)) perfume.imagen2 = p.imagen2;
            }
        }*/


        /*private async Task CargarImagenDesdeUrlOLocalAsync(string url, string nombreLocalSinExt, PictureBox pictureBox)
        {
            if (pictureBox.Image != null)
            {
                var old = pictureBox.Image;
                pictureBox.Image = null;
                old.Dispose();
            }

            string urlNormalizada = NormalizarUrl(url);

            bool intentoUrl = false;
            if (!string.IsNullOrWhiteSpace(urlNormalizada))
            {
                intentoUrl = true;
                try
                {
                    var img = await ApiImageUploader.DownloadImageAsync(urlNormalizada);
                    if (img != null && !ReferenceEquals(img, Properties.Resources.sinImagen))
                    {
                        pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                        pictureBox.Image = img;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Fallo descarga URL: " + ex.Message);
                }
            }

            // Fallback local
            string rutaCompleta = Path.Combine(Program.Ruta_Base, (nombreLocalSinExt ?? "") + ".jpg");
            if (File.Exists(rutaCompleta))
            {
                using (var fs = new FileStream(rutaCompleta, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox.Image = Image.FromStream(fs);
                }
            }
            else
            {
                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Image = Properties.Resources.sinImagen;

                if (intentoUrl)
                {
                    // 👉 te avisa una sola vez por imagen que la URL falló y no existe local
                    Debug.WriteLine($"No se pudo cargar la URL ni el archivo local. URL normalizada: {urlNormalizada}");
                    // (Opcional) MessageBox.Show(...) si querés verlo en pantalla
                }
            }
        }*/


        // Pequeño helper para evitar 404 por espacios/acentos/ñ

        /*private string NormalizarUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            // Intento parsear tal cual
            if (Uri.TryCreate(url, UriKind.Absolute, out var uriOk))
                return uriOk.AbsoluteUri;

            // Si falla, intento “arreglar” el path codificando cada segmento
            try
            {
                // Ej: https://etereaparfums.com.ar/imagenes/Paco Rabanne - 1234 - envase y caja.jpg
                var idx = url.IndexOf("://", StringComparison.Ordinal);
                if (idx < 0) return null;

                var scheme = url.Substring(0, idx);
                var rest = url.Substring(idx + 3);             // host + path
                var firstSlash = rest.IndexOf('/');
                var host = firstSlash >= 0 ? rest.Substring(0, firstSlash) : rest;
                var path = firstSlash >= 0 ? rest.Substring(firstSlash) : "/";

                // codifico cada segmento del path
                var encodedSegments = string.Join("/",
                    path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(seg => Uri.EscapeDataString(seg))
                );
                var normalized = $"{scheme}://{host}/{encodedSegments}";

                return normalized;
            }
            catch { return null; }
        }*/


        internal async Task SubirSiCambioImagenAsync()
        {
            // IMAGEN 1: si el usuario cargó una nueva en el form de editar
            if (imagen1 != null)
            {
                nombre_foto_uno = AsignarNombreImagenHelper.BuildNombreImagen(txt_nombre.Text, "envase");
                string desiredFileName1 = nombre_foto_uno + ".jpg";

                string temp1 = GuardarComoJpegTemporal(imagen1, desiredFileName1);
                try
                {
                    var r1 = await ApiImageUploader.UploadAsync(temp1, desiredFileName1);
                    urlImagen1Actual = r1.url; // <- guardás la URL nueva para persistir
                }
                finally { try { System.IO.File.Delete(temp1); } catch { } }
            }

            // IMAGEN 2
            if (imagen2 != null)
            {
                nombre_foto_dos = AsignarNombreImagenHelper.BuildNombreImagen(txt_nombre.Text, "envase y caja");
                string desiredFileName2 = nombre_foto_dos + ".jpg";

                string temp2 = GuardarComoJpegTemporal(imagen2, desiredFileName2);
                try
                {
                    var r2 = await ApiImageUploader.UploadAsync(temp2, desiredFileName2);
                    urlImagen2Actual = r2.url;
                }
                finally { try { System.IO.File.Delete(temp2); } catch { } }
            }
        }


        /*private void cargarImagen(string nombreImg, PictureBox pictureBox)
        {
            string rutaCompletaImagen = Program.Ruta_Base + nombreImg + ".jpg";
            if (System.IO.File.Exists(rutaCompletaImagen))
            {
                pictureBox.Image = Image.FromFile(rutaCompletaImagen);
            }
            else
            {
                MessageBox.Show("La imagen no se encontró en la ruta especificada: " + rutaCompletaImagen, "Error de carga de imagen", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }*/

        /*private bool Eliminar_Imagen_Existente(string nombreImg)
        {
            String rutaImagen = Program.Ruta_Base + nombreImg + ".jpg";
            try
            {
                if (System.IO.File.Exists(rutaImagen) && nombreImg != "imagen1.jpg" && nombreImg != "imagen2.jpg")
                {
                    // Intentar liberar el archivo si está en uso
                    LiberarImagen(rutaImagen);
                    // Esperar a que el sistema libere el archivo
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    System.IO.File.Delete(rutaImagen);
                    Console.WriteLine("Imagen eliminada correctamente.");
                    return true;
                }
                else
                {
                    Console.WriteLine("La imagen no existe en la ruta especificada o no tiene permisos para eliminarlo.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error al eliminar la imagen: " + ex.Message);
            }
            return false;
        }*/

        /*private void LiberarImagen(string rutaImagen)
        {
            try
            {
                using (Image img = Image.FromFile(rutaImagen))
                {
                    img.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("No se pudo liberar la imagen: " + ex.Message);
            }
        }*/


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

        private void btn_cargar_img1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JPG(*.JPG)|*.JPG|PNG(*.png)|*.png";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                imagen1 = Image.FromFile(ofd.FileName);
                pictureBoxProducto1.Image = imagen1;
                pathLocalImg1 = ofd.FileName; // ✅ se guarda la ruta

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
                pathLocalImg2 = ofd.FileName; // ✅ se guarda la ruta

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
            


            if (combo_genero.SelectedItem == null || string.IsNullOrEmpty(combo_marca.Text))
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

            if (combo_activo.SelectedItem == null || string.IsNullOrEmpty(combo_activo.Text))
            {
                errorMsg += "Debes indicar si el producto ingresa como activo o no" + Environment.NewLine;
                lbl_error_activo.Text = "Debes indicar si el producto ingresa como activo o no";
                lbl_error_activo.Show();
            }
            else
            {
                lbl_error_activo.Visible = false;
            }

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


        /*internal void eliminarImgExistenteYGuardarNueva()
        {
            if (imagen1 != null)
            {
                Eliminar_Imagen_Existente(nombre_foto_uno);
                saveImagenResources(out nombre_foto_uno, imagen1, "envase");
            }

            if (imagen2 != null)
            {
                Eliminar_Imagen_Existente(nombre_foto_dos);
                saveImagenResources(out nombre_foto_dos, imagen2, "envase y caja");
            }
        }*/
        /*internal async Task SubirImagenesEditadasAsync()
        {
            // si el usuario no cambió la imagen, no tocamos nada
            if (imagen1 != null)
            {
                buildNombreImagen(out nombre_foto_uno, "envase");
                string desired1 = nombre_foto_uno + ".jpg";
                string temp1 = GuardarComoJpegTemporal(imagen1, desired1);
                try
                {
                    var res1 = await ApiImageUploader.UploadAsync(temp1, desired1);
                    urlImagen1Actual = res1.url; // guardo para persistir
                }
                finally { try { File.Delete(temp1); } catch { } }
            }

            if (imagen2 != null)
            {
                buildNombreImagen(out nombre_foto_dos, "envase y caja");
                string desired2 = nombre_foto_dos + ".jpg";
                string temp2 = GuardarComoJpegTemporal(imagen2, desired2);
                try
                {
                    var res2 = await ApiImageUploader.UploadAsync(temp2, desired2);
                    urlImagen2Actual = res2.url;
                }
                finally { try { File.Delete(temp2); } catch { } }
            }

            // si el usuario no eligió imágenes nuevas, conservo lo que trajo BD
            if (string.IsNullOrWhiteSpace(urlImagen1Actual))
                urlImagen1Actual = perfume.imagen1_URL;
            if (string.IsNullOrWhiteSpace(urlImagen2Actual))
                urlImagen2Actual = perfume.imagen2_URL;
        }*/



        private void buildNombreImagen(out string nombreArchivoSinExtension, string sufijo)
        {
            int numero_aleatorio = numeroAleatorio();
            string baseNombre = (txt_nombre.Text ?? "").Trim();

            // saneo básico
            string inval = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (var c in inval) baseNombre = baseNombre.Replace(c.ToString(), "");

            nombreArchivoSinExtension = $"{baseNombre} - {numero_aleatorio} - {sufijo}";
        }

        // Guarda la Image en un .jpg temporal y devuelve la ruta
        private string GuardarComoJpegTemporal(Image imagen, string nombreDeseadoConExtension)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), nombreDeseadoConExtension);
            imagen.Save(tempPath, System.Drawing.Imaging.ImageFormat.Jpeg);
            return tempPath;
        }


        private void btn_siguiente_Click(object sender, EventArgs e)
        {
            //Validar datos del perfume
            bool validacionDatosPerfume = ValidarPerfume();
            if (validacionDatosPerfume)
            {
                Perfume perfume = editar();

                // Obtener la instancia del FormStart..
                //Form formStart = Application.OpenForms["FormStart"];
                FormEditarPerfume2 editarAromaNota = new FormEditarPerfume2(perfume, this, perfumesUC);

                DialogResult dr = editarAromaNota.ShowDialog(this);

                if (dr == DialogResult.OK)
                {
                    Trace.WriteLine("OK");

                }
                // Retrasamos la llamada a Hide() para evitar el salto
                //this.BeginInvoke(new Action(() => this.Hide()));

                // Crear el formulario a mostrar y pasarle, como owner, el formStart
                //editarAromaNota.ShowDialog(formStart);

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

            // 👇 MUY IMPORTANTE: conservar la fecha_baja ya existente (puede ser null)
            DateTime? fechaBaja = perfume.fecha_baja; // o perfume.FechaBaja según tu propiedad

            return new Perfume(
                perfume.id,                 // id
                txt_codigo.Text,            // codigo
                marca,                      // marca
                txt_nombre.Text,            // nombre
                tipo_de_perfume,            // tipo_de_perfume
                genero,                     // genero
                presentacionMl,             // presentacion_ml
                pais,                       // pais
                spray,                      // spray
                recargable,                 // recargable
                richTextBox_descripcion.Text, // descripcion
                anio,                       // anio_de_lanzamiento
                precio,                     // precio_en_pesos
                activo,                     // activo
                nombre_foto_uno,            // imagen1 (legacy local)
                nombre_foto_dos,            // imagen2 (legacy local)
                fechaBaja,                  // DateTime? fecha_baja
                urlImagen1Actual,           // imagen1_URL
                urlImagen2Actual            // imagen2_URL
            );
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
