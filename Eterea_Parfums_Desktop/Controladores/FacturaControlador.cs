using Eterea_Parfums_Desktop.Modelos;
using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop.Controladores
{
    internal class FacturaControlador
    {
        //Este metodo es el viejo, calcula el max id, no el maximo num_factura
        public static int ObtenerProximoIdFactura()
        {
            int proximoId = 0;

            string query = "SELECT MAX(id) FROM dbo.Factura;";



            // 🔹 Usamos "using" para que la conexión se cierre automáticamente
            using (SqlConnection conexion = new SqlConnection(DB_Controller.GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, conexion))
                {
                    conexion.Open();
                    var result = cmd.ExecuteScalar();
                    conexion.Close();

                    if (result != null && result != DBNull.Value)
                    {
                        proximoId = Convert.ToInt32(result) + 1;
                    }
                    else
                    {
                        proximoId = 1;
                    }
                }
            }

            return proximoId;
        }

        public static int ObtenerMaxIdFactura()
        {
            int proximoId = 0;

            string query = "SELECT MAX(id) FROM dbo.Factura;";

            using (SqlConnection conexion = new SqlConnection(DB_Controller.GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, conexion))
                {
                    conexion.Open();
                    var result = cmd.ExecuteScalar();
                    conexion.Close();

                    if (result != null && result != DBNull.Value)
                    {
                        proximoId = Convert.ToInt32(result);
                    }
                    else
                    {
                        proximoId = 1;
                    }
                }
            }

            return proximoId;
        }


        public static string ObtenerProximoNumFactura(string tipoDeFactura, string puntoDeVenta)
        {
            string proximoNumFactura = "";
            int nuevoNumero = 1;

            string query = @"
        SELECT MAX(num_factura)
        FROM dbo.Factura
        WHERE tipo_de_factura = @tipo_de_factura
          AND LEFT(num_factura, 4) = @punto_de_venta;
    ";

            using (SqlConnection conexion = new SqlConnection(DB_Controller.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conexion))
            {
                cmd.Parameters.AddWithValue("@tipo_de_factura", tipoDeFactura);
                cmd.Parameters.AddWithValue("@punto_de_venta", puntoDeVenta); // ej: "0001" o "0004"

                try
                {
                    conexion.Open();
                    var result = cmd.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        string maxNumFactura = result.ToString();

                        if (maxNumFactura.Length >= 12)
                        {
                            // Primeros 4 = punto de venta, últimos 8 = número correlativo
                            string parteNumerica = maxNumFactura.Substring(4, 8);
                            nuevoNumero = int.Parse(parteNumerica) + 1;
                        }
                        else
                        {
                            throw new Exception("El formato de num_factura no es válido. Valor: " + maxNumFactura);
                        }
                    }

                    proximoNumFactura = puntoDeVenta + nuevoNumero.ToString("D8");
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al obtener el próximo número de factura: " + ex.Message);
                }
            }

            return proximoNumFactura;
        }


        //CREAR UNA FACTURA AL IMPRIMIR LA VENTA

        public static bool crearFactura(int id, DateTime fecha, int sucursal_id, int vendedor_id, int cliente_id, string forma_de_pago,
            double precio_total, double recargo_tarjeta, double descuento, int numero_de_caja, string tipo_consumidor, string origen, 
            string factura_pdf, string num_factura, string tipo_de_factura)
        {

            string query = "INSERT INTO dbo.Factura (" +
                "id, fecha, sucursal_id, empleado_id, cliente_id, forma_de_pago, precio_total, " +
                "recargo_tarjeta, descuento, numero_de_caja, tipo_de_consumidor, origen, factura_pdf, " +
                "num_factura, tipo_de_factura) " +
                "VALUES (@id, @fecha, @sucursal_id, @vendedor_id, @cliente_id, @forma_de_pago, @precio_total, " +
                "@recargo_tarjeta, @descuento, @numero_de_caja, @tipo_consumidor, @origen, @factura_pdf, " +
                "@num_factura, @tipo_de_factura);";

            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@fecha", fecha);
            cmd.Parameters.AddWithValue("@sucursal_id", sucursal_id);
            cmd.Parameters.AddWithValue("@vendedor_id", vendedor_id);
            cmd.Parameters.AddWithValue("@cliente_id", cliente_id);
            cmd.Parameters.AddWithValue("@forma_de_pago", forma_de_pago);
            cmd.Parameters.AddWithValue("@precio_total", precio_total);
            cmd.Parameters.AddWithValue("@recargo_tarjeta", recargo_tarjeta);
            cmd.Parameters.AddWithValue("@descuento", descuento);
            cmd.Parameters.AddWithValue("@numero_de_caja", numero_de_caja);
            cmd.Parameters.AddWithValue("@tipo_consumidor", tipo_consumidor);
            cmd.Parameters.AddWithValue("@origen", origen);
            cmd.Parameters.AddWithValue("@factura_pdf", factura_pdf);
            cmd.Parameters.AddWithValue("@num_factura", num_factura);
            cmd.Parameters.AddWithValue("@tipo_de_factura", tipo_de_factura);

            try
            {
                DB_Controller.connection.Open();
                cmd.ExecuteNonQuery();
                DB_Controller.connection.Close();
                return true;
            }
            catch (Exception e)
            {
                throw new Exception("Hay un error en la query: " + e.Message);
            }




        }

        public static List<Factura> getAllIntervaloFechas(DateTime fecha_inicio, DateTime fecha_final)
        {
            List<Factura> facturas = new List<Factura>();
            string query = "SELECT * FROM dbo.factura WHERE fecha BETWEEN @fecha_inicio AND @fecha_final AND sucursal_id != 0";
            // Modificar fecha_final para incluir todo el día
            DateTime fecha_final_modificada = fecha_final.AddDays(1).AddTicks(-1);
            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);
            cmd.Parameters.Add("@fecha_inicio", SqlDbType.DateTime).Value = fecha_inicio;
            cmd.Parameters.Add("@fecha_final", SqlDbType.DateTime).Value = fecha_final_modificada;

            try
            {
                DB_Controller.connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Sucursal sucursal = new Sucursal();
                    Empleado empleado = new Empleado();
                    Cliente cliente = new Cliente();
                    sucursal.id = reader.GetInt32(2);
                    empleado.id = reader.GetInt32(3);
                    cliente.id = reader.GetInt32(4);
                    string factura_pdf = reader.IsDBNull(12) ? string.Empty : reader.GetString(12);

                    Factura factura = new Factura(reader.GetInt32(0), reader.GetDateTime(1), sucursal, empleado, cliente, reader.GetString(5), reader.GetDouble(6),
                        reader.GetDouble(7), reader.GetDouble(8), reader.GetInt32(9), reader.GetString(10), reader.GetString(11), factura_pdf, reader.GetString(13), reader.GetString(14));

                    facturas.Add(factura);
                }
                reader.Close();
                DB_Controller.connection.Close();
                return facturas;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                throw new Exception("Hay un error en la query: " + e.Message);

            }

        }

        public static Factura getById(int id)
        {
            Factura factura = new Factura();
            string query = "SELECT * FROM dbo.factura WHERE id = @id";
            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);
            cmd.Parameters.AddWithValue("@id", id);
            try
            {
                DB_Controller.connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    factura.id = reader.GetInt32(0);
                    factura.fecha = reader.GetDateTime(1);
                    factura.sucursal_id = SucursalControlador.getById(reader.GetInt32(2));
                    factura.empleado_id = EmpleadoControlador.obtenerPorId(reader.GetInt32(3));
                    factura.cliente_id = ClienteControlador.obtenerPorId(reader.GetInt32(4));
                    factura.forma_de_pago = reader.GetString(5);
                    factura.precio_total = reader.GetDouble(6);
                    factura.recargo_tarjeta = reader.GetDouble(7);
                    factura.descuento = reader.GetDouble(8);
                    factura.numero_de_caja = reader.GetInt32(9);
                    factura.tipo_de_consumidor = reader.GetString(10);
                    factura.origen = reader.GetString(11);
                    factura.factura_pdf = reader.GetString(12);
                    factura.num_factura = reader.GetString(13);
                    factura.tipo_de_factura = reader.GetString(14);
                }
                reader.Close();
                DB_Controller.connection.Close();
                return factura;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return factura;
        }
    }
}
