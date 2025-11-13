using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2.Core;

namespace Eterea_Parfums_Desktop
{
    public partial class FormVisorPDF : Form
    {
        private readonly string _rutaPdf;
        private WebView2 visor;

        public FormVisorPDF(string rutaPDF)
        {
            InitializeComponent();

            this.Text = "Etiqueta de Envío";
            this.WindowState = FormWindowState.Maximized;

            _rutaPdf = rutaPDF;

            // Crear el control WebView2 y agregarlo al form
            visor = new WebView2
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(visor);

            // Inicializamos WebView2 en el evento Load
            this.Load += FormVisorPDF_Load;
        }

        private async void FormVisorPDF_Load(object sender, EventArgs e)
        {
            try
            {
                // Carpeta de datos de WebView2 en AppData del usuario
                string userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "EtereaParfums",
                    "WebView2Data"
                );

                Directory.CreateDirectory(userDataFolder);

                // Crear entorno de WebView2 usando esa carpeta
                var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);

                // Inicializar WebView2 con ese entorno
                await visor.EnsureCoreWebView2Async(env);

                // Opcional: ajustar algunas opciones
                visor.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                visor.CoreWebView2.Settings.AreDevToolsEnabled = false;

                // Navegar al PDF
                string ruta = _rutaPdf;

                // Asegurar que usamos un URI de archivo correcto (file:///C:/...)
                if (!ruta.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                {
                    ruta = "file:///" + ruta.Replace("\\", "/");
                }

                visor.CoreWebView2.Navigate(ruta);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo inicializar el visor de PDF (WebView2).\n\nDetalle: " + ex.Message,
                    "Error en visor PDF",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }
}

