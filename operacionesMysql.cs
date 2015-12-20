using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MySql.Data.MySqlClient;

namespace opBbbd
{
    /// <summary>
    /// Clase para la manipulación de bases de datos de MySQL
    /// </summary>
    public class operacionesMysql
    {
        /// <summary>
        /// Objeto de conexión MySql
        /// </summary>
        private MySqlConnection conn;
        /// <summary>
        /// Comando a ejecutar por MySql
        /// </summary>
        private MySqlCommand cmd;
        /// <summary>
        /// Resultados de una consulta SELECT
        /// </summary>
        private MySqlDataReader rs;
        /// <summary>
        /// Nombre de las columnas del resultado de un SELECT
        /// </summary>
        public List<String> nombreColumnas = new List<string>();
        /// <summary>
        /// Tabla que contendrá el resultado de un SELECT
        /// </summary>
        private DataTable dt = new DataTable();
        /// <summary>
        /// Bit que especifica si la conexión será permanente
        /// </summary>
        private Boolean permanente = false;
        /// <summary>
        /// Obtiene el último mensaje de error generado por la base de datos
        /// </summary>
        public String getMensajeError { get { return exMsg; } }
        /// <summary>
        /// Obtiene/Establece el último mensaje de error de la base de datos
        /// </summary>
        private String exMsg = "Sin Errores";
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
        /// Constructor de la clase operacionesMysql con conexión no permanente
        /// </summary>
        /// <param name="connectionString">String de conexión a la base de datos -> server=ip;userid=user;password=pwd;database=db</param>
        public operacionesMysql(String connectionString)
        {
            conn = new MySqlConnection(connectionString);
        }
        /// <summary>
        /// Segundo constructor de la clase con opción de hacer una conexión permanente a la base de datos
        /// </summary>
        /// <param name="connectionString">String de conexión a la base de datos -> server=ip;userid=user;password=pwd;database=db</param>
        /// <param name="permanente">Especifica si la conexión es permanente</param>
        public operacionesMysql(String connectionString, Boolean permanente)
        {
            conn = new MySqlConnection(connectionString);
            if (permanente)
            {
                this.permanente = true;
                this.abrirConexion();
            }
        }
        /// <summary>
        /// Ejecuta un select contra la base de datos y devuelve un dataTable
        /// </summary>
        /// <param name="query">Consulta SQL a ejecutar</param>
        /// <returns>Número de filas del resultado. -1 si error</returns>
        public Int32 ejecutarSelect(String query)
        {
            Int32 numResultados = 0;
            try
            {
                this.exMsg = "Sin Errores";
                dt = new DataTable();
                this.abrirConexion();
                cmd = new MySqlCommand(query, conn);
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
                return rs.FieldCount;
            }
            catch (Exception ex)
            {
                exMsg = ex.Message;
                Console.WriteLine(ex.Message);
                return -1;
            }
            finally
            {
                if (!permanente) cerrarConexion();
            }
        }
        /// <summary>
        /// Ejecuta una consulta distinta a SELECT. Almacena en ultimoIdInsertado el último id de registro afectado.
        /// </summary>
        /// <param name="query">Consulta a ejecutar</param>
        /// <returns></returns>
        public Int32 ejecutarNoSelect(String query)
        {
            try
            {
                this.exMsg = "Sin Errores";
                Int32 filasAfectadas = 0;
                this.abrirConexion();
                cmd = new MySqlCommand(query, conn);
                filasAfectadas = cmd.ExecuteNonQuery();
                _lastInsertId = cmd.LastInsertedId;
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
            String sql = "UPDATE " + table + " SET " + camposvalores + " WHERE " + idFieldName + " = '" + idUpdatingRow + "' LIMIT 1";
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
            String sql = "UPDATE " + table + " SET " + camposvalores + " WHERE " + idFieldName + " = '" + row[idFieldName].ToString() + "' LIMIT 1";
            return this.ejecutarNoSelect(sql);
        }
        /// <summary>
        /// Abre la conexión con la base de datos
        /// </summary>
        private void abrirConexion()
        {
            try
            {
                if (conn == null || conn.State != ConnectionState.Open)
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
        /// Obtiene una matriz de Strings con los resultados de la consulta
        /// </summary>
        /// <returns></returns>
        private String[] obtenerFila()
        {
            String[] fila = new String[rs.FieldCount];
            for (int i = 0; i < rs.FieldCount; i++ )
            {
                fila[i] = rs.GetValue(i).ToString();
            }
            return fila;
        }
        /// <summary>
        /// Destructor de la clase que cierra la conexión con la base de datos
        /// </summary>
        ~operacionesMysql()
        {
            this.cerrarConexion();
        }

    }
}
