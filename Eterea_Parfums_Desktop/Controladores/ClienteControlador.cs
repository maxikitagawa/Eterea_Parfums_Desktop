using Eterea_Parfums_Desktop.Modelos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;

namespace Eterea_Parfums_Desktop.Controladores
{
    internal class ClienteControlador
    {

        //GET ONE


        //POST

        public static bool crearCliente(Cliente cliente)
        {
            //Dar de alta un cliente en la base de datos

            string query = "insert into dbo.cliente (id,usuario,clave,nombre,apellido,dni,condicion_frente_al_iva,fecha_nacimiento,celular,e_mail,pais_id,provincia_id,localidad_id,codigo_postal,calle_id,numeracion_calle,piso,departamento,comentarios_domicilio,activo,rol) values" +
                "(@id, " +
                "@usuario, " +
                "@clave, " +
                "@nombre, " +
                "@apellido, " +
                "@dni, " +
                "@condicion_frente_al_iva, " +
                "@fecha_nacimiento, " +
                "@celular, " +
                "@e_mail, " +
                "@pais_id, " +
                "@provincia_id, " +
                "@localidad_id, " +
                "@codigo_postal, " +
                "@calle_id, " +
                "@numeracion_calle, " +
                "@piso, " +
                "@departamento, " +
                "@comentarios_domicilio, " +

                "@activo, " +
                "@rol);";

            using (SqlConnection conn = new SqlConnection(DB_Controller.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", obtenerMaxId() + 1);
                cmd.Parameters.AddWithValue("@usuario", cliente.usuario);
                cmd.Parameters.AddWithValue("@clave", cliente.clave);
                cmd.Parameters.AddWithValue("@nombre", cliente.nombre);
                cmd.Parameters.AddWithValue("@apellido", cliente.apellido);
                cmd.Parameters.AddWithValue("@dni", cliente.dni);
                cmd.Parameters.AddWithValue("@condicion_frente_al_iva", cliente.condicion_frente_al_iva);
                cmd.Parameters.Add("@fecha_nacimiento", System.Data.SqlDbType.DateTime).Value = cliente.fecha_nacimiento ?? (object)DBNull.Value;
                //cmd.Parameters.AddWithValue("@fecha_nacimiento", cliente.fecha_nacimiento);
                cmd.Parameters.AddWithValue("@celular", cliente.celular);
                cmd.Parameters.AddWithValue("@e_mail", cliente.e_mail);
                cmd.Parameters.AddWithValue("@pais_id", cliente.pais_id.id);
                cmd.Parameters.AddWithValue("@provincia_id", cliente.provincia_id.id);
                cmd.Parameters.AddWithValue("@localidad_id", cliente.localidad_id.id);
                cmd.Parameters.Add("@codigo_postal", System.Data.SqlDbType.Int).Value = cliente.codigo_postal ?? (object)DBNull.Value;
                //cmd.Parameters.AddWithValue("@codigo_postal", cliente.codigo_postal);
                cmd.Parameters.AddWithValue("@calle_id", cliente.calle_id.id);
                cmd.Parameters.Add("@numeracion_calle", System.Data.SqlDbType.Int).Value = cliente.numeracion_calle ?? (object)DBNull.Value;
                //cmd.Parameters.AddWithValue("@numeracion_calle", cliente.numeracion_calle);
                cmd.Parameters.Add("@piso", System.Data.SqlDbType.VarChar).Value = cliente.piso ?? (object)DBNull.Value;
                //cmd.Parameters.AddWithValue("@piso", cliente.piso);
                cmd.Parameters.Add("@departamento", System.Data.SqlDbType.VarChar).Value = cliente.departamento ?? (object)DBNull.Value;
                //cmd.Parameters.AddWithValue("@departamento", cliente.departamento);
                cmd.Parameters.Add("@comentarios_domicilio", System.Data.SqlDbType.VarChar).Value = cliente.comentarios_domicilio ?? (object)DBNull.Value;
                //cmd.Parameters.AddWithValue("@comentarios_domicilio", cliente.comentarios_domicilio);

                cmd.Parameters.AddWithValue("@activo", cliente.activo);
                cmd.Parameters.AddWithValue("@rol", cliente.rol);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception("Hay un error en la query: " + e.Message);
                }
            }
        }

