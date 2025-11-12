using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Modelos;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop
{
    static class Program
    {
        public static BarcodeReceiver BarcodeService = new BarcodeReceiver();

        // Estado global que usás en otras partes
        public static Empleado logueado;
        public static int sucursal = 1;

        public static string NumeroCajaActual = "Caja sin asignar";
        public static int IdHistorialCajaActual = 0;

        public static string Ruta_Base;
        public static string Ruta_Web;     // opcional (imagenes CDN / sitio)
        public static float ScaleFactor = 1.0f;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1) Cargar configuración desde ProgramData (o defaults si no existe)
            var cfg = AppConfigLoader.Load();

            // 2) Variables globales
            sucursal = cfg.Sucursal > 0 ? cfg.Sucursal : 1;
            Ruta_Base = string.IsNullOrWhiteSpace(cfg.RutaBase) ? AppConfigLoader.DefaultProgramDataPath : cfg.RutaBase;
            Ruta_Web = string.IsNullOrWhiteSpace(cfg.RutaWeb) ? "https://etereaparfums.com.ar/imagenes" : cfg.RutaWeb;

            // 3) Asegurar carpeta de datos
            EnsureDir(Ruta_Base);

            // 4) Configurar conexión BD desde config.json
            // (usa la versión que hicimos: lee cfg.Db y arma el connection string)
            DB_Controller.InitializeFromConfig(cfg);

            // 5) TLS
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // 6) Actualizar promos al inicio
            PromocionService.ActualizarEstadoPromociones();

            // 7) (Opcional) test de conexión
            if (!DB_Controller.ProbarConexion(out var err))
            {
                MessageBox.Show(
                    "No se pudo conectar a la base de datos.\n\n" + err +
                    "\n\nRevisá C:\\ProgramData\\EtereaParfums\\EtereaDesktop\\config.json",
                    "Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Podés abrir un Form de Configuración aquí si querés.
            }

            // 8) Iniciar la aplicación
            Application.Run(new FormStart());
        }

        private static void EnsureDir(string path)
        {
            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo crear la carpeta de datos:\n{path}\n\n{ex.Message}",
                                "Error de carpeta", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // HttpClient compartido
        public static class Net
        {
            public static readonly HttpClient Http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }
    }

    /// <summary>
    /// Loader mínimo para ProgramData\config.json con defaults seguros.
    /// Si mañana agregás más campos, extendé esta clase (mantiene compatibilidad).
    /// </summary>
    internal static class AppConfigLoader
    {
        private const string Company = "EtereaParfums";
        private const string Product = "EtereaDesktop";

        public static string DefaultProgramDataPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                         Company, Product);

        private static string ConfigPath => Path.Combine(DefaultProgramDataPath, "config.json");

        public static AppConfig.ConfigModel Load()
        {
            try
            {
                Directory.CreateDirectory(DefaultProgramDataPath);
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);

                    // ✅ Usamos Newtonsoft.Json y el tipo correcto
                    var cfg = JsonConvert.DeserializeObject<AppConfig.ConfigModel>(json);

                    if (cfg != null)
                    {
                        if (string.IsNullOrWhiteSpace(cfg.RutaBase))
                            cfg.RutaBase = DefaultProgramDataPath;
                        if (cfg.Sucursal <= 0)
                            cfg.Sucursal = 1;
                        if (cfg.Db == null)
                            cfg.Db = new AppConfig.DbSettings();

                        return cfg; // ✅ ahora coincide con AppConfig.ConfigModel
                    }
                }
            }
            catch
            {
                // Si falla, devolvemos defaults seguros
            }

            // ✅ Configuración por defecto
            return new AppConfig.ConfigModel
            {
                Sucursal = 1,
                RutaBase = DefaultProgramDataPath,
                RutaWeb = "https://etereaparfums.com.ar/imagenes",
                Db = new AppConfig.DbSettings
                {
                    
                    Mode = "Server",
                    DataSource = @"DESKTOP-12IG1S9\MSSQLSERVER2025",
                    Database = "eterea",
                    IntegratedSecurity = true,
                    UserId = "",
                    Password = "",
                    UseLocalDb = false,
                    AttachDbFile = ""

                }
            };
        }

        // === modelos del JSON ===
        public class AppConfigModel
        {
            public int Sucursal { get; set; }
            public string RutaBase { get; set; }
            public string RutaWeb { get; set; }     // opcional
            public DbSettings Db { get; set; } = new DbSettings();
        }

        public class DbSettings
        {
            public string Mode { get; set; }                // "Local" | "Server"
            public string DataSource { get; set; }
            public string Database { get; set; }
            public bool IntegratedSecurity { get; set; }
            public string UserId { get; set; }
            public string Password { get; set; }
            public bool UseLocalDb { get; set; }
            public string AttachDbFile { get; set; }
        }
    }
}
