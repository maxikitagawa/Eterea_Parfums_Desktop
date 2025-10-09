using Eterea_Parfums_Desktop.Controladores;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net;



namespace Eterea_Parfums_Desktop
{
    static class Program
    {

        public static BarcodeReceiver BarcodeService = new BarcodeReceiver();

        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        //public static int debug_mode = 1;

        public static Empleado logueado;

        public static int sucursal = 1;

        public static string NumeroCajaActual = "Caja sin asignar";
        public static int IdHistorialCajaActual = 0;


        //public static String Ruta_Base_Mini;
        public static String Ruta_Base;
        public static String Ruta_Web;
        public static String entorno = "adri";

        public static float ScaleFactor = 1.0f; // Valor por defecto



        [STAThread]
        static void Main()
        {
            /* Codigo usado una vez para generar el password
             del primer usuario administrador

            // Define la contraseña original (por ejemplo, "123456")
            string claveOriginal = "admin";

            // Genera el hash utilizando PasswordHelper
            string claveHasheada = PasswordHelper.CrearHash(claveOriginal);

            // Muestra el hash generado en la consola
            Console.WriteLine("Clave original: " + claveOriginal);
            Console.WriteLine("Clave hasheada: " + claveHasheada);
            */

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Mostrar el formulario de selección de usuario
            FormSeleccionarUsuario formSeleccionarUsuario = new FormSeleccionarUsuario();
            if (formSeleccionarUsuario.ShowDialog() == DialogResult.OK)
            {
                string usuarioSeleccionado = formSeleccionarUsuario.UsuarioSeleccionado;

                // Configurar la conexión a la base de datos con el usuario seleccionado
                DB_Controller.ConfigurarConexion(usuarioSeleccionado);

                // Configurar rutas según el usuario
                ConfigurarRutas(usuarioSeleccionado);

                // Llamamos a ActualizarEstadoPromociones al inicio del programa
                PromocionService.ActualizarEstadoPromociones();

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                // (Opcional) Ver si necesitamos usar proxies/self-signed:
                // ServicePointManager.ServerCertificateValidationCallback += (s, cert, chain, sslPolicyErrors) => true;


                // Iniciar la aplicación principal

                Application.Run(new FormStart());
                //Application.Run(new FormInicioAdministrador());
                //Application.Run(new MenuABM());
                //Application.Run(new FormInicioAutoconsulta());
            }
            else
            {
                // Si el usuario no selecciona nada y cierra el formulario, salir de la aplicación
                MessageBox.Show("Debe seleccionar un usuario para continuar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

        }
           

        // Método para configurar rutas según el usuario
        private static void ConfigurarRutas(string usuario)
        {
            switch (usuario.ToLower())
            {
                case "servidor":
                    Ruta_Web = "https://etereaparfums.com.ar/imagenes/";
                    Ruta_Base = @"C:\Users\intersan\Desktop\TESIS_New\Eterea_Parfums_Desktop\Eterea_Parfums_Desktop\Resources\";
                    //Ruta_Base_Mini = @"C:\Users\intersan\Desktop\TESIS_New\Eterea_Parfums_Desktop\Eterea_Parfums_Desktop\Resources\Mini\";
                    //Ruta_Web = @"C:\Users\intersan\source\repos\Eterea_Web\Eterea_Web\Content\ImgPerfumes\";
                    break;
                case "escuela":
                    Ruta_Base = @"C:\Users\Alumno\Desktop\Eterea_Parfums\Eterea_Parfums\Resources\";
                    Ruta_Web = @"C:\Users\Alumno\Desktop\Eterea_Web\Eterea_Web\Content\ImgPerfumes\";
                    break;
                case "dami":
                    Ruta_Base = @"C:\Users\damim\source\repos\Eterea_Parfums_Desktop\Eterea_Parfums_Desktop\Resources\";
                    Ruta_Web = @"C:\Users\damim\Source\Repos\Eterea_Parfums_Desktop\Eterea_Parfums_Desktop\Resources\";
                    break;
                case "adri":
                    Ruta_Web = "https://etereaparfums.com.ar/imagenes/";
                    Ruta_Base = @"C:\Users\intersan\Desktop\TESIS_New\Eterea_Parfums_Desktop\Eterea_Parfums_Desktop\Resources\";
                    //Ruta_Base_Mini = @"C:\Users\intersan\Desktop\TESIS_New\Eterea_Parfums_Desktop\Eterea_Parfums_Desktop\Resources\Mini\";
                    //Ruta_Web = @"C:\Users\intersan\source\repos\Eterea_Web\Eterea_Web\Content\ImgPerfumes\";
                    break;
                case "luis":
                    Ruta_Base = @"C:\Users\josel\source\repos\Eterea_Parfums_Desktop\Eterea_Parfums_Desktop\Resources\";
                    Ruta_Web = @"C:\Users\intersan\source\repos\Eterea_Web\Eterea_Web\Content\ImgPerfumes\";
                    break;
                case "maxi":
                        Ruta_Base = @"C:\Users\Maxi\source\repos\Eterea_Parfums_Desktop\Eterea_Parfums_Desktop\Resources\";
                        Ruta_Web = @"C:\Users\usuario\source\repos\EEterea_Web\Eterea_Web\Content\ImgPerfumes\";
                        break;
                case "notebook adri":
                    Ruta_Base = @"C:\Users\PC\source\repos\Eterea_Parfums_Desktop\Eterea_Parfums_Desktop\Resources\";
                    Ruta_Web = @"C:\Users\intersan\source\repos\Eterea_Web\Eterea_Web\Content\ImgPerfumes\";
                    break;
                default:
                        MessageBox.Show("Usuario no válido, no se configuraron rutas.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
            }
        }

        public static class Net
        {
            public static readonly HttpClient Http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }



    }




}