        // OBTENER EL MAX ID

        public static int obtenerMaxId()
        {
            int MaxId = 0;
            string query = "select max(id) from dbo.cliente;";

            using (SqlConnection conn = new SqlConnection(DB_Controller.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))

            { 
                try
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    MaxId = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
                catch (Exception e)
                {
                    throw new Exception("Hay un error en la query: " + e.Message);
                }
            }

            return MaxId;
        }

        // GET ALL
        public static List<Cliente> obtenerTodos()
        {
            List<Cliente> list = new List<Cliente>();

            string query = @"
        SELECT
            c.id, c.usuario, c.clave, c.nombre, c.apellido, c.dni,
            c.condicion_frente_al_iva, c.fecha_nacimiento, c.celular, c.e_mail,
            c.pais_id, c.provincia_id, c.localidad_id, c.codigo_postal, c.calle_id,
            c.numeracion_calle, c.piso, c.departamento, c.comentarios_domicilio,
            c.activo, c.rol,

            -- datos de las tablas relacionadas
            p.nombre  AS pais_nombre,
            pr.nombre AS provincia_nombre,
            l.nombre  AS localidad_nombre,
            ca.nombre AS calle_nombre

        FROM dbo.cliente c
        LEFT JOIN dbo.pais p        ON p.id  = c.pais_id
        LEFT JOIN dbo.provincia pr  ON pr.id = c.provincia_id
        LEFT JOIN dbo.localidad l   ON l.id  = c.localidad_id
        LEFT JOIN dbo.calle ca      ON ca.id = c.calle_id
        ORDER BY c.apellido, c.nombre;";

            using (SqlConnection conexion = new SqlConnection(DB_Controller.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conexion))
            {
                try
                {
                    conexion.Open();

                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        // índices “base” de cliente
                        int ordId = r.GetOrdinal("id");
                        int ordUsuario = r.GetOrdinal("usuario");
                        int ordNombre = r.GetOrdinal("nombre");
                        int ordApellido = r.GetOrdinal("apellido");
                        int ordDni = r.GetOrdinal("dni");
                        int ordCondIva = r.GetOrdinal("condicion_frente_al_iva");
                        int ordFechaNac = r.GetOrdinal("fecha_nacimiento");
                        int ordCel = r.GetOrdinal("celular");
                        int ordEmail = r.GetOrdinal("e_mail");
                        int ordPaisId = r.GetOrdinal("pais_id");
                        int ordProvId = r.GetOrdinal("provincia_id");
                        int ordLocId = r.GetOrdinal("localidad_id");
                        int ordCodPostal = r.GetOrdinal("codigo_postal");
                        int ordCalleId = r.GetOrdinal("calle_id");
                        int ordNumCalle = r.GetOrdinal("numeracion_calle");
                        int ordPiso = r.GetOrdinal("piso");
                        int ordDepto = r.GetOrdinal("departamento");
                        int ordComent = r.GetOrdinal("comentarios_domicilio");
                        int ordActivo = r.GetOrdinal("activo");
                        int ordRol = r.GetOrdinal("rol");

                        // índices “nombres” join
                        int ordPaisNombre = r.GetOrdinal("pais_nombre");
                        int ordProvNombre = r.GetOrdinal("provincia_nombre");
                        int ordLocNombre = r.GetOrdinal("localidad_nombre");
                        int ordCalleNombre = r.GetOrdinal("calle_nombre");

                        while (r.Read())
                        {
                            // ✅ construir objetos relacionados sin getById
                            var pais = new Pais(
                                r.IsDBNull(ordPaisId) ? 0 : r.GetInt32(ordPaisId),
                                r.IsDBNull(ordPaisNombre) ? "" : r.GetString(ordPaisNombre)
                            );

                            var provincia = new Provincia(
                                r.IsDBNull(ordProvId) ? 0 : r.GetInt32(ordProvId),
                                r.IsDBNull(ordProvNombre) ? "" : r.GetString(ordProvNombre)
                            );

                            var localidad = new Localidad(
                                r.IsDBNull(ordLocId) ? 0 : r.GetInt32(ordLocId),
                                r.IsDBNull(ordLocNombre) ? "" : r.GetString(ordLocNombre)
                            );

                            var calle = new Calle(
                                r.IsDBNull(ordCalleId) ? 0 : r.GetInt32(ordCalleId),
                                r.IsDBNull(ordCalleNombre) ? "" : r.GetString(ordCalleNombre)
                            );

                            list.Add(new Cliente(
                                r.GetInt32(ordId),
                                r.GetString(ordUsuario),
                                "", // clave no la cargás por seguridad (como ya hacías) :contentReference[oaicite:1]{index=1}
                                r.GetString(ordNombre),
                                r.GetString(ordApellido),
                                r.GetInt64(ordDni),
                                r.GetString(ordCondIva),
                                r.IsDBNull(ordFechaNac) ? default(DateTime) : r.GetDateTime(ordFechaNac),
                                r.GetString(ordCel),
                                r.GetString(ordEmail),
                                pais,
                                provincia,
                                localidad,
                                r.IsDBNull(ordCodPostal) ? 0 : r.GetInt32(ordCodPostal),
                                calle,
                                r.IsDBNull(ordNumCalle) ? 0 : r.GetInt32(ordNumCalle),
                                r.IsDBNull(ordPiso) ? "" : r.GetString(ordPiso),
                                r.IsDBNull(ordDepto) ? "" : r.GetString(ordDepto),
                                r.IsDBNull(ordComent) ? "" : r.GetString(ordComent),
                                r.GetBoolean(ordActivo),
                                r.GetString(ordRol)
                            ));
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Hay un error en la query: " + e.Message);
                }
            }

            return list;
        }

        // GET ONE BY ID
        public static Cliente obtenerPorId(int id)
        {
            Cliente cliente = new Cliente();

            Pais pais = new Pais();
            Provincia provincia = new Provincia();
            Localidad localidad = new Localidad();
            Calle calle = new Calle();

            string query = "select * from dbo.cliente where id = @id;";

            using (SqlConnection conn = new SqlConnection(DB_Controller.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);

                try
                {
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())

                        while (r.Read())
                        {
                            // Verifica si los valores son DBNull antes de acceder
                            pais.id = r.IsDBNull(10) ? 0 : r.GetInt32(10); // Si es DBNull, asigna 0
                            provincia.id = r.IsDBNull(11) ? 0 : r.GetInt32(11); // Si es DBNull, asigna 0
                            localidad.id = r.IsDBNull(12) ? 0 : r.GetInt32(12); // Si es DBNull, asigna 0
                            calle.id = r.IsDBNull(14) ? 0 : r.GetInt32(14); // Si es DBNull, asigna 0

                            cliente = new Cliente(
                                r.GetInt32(0),
                                r.GetString(1),
                                "",
                                r.GetString(3),
                                r.GetString(4),
                                r.GetInt64(5),
                                r.GetString(6),
                                r.IsDBNull(7) ? default(DateTime) : r.GetDateTime(7), // Si es DBNull, asigna default(DateTime)
                                r.GetString(8),
                                r.GetString(9),
                                pais,
                                provincia,
                                localidad,
                                r.IsDBNull(13) ? 0 : r.GetInt32(13), // Si es DBNull, asigna 0
                                calle,
                                r.IsDBNull(15) ? 0 : r.GetInt32(15), // Si es DBNull, asigna 0
                                r.IsDBNull(16) ? "" : r.GetString(16), // Si es DBNull, asigna ""
                                r.IsDBNull(17) ? "" : r.GetString(17), // Si es DBNull, asigna ""
                                r.IsDBNull(18) ? "" : r.GetString(18), // Si es DBNull, asigna ""
                                r.GetBoolean(19),
                                r.GetString(20)
                            );
                        }


                    // Asignación de los IDs de pais, provincia, localidad y calle
                    cliente.pais_id = PaisControlador.getById(pais.id);
                    cliente.provincia_id = ProvinciaControlador.getById(provincia.id);
                    cliente.localidad_id = LocalidadControlador.getById(localidad.id);
                    cliente.calle_id = CalleControlador.getById(calle.id);

                }
                catch (Exception e)
                {
                    throw new Exception("Hay un error en la query: " + e.Message);
                }
            }
            return cliente;
        }


