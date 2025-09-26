using System;
using System.Configuration.Provider;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Eterea_Parfums_Desktop.Controladores
{
    public static class DB_Controller
    {
        //public static string connectionString = "";
        private static string connectionString;
        public static SqlConnection connection;



        // Método para configurar la conexión basada en el usuario
        public static void ConfigurarConexion(string usuario)
        {
            usuario = usuario.ToLower();
            // Definir la cadena de conexión según el usuario seleccionado


            switch (usuario)
            {
                case "servidor":
                    connectionString = "Data Source=SQL8010.site4now.net;Initial Catalog=db_abe44c_eterea;User ID=db_abe44c_eterea_admin;Password=Davinci-1999";
                    break;
                case "adri":
                    //connectionString = "Data Source=(localdb)\\Local;Initial Catalog=eterea;Integrated Security=True;";
                    connectionString = "Data Source=DESKTOP-12IG1S9\\MSSQLSERVER2025;Initial Catalog=eterea;User ID=sa;Password=Melona88";
                    //connectionString = "Data Source=DESKTOP-12IG1S9\\MSSQLSERVER2025;Initial Catalog=eterea;Integrated Security=True;";
                    break;
                case "dami":
                    connectionString = "Data Source=LocalHost;Initial Catalog=eterea;Integrated Security=True;";
                    break;
                case "luis":
                    connectionString = "Data Source=JLUIS\\MSSQLSERVER2025;Initial Catalog=eterea;User ID=sa;Password=12345;";
                    break;
                case "maxi":
                    connectionString = "Data Source=DESKTOP-N6TI9JV\\MSSQLSERVER2025;Initial Catalog=eterea;User ID=sa;Password=12345;";
                    break;
                case "marino":
                    connectionString = "Data Source=(localdb)\\Local;Initial Catalog=eterea;Integrated Security=True;";
                    break;
                case "notebook adri":
                    connectionString = "Data Source=DESKTOP-U4RUEJ1\\SQLEXPRESS;Initial Catalog=eterea;Integrated Security=True;";
                    break;
                default:
                    throw new Exception("Usuario no válido.");
            }

            connection = new SqlConnection(connectionString);

            // Mostrar los datos de conexión en la consola
            Trace.WriteLine("=================================");
            Trace.WriteLine($"Usuario seleccionado: {usuario}");
            Trace.WriteLine($"Cadena de conexión: {connectionString}");
            Trace.WriteLine("=================================");

        }


        public static string GetConnectionString()
        {
            return connectionString; // Devolver la cadena de conexión actual
        }



    }
}