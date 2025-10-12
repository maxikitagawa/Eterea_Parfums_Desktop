using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;

namespace Eterea_Parfums_Desktop.Controladores
{
    internal class PerfumeControlador
    {
        public static List<Perfume> getAll()
        {
            List<Perfume> lista_perfumes = new List<Perfume>();

            Marca marca = new Marca();
            TipoDePerfume tipo_de_perfume = new TipoDePerfume();
            Genero genero = new Genero();
            Pais pais = new Pais();

            string query = "SELECT * FROM dbo.perfume ORDER BY nombre ASC;";
            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);
            try
            {
                DB_Controller.connection.Open();
                SqlDataReader r = cmd.ExecuteReader();

                while (r.Read())
                {

                    /* 
                     Marca marca = new Marca(r.GetInt32(2), null);
                     TipoDePerfume tipo_de_perfume = new TipoDePerfume(r.GetInt32(4), null);
                     Genero genero = new Genero(r.GetInt32(5), null);
                     Pais pais = new Pais(r.GetInt32(7), null);
                    */
                    marca.id = r.GetInt32(2);
                    tipo_de_perfume.id = r.GetInt32(4);
                    genero.id = r.GetInt32(5);
                    pais.id = r.GetInt32(7);

                    Marca marcaOb = new Marca(marca.id, "");
                    TipoDePerfume tipo_de_perfumeOb = new TipoDePerfume(tipo_de_perfume.id, "");
                    Genero generoOb = new Genero(genero.id, "");
                    Pais paisOb = new Pais(pais.id, "");

                    DateTime? fechaBaja = r.IsDBNull(16) ? (DateTime?)null : r.GetDateTime(16);

                    /*if (r.GetInt32(13) == 1)
                    {
                        lista_perfumes.Add(new Perfume(r.GetInt32(0), r.GetString(1), marcaOb, r.GetString(3),
                        tipo_de_perfumeOb, generoOb, r.GetInt32(6), paisOb,
                        r.GetInt32(8), r.GetInt32(9), r.GetString(10), r.GetInt32(11), r.GetDouble(12),
                        r.GetInt32(13), r.GetString(14), r.GetString(15)));               
                    }*/

                    lista_perfumes.Add(new Perfume(r.GetInt32(0), r.GetString(1), marcaOb, r.GetString(3),
                        tipo_de_perfumeOb, generoOb, r.GetInt32(6), paisOb,
                       r.GetBoolean(8), r.GetBoolean(9), r.GetString(10), r.GetInt32(11), r.GetDouble(12),
                        r.GetBoolean(13), r.GetString(14), r.GetString(15), fechaBaja, r.GetString(17), r.GetString(18)));


                }
                r.Close();
                DB_Controller.connection.Close();

            }
            catch (Exception e)
            {
                throw new Exception("Hay un error en la query: " + e.Message);
            }
            return lista_perfumes;

        }


        public static bool create(Perfume perfume)
        {
            int nuevoId = GetByMaxId() + 1;


            string query = "insert into dbo.perfume values" +
                 "(@id, " +
                "@codigo, " +
                "@marca, " +
                "@nombre, " +
                "@tipo_de_perfume, " +
                "@genero, " +
                "@presentacion_ml, " +
                "@pais, " +
                "@spray, " +
                "@recargable, " +
                "@descripcion, " +
                "@anio_de_lanzamiento, " +
                "@precio_en_pesos, " +
                "@activo, " +
                "@imagen1, " +
                "@imagen2, " +
                "@fecha_baja," +
                "@imagen1_URL," +
                "@imagen2_URL);";

            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);

