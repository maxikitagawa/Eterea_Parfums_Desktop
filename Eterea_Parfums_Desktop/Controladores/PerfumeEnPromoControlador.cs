using Eterea_Parfums_Desktop.DTOs;
using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Eterea_Parfums_Desktop.Controladores
{
    public class PerfumeEnPromoControlador
    {


        //POST

        public static bool guardarRelacionPromoPerfume(int idPerfume, int idPromo)
        {
            //Relacionar una promoción con uno o varios perfumes en la base de datos

            // Definir la consulta SQL para insertar la relación en la tabla Promo_Perfume
            string query = "INSERT INTO dbo.perfumes_en_promo (perfume_id, promocion_id) VALUES (@perfumeId, @promoId)";

            // Utilizar parámetros para evitar SQL Injection
            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);

            cmd.Parameters.AddWithValue("@perfumeId", idPerfume);
            cmd.Parameters.AddWithValue("@promoId", idPromo);


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








        //obtener los perfumes de una promo

        public List<PerfumeDTO> CargarPerfumesPorIdPromocion(int idPromo)
        {
            string query = @"
        SELECT p.id, m.nombre AS marca, p.nombre, p.presentacion_ml, g.genero AS genero
        FROM dbo.perfumes_en_promo pep
        JOIN dbo.perfume p ON pep.perfume_id = p.id
        JOIN dbo.marca m ON p.marca_id = m.id
        JOIN dbo.genero g ON p.genero_id = g.id
        WHERE pep.promocion_id = @idPromo;";

            List<PerfumeDTO> perfumes = new List<PerfumeDTO>();

            // ✅ Usá el método que retorna la cadena correctamente configurada
            using (SqlConnection connection = new SqlConnection(DB_Controller.GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@idPromo", idPromo);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PerfumeDTO perfume = new PerfumeDTO
                                {
                                    id = reader.GetInt32(0),
                                    marca = reader.GetString(1),
                                    nombre = reader.GetString(2),
                                    mililitros = reader.GetInt32(3),
                                    genero = reader.GetString(4)
                                };
                                perfumes.Add(perfume);

                                Console.WriteLine($"ID: {perfume.id}, Marca: {perfume.marca}, Nombre: {perfume.nombre}, " +
                                                  $"Mililitros: {perfume.mililitros}, Género: {perfume.genero}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al cargar los perfumes de la promoción: {ex.Message}");
                        throw new Exception("Error al cargar los perfumes de la promoción.", ex);
                    }
                }
            }

            Console.WriteLine($"Total perfumes recuperados: {perfumes.Count}");
            return perfumes;
        }


        /* using (SqlCommand cmd = new SqlCommand(query, DB_Controller.connection))
         {
             cmd.Parameters.AddWithValue("@idPromo", idPromo);

             try
             {
                 DB_Controller.connection.Open();
                 using (SqlDataReader reader = cmd.ExecuteReader())
                 {
                     while (reader.Read())
                     {
                         PerfumeDTO perfume = new PerfumeDTO
                         {
                             id = reader.GetInt32(0),
                             marca = reader.GetString(1),
                             nombre = reader.GetString(2),
                             mililitros = reader.GetInt32(3),
                             genero = reader.GetString(4)
                         };
                         perfumes.Add(perfume);
                     }
                 }
             }
             catch (Exception ex)
             {
                 throw new Exception("Error al cargar los perfumes de la promoción.", ex);
             }
             finally
             {
                 DB_Controller.connection.Close();
             }
         }
         // Verifica que los datos se están recuperando correctamente
         MessageBox.Show($"Se recuperaron {perfumes.Count} perfumes.");

         return perfumes;
     }*/

        public static List<Promocion> getByIDPerfume(int idPerfume)
        {
            string query = @"
        SELECT p.id,
               p.nombre,
               p.fecha_inicio,
               p.fecha_fin,
               p.descuento,
               p.activo,
               p.descripcion,
               p.banner,
               p.imagen_URL
        FROM dbo.promocion p
        INNER JOIN dbo.perfumes_en_promo pep ON p.id = pep.promocion_id
        WHERE pep.perfume_id = @idPerfume AND p.activo = 1";

            var listaPromociones = new List<Promocion>();

            using (var cmd = new SqlCommand(query, DB_Controller.connection))
            {
                cmd.Parameters.AddWithValue("@idPerfume", idPerfume);

                try
                {
                    DB_Controller.connection.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            listaPromociones.Add(new Promocion(
                                r.GetInt32(0),                   // id
                                r.GetString(1),                  // nombre
                                r.GetDateTime(2),                // fecha_inicio
                                r.GetDateTime(3),                // fecha_fin
                                r.GetInt32(4),                   // descuento
                                r.GetBoolean(5),                 // activo
                                r.GetString(6),                  // descripcion
                                r.GetString(7),                  // banner
                                r.IsDBNull(8) ? null : r.GetString(8) // imagen_URL
                            ));
                        }
                    }
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

            return listaPromociones;
        }





        //ELIMINAR TODOS LOS REGISTROS DE LA RELACION DE LA PROMOCION CON LOS PERFUMES QUE INCLUYE

        public static void eliminarRegistrosPromoPerfumes(int id_promo, SqlTransaction transaction)
        {
            string query = "DELETE FROM dbo.perfumes_en_promo WHERE promocion_id = @id_editar";


            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);
            cmd.Parameters.AddWithValue("@id_editar", id_promo);

            cmd.Transaction = transaction;

            cmd.ExecuteNonQuery();

        }

        /*
                public static bool EliminarRegistrosPromoPerfumes(int id_promo)
                {
                    string query = "DELETE FROM dbo.perfumes_en_promo WHERE promocion_id = @id_editar";

                    SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);

                    try
                    {
                        DB_Controller.connection.Open();
                        cmd.Parameters.AddWithValue("@id_editar", id_promo);
                        cmd.ExecuteNonQuery();

                        DB_Controller.connection.Close();
                        return true;
                    }
                    catch (Exception e)
                    {
                        if (DB_Controller.connection.State == System.Data.ConnectionState.Open)
                        {
                            DB_Controller.connection.Close();
                        }
                        throw new Exception("Error al eliminar registros de perfumes de la promoción: " + e.Message);
                    }
                }
                */




        //METODO PARA GUARDAR EN LA BD LA RELACION DE LA PROMOCION CON LOS PERFUMES QUE ESTA INCLUYE

        public static bool agregarRegistrosPromoPerfumes(int id_promo, List<int> perfumeIds)
        {
            string query = @"
        INSERT INTO dbo.perfumes_en_promo (perfume_id, promocion_id)
        VALUES (@perfumeId, @promoId)";

            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);

            try
            {
                DB_Controller.connection.Open();

                foreach (int perfumeId in perfumeIds)
                {
                    cmd.Parameters.Clear(); // Limpia los parámetros para cada iteración
                    cmd.Parameters.AddWithValue("@perfumeId", perfumeId);
                    cmd.Parameters.AddWithValue("@promoId", id_promo);
                    cmd.ExecuteNonQuery();
                }

                DB_Controller.connection.Close();
                return true;
            }
            catch (Exception e)
            {
                if (DB_Controller.connection.State == System.Data.ConnectionState.Open)
                {
                    DB_Controller.connection.Close();
                }
                throw new Exception("Error al agregar registros de perfumes a la promoción: " + e.Message);
            }
        }

        public int? obtenerMayorDescuentoPorPerfume(int perfumeId)
        {
            string query = @"
                SELECT TOP 1 p.descuento
                FROM dbo.perfumes_en_promo pep
                JOIN dbo.promocion p ON pep.promocion_id = p.id
                WHERE pep.perfume_id = @perfumeId AND p.activo = 1
                ORDER BY p.descuento DESC;";

            int descuento = 0;

            using (SqlConnection connection = new SqlConnection(DB_Controller.GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@perfumeId", perfumeId);

                    try
                    {
                        connection.Open();
                        object result = cmd.ExecuteScalar(); // Obtener un único valor

                        if (result != null && result != DBNull.Value)
                        {
                            descuento = Convert.ToInt32(result); // Ahora lo convertimos a int
                        }

                        Console.WriteLine($"Perfume ID: {perfumeId}, Descuento Aplicado: {descuento}%");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al obtener descuento para perfume ID {perfumeId}: {ex.Message}");
                        throw new Exception("Error al obtener descuento del perfume.", ex);
                    }
                }
            }

            return descuento;
        }

        public int? obtenerPromocionIdPorPerfume(int perfumeId)
        {
            string query = @"
                SELECT TOP 1 p.id
                FROM dbo.perfumes_en_promo pep
                JOIN dbo.promocion p ON pep.promocion_id = p.id
                WHERE pep.perfume_id = @perfumeId AND p.activo = 1 AND p.descuento >= 20
                ORDER BY p.id DESC;
                ";

            int? promocionId = null;  // Usamos int? para permitir valores nulos

            using (SqlConnection connection = new SqlConnection(DB_Controller.GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@perfumeId", perfumeId);

                    try
                    {
                        connection.Open();
                        object result = cmd.ExecuteScalar(); // Obtener un único valor

                        if (result != null && result != DBNull.Value)
                        {
                            promocionId = Convert.ToInt32(result); // Convertimos a int
                        }

                        Console.WriteLine($"Perfume ID: {perfumeId}, Promoción Aplicada: {promocionId}");
                    }
                    /*catch (Exception ex)
                    {
                        Console.WriteLine($"Error al obtener promoción para perfume ID {perfumeId}: {ex.Message}");
                        throw new Exception("Error al obtener la promoción del perfume.", ex);
                    }*/
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ERROR REAL: {ex.Message}");
                        Console.WriteLine($"INNER EXCEPTION: {ex.InnerException?.Message}");
                        Console.WriteLine($"STACKTRACE: {ex.StackTrace}");
                        throw;
                    }
                }
            }

            return promocionId;
        }

        public int? obtenerPromocionPorPerfumeConDescuento10(int perfumeId)
        {
            string query = @"
            SELECT TOP 1 p.descuento
            FROM dbo.perfumes_en_promo pep
            JOIN dbo.promocion p ON pep.promocion_id = p.id
            WHERE pep.perfume_id = @perfumeId 
              AND p.activo = 1
              AND p.descuento = 10
            ORDER BY p.descuento DESC;";

            int? descuento = null;  // Usamos int? para permitir valores nulos

            using (SqlConnection connection = new SqlConnection(DB_Controller.GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@perfumeId", perfumeId);

                    try
                    {
                        connection.Open();
                        object result = cmd.ExecuteScalar(); // Obtener un único valor

                        if (result != null && result != DBNull.Value)
                        {
                            descuento = Convert.ToInt32(result); // Convertimos a int
                        }

                        Console.WriteLine($"Perfume ID: {perfumeId}, Promoción con Descuento 10 Aplicada: {descuento}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al obtener promoción para perfume ID {perfumeId}: {ex.Message}");
                        throw new Exception($"Error al obtener la promoción del perfume ID {perfumeId}: {ex.Message}", ex);
                    }
                }
            }

            return descuento;
        }

        public int? obtenerPromocionIdPorPerfumeConDescuento10(int perfumeId)
        {
            string query = @"
                SELECT TOP 1 p.id
                FROM dbo.perfumes_en_promo pep
                JOIN dbo.promocion p ON pep.promocion_id = p.id
                WHERE pep.perfume_id = @perfumeId AND p.activo = 1 AND p.descuento = 10
                ORDER BY p.id DESC;
                ";

            int? promocionId = null;  // Usamos int? para permitir valores nulos

            using (SqlConnection connection = new SqlConnection(DB_Controller.GetConnectionString()))
            {
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@perfumeId", perfumeId);

                    try
                    {
                        connection.Open();
                        object result = cmd.ExecuteScalar(); // Obtener un único valor

                        if (result != null && result != DBNull.Value)
                        {
                            promocionId = Convert.ToInt32(result); // Convertimos a int
                        }

                        Console.WriteLine($"Perfume ID: {perfumeId}, Promoción Aplicada: {promocionId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al obtener promoción para perfume ID {perfumeId}: {ex.Message}");
                        throw new Exception("Error al obtener la promoción del perfume.", ex);
                    }
                }
            }

            return promocionId;
        }

        public static void AsegurarRelacionSinPromo(int idPerfume, SqlTransaction transaccion = null)
        {
            string query = "IF NOT EXISTS (SELECT * FROM dbo.perfumes_en_promo WHERE perfume_id = @idPerfume) " +
                           "INSERT INTO dbo.perfumes_en_promo (perfume_id, promocion_id) VALUES (@idPerfume, 1);";

            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);
            cmd.Parameters.AddWithValue("@idPerfume", idPerfume);

            if (transaccion != null)
                cmd.Transaction = transaccion;

            cmd.ExecuteNonQuery();
        }


        public static void EliminarRelacionesPromoExceptoSinPromo(int idPerfume, SqlTransaction transaction)
        {
            string query = @"
        DELETE FROM dbo.perfumes_en_promo
        WHERE perfume_id = @idPerfume AND promocion_id <> 1;";

            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection, transaction);
            cmd.Parameters.AddWithValue("@idPerfume", idPerfume);

            cmd.ExecuteNonQuery();
        }



    }
}
