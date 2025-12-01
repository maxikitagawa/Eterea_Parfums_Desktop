using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop.Controladores
{
    internal class DetalleFacturaControlador
    {
        public static List<DetalleFactura> getByIdFactura(int factura_id)
        {
            List<DetalleFactura> detalles = new List<DetalleFactura>();
            string query = "SELECT * FROM dbo.detalle_factura WHERE factura_id = @factura_id";

            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);
            cmd.Parameters.AddWithValue("@factura_id", factura_id);

            try
            {
                if (DB_Controller.connection.State != ConnectionState.Open)
                    DB_Controller.connection.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    Factura factura = new Factura { id = reader.GetInt32(0) };
                    Perfume perfume = new Perfume { id = reader.GetInt32(1) };
                    int cantidad = reader.GetInt32(2);
                    double precioUnitario = reader.GetDouble(3);

                    Promocion promocion1 = null;
                    Promocion promocion2 = null;

                    if (!reader.IsDBNull(4))
                        promocion1 = new Promocion { id = reader.GetInt32(4) };

                    if (!reader.IsDBNull(5))
                        promocion2 = new Promocion { id = reader.GetInt32(5) };

                    DetalleFactura detalle = new DetalleFactura(factura, perfume, cantidad, precioUnitario, promocion1, promocion2);
                    detalles.Add(detalle);
                }

                reader.Close();
            }
            catch (Exception e)
            {
                throw new Exception("Hay un error en la query: " + e.Message);
            }
            finally
            {
                if (DB_Controller.connection.State != ConnectionState.Closed)
                    DB_Controller.connection.Close();
            }

            return detalles;
        }


        public static bool crearDetalleFactura(int factura_id, int perfume_id, int cantidad, float precio_unitario, int? promocion_id, int? promocion2_id)
        {
            string query = "INSERT INTO dbo.detalle_factura (factura_id, perfume_id, cantidad, precio_unitario, promocion_id, promocion2_id) " +
                           "VALUES (@factura_id, @perfume_id, @cantidad, @precio_unitario, @promocion_id, @promocion2_id);";

            try
            {
                using (SqlConnection connection = new SqlConnection(DB_Controller.GetConnectionString()))
                {
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@factura_id", factura_id);
                        cmd.Parameters.AddWithValue("@perfume_id", perfume_id);
                        cmd.Parameters.AddWithValue("@cantidad", cantidad);
                        cmd.Parameters.AddWithValue("@precio_unitario", precio_unitario);
                        cmd.Parameters.AddWithValue("@promocion_id",
                        (object)promocion_id ?? 1);
                        cmd.Parameters.AddWithValue("@promocion2_id",
                        (object)promocion2_id ?? DBNull.Value);

                        connection.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
                return true; // Si la operación fue exitosa
            }
            catch (Exception e)
            {

                throw new Exception("Hay un error en la query: " + e.Message);
               
            }
        }
    }
}