            cmd.Parameters.AddWithValue("@id", nuevoId);
            cmd.Parameters.AddWithValue("@codigo", perfume.codigo);
            cmd.Parameters.AddWithValue("@marca", perfume.marca.id);
            cmd.Parameters.AddWithValue("@nombre", perfume.nombre);
            cmd.Parameters.AddWithValue("@tipo_de_perfume", perfume.tipo_de_perfume.id);
            cmd.Parameters.AddWithValue("@genero", perfume.genero.id);
            cmd.Parameters.AddWithValue("@presentacion_ml", perfume.presentacion_ml);
            cmd.Parameters.AddWithValue("@pais", perfume.pais.id);
            cmd.Parameters.AddWithValue("@spray", perfume.spray);
            cmd.Parameters.AddWithValue("@recargable", perfume.recargable);
            cmd.Parameters.AddWithValue("@descripcion", perfume.descripcion);
            cmd.Parameters.AddWithValue("@anio_de_lanzamiento", perfume.anio_de_lanzamiento);
            cmd.Parameters.AddWithValue("@precio_en_pesos", perfume.precio_en_pesos);
            cmd.Parameters.AddWithValue("@activo", false);
            cmd.Parameters.AddWithValue("@imagen1", perfume.imagen1);
            cmd.Parameters.AddWithValue("@imagen2", perfume.imagen2);
            cmd.Parameters.AddWithValue("@fecha_baja", DateTime.Now);
            cmd.Parameters.AddWithValue("@imagen1_URL", perfume.imagen1_URL);
            cmd.Parameters.AddWithValue("@imagen2_URL", perfume.imagen2_URL);