        //GET ONE BY DNI

        public static Cliente obtenerPorDni(long dni)
        {
            Cliente cliente = null;

            Pais pais = new Pais();
            Provincia provincia = new Provincia();
            Localidad localidad = new Localidad();
            Calle calle = new Calle();

            string query = "select * from dbo.cliente where dni = @dni;";

            using (SqlConnection conn = new SqlConnection(DB_Controller.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@dni", dni);

                try
                {
                    conn.Open();
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {

                        while (r.Read())
                        {
                            pais.id = r.GetInt32(10);
                            provincia.id = r.GetInt32(11);
                            localidad.id = r.GetInt32(12);
                            calle.id = r.GetInt32(14);

                            cliente = new Cliente(
                                r.GetInt32(0),                                  // ID
                                r.GetString(1),                                 // Usuario
                                r.GetString(2),                                 // Clave
                                r.GetString(3),                                 // Nombre
                                r.GetString(4),                                 // Apellido
                                r.GetInt64(5),                                  // DNI o CUIT
                                r.GetString(6),                                 // Condición frente al IVA
                                r.GetDateTime(7),                               // Fecha de nacimiento
                                r.GetString(8),                                 // Celular
                                r.GetString(9),                                 // E-mail
                                pais,                                           // País
                                provincia,                                      // Provincia
                                localidad,                                      // Localidad
                                r.IsDBNull(13) ? 0 : r.GetInt32(13),            // Código postal
                                calle,                                          // Calle
                                r.IsDBNull(15) ? 0 : r.GetInt32(15),            // Numeración de calle
                                r.IsDBNull(16) ? "" : r.GetString(16),          // Piso
                                r.IsDBNull(17) ? "" : r.GetString(17),          // Departamento
                                r.IsDBNull(18) ? "" : r.GetString(18),          // Comentarios domicilio
                                r.GetBoolean(19),                                 // Activo (corregido)
                                r.GetString(20)                                 // Rol
                            );

                            /* cliente = new Cliente(r.GetInt32(0), r.GetString(1), "", r.GetString(3), r.GetString(4),
                                 r.GetInt32(5), r.GetString(6), r.GetDateTime(7), r.GetString(8), r.GetString(9), pais,
                                 provincia, localidad, r.GetInt32(13), calle, r.GetInt32(15),
                                 r.GetString(16), r.GetString(17), r.GetString(18),
                                  r.GetInt32(19), r.GetString(20)); */
                        }
                    }


                    if (cliente != null)
                    {
                        cliente.pais_id = PaisControlador.getById(pais.id);
                        cliente.provincia_id = ProvinciaControlador.getById(provincia.id);
                        cliente.localidad_id = LocalidadControlador.getById(localidad.id);
                        cliente.calle_id = CalleControlador.getById(calle.id);
                    }

                }
                catch (Exception e)
                {
                    throw new Exception("Hay un error en la query: " + e.Message);
                }
            }
            return cliente;

        }


        //EDIT ó PUT

        public static bool editarCliente(Cliente cliente)
        {


            string query = "update dbo.cliente set usuario = @usuario, " +
                "nombre = @nombre, " +
                "apellido = @apellido, " +
                "dni = @dni, " +
                "condicion_frente_al_iva = @condicion_frente_al_iva, " +
                "fecha_nacimiento = @fecha_nacimiento, " +
                "celular = @celular, " +
                "e_mail = @e_mail, " +
                "pais_id = @pais_id, " +
                "provincia_id = @provincia_id, " +
                "localidad_id = @localidad_id, " +
                "codigo_postal = @codigo_postal, " +
                "calle_id = @calle_id, " +
                "numeracion_calle = @numeracion_calle, " +
                "piso = @piso, " +
                "departamento = @departamento, " +
                "comentarios_domicilio = @comentarios_domicilio, " +

                "activo = @activo, " +
                "rol = @rol " +
                "where id = @id;";


            using (SqlConnection conn = new SqlConnection(DB_Controller.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", cliente.id);
                cmd.Parameters.AddWithValue("@usuario", cliente.usuario);
                //cmd.Parameters.AddWithValue("@clave", vendedor.contraseña);  //hc.PassHash(vendedor.contraseña)
                cmd.Parameters.AddWithValue("@nombre", cliente.nombre);
                cmd.Parameters.AddWithValue("@apellido", cliente.apellido);
                cmd.Parameters.AddWithValue("@dni", cliente.dni);
                cmd.Parameters.AddWithValue("@condicion_frente_al_iva", cliente.condicion_frente_al_iva);
                cmd.Parameters.Add("@fecha_nacimiento", System.Data.SqlDbType.DateTime).Value = cliente.fecha_nacimiento ?? (object)DBNull.Value;
                //cmd.Parameters.AddWithValue("@fecha_nacimiento", cliente.fecha_nacimiento);
                cmd.Parameters.AddWithValue("@celular", cliente.celular);
                cmd.Parameters.AddWithValue("@e_mail", cliente.e_mail);
                cmd.Parameters.AddWithValue("@pais_id", cliente.pais_id.id);
                cmd.Parameters.AddWithValue("@provincia_id", cliente.provincia_id.id);
                cmd.Parameters.AddWithValue("@localidad_id", cliente.localidad_id.id);
                cmd.Parameters.Add("@codigo_postal", System.Data.SqlDbType.Int).Value = cliente.codigo_postal ?? (object)DBNull.Value;
                //cmd.Parameters.AddWithValue("@codigo_postal", cliente.codigo_postal);
                cmd.Parameters.AddWithValue("@calle_id", cliente.calle_id.id);
                cmd.Parameters.Add("@numeracion_calle", System.Data.SqlDbType.Int).Value = cliente.numeracion_calle ?? (object)DBNull.Value;
                //cmd.Parameters.AddWithValue("@numeracion_calle", cliente.numeracion_calle);
                cmd.Parameters.Add("@piso", System.Data.SqlDbType.VarChar).Value = cliente.piso ?? (object)DBNull.Value;
                //cmd.Parameters.AddWithValue("@piso", cliente.piso);
                cmd.Parameters.Add("@departamento", System.Data.SqlDbType.VarChar).Value = cliente.departamento ?? (object)DBNull.Value;
                //cmd.Parameters.AddWithValue("@departamento", cliente.departamento);
                cmd.Parameters.Add("@comentarios_domicilio", System.Data.SqlDbType.VarChar).Value = cliente.comentarios_domicilio ?? (object)DBNull.Value;
                //cmd.Parameters.AddWithValue("@comentarios_domicilio", cliente.comentarios_domicilio);

                //cmd.Parameters.AddWithValue("@fecha_nacimiento", cliente.fecha_nacimiento);
                //cmd.Parameters.AddWithValue("@celular", cliente.celular);
                //cmd.Parameters.AddWithValue("@e_mail", cliente.e_mail);
                //cmd.Parameters.AddWithValue("@pais_id", cliente.pais_id.id);
                //cmd.Parameters.AddWithValue("@provincia_id", cliente.provincia_id.id);
                //cmd.Parameters.AddWithValue("@localidad_id", cliente.localidad_id.id);
                //cmd.Parameters.AddWithValue("@codigo_postal", cliente.codigo_postal);
                //cmd.Parameters.AddWithValue("@calle_id", cliente.calle_id.id);
                //cmd.Parameters.AddWithValue("@numeracion_calle", cliente.numeracion_calle);
                //cmd.Parameters.AddWithValue("@piso", cliente.piso);
                //cmd.Parameters.AddWithValue("@departamento", cliente.departamento);
                //cmd.Parameters.AddWithValue("@comentarios_domicilio", cliente.comentarios_domicilio);

                cmd.Parameters.AddWithValue("@activo", cliente.activo);
                cmd.Parameters.AddWithValue("@rol", cliente.rol);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }

                catch (Exception e)
                {
                    throw new Exception("Hay un error en la query: " + e.Message);
                }
            }

        }

        public static bool eliminarCliente(int id)
        {
            int activo = 0;
            //Dar de baja lógica a un cliente en la base de datos
            string query = "update dbo.cliente set activo = @activo " + "where id = @id;";

            using (SqlConnection conn = new SqlConnection(DB_Controller.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@activo", activo);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception e)
                {
                    throw new Exception("Hay un error en la query: " + e.Message);
                }
            }
        }

        /*internal static List<Cliente> obtenerPorDni(string nombre)
        {
            throw new NotImplementedException();
        }*/

        public static int? BuscarIdPorDni(string dni)
        {
            int? idCliente = null;

            string query = "SELECT id FROM cliente WHERE dni = @dni";

            using (SqlConnection conn = new SqlConnection(DB_Controller.GetConnectionString()))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                // Intentamos convertir el string a int
                if (!long.TryParse(dni, out long dniNumerico))
                {
                    throw new Exception("El DNI proporcionado no es numérico.");
                }

                cmd.Parameters.AddWithValue("@dni", dniNumerico);

                try
                {
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            idCliente = reader.GetInt32(0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al buscar el empleado por DNI: " + ex.Message);
                }
               
            }

            return idCliente; // null si no se encontró
        }

        public static bool ExisteUsuario(string usuario)
        {
            string query = "SELECT COUNT(*) FROM dbo.cliente WHERE usuario = @usuario";

            using (SqlConnection connection = new SqlConnection(DB_Controller.GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@usuario", usuario);

                connection.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public static bool ExisteEmail(string email)
        {
            string query = "SELECT COUNT(*) FROM dbo.cliente WHERE e_mail = @email";

            using (SqlConnection connection = new SqlConnection(DB_Controller.GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@email", email);

                connection.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public static bool ExisteUsuarioEnOtroCliente(string usuario, int idClienteActual)
        {
            string query = "SELECT COUNT(*) FROM dbo.cliente WHERE usuario = @usuario AND id <> @id";

            using (SqlConnection connection = new SqlConnection(DB_Controller.GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@usuario", usuario);
                cmd.Parameters.AddWithValue("@id", idClienteActual);

                connection.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public static bool ExisteEmailEnOtroCliente(string email, int idClienteActual)
        {
            string query = "SELECT COUNT(*) FROM dbo.cliente WHERE e_mail = @email AND id <> @id";

            using (SqlConnection connection = new SqlConnection(DB_Controller.GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@id", idClienteActual);

                connection.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        public static bool ExisteDniEnOtroCliente(string dni, int idClienteActual)
        {
            string query = "SELECT COUNT(*) FROM cliente WHERE dni = @dni AND id <> @id";

            using (SqlConnection connection = new SqlConnection(DB_Controller.GetConnectionString()))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@dni", dni);
                cmd.Parameters.AddWithValue("@id", idClienteActual);

                connection.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }



    }
}
