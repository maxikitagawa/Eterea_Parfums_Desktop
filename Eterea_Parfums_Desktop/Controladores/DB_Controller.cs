using Newtonsoft.Json;
using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;



namespace Eterea_Parfums_Desktop.Controladores
{
    public static class DB_Controller
    {
        private static string connectionString;
        public static SqlConnection connection;

        // Llamar una sola vez al arrancar la app (Program.Main o FormStart_Load)
        public static void InitializeFromConfig(AppConfig.ConfigModel cfg)
        {
            connectionString = BuildConnectionStringFrom(cfg);
            connection = new SqlConnection(connectionString);
        }

        // Mantengo tu API: permite forzar la conexión por “usuario”
        // "servidor" => server remoto | "adri" o "local" => SQL local
        public static void ConfigurarConexion(string usuario)
        {
            usuario = (usuario ?? "").Trim().ToLower();

            switch (usuario)
            {
                case "servidor":
                    // === SERVIDOR === (recomendado: sacar credenciales del config.json)
                    // Si preferís seguir usando este perfil fijo, dejalo:
                    connectionString =
                        "Data Source=SQL8010.site4now.net;" +
                        "Initial Catalog=db_abe44c_eterea;" +
                        "User ID=db_abe44c_eterea_admin;" +
                        "Password=Davinci-1999;" +
                        "Encrypt=True;TrustServerCertificate=True;";
                    break;

                case "adri":
                case "local":
                    // === LOCAL ===
                    connectionString =
                        "Data Source=DESKTOP-12IG1S9\\MSSQLSERVER2025;" +
                        "Initial Catalog=eterea;" +
                        "Integrated Security=True;" +
                        "Encrypt=True;TrustServerCertificate=True;";
                    break;

                default:
                    // Si no coincide, caemos a config.json
                    InitializeFromConfig(Eterea_Parfums_Desktop.AppConfigLoader.Load());
                    return;// ya configuró y logueó
            }

            connection = new SqlConnection(connectionString);

            Trace.WriteLine("=================================");
            Trace.WriteLine($"Usuario seleccionado: {usuario}");
            Trace.WriteLine($"Cadena de conexión: {connectionString}");
            Trace.WriteLine("=================================");
        }

        public static string GetConnectionString() => connectionString;

        // Abre y devuelve una conexión abierta (útil para using(var cn = DB_Controller.GetOpenConnection()) {...})
        public static SqlConnection GetOpenConnection()
        {
            if (connection == null)
                InitializeFromConfig(Eterea_Parfums_Desktop.AppConfigLoader.Load());

            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            return connection;
        }

        // Test rápido para mostrar un mensaje claro al usuario si falla
        public static bool ProbarConexion(out string error)
        {
            error = null;
            try
            {
                using (var cn = new SqlConnection(connectionString ?? ""))
                {
                    cn.Open();
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        // Construye el connection string según el JSON
        private static string BuildConnectionStringFrom(AppConfig.ConfigModel cfg)
        {
            // Defaults seguros por si algo falta
            var db = cfg?.Db ?? new AppConfig.DbSettings
            {
                Mode = "Local",
                DataSource = @".\SQLEXPRESS",
                Database = "eterea",
                IntegratedSecurity = true
            };

            const string common = "Encrypt=True;TrustServerCertificate=True;";

            // Servidor (remoto o LAN)
            if (string.Equals(db.Mode, "Server", StringComparison.OrdinalIgnoreCase))
            {
                if (db.IntegratedSecurity)
                    return $"Server={db.DataSource};Database={db.Database};Integrated Security=True;{common}";
                else
                    return $"Server={db.DataSource};Database={db.Database};User Id={db.UserId};Password={db.Password};{common}";
            }

            // LocalDB con .mdf adjunto
            if (db.UseLocalDb)
            {
                var attach = string.IsNullOrWhiteSpace(db.AttachDbFile)
                    ? ""
                    : $";AttachDbFilename={db.AttachDbFile}";

                return $"Data Source=(LocalDB)\\MSSQLLocalDB;Integrated Security=True{attach};Connect Timeout=30;{common}";
            }

            // SQL Express local (instancia)
            // Si IntegratedSecurity=false, también respeta usuario/clave
            var integrated = db.IntegratedSecurity ? "True" : "False";
            var userBlock = db.IntegratedSecurity ? "" : $"User Id={db.UserId};Password={db.Password};";
            return $"Server={db.DataSource};Database={db.Database};Integrated Security={integrated};{userBlock}{common}";
        }
    }

    // ====== Config JSON minimal ======
    public static class AppConfig
    {
        private const string Company = "EtereaParfums";
        private const string Product = "EtereaDesktop";

        private static readonly string ProgramDataDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Company, Product);

        public static ConfigModel Load()
        {
            try
            {
                Directory.CreateDirectory(ProgramDataDir);
                var cfgPath = Path.Combine(ProgramDataDir, "config.json");
                if (File.Exists(cfgPath))
                {
                    var json = File.ReadAllText(cfgPath);
                    var cfg = JsonConvert.DeserializeObject<ConfigModel>(json);
                    if (cfg != null) return cfg;
                }
            }
            catch { /* ignore y usa defaults */ }

            // Defaults si no hay archivo
            return new ConfigModel
            {
                Sucursal = 1,
                RutaBase = ProgramDataDir,
                Db = new DbSettings
                {
                    Mode = "Local",
                    DataSource = @".\SQLEXPRESS",
                    Database = "eterea",
                    IntegratedSecurity = true
                }
            };
        }

        public class ConfigModel
        {
            public int Sucursal { get; set; }
            public string RutaBase { get; set; }
            public string RutaWeb { get; set; }
            public DbSettings Db { get; set; } = new DbSettings();
        }

        public class DbSettings
        {
            public string Mode { get; set; }                // "Local" | "Server"
            public string DataSource { get; set; }          // instancia o servidor
            public string Database { get; set; }            // nombre de BD
            public bool IntegratedSecurity { get; set; }    // Windows Auth
            public string UserId { get; set; }              // SQL Auth
            public string Password { get; set; }            // SQL Auth
            public bool UseLocalDb { get; set; }            // (LocalDB)\MSSQLLocalDB
            public string AttachDbFile { get; set; }        // ruta .mdf si LocalDB
        }
    }
}
