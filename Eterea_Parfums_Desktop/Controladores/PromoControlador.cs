using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Eterea_Parfums_Desktop.Controladores
{
    internal class PromoControlador
    {


        //POST - CREAR UNA PROMOCION

        public static bool crearPromocion(Promocion promo)
        {
            string query = @"
                           INSERT INTO dbo.promocion
                               (id, nombre, fecha_inicio, fecha_fin, descuento, activo, descripcion, banner, imagen_URL)
                           VALUES
                               (@id, @nombre, @fecha_inicio, @fecha_fin, @descuento, @activo, @descripcion, @banner, @imagen_URL);";

            using (var cmd = new SqlCommand(query, DB_Controller.connection))
            {
                cmd.Parameters.AddWithValue("@id", obtenerMaxId() + 1);
                cmd.Parameters.AddWithValue("@nombre", promo.nombre);
                cmd.Parameters.AddWithValue("@fecha_inicio", promo.fecha_inicio);
                cmd.Parameters.AddWithValue("@fecha_fin", promo.fecha_fin);
                cmd.Parameters.AddWithValue("@descuento", promo.descuento);
                cmd.Parameters.AddWithValue("@activo", promo.activo);
                cmd.Parameters.AddWithValue("@descripcion", promo.descripcion);
                cmd.Parameters.AddWithValue("@banner", promo.banner);
                cmd.Parameters.AddWithValue("@imagen_URL", (object)promo.imagen_URL ?? DBNull.Value);

                SqlTransaction transaction = null;
                try
                {
                    DB_Controller.connection.Open();
                    transaction = DB_Controller.connection.BeginTransaction();
                    cmd.Transaction = transaction;

                    cmd.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    transaction?.Rollback();
                    throw new Exception("Hay un error en la query: " + e.Message);
                }
                finally
                {
                    if (DB_Controller.connection.State == System.Data.ConnectionState.Open)
                        DB_Controller.connection.Close();
                }
            }
        }








        // OBTENER EL MAX ID

        public static int obtenerMaxId()
        {
            int MaxId = 0;
            string query = "select max(id) from dbo.promocion;";

            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);

            try
            {
                DB_Controller.connection.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    MaxId = reader.GetInt32(0);
                }
                reader.Close();
                DB_Controller.connection.Close();
                return MaxId;
            }
            catch (Exception e)
            {
                throw new Exception("Hay un error en la query: " + e.Message);
            }
        }


        // GET ALL -  OBTENER TODAS LAS PROMOCIONES DE LA BD
        public static List<Promocion> obtenerTodos()
        {
            var list = new List<Promocion>();
            string query = "SELECT * FROM dbo.promocion ORDER BY nombre ASC;";

            using (var cmd = new SqlCommand(query, DB_Controller.connection))
            {
                try
                {
                    DB_Controller.connection.Open();
                    var r = cmd.ExecuteReader();
                    while (r.Read())
                    {
                        list.Add(new Promocion(
                            r.GetInt32(0),        // id
                            r.GetString(1),       // nombre
                            r.GetDateTime(2),     // fecha_inicio
                            r.GetDateTime(3),     // fecha_fin
                            r.GetInt32(4),        // descuento
                            r.GetBoolean(5),      // activo
                            r.GetString(6),       // descripcion
                            r.GetString(7),       // banner
                            r.IsDBNull(8) ? null : r.GetString(8) // imagen_URL
                        ));
                    }
                    r.Close();
                }
                catch (Exception e)
                {
                    throw new Exception("Hay un error en la query: " + e.Message);
                }
                finally
                {
                    if (DB_Controller.connection.State == System.Data.ConnectionState.Open)
                        DB_Controller.connection.Close();
                }
            }
            return list;
        }





        //GET ONE BY ID - OBTENER UNA PROMOCION POR SU ID

        public static Promocion obtenerPorId(int id)
        {
            Promocion promo = null;
            string query = "SELECT * FROM dbo.promocion WHERE id = @id;";

            using (var cmd = new SqlCommand(query, DB_Controller.connection))
            {
                cmd.Parameters.AddWithValue("@id", id);
                try
                {
                    DB_Controller.connection.Open();
                    var r = cmd.ExecuteReader();
                    if (r.Read())
                    {
                        promo = new Promocion(
                            r.GetInt32(0),
                            r.GetString(1),
                            r.GetDateTime(2),
                            r.GetDateTime(3),
                            r.GetInt32(4),
                            r.GetBoolean(5),
                            r.GetString(6),
                            r.GetString(7),
                            r.IsDBNull(8) ? null : r.GetString(8)
                        );
                    }
                    r.Close();
                }
                catch (Exception e)
                {
                    throw new Exception("Hay un error en la query: " + e.Message);
                }
                finally
                {
                    if (DB_Controller.connection.State == System.Data.ConnectionState.Open)
                        DB_Controller.connection.Close();
                }
            }
            return promo;
        }








        //Método para obtener el nombre de la imagen del banner de la foto buscandola por su id

        public static string obtenerNombreImagen(int promoId)
        {
            string nombreArchivo = string.Empty;

            string query = "SELECT banner FROM dbo.promocion WHERE id = @id;";

            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);
            cmd.Parameters.AddWithValue("@id", promoId);

            try
            {
                DB_Controller.connection.Open();
                SqlDataReader r = cmd.ExecuteReader();

                if (r.Read()) // Si hay un resultado
                {
                    nombreArchivo = r.IsDBNull(0) ? string.Empty : r.GetString(0);
                }

                r.Close();
                DB_Controller.connection.Close();
            }
            catch (Exception e)
            {
                throw new Exception("Error al obtener el nombre de la imagen: " + e.Message);
            }

            return nombreArchivo;
        }







        //EDIT ó PUT  -  EDITAR UNA PROMO
        public static bool editarPromo(Promocion promo)
        {
            string query = @"
        UPDATE dbo.promocion
        SET  id = @id_editar,
             nombre = @nombre,
             fecha_inicio = @fechaInicio,
             fecha_fin = @fechaFin,
             descuento = @descuento,
             activo = @activo,
             descripcion = @descripcion,
             banner = @banner,
             imagen_URL = @imagen_URL
        WHERE id = @id_editar;";

            using (var cmd = new SqlCommand(query, DB_Controller.connection))
            {
                cmd.Parameters.AddWithValue("@id_editar", promo.id);
                cmd.Parameters.AddWithValue("@nombre", promo.nombre);
                cmd.Parameters.AddWithValue("@fechaInicio", promo.fecha_inicio);
                cmd.Parameters.AddWithValue("@fechaFin", promo.fecha_fin);
                cmd.Parameters.AddWithValue("@descuento", promo.descuento);
                cmd.Parameters.AddWithValue("@activo", promo.activo);
                cmd.Parameters.AddWithValue("@descripcion", promo.descripcion);
                cmd.Parameters.AddWithValue("@banner", promo.banner);
                cmd.Parameters.AddWithValue("@imagen_URL", (object)promo.imagen_URL ?? DBNull.Value);

                SqlTransaction transaction = null;
                try
                {
                    DB_Controller.connection.Open();
                    transaction = DB_Controller.connection.BeginTransaction();
                    cmd.Transaction = transaction;

                    cmd.ExecuteNonQuery();

                    PerfumeEnPromoControlador.eliminarRegistrosPromoPerfumes(promo.id, transaction);

                    transaction.Commit();
                    return true;
                }
                catch (Exception e)
                {
                    transaction?.Rollback();
                    throw new Exception("Hay un error en la query: " + e.Message);
                }
                finally
                {
                    if (DB_Controller.connection.State == System.Data.ConnectionState.Open)
                        DB_Controller.connection.Close();
                }
            }
        }






        //METODO PARA ELIMINAR UNA PROMOCION

        // ELIMINADO LÓGICO (nuevo)
        public static bool eliminarPromo(int id_promo)
        {
            string query = @"
        UPDATE dbo.promocion
        SET fecha_inicio = @fechaInicio,
            fecha_fin    = @fechaFin,
            activo       = @activo
        WHERE id = @id;";

            using (var cmd = new SqlCommand(query, DB_Controller.connection))
            {
                // Fechas: hoy-2 y ayer
                DateTime hoy = DateTime.Today;
                DateTime fechaInicio = hoy.AddDays(-2);
                DateTime fechaFin = hoy.AddDays(-1);

                cmd.Parameters.AddWithValue("@id", id_promo);
                cmd.Parameters.AddWithValue("@fechaInicio", fechaInicio);
                cmd.Parameters.AddWithValue("@fechaFin", fechaFin);
                cmd.Parameters.AddWithValue("@activo", false);

                try
                {
                    DB_Controller.connection.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception("Ocurrió un error al eliminar lógicamente la promoción: " + e.Message);
                }
                finally
                {
                    if (DB_Controller.connection.State == ConnectionState.Open)
                        DB_Controller.connection.Close();
                }
            }
        }




        //Método para verificar que el nombre de la promo no se repite
        public static bool ExisteNombrePromo(string nombrePromo, int? idPromo = null)
        {
            string query = "SELECT COUNT(*) FROM promocion WHERE nombre = @nombrePromo";

            if (idPromo != null)
            {
                query += " AND id <> @idPromo"; // Evita conflicto al editar
            }

            using (SqlConnection conexion = new SqlConnection(DB_Controller.GetConnectionString()))
            {
                conexion.Open();
                using (SqlCommand cmd = new SqlCommand(query, conexion))
                {
                    cmd.Parameters.AddWithValue("@nombrePromo", nombrePromo);
                    if (idPromo != null)
                    {
                        cmd.Parameters.AddWithValue("@idPromo", idPromo);
                    }

                    int count = (int)cmd.ExecuteScalar();
                    return count > 0; // Retorna true si ya existe
                }
            }
        }

        public static bool editarPromoYRelaciones(Promocion promo, List<int> perfumeIds)
        {
            const string updateSql = @"
        UPDATE dbo.promocion
        SET nombre=@nombre, fecha_inicio=@fechaInicio, fecha_fin=@fechaFin,
            descuento=@descuento, activo=@activo, descripcion=@descripcion,
            banner=@banner, imagen_URL=@imagen_URL
        WHERE id=@id";

            const string deleteSql = @"DELETE FROM dbo.perfumes_en_promo WHERE promocion_id = @promoId";

            const string insertSql = @"INSERT INTO dbo.perfumes_en_promo (perfume_id, promocion_id)
                               VALUES (@perfumeId, @promoId)";

            using (var conn = new SqlConnection(DB_Controller.GetConnectionString()))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // UPDATE promo
                        using (var cmd = new SqlCommand(updateSql, conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@id", promo.id);
                            cmd.Parameters.AddWithValue("@nombre", promo.nombre);
                            cmd.Parameters.AddWithValue("@fechaInicio", promo.fecha_inicio);
                            cmd.Parameters.AddWithValue("@fechaFin", promo.fecha_fin);
                            cmd.Parameters.AddWithValue("@descuento", promo.descuento);
                            cmd.Parameters.AddWithValue("@activo", promo.activo);
                            cmd.Parameters.AddWithValue("@descripcion", promo.descripcion ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@banner", promo.banner ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@imagen_URL", (object)promo.imagen_URL ?? DBNull.Value);

                            cmd.ExecuteNonQuery();
                        }

                        // DELETE relaciones actuales
                        using (var del = new SqlCommand(deleteSql, conn, tx))
                        {
                            del.Parameters.AddWithValue("@promoId", promo.id);
                            del.ExecuteNonQuery();
                        }

                        // INSERT relaciones nuevas (distintas)
                        var ids = (perfumeIds ?? new List<int>()).Distinct().ToList();
                        if (ids.Count > 0)
                        {
                            using (var ins = new SqlCommand(insertSql, conn, tx))
                            {
                                ins.Parameters.Add("@perfumeId", SqlDbType.Int);
                                ins.Parameters.Add("@promoId", SqlDbType.Int).Value = promo.id;

                                foreach (var pid in ids)
                                {
                                    ins.Parameters["@perfumeId"].Value = pid;
                                    ins.ExecuteNonQuery();
                                }
                            }
                        }

                        tx.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tx.Rollback();
                        throw new Exception("Hay un error en la query: " + ex.Message, ex);
                    }
                }
            }
        }




    }
}