            Console.WriteLine("genero_id" + perfume.genero.id);
            try
            {

                DB_Controller.connection.Open();
                cmd.ExecuteNonQuery();


                // 🔗 Asegurar relación con promoción ID 1 si no tiene otra
                PerfumeEnPromoControlador.AsegurarRelacionSinPromo(nuevoId);

                DB_Controller.connection.Close();
                return true;
            }
            catch (Exception e)
            {
                throw new Exception("Hay un error en la query: " + e.Message);

            }
        }


        public static Perfume getByID(int id)
        {
            Perfume perfume = new Perfume();
            string query = "select * from dbo.perfume where id = @id;";
            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);
            cmd.Parameters.AddWithValue("@id", id);
            try
            {
                DB_Controller.connection.Open();
                SqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    Marca marca = new Marca(r.GetInt32(2), null);
                    TipoDePerfume tipo_de_perfume = new TipoDePerfume(r.GetInt32(4), null);
                    Genero genero = new Genero(r.GetInt32(5), null);
                    Pais pais = new Pais(r.GetInt32(7), null);
                    perfume.id = r.GetInt32(0);
                    perfume.codigo = r.GetString(1);
                    perfume.marca = marca;
                    perfume.nombre = r.GetString(3);
                    perfume.tipo_de_perfume = tipo_de_perfume;
                    perfume.genero = genero;
                    perfume.presentacion_ml = r.GetInt32(6);
                    perfume.pais = pais;
                    perfume.spray = r.GetBoolean(8);
                    perfume.recargable = r.GetBoolean(9);
                    perfume.descripcion = r.GetString(10);
                    perfume.anio_de_lanzamiento = r.GetInt32(11);
                    perfume.precio_en_pesos = r.GetDouble(12);
                    perfume.activo = r.GetBoolean(13);
                    perfume.imagen1 = r.GetString(14);
                    perfume.imagen2 = r.GetString(15);
                }
                r.Close();

            }
            catch (Exception e)
            {

                throw new Exception("Hay un error en la query: " + e.Message);
            }
            finally
            {
                DB_Controller.connection.Close();
            }
            return perfume;
        }

        //GET ONE ByMaxId
        public static int GetByMaxId()
        {
            int MaxId = 0;
            string query = "select max(id) from dbo.perfume;";
            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);
            try
            {
                DB_Controller.connection.Open();
                SqlDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    MaxId = r.GetInt32(0);
                    Console.WriteLine("MaxId: " + MaxId);
                }

                r.Close();
                DB_Controller.connection.Close();
                return MaxId;
            }
            catch (Exception e)
            {
                throw new Exception("Hay un error en la query: " + e.Message);
            }

        }

        public static bool update(Perfume perfume)
        {
            bool result = false;

            // 1) Leer "activo" original
            bool? activoOriginal = null;
            const string querySelect = "SELECT activo FROM dbo.perfume WHERE id = @id;";
            using (var cmdSelect = new SqlCommand(querySelect, DB_Controller.connection))
            {
                cmdSelect.Parameters.AddWithValue("@id", perfume.id);
                DB_Controller.connection.Open();
                var valor = cmdSelect.ExecuteScalar();
                DB_Controller.connection.Close();
                if (valor != null && valor != DBNull.Value)
                    activoOriginal = Convert.ToBoolean(valor);
            }

            // 2) ¿Actualizar fecha_baja?
            bool actualizarFechaBaja = (activoOriginal != null && perfume.activo != null && activoOriginal != perfume.activo);

            // 3) UPDATE: incluye URLs y conserva valor si llegan NULL
            string query =
                "UPDATE dbo.perfume SET " +
                "codigo = @codigo, " +
                "marca_id = @marca, " +
                "nombre = @nombre, " +
                "tipo_de_perfume_id = @tipo_de_perfume, " +
                "genero_id = @genero, " +
                "presentacion_ml = @presentacion_ml, " +
                "pais_id = @pais, " +
                "spray = @spray, " +
                "recargable = @recargable, " +
                "descripcion = @descripcion, " +
                "anio_de_lanzamiento = @anio_de_lanzamiento, " +
                "precio_en_pesos = @precio_en_pesos, " +
                "activo = @activo, " +
                "imagen1 = COALESCE(@imagen1, imagen1), " +       
                "imagen2 = COALESCE(@imagen2, imagen2), " +       

                "imagen1_URL = COALESCE(@imagen1_URL, imagen1_URL), " + 
                "imagen2_URL = COALESCE(@imagen2_URL, imagen2_URL)";    

            if (actualizarFechaBaja)
            {
                query += ", fecha_baja = @fecha_baja";
            }

            query += " WHERE id = @id;";

            using (var cmd = new SqlCommand(query, DB_Controller.connection))
            {
                cmd.Parameters.AddWithValue("@id", perfume.id);
                cmd.Parameters.AddWithValue("@codigo", perfume.codigo);
                cmd.Parameters.AddWithValue("@marca", perfume.marca.id);
                cmd.Parameters.AddWithValue("@nombre", perfume.nombre);
                cmd.Parameters.AddWithValue("@tipo_de_perfume", perfume.tipo_de_perfume.id);
                cmd.Parameters.AddWithValue("@genero", perfume.genero.id);
                cmd.Parameters.AddWithValue("@presentacion_ml", perfume.presentacion_ml);
                cmd.Parameters.AddWithValue("@pais", perfume.pais.id);
                cmd.Parameters.AddWithValue("@spray", perfume.spray);
                cmd.Parameters.AddWithValue("@recargable", perfume.recargable);
                cmd.Parameters.AddWithValue("@descripcion", perfume.descripcion);
                cmd.Parameters.AddWithValue("@anio_de_lanzamiento", perfume.anio_de_lanzamiento);
                cmd.Parameters.AddWithValue("@precio_en_pesos", perfume.precio_en_pesos);
                cmd.Parameters.AddWithValue("@activo", perfume.activo);

                
                cmd.Parameters.AddWithValue("@imagen1", (object)perfume.imagen1);
                cmd.Parameters.AddWithValue("@imagen2", (object)perfume.imagen2);
                cmd.Parameters.AddWithValue("@imagen1_URL", (object)perfume.imagen1_URL ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@imagen2_URL", (object)perfume.imagen2_URL ?? DBNull.Value);

                if (actualizarFechaBaja)
                {
                    // Esta columna admite NULL:
                    var pFecha = new SqlParameter("@fecha_baja", System.Data.SqlDbType.DateTime);
                    pFecha.Value = (perfume.activo == true) ? (object)DBNull.Value : DateTime.Now;
                    cmd.Parameters.Add(pFecha);

                }

                try
                {
                    DB_Controller.connection.Open();
                    cmd.ExecuteNonQuery();
                    result = true;
                }
                catch (Exception e)
                {
                    throw new Exception("Hay un error en la query: " + e.Message);
                }
                finally
                {
                    DB_Controller.connection.Close();
                }
            }

            return result;
        }




        /* public static bool delete(int id)
         {
             bool result = false;
             string query = "update dbo.perfume set activo = 0 where id = @id;";
             SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);
             cmd.Parameters.AddWithValue("@id", id);
             try
             {
                 DB_Controller.connection.Open();
                 cmd.ExecuteNonQuery();
                 DB_Controller.connection.Close();
                 result = true;
             }
             catch (Exception e)
             {
                 throw new Exception("Hay un error en la query: " + e.Message);
             }
             return result;
         }*/


        public static bool delete(int id)
        {
            bool result = false;

            try
            {
                DB_Controller.connection.Open();
                SqlTransaction transaction = DB_Controller.connection.BeginTransaction();

                // 1. Marcar como inactivo y registrar fecha de baja
                string queryUpdate = "UPDATE dbo.perfume SET activo = 0, fecha_baja = @fecha_baja WHERE id = @id;";
                SqlCommand cmdUpdate = new SqlCommand(queryUpdate, DB_Controller.connection, transaction);
                cmdUpdate.Parameters.AddWithValue("@id", id);
                cmdUpdate.Parameters.AddWithValue("@fecha_baja", DateTime.Now); // ✅ agrega fecha actual
                cmdUpdate.ExecuteNonQuery();

                // 2. Eliminar relaciones con promo excepto la ID = 1
                PerfumeEnPromoControlador.EliminarRelacionesPromoExceptoSinPromo(id, transaction);

                // 3. Confirmar transacción
                transaction.Commit();
                result = true;
            }
            catch (Exception e)
            {
                try { DB_Controller.connection?.Close(); } catch { }
                throw new Exception("Error al eliminar el perfume: " + e.Message);
            }
            finally
            {
                if (DB_Controller.connection.State == System.Data.ConnectionState.Open)
                    DB_Controller.connection.Close();
            }

            return result;
        }




        /* public static List<Perfume> filtrarPorNombre(string nombre)
         {
             List<Perfume> list = new List<Perfume>();

             Marca marca = new Marca();
             TipoDePerfume tipo = new TipoDePerfume();
             Genero genero = new Genero();
             Pais pais = new Pais();


             if (nombre == null)
             {
                 list = PerfumeControlador.getAll();
             }
             else
             {

                 string query = "select * from dbo.Perfume where nombre like @nombre;";


                 SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);
                 cmd.Parameters.AddWithValue("@nombre", "%" + nombre + "%");


                 try
                 {
                     DB_Controller.connection.Open();
                     SqlDataReader r = cmd.ExecuteReader();

                     while (r.Read())
                     {

                         marca.id = r.GetInt32(2);
                         tipo.id = r.GetInt32(4);
                         genero.id = r.GetInt32(5);
                         pais.id = r.GetInt32(7);

                         Marca marcaOb = new Marca(marca.id, "");
                         TipoDePerfume tipo_de_perfumeOb = new TipoDePerfume(tipo.id, "");
                         Genero generoOb = new Genero(genero.id, "");
                         Pais paisOb = new Pais(pais.id, "");


                         list.Add(new Perfume(r.GetInt32(0), r.GetString(1), marcaOb, r.GetString(3),
                             tipo_de_perfumeOb, generoOb, r.GetInt32(6), paisOb,
                             r.GetBoolean(8), r.GetBoolean(9), r.GetString(10), r.GetInt32(11), r.GetDouble(12),
                             r.GetBoolean(13), r.GetString(14), r.GetString(15)));


                     }
                     r.Close();
                     DB_Controller.connection.Close();


                 }
                 catch (Exception e)
                 {
                     throw new Exception("Hay un error en la query: " + e.Message);
                 }


             }
             return list;
         }*/

        public static List<Perfume> getByNombre(string nombre)
        {
            List<Perfume> list = new List<Perfume>();

            string query = @"
                                SELECT p.*, tp.tipo_de_perfume, g.genero
                                FROM dbo.Perfume p
                                JOIN dbo.tipo_de_perfume tp ON p.tipo_de_perfume_id = tp.id
                                JOIN dbo.genero g ON p.genero_id = g.id
                                WHERE p.nombre = @nombre;
                            ";

            using (SqlConnection connection = new SqlConnection(DB_Controller.GetConnectionString()))
            {
                connection.Open();

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@nombre", nombre);

                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            try
                            {
                                // 👇 Log por consola de todos los campos
                                for (int i = 0; i < r.FieldCount; i++)
                                {
                                    string columnName = r.GetName(i);
                                    bool isNull = r.IsDBNull(i);
                                    string value = isNull ? "NULL" : r.GetValue(i).ToString();
                                    Console.WriteLine($"[{i}] {columnName} = {value}");
                                }

                                int marcaId = r.IsDBNull(2) ? 0 : r.GetInt32(2);
                                int paisId = r.IsDBNull(7) ? 0 : r.GetInt32(7);

                                Marca marcaOb = MarcaControlador.getById(marcaId);
                                Pais paisOb = PaisControlador.getById(paisId);

                                TipoDePerfume tipoOb = new TipoDePerfume(
                                    r.GetInt32(4),
                                    r.IsDBNull(r.GetOrdinal("tipo_de_perfume")) ? null : r.GetString(r.GetOrdinal("tipo_de_perfume"))
                                );

                                Genero generoOb = new Genero(
                                    r.GetInt32(5),
                                    r.IsDBNull(r.GetOrdinal("genero")) ? null : r.GetString(r.GetOrdinal("genero"))
                                );

                                DateTime? fechaBaja = r.IsDBNull(16) ? (DateTime?)null : r.GetDateTime(16);

                                list.Add(new Perfume(
                                    r.GetInt32(0),                             // id
                                    r.GetString(1),                            // codigo
                                    marcaOb,                                   // marca
                                    r.GetString(3),                            // nombre
                                    tipoOb,                                    // tipo de perfume
                                    generoOb,                                  // genero
                                    r.GetInt32(6),                             // presentacion_ml
                                    paisOb,                                    // pais
                                    r.GetBoolean(8),                           // spray
                                    r.GetBoolean(9),                           // recargable
                                    r.IsDBNull(10) ? null : r.GetString(10),   // descripcion
                                    r.GetInt32(11),                            // año
                                    r.GetDouble(12),                           // precio
                                    r.IsDBNull(13) ? (bool?)null : r.GetBoolean(13), // activo
                                    r.IsDBNull(14) ? null : r.GetString(14),   // imagen1
                                    r.IsDBNull(15) ? null : r.GetString(15),   // imagen2
                                    fechaBaja,                                  // fecha_baja
                                    r.IsDBNull(17) ? null : r.GetString(17),   //URL de imagen 1
                                    r.IsDBNull(18) ? null : r.GetString(18)    //URL de imagen 2

                                ));
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("❌ ERROR al construir el objeto Perfume:");
                                Console.WriteLine(ex.ToString());
                            }
                        }
                    }
                }
            }

            return list;
        }

        public static Perfume getByCodigo(string codigo)
        {
            Perfume perfume = null; // Inicialmente null
            string query = "select * from dbo.perfume where codigo = @codigo;";
            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);
            cmd.Parameters.AddWithValue("@codigo", codigo);
            try
            {
                DB_Controller.connection.Open();
                SqlDataReader r = cmd.ExecuteReader();
                if (r.Read())
                {
                    perfume = new Perfume();

                    Marca marca = new Marca(r.GetInt32(2), null);
                    TipoDePerfume tipo_de_perfume = new TipoDePerfume(r.GetInt32(4), null);
                    Genero genero = new Genero(r.GetInt32(5), null);
                    Pais pais = new Pais(r.GetInt32(7), null);
                    perfume.id = r.GetInt32(0);
                    perfume.codigo = r.GetString(1);
                    perfume.marca = marca;
                    perfume.nombre = r.GetString(3);
                    perfume.tipo_de_perfume = tipo_de_perfume;
                    perfume.genero = genero;
                    perfume.presentacion_ml = r.GetInt32(6);
                    perfume.pais = pais;
                    perfume.spray = r.GetBoolean(8);
                    perfume.recargable = r.GetBoolean(9);
                    perfume.descripcion = r.GetString(10);
                    perfume.anio_de_lanzamiento = r.GetInt32(11);
                    perfume.precio_en_pesos = r.GetDouble(12);
                    perfume.activo = r.GetBoolean(13);
                    perfume.imagen1 = r.GetString(14);
                    perfume.imagen2 = r.GetString(15);
                }
                r.Close();

            }
            catch (Exception e)
            {

                throw new Exception("Hay un error en la query: " + e.Message);
            }
            finally
            {
                DB_Controller.connection.Close();
            }
            return perfume;
        }


        public static List<Perfume> getPerfumesSimilares(Perfume perfume)
        {
            List<Perfume> perfumesSimilares = new List<Perfume>();

            string query = @"
                    SELECT TOP 10
                        p.id, 
                        p.codigo,
                        p.marca_id, 
                        p.nombre,  
                        p.tipo_de_perfume_id, 
                        p.genero_id, 
                        p.presentacion_ml, 
                        p.pais_id, 
                        p.spray, 
                        p.recargable, 
                        p.descripcion,
                        p.anio_de_lanzamiento, 
                        p.precio_en_pesos, 
                        p.activo, 
                        p.imagen1, 
                        p.imagen2,
                        p.fecha_baja,
                        p.imagen1_URL,
                        p.imagen2_URL,

                        COUNT(DISTINCT CASE 
                            WHEN ntn.tipo_de_nota_id IN (2, 3) THEN np.nota_con_tipo_de_nota_id 
                            ELSE NULL 
                        END) AS notas_comunes,

                        COUNT(DISTINCT pta.tipo_de_aroma_id) AS aromas_comunes,
                        SUM(s.cantidad) AS total_stock
                    FROM dbo.perfume p
                    JOIN dbo.notas_del_perfume np ON p.id = np.perfume_id
                    JOIN dbo.nota_con_tipo_de_nota ntn ON np.nota_con_tipo_de_nota_id = ntn.id
                    LEFT JOIN dbo.aroma_del_perfume pta ON p.id = pta.perfume_id
                    JOIN dbo.Stock s ON p.id = s.perfume_id
                    WHERE np.nota_con_tipo_de_nota_id IN (
                        SELECT nota_con_tipo_de_nota_id
                        FROM dbo.notas_del_perfume
                        WHERE perfume_id = @idPerfume
                    )
                    AND pta.tipo_de_aroma_id IN (
                        SELECT tipo_de_aroma_id
                        FROM dbo.aroma_del_perfume
                        WHERE perfume_id = @idPerfume
                    )
                    AND p.id <> @idPerfume
                    AND p.nombre <> @nombrePerfume -- ❌ excluye perfumes con el mismo nombre
                    AND p.activo = 1
                    AND s.cantidad > 0
                    GROUP BY p.id, p.codigo, p.marca_id, p.nombre, p.tipo_de_perfume_id, 
                             p.genero_id, p.presentacion_ml, p.pais_id, p.spray, p.recargable, 
                             p.descripcion, p.anio_de_lanzamiento, p.precio_en_pesos, p.activo, 
                             p.imagen1, p.imagen2, p.fecha_baja, p.imagen1_URL, p.imagen2_URL
                    ORDER BY notas_comunes DESC, aromas_comunes DESC;";



            SqlCommand cmd = new SqlCommand(query, DB_Controller.connection);
            cmd.Parameters.AddWithValue("@idPerfume", perfume.id);
            cmd.Parameters.AddWithValue("@nombrePerfume", perfume.nombre);

            try
            {
                DB_Controller.connection.Open();
                SqlDataReader r = cmd.ExecuteReader();

                while (r.Read())
                {
                    Marca marca = new Marca(r.GetInt32(2), null);
                    TipoDePerfume tipoDePerfume = new TipoDePerfume(r.GetInt32(4), null);
                    Genero genero = new Genero(r.GetInt32(5), null);
                    Pais pais = new Pais(r.GetInt32(7), null);


                    perfumesSimilares.Add(new Perfume(
                        r.GetInt32(0),
                        r.GetString(1),
                        marca,
                        r.GetString(3),
                        tipoDePerfume,
                        genero,
                        r.GetInt32(6),
                        pais,
                        r.GetBoolean(8),
                        r.GetBoolean(9),
                        r.GetString(10),
                        r.GetInt32(11),
                        r.GetDouble(12),
                        r.GetBoolean(13),
                        r.GetString(14),
                        r.GetString(15),
                        r.IsDBNull(16) ? (DateTime?)null : r.GetDateTime(16),
                        r.GetString(17),
                        r.GetString(18)

                    ));

                }

                r.Close();
                DB_Controller.connection.Close();
            }
            catch (Exception e)
            {
                throw new Exception("Error en la consulta de perfumes similares: " + e.Message);
            }

            return perfumesSimilares;
        }

        public static void marcarComoActivo(int idPerfume)
        {
            string query = "UPDATE dbo.perfume SET activo = 1 WHERE id = @id";

            using (SqlConnection conn = new SqlConnection(DB_Controller.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", idPerfume);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static DataTable ListarPerfumesBajoStock(int idSucursal)
        {
            Dictionary<int, int> stocks = StockControlador.ObtenerTodosLosStocksPorSucursal(idSucursal);

            string query = @"
                                SELECT 
                                    p.id,
                                    p.codigo,
                                    p.nombre AS nombre_perfume,
                                    p.presentacion_ml,
                                    p.precio_en_pesos,
                                    p.activo,
                                    p.fecha_baja,
                                    m.nombre AS marca,
                                    tp.tipo_de_perfume AS tipo,
                                    g.genero AS genero
                                FROM perfume p
                                JOIN marca m ON p.marca_id = m.id
                                JOIN tipo_de_perfume tp ON p.tipo_de_perfume_id = tp.id
                                JOIN genero g ON p.genero_id = g.id
                            ";

            DataTable resultTable = new DataTable();
            resultTable.Columns.Add("codigo");
            resultTable.Columns.Add("marca");
            resultTable.Columns.Add("nombre_perfume");
            resultTable.Columns.Add("tipo");
            resultTable.Columns.Add("genero");
            resultTable.Columns.Add("presentacion_ml");
            resultTable.Columns.Add("precio_en_pesos");
            resultTable.Columns.Add("stock");

            using (SqlConnection conn = new SqlConnection(DB_Controller.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int perfumeId = reader.GetInt32(reader.GetOrdinal("id"));
                        int stock = stocks.ContainsKey(perfumeId) ? stocks[perfumeId] : 0;
                        bool activo = reader.GetBoolean(reader.GetOrdinal("activo"));

                        DateTime? fechaBaja = null;
                        int ordFechaBaja = reader.GetOrdinal("fecha_baja");
                        if (!reader.IsDBNull(ordFechaBaja))
                        {
                            fechaBaja = reader.GetDateTime(ordFechaBaja);
                        }

                        // Reglas de inclusión
                        bool incluir =
                            (stock > 0 && stock <= 5)
                            || (stock == 0 && (
                                activo
                                || (!activo && fechaBaja.HasValue && fechaBaja.Value >= DateTime.Now.AddDays(-30))
                            ));

                        if (incluir)
                        {
                            resultTable.Rows.Add(
                                reader["codigo"].ToString(),
                                reader["marca"].ToString(),
                                reader["nombre_perfume"].ToString(),
                                reader["tipo"].ToString(),
                                reader["genero"].ToString(),
                                $"{reader["presentacion_ml"]} ml",
                                Convert.ToDecimal(reader["precio_en_pesos"]).ToString("C"),
                                stock
                            );
                        }
                    }
                }
            }

            return resultTable;
        }


        public static void RegistrarFechaDeBaja(int idPerfume)
        {
            string query = @"
        UPDATE perfume
        SET activo = 0,
            fecha_baja = GETDATE()
        WHERE id = @idPerfume;
    ";

            using (SqlConnection conn = new SqlConnection(DB_Controller.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idPerfume", idPerfume);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public static void LimpiarFechaDeBaja(int idPerfume)
        {
            string query = @"
        UPDATE perfume
        SET activo = 1,
            fecha_baja = NULL
        WHERE id = @idPerfume;
    ";

            using (SqlConnection conn = new SqlConnection(DB_Controller.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@idPerfume", idPerfume);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }






    }
}
