using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
namespace opBbbd
{
    /// <summary>
    /// Tipos de usuarios disponibles en evolution
    /// </summary>
    public enum tipoUsuario
    {
        /// <summary>
        /// Usuario de tipo agente válido para logarse en iAgent
        /// </summary>
        agente = 0,
        /// <summary>
        /// Usuario de tipo comercial, ¡No válido para logarse en iAgent!
        /// </summary>
        comercial = 4,
        /// <summary>
        /// Usuario de tipo supervisor
        /// </summary>
        supervisor = 1,
        /// <summary>
        /// ¡Ojo! Superuser
        /// </summary>
        admin = 5
    }
    /// <summary>
    /// Clase para la manipulación de bases de datos de SQL Server
    /// </summary>
    public class operacionesSqlserver
    {
        /// <summary>
        /// Objeto de conexión SQL Server
        /// </summary>
        protected SqlConnection conn;
        /// <summary>
        /// Comando a ejecutar por SQL Server
        /// </summary>
        protected SqlCommand cmd;
        /// <summary>
        /// Resultados de una consulta SELECT
        /// </summary>
        protected SqlDataReader rs;
        /// <summary>
        /// Nombre de las columnas del resultado de un SELECT
        /// </summary>
        public List<String> nombreColumnas = new List<string>();
        /// <summary>
        /// Tabla que contendrá el resultado de un SELECT
        /// </summary>
        protected DataTable dt = new DataTable();
        /// <summary>
        /// Bit que especifica si la conexión será permanente
        /// </summary>
        protected Boolean permanente = false;
        /// <summary>
        /// Tiempo máximo de ejecución de un comando
        /// </summary>
        protected Int32 cmdTimeout = 30;
        /// <summary>
        /// Obtiene el último mensaje de error generado por la base de datos
        /// </summary>
        public String getMensajeError { get { return exMsg; } }
        /// <summary>
        /// Obtiene/Establece el último mensaje de error de la base de datos
        /// </summary>
        protected String exMsg = "Sin Errores";
        /// <summary>
        /// Obtiene la tabla con los resultados de la última consulta SELECT
        /// </summary>
        public DataTable resultados {get{return dt;}}
        /// <summary>
        /// Obtiene/Establece el último ID afectado por una consulta
        /// </summary>
        private long _lastInsertId = 0;
        /// <summary>
        /// Obtiene el último ID afectado por una consulta
        /// </summary>
        public long ultimoIdAfectado { get { return _lastInsertId; } }
        /// <summary>
        /// Constructor vacío para la clase
        /// </summary>
        public operacionesSqlserver() { }
        /// <summary>
        /// Constructor que estable el objeto de conexión con la base de datos
        /// </summary>
        /// <param name="connectionString"></param>
        public operacionesSqlserver(String connectionString)
        {
            conn = new SqlConnection(connectionString);
        }
        /// <summary>
        /// Constructor que estable el objeto de conexión con la base de datos y el bit de conexión permanente
        /// </summary>
        /// <param name="connectionString">Cadena de conexión con la base de datos</param>
        /// <param name="permanente">Bit que especifica si la conexión se cerrará después de cada consulta o permanecerá abierta hasta que se especifique</param>
        public operacionesSqlserver(String connectionString, Boolean permanente)
        {
            conn = new SqlConnection(connectionString);
            if (permanente)
            {
                this.permanente = true;
                this.abrirConexion();
            }
        }
        /// <summary>
        /// Constructor que estable el objeto de conexión con la base de datos, el bit de conexión permanente y el timeOut de los comandos
        /// </summary>
        /// <param name="connectionString">adena de conexión con la base de datos</param>
        /// <param name="cmdTimeout">Tiempo máximo de espera en segundos para realizar una consula en la base de datos </param>
        /// <param name="permanente">Bit que especifica si la conexión se cerrará después de cada consulta o permanecerá abierta hasta que se especifique</param>
        public operacionesSqlserver(String connectionString, Int32 cmdTimeout, Boolean permanente)
        {
            conn = new SqlConnection(connectionString);
            this.cmdTimeout = cmdTimeout;
            if (permanente)
            {
                this.permanente = true;
                this.abrirConexion();
            }
        }
        /// <summary>
        /// Ejecuta la consulta indicada y rellena el DataTable con los resultados
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public Int32 ejecutarSelect(String query)
        {
            Int32 numResultados = 0;
            dt = new DataTable();
            try
            {
                this.exMsg = "Sin Errores";
                this.abrirConexion();
                cmd = new SqlCommand(query, conn);
                cmd.CommandTimeout = cmdTimeout;
                rs = cmd.ExecuteReader();
                if (rs.Read())
                {
                    for (int i = 0; i < rs.FieldCount; i++)
                    {
                        DataColumn dc = new DataColumn(rs.GetName(i));
                        dt.Columns.Add(dc);
                    }
                    dt.Rows.Add(obtenerFila());
                }
                while (rs.Read())
                {
                    dt.Rows.Add(obtenerFila());
                }
                numResultados = rs.FieldCount;
                rs.Close();
                return numResultados;
            }
            catch (Exception ex)
            {
                exMsg = ex.Message;
                Console.WriteLine(ex.Message);
                return -1;
            }
            finally
            {
                if (!permanente) this.cerrarConexion();
            }
        }
        /// <summary>
        /// Ejecuta una consulta distinta a SELECT. Almacena en ultimoIdInsertado el último id de registro afectado.
        /// </summary>
        /// <param name="query">Consulta a ejecutar</param>
        /// <returns>Número de filas afectadas por la consulta</returns>
        public Int32 ejecutarNoSelect(String query)
        {
            try
            {
                Int32 filasAfectadas = 0;
                this.exMsg = "Sin Errores";
                this.abrirConexion();
                cmd = new SqlCommand(query, conn);
                cmd.CommandTimeout = cmdTimeout;
                filasAfectadas = cmd.ExecuteNonQuery();
                this.ejecutarSelect("SELECT @@IDENTITY");
                try
                {
                    this._lastInsertId = -1;
                    if (this.resultados.Rows[0][0].ToString() != "")
                        long.TryParse(this.resultados.Rows[0].ItemArray[0].ToString(), out _lastInsertId);                       
                }
                catch (Exception ex)
                {
                    this._lastInsertId = -2;
                    this.exMsg = ex.Message;
                }
                if (!permanente) conn.Close();
                return filasAfectadas;
            }
            catch (Exception ex)
            {
                exMsg = ex.Message;
                Console.WriteLine(ex.Message);
                return -1;
            }
        }
        /// <summary>
        /// Inserta el DataRow pasado por parámetros en la tabla especificada. Las cabeceras de las columnas del DataRow han de coincidir con las de la tabla.
        /// </summary>
        /// <param name="row">DataRow que se insertará en la tabla de la base de datos.</param>
        /// <param name="table">Nombre de la tabla en la que se realizará la insercción.</param>
        /// <returns>Devuelve 1 en caso de insercción correcta o -1 en caso de error</returns>
        public Int32 DataRow2SQL(DataRow row, String table)
        {
            String campos = "(";
            String valores = "(";
            foreach (DataColumn dc in row.Table.Columns)
                campos += " " + dc.ColumnName + ", ";
            foreach (object val in row.ItemArray)
                valores += " '" + val.ToString() + "', ";

            campos = campos.Substring(0, campos.Length - 2) + " ) ";
            valores = valores.Substring(0, valores.Length - 2) + " ) ";
            String sql = "INSERT INTO " + table + " " + campos + " VALUES " + valores;
            return this.ejecutarNoSelect(sql);
        }
        /// <summary>
        /// Actualiza la tabla de la base de datos con la información contenida en el DataRow de la fila con id indicado 
        /// </summary>
        /// <param name="row">DataRow con la información a actualizar</param>
        /// <param name="table">Tabla de la base de datos que se desea actualizar</param>
        /// <param name="idUpdatingRow">ID de la fila que se desea actualizar</param>
        /// <param name="idFieldName">Nombre del campo con la id sobre la que se realizará el UPDATE</param>
        /// <returns></returns>
        public Int32 DataRow2SQL(DataRow row, String table, String idUpdatingRow, String idFieldName)
        {
            String camposvalores = "";
            Int32 i = 0;
            foreach (DataColumn dc in row.Table.Columns)
            {
                camposvalores += " " + dc.ColumnName + " = '" + row[i].ToString() + "', ";
                i++;
            }
            camposvalores = camposvalores.Substring(0, camposvalores.Length - 2) + " ";
            String sql = "UPDATE TOP (1) " + table + " SET " + camposvalores + " WHERE " + idFieldName + " = '" + idUpdatingRow + "' ";
            return this.ejecutarNoSelect(sql);
        }
        /// <summary>
        /// Actualiza la tabla de la base de datos con la información contenida en el DataRow de la fila con id indicado 
        /// </summary>
        /// <param name="row">DataRow con la información a actualizar</param>
        /// <param name="table">Tabla de la base de datos que se desea actualizar</param>
        /// <param name="idFieldName">Nombre del campo con la id sobre la que se realizará el UPDATE</param>
        /// <returns></returns>
        public Int32 DataRow2SQL(DataRow row, String table, String idFieldName)
        {
            String camposvalores = "";
            Int32 i = 0;
            foreach (DataColumn dc in row.Table.Columns)
            {
                camposvalores += " " + dc.ColumnName + " = '" + row[i].ToString() + "', ";
                i++;
            }
            camposvalores = camposvalores.Substring(0, camposvalores.Length - 2) + " ";
            String sql = "UPDATE TOP (1) " + table + " SET " + camposvalores + " WHERE " + idFieldName + " = '" + row[idFieldName].ToString() + "'";
            return this.ejecutarNoSelect(sql);
        }
        /// <summary>
        /// Cierra la conexión con la base de datos
        /// </summary>
        public void cerrarConexion()
        {
            try
            {
                if (rs != null && !rs.IsClosed)
                    rs.Close();
                if(conn != null)
                    conn.Close();
            }
            catch (Exception ex)
            {
                exMsg = ex.Message;
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Abre la conexión con la base de datos
        /// </summary>
        protected void abrirConexion()
        {
            try
            {
                if (conn == null || conn.State!= ConnectionState.Open)
                    conn.Open();
                else
                    if (rs != null) rs.Close();
            }
            catch (Exception ex)
            {
                exMsg = ex.Message;
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Obtiene las filas del resultado de la consulta y devuelve una matriz de cadenas
        /// </summary>
        /// <returns></returns>
        protected String[] obtenerFila()
        {
            String[] fila = new String[rs.FieldCount];
            for (int i = 0; i < rs.FieldCount; i++ )
            {
                fila[i] = rs.GetValue(i).ToString();
            }
            return fila;
        }
        /// <summary>
        /// Destructor de la clase que fuerza el cierre de la conexión
        /// </summary>
        ~operacionesSqlserver()
        {
            this.cerrarConexion();
        }
    }
    /// <summary>
    /// Extensión de la clase anterior con métodos propios para la base de datos de Evolution
    /// </summary>
    public class operacionesSqlEvolution : operacionesSqlserver
    {
        /// <summary>
        /// Constructor vacío para la clase
        /// </summary>
        public operacionesSqlEvolution() { }
        /// <summary>
        /// Constructor que estable el objeto de conexión con la base de datos y el bit de conexión permanente
        /// </summary>
        /// <param name="connectionString">adena de conexión con la base de datos</param>
        /// <param name="permanente">Bit que especifica si la conexión se cerrará después de cada consulta o permanecerá abierta hasta que se especifique</param>
        public operacionesSqlEvolution(String connectionString, Boolean permanente)
        {
            base.conn = new SqlConnection(connectionString);
            if (base.permanente)
            {
                base.permanente = true;
                base.abrirConexion();
            }
        }
        /// <summary>
        /// Constructor que estable el objeto de conexión con la base de datos, el bit de conexión permanente y el timeOut de los comandos
        /// </summary>
        /// <param name="connectionString">adena de conexión con la base de datos</param>
        /// <param name="cmdTimeout">Tiempo máximo de espera en segundos para realizar una consula en la base de datos </param>
        /// <param name="permanente">Bit que especifica si la conexión se cerrará después de cada consulta o permanecerá abierta hasta que se especifique</param>
        public operacionesSqlEvolution(String connectionString, Int32 cmdTimeout, Boolean permanente)
        {
            conn = new SqlConnection(connectionString);
            this.cmdTimeout = cmdTimeout;
            if (permanente)
            {
                this.permanente = true;
                this.abrirConexion();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCampanya"></param>
        /// <param name="idOriginal"></param>
        /// <param name="nombre"></param>
        /// <param name="apellido1"></param>
        /// <param name="apellido2"></param>
        /// <param name="direccion"></param>
        /// <param name="poblacion"></param>
        /// <param name="codigoPostal"></param>
        /// <param name="provincia"></param>
        /// <param name="pais"></param>
        /// <param name="idioma"></param>
        /// <param name="email1"></param>
        /// <param name="email2"></param>
        /// <param name="movil"></param>
        /// <param name="movil2"></param>
        /// <param name="segmento"></param>
        /// <param name="telefono"></param>
        /// <param name="telefono2"></param>
        /// <param name="observaciones"></param>
        /// <param name="fax"></param>
        /// <param name="llamarDesde"></param>
        /// <param name="llamarHasta"></param>
        /// <returns></returns>
        public Int32 cargaClienteEvoParametrica(String idCampanya, String idOriginal, String nombre, String apellido1, String apellido2,
            String direccion, String poblacion, String codigoPostal, String provincia, String pais, String idioma, String email1, String email2,
            String movil, String movil2, String segmento, String telefono, String telefono2, String observaciones, String fax, String llamarDesde, String llamarHasta)
        {
            try
            {
                base.abrirConexion();
                SqlCommand cmd = new SqlCommand("sp_newSujeto", conn);
                if (codigoPostal == "") codigoPostal = "0";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@idCamp", idCampanya));
                cmd.Parameters.Add(new SqlParameter("@idOriginal", "teampCargaFibraWeb"));
                cmd.Parameters.Add(new SqlParameter("@sNombre", nombre));
                cmd.Parameters.Add(new SqlParameter("@sApellido1", apellido1 ));
                cmd.Parameters.Add(new SqlParameter("@sApellido2", apellido2 ));
                cmd.Parameters.Add(new SqlParameter("@sDireccion", direccion ));
                cmd.Parameters.Add(new SqlParameter("@sPoblacion", poblacion ));
                cmd.Parameters.Add(new SqlParameter("@nCodigo_Postal", Int32.Parse(codigoPostal)));
                cmd.Parameters.Add(new SqlParameter("@sProvincia", provincia ));
                cmd.Parameters.Add(new SqlParameter("@sPais", pais ));
                cmd.Parameters.Add(new SqlParameter("@nIdioma", DBNull.Value ));
                cmd.Parameters.Add(new SqlParameter("@Email", email1 ));
                cmd.Parameters.Add(new SqlParameter("@Email2", email2 ));
                cmd.Parameters.Add(new SqlParameter("@Movil", movil ));
                cmd.Parameters.Add(new SqlParameter("@Movil2", movil2 ));
                cmd.Parameters.Add(new SqlParameter("@Segmento", segmento ));
                cmd.Parameters.Add(new SqlParameter("@sTelefono", telefono ));
                cmd.Parameters.Add(new SqlParameter("@sFax", fax ));
                cmd.Parameters.Add(new SqlParameter("@nLlamarDesde", llamarDesde ));
                cmd.Parameters.Add(new SqlParameter("@nLlamarHasta", llamarHasta ));
                cmd.Parameters.Add(new SqlParameter("@IdSujeto", SqlDbType.Int) { Size = 10, Direction = ParameterDirection.Output });
                if (cmd.ExecuteNonQuery() > 0)
                {
                    String idSujetoEvo = cmd.Parameters["@IdSujeto"].Value.ToString();
                    if(idOriginal == "0" || idOriginal == "-1")
                        base.ejecutarNoSelect("UPDATE [EVOLUTIONDB].[dbo].[CLIENTES] SET [IDORIGINAL] = '" + idSujetoEvo +
                            "', [TELEFONO2] = '" + telefono2.ToString() + "', [OBSERVACIONES] = '" + observaciones + "' WHERE [IDSUJETO] = " + idSujetoEvo);
                    else
                        base.ejecutarNoSelect("UPDATE [EVOLUTIONDB].[dbo].[CLIENTES] SET [IDORIGINAL] = '" + idOriginal +
                            "', [TELEFONO2] = '" + telefono2.ToString() + "', [OBSERVACIONES] = '" + observaciones + "' WHERE [IDSUJETO] = " + idSujetoEvo);
                    return Int32.Parse(idSujetoEvo);
                }
                else
                    return -1;
            }
            catch (Exception ex)
            {
                exMsg = ex.Message;
                Console.WriteLine(ex.Message);
                return -1;
            }
            finally
            {
                if (!permanente) base.cerrarConexion();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCampanya"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        public Int32 cargaClienteEvo(String idCampanya, DataRow dr)
        {
            Int32 filasAfectadas = 0;
            try
            {
                String date = DateTime.Now.ToString("yyyy-MM-dd");
                String sql;

                base.abrirConexion();
                SqlCommand cmd = new SqlCommand("sp_newSujeto", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@idCamp", idCampanya));
                cmd.Parameters.Add(new SqlParameter("@idOriginal", dr.ItemArray[0]));
                cmd.Parameters.Add(new SqlParameter("@sNombre", dr.ItemArray[2]));
                cmd.Parameters.Add(new SqlParameter("@sApellido1", dr.ItemArray[0]));
                cmd.Parameters.Add(new SqlParameter("@sApellido2", dr.ItemArray[1]));
                cmd.Parameters.Add(new SqlParameter("@sDireccion", dr.ItemArray[5]));
                cmd.Parameters.Add(new SqlParameter("@sPoblacion", dr.ItemArray[6]));
                cmd.Parameters.Add(new SqlParameter("@nCodigo_Postal", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@sProvincia", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@sPais", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@nIdioma", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Email", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Email2", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Movil", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Movil2", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Segmento", dr.ItemArray[6]));
                cmd.Parameters.Add(new SqlParameter("@sTelefono", dr.ItemArray[3]));
                cmd.Parameters.Add(new SqlParameter("@sFax", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@nLlamarDesde", "0000"));
                cmd.Parameters.Add(new SqlParameter("@nLlamarHasta", 2400));
                cmd.Parameters.Add(new SqlParameter("@IdSujeto", SqlDbType.Int) { Size = 10, Direction = ParameterDirection.Output });
                if (cmd.ExecuteNonQuery() > 0)
                {
                    filasAfectadas++;
                    String idSujetoEvo = cmd.Parameters["@IdSujeto"].Value.ToString();
                    sql = "UPDATE [EVOLUTIONDB].[dbo].[CLIENTES] SET [IDORIGINAL] = '" + dr.ItemArray[0] +
                        "', [TELEFONO2] = '" + dr.ItemArray[4] + "' WHERE [IDSUJETO] = " + idSujetoEvo;
                    cmd = new SqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
                //if (!permanente) this.cerrarConexion();
                return filasAfectadas;
            }
            catch (Exception ex)
            {
                exMsg = ex.Message;
                Console.WriteLine(ex.Message);
                return -1;
            }
            finally
            {
                if (!permanente) base.cerrarConexion();
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCampanya"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        public Int32 cargaClienteEvoEon(String idCampanya, DataRow dr)
        {
            Int32 filasAfectadas = 0;
            try
            {
                String date = DateTime.Now.ToString("yyyy-MM-dd");
                String sql;
                base.abrirConexion();
                SqlCommand cmd = new SqlCommand("sp_newSujeto", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@idCamp", idCampanya));
                cmd.Parameters.Add(new SqlParameter("@idOriginal", dr.ItemArray[0]));
                cmd.Parameters.Add(new SqlParameter("@sNombre", dr.ItemArray[2]));
                cmd.Parameters.Add(new SqlParameter("@sApellido1", dr.ItemArray[3]));
                cmd.Parameters.Add(new SqlParameter("@sApellido2", dr.ItemArray[4]));
                cmd.Parameters.Add(new SqlParameter("@sDireccion", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@sPoblacion", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@nCodigo_Postal", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@sProvincia", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@sPais", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@nIdioma", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Email", dr.ItemArray[7]));
                cmd.Parameters.Add(new SqlParameter("@Email2", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Movil", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Movil2", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Segmento", "Eon"));
                cmd.Parameters.Add(new SqlParameter("@sTelefono", dr.ItemArray[5]));
                cmd.Parameters.Add(new SqlParameter("@sFax", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@nLlamarDesde", "0000"));
                cmd.Parameters.Add(new SqlParameter("@nLlamarHasta", 2400));
                cmd.Parameters.Add(new SqlParameter("@IdSujeto", SqlDbType.Int) { Size = 10, Direction = ParameterDirection.Output });
                if (cmd.ExecuteNonQuery() > 0)
                {
                    filasAfectadas++;
                    String idSujetoEvo = cmd.Parameters["@IdSujeto"].Value.ToString();
                    sql = "UPDATE [EVOLUTIONDB].[dbo].[CLIENTES] SET [IDORIGINAL] = '" + dr.ItemArray[0] +
                        "', [TELEFONO2] = '" + dr.ItemArray[6] + "' WHERE [IDSUJETO] = " + idSujetoEvo;
                    cmd = new SqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                    operacionesBbdd op = new operacionesBbdd(cadenasConexionesSqlServer.connEon, false);
                    op.ejecutarNoSelect("UPDATE cliente SET idSujetoEvo = " + idSujetoEvo + " WHERE ID_CLIENTE = " + dr.ItemArray[1]);
                }
                //if (!permanente) this.cerrarConexion();
                return filasAfectadas;
            }
            catch (Exception ex)
            {
                exMsg = ex.Message;
                Console.WriteLine(ex.Message);
                return -1;
            }
            finally
            {
                if (!permanente) base.cerrarConexion();
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCampanya"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        public Int32 cargaCompletaClienteEvo(String idCampanya, DataRow dr)
        {
            Int32 filasAfectadas = 0;
            try
            {
                String date = DateTime.Now.ToString("yyyy-MM-dd");
                String sql;
                base.abrirConexion();
                SqlCommand cmd = new SqlCommand("sp_newSujeto", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@idCamp", idCampanya));
                cmd.Parameters.Add(new SqlParameter("@idOriginal", dr.ItemArray[0]));
                cmd.Parameters.Add(new SqlParameter("@sNombre", dr.ItemArray[4]));
                cmd.Parameters.Add(new SqlParameter("@sApellido1", dr.ItemArray[5]));
                cmd.Parameters.Add(new SqlParameter("@sApellido2", dr.ItemArray[6]));
                cmd.Parameters.Add(new SqlParameter("@sDireccion", dr.ItemArray[7]));
                cmd.Parameters.Add(new SqlParameter("@sPoblacion", dr.ItemArray[8]));
                cmd.Parameters.Add(new SqlParameter("@nCodigo_Postal", dr.ItemArray[9]));
                cmd.Parameters.Add(new SqlParameter("@sProvincia", dr.ItemArray[10]));
                cmd.Parameters.Add(new SqlParameter("@sPais", dr.ItemArray[11]));
                cmd.Parameters.Add(new SqlParameter("@nIdioma", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Email", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Email2", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Movil", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Movil2", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Segmento", dr.ItemArray[3]));
                cmd.Parameters.Add(new SqlParameter("@sTelefono", dr.ItemArray[1]));
                cmd.Parameters.Add(new SqlParameter("@sFax", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@nLlamarDesde", "0000"));
                cmd.Parameters.Add(new SqlParameter("@nLlamarHasta", 2400));
                cmd.Parameters.Add(new SqlParameter("@IdSujeto", SqlDbType.Int) { Size = 10, Direction = ParameterDirection.Output });
                if (cmd.ExecuteNonQuery() > 0)
                {
                    filasAfectadas++;
                    String idSujetoEvo = cmd.Parameters["@IdSujeto"].Value.ToString();
                    sql = "UPDATE [EVOLUTIONDB].[dbo].[CLIENTES] SET [IDORIGINAL] = '" + dr.ItemArray[0] +
                        "', [TELEFONO2] = '" + dr.ItemArray[2] + "', [OBSERVACIONES] = '" + dr.ItemArray[12] + "' WHERE [IDSUJETO] = " + idSujetoEvo;
                    cmd = new SqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
                //if (!permanente) this.cerrarConexion();
                return filasAfectadas;
            }
            catch (Exception ex)
            {
                exMsg = ex.Message;
                Console.WriteLine(ex.Message);
                return -1;
            }
            finally
            {
                if (!permanente) base.cerrarConexion();
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCampanya"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        public Int32 cargaCompletaClienteEvo(String idCampanya, List<String> dr)
        {
            Int32 filasAfectadas = 0;
            try
            {
                String date = DateTime.Now.ToString("yyyy-MM-dd");
                String sql;
                base.abrirConexion();
                SqlCommand cmd = new SqlCommand("sp_newSujeto", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@idCamp", idCampanya));
                cmd.Parameters.Add(new SqlParameter("@idOriginal", dr[0]));
                cmd.Parameters.Add(new SqlParameter("@sNombre", dr[4]));
                cmd.Parameters.Add(new SqlParameter("@sApellido1", dr[5]));
                cmd.Parameters.Add(new SqlParameter("@sApellido2", dr[6]));
                cmd.Parameters.Add(new SqlParameter("@sDireccion", dr[7]));
                cmd.Parameters.Add(new SqlParameter("@sPoblacion", dr[8]));
                cmd.Parameters.Add(new SqlParameter("@nCodigo_Postal", dr[9]));
                cmd.Parameters.Add(new SqlParameter("@sProvincia", dr[10]));
                cmd.Parameters.Add(new SqlParameter("@sPais", dr[11]));
                cmd.Parameters.Add(new SqlParameter("@nIdioma", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Email", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Email2", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Movil", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Movil2", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@Segmento", dr[3]));
                cmd.Parameters.Add(new SqlParameter("@sTelefono", dr[1]));
                cmd.Parameters.Add(new SqlParameter("@sFax", DBNull.Value));
                cmd.Parameters.Add(new SqlParameter("@nLlamarDesde", "0000"));
                cmd.Parameters.Add(new SqlParameter("@nLlamarHasta", 2400));
                cmd.Parameters.Add(new SqlParameter("@IdSujeto", SqlDbType.Int) { Size = 10, Direction = ParameterDirection.Output });
                if (cmd.ExecuteNonQuery() > 0)
                {
                    filasAfectadas++;
                    String idSujetoEvo = cmd.Parameters["@IdSujeto"].Value.ToString();
                    sql = "UPDATE [EVOLUTIONDB].[dbo].[CLIENTES] SET [IDORIGINAL] = '" + dr[0] +
                        "', [TELEFONO2] = '" + dr[2] + "', [OBSERVACIONES] = '" + dr[12] + "' WHERE [IDSUJETO] = " + idSujetoEvo;
                    cmd = new SqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
                //if (!permanente) this.cerrarConexion();
                return filasAfectadas;
            }
            catch (Exception ex)
            {
                exMsg = ex.Message;
                Console.WriteLine(ex.Message);
                return -1;
            }
            finally
            {
                if (!permanente) base.cerrarConexion();
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCampanya"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public Int32 cargaListadoEvo(String idCampanya, DataTable dt)
        {
            Int32 filasAfectadas = 0;
            try
            {
                foreach (DataRow dr in dt.Rows)
                {
                    filasAfectadas = filasAfectadas + this.cargaClienteEvo(idCampanya, dr);
                }
                return filasAfectadas;
            }
            catch (Exception ex)
            {
                exMsg = ex.Message;
                Console.WriteLine(ex.Message);
                return -1;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="idCampanya"></param>
        /// <returns></returns>
        public Boolean limpiarCampanyaEvo(String idCampanya)
        {
            try
            {
                base.abrirConexion();
                SqlCommand cmd = new SqlCommand("sp_LimpiarCampanya2", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@idCamp", idCampanya));
                cmd.Parameters.Add(new SqlParameter("@idDescarga", "0"));
                cmd.Parameters.Add(new SqlParameter("@bDump", "0"));
                cmd.Parameters.Add(new SqlParameter("@wStatus", SqlDbType.Int) { Size = 5, Direction = ParameterDirection.Output });
                if (cmd.ExecuteNonQuery() > 0)
                {
                    String wStatus = cmd.Parameters["@wStatus"].Value.ToString();
                    Console.WriteLine("Status Limpia: " + wStatus);
                }
                if (!permanente) cerrarConexion();
                return true;
            }
            catch (Exception ex)
            {
                exMsg = ex.Message;
                Console.WriteLine(ex.Message);
                if (!permanente) base.cerrarConexion();
                return false;
            }
        }
        /// <summary>
        /// Da de alta un nuevo usuario en Evolution 10
        /// </summary>
        /// <param name="login">Login del usuario</param>
        /// <param name="password">Password del usuario en texto claro</param>
        /// <param name="nombre"></param>
        /// <param name="apellido1"></param>
        /// <param name="apellido2"></param>
        /// <param name="dni"></param>
        /// <param name="loginACD">Puede ser vacío</param>
        /// <param name="passwordACD">Pueder ser vacío</param>
        /// <param name="tipoAgente">Nivel de privilegios del usuario</param>
        /// <param name="idPuesto">Puede ser 0</param>
        /// <param name="idServicio">ID del servicio de evolution predeterminado</param>
        /// <param name="idSkill">Skill predeterminado asignado</param>
        /// <param name="valorSkill">Valor de 1 a 100 del skill predeterminado</param>
        /// <returns>Devuelve true en caso de creación correcta del usuario</returns>
        public Boolean altaUsuario(String login, String password, String nombre, String apellido1, String apellido2, String dni, 
            String loginACD, String passwordACD, tipoUsuario tipoAgente, Int32 idPuesto, Int32 idServicio, Int32 idSkill, Int32 valorSkill)
        {
            Int32 idTipoAgente = (Int32)tipoAgente;
            byte[] encodedPassword = new UTF8Encoding().GetBytes(password);

            // need MD5 to calculate the hash
            byte[] hash = ((HashAlgorithm)CryptoConfig.CreateFromName("MD5")).ComputeHash(encodedPassword);

            // string representation (similar to UNIX format)
            string encoded = BitConverter.ToString(hash)
                // without dashes
               .Replace("-", string.Empty)
                // make lowercase
               .ToLower();
            encodedPassword = new UTF8Encoding().GetBytes(encoded);
            // encoded contains the hash you are wanting
            try
            {
                base.abrirConexion();
                SqlCommand cmd = new SqlCommand("sp_AltaAgenteExtendido", conn);

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@sNombre", nombre));
                cmd.Parameters.Add(new SqlParameter("@sApellido1", apellido1 ));
                cmd.Parameters.Add(new SqlParameter("@sApellido2", apellido2 ));
                cmd.Parameters.Add(new SqlParameter("@sDni", dni ));
                cmd.Parameters.Add(new SqlParameter("@sLogin", login ));
                cmd.Parameters.Add(new SqlParameter("@sLoginACD",  loginACD ));
                cmd.Parameters.Add(new SqlParameter("@sPasswACD",  passwordACD ));
                cmd.Parameters.Add(new SqlParameter("@iTipoAgente",  idTipoAgente));
                cmd.Parameters.Add(new SqlParameter("@idPuesto", idPuesto ));
                cmd.Parameters.Add(new SqlParameter("@Password", hash));
                cmd.Parameters.Add(new SqlParameter("@idServicio", idServicio));
                cmd.Parameters.Add(new SqlParameter("@idSkill", idSkill));
                cmd.Parameters.Add(new SqlParameter("@iValorSkill", valorSkill));
                cmd.Parameters.Add(new SqlParameter("@idAgente", SqlDbType.Int) { Size = 10, Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new SqlParameter("@lRet", SqlDbType.Int) { Size = 10, Direction = ParameterDirection.Output });
                if (cmd.ExecuteNonQuery() > 0)
                {
                    String idAgente = cmd.Parameters["@idAgente"].Value.ToString();
                    return true;
                }
                else
                    return false;
                
            }
            catch (Exception ex)
            {
                exMsg = ex.Message;
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                if (!permanente) base.cerrarConexion();
            }
        }
        
    }
}
