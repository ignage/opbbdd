using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
namespace opBbbd
{
    /// <summary>
    /// Clase controladora para realizar operaciones de lectura/escritra en bases de datos de SqlServer y MySql. Estandariza la recogida de datos mediante DataTables.
    /// </summary>
    public class operacionesBbdd
    {
        /// <summary>
        /// 
        /// </summary>
        private operacionesMysql opMy = null;
        /// <summary>
        /// 
        /// </summary>
        private operacionesSqlserver opMs = null;
        /// <summary>
        /// 
        /// </summary>
        protected operacionesSqlEvolution opEvo = null;
        /// <summary>
        /// DataTable que contiene los resultados de una operación de lectura a la base de datos
        /// </summary>
        public DataTable resultados { get { if (opMy != null) return opMy.resultados; else if (opMs != null) return opMs.resultados; else if (opEvo != null) return opEvo.resultados; else return null; } }
        /// <summary>
        /// Cadena que contiene la descripción de los posibles errores a la hora de realizar consultas por el usuario
        /// </summary>
        public String getMensajeError { get { if (opMy != null) return opMy.getMensajeError; else if (opMs != null) return opMs.getMensajeError; else if (opEvo != null) return opEvo.getMensajeError; else return "-1"; } }
        /// <summary>
        /// Lista que contiene los nombres de las columnas de la tabla al realizar una operación de lectura de la base de datos
        /// </summary>
        public List<String> nombreColumnas { get { if (opMy != null) return opMy.nombreColumnas; else if (opMs != null) return opMs.nombreColumnas; else if (opEvo != null) return opEvo.nombreColumnas; else return null; } }
        /// <summary>
        /// Constructor predeterminado vacío. No realiza operación alguna
        /// </summary>
        protected operacionesBbdd() { }
        /// <summary>
        /// Constructor para operaciones con bases de datos de MySql de Oracle
        /// </summary>
        /// <param name="conexion">Cadena de conexión proporcionada por la clase cadenasConexionesMysql</param>
        /// <param name="permanente">Indica si l aconexión con el servidor MySql se quedará abierta (true) o se cerrará al finalizar la operación (método) solicitado (false)</param>
        public operacionesBbdd(cadenasConexionesMySql conexion, bool permanente)
        {
                opMy = new operacionesMysql(conexion.ToString(), permanente);
        }
        /// <summary>
        /// Constructor para operaciones con bases de datos de Sql Server de Microsft
        /// </summary>
        /// <param name="conexion">Cadena de conexión proporcionada por la clase cadenasConexionesSqlServer</param>
        /// <param name="permanente">Indica si l aconexión con el servidor MySql se quedará abierta (true) o se cerrará al finalizar la operación (método) solicitado (false)</param>
        public operacionesBbdd(cadenasConexionesSqlServer conexion, bool permanente)
        {
                opMs = new operacionesSqlserver(conexion.ToString(), permanente);
        }
        /// <summary>
        /// Constructor para operaciones con bases de datos de Sql Server de Microsft
        /// </summary>
        /// <param name="conexion">Cadena de conexión proporcionada por la clase cadenasConexionesSqlServer</param>
        /// <param name="cmdTimeout">Tiempo en el que expirará la ejecución de laconsulta</param>
        /// <param name="permanente">Indica si l aconexión con el servidor MySql se quedará abierta (true) o se cerrará al finalizar la operación (método) solicitado (false)</param>
        public operacionesBbdd(cadenasConexionesSqlServer conexion, Int32 cmdTimeout , bool permanente)
        {
            opMs = new operacionesSqlserver(conexion.ToString(), cmdTimeout, permanente);
        }
        /// <summary>
        /// Contructor para operaciones con bases de datos SQLServer con la cadena de conexión y el command TimeOut
        /// </summary>
        /// <param name="cadenaConexion">Cadena de conexión para el servidor</param>
        /// <param name="cmdTimeout">Tiempo en el que expirará la ejecución de la consulta</param>
        /// <param name="permanente">Indica si l aconexión con el servidor MySql se quedará abierta (true) o se cerrará al finalizar la operación (método) solicitado (false)</param>
        public operacionesBbdd(String cadenaConexion, Int32 cmdTimeout, bool permanente)
        {
            opMs = new operacionesSqlserver(cadenaConexion, cmdTimeout, permanente);
        }
        /// <summary>
        /// Contructor para operaciones con bases de datos con la cadena de conexión y el tipo de servidor parametrizados
        /// </summary>
        /// <param name="cadenaConexion">Cadena de conexión para el servidor</param>
        /// <param name="tipo">Tipo del servidor de base de datos</param>
        /// <param name="permanente">Indica si l aconexión con el servidor MySql se quedará abierta (true) o se cerrará al finalizar la operación (método) solicitado (false)</param>
        public operacionesBbdd(String cadenaConexion, tipoBaseDeDatos tipo, bool permanente)
        {
            if (tipo == tipoBaseDeDatos.MySql) opMy = new operacionesMysql(cadenaConexion, permanente);
            else if (tipo == tipoBaseDeDatos.SqlServer) opMs = new operacionesSqlserver(cadenaConexion, permanente);
        }
        /// <summary>
        /// Método para ejecutar operaciones de lectura de consultas tipo SELECT. Exporta los resultados al atributo DataTable resultados de la clase
        /// </summary>
        /// <param name="query">Cadena con la consulta a realizar a la base de datos</param>
        public Int32 ejecutarSelect(String query)
        {
            if (opMy != null) return opMy.ejecutarSelect(query);
            else if (opMs != null) return opMs.ejecutarSelect(query);
            else if (opEvo != null) return opEvo.ejecutarSelect(query);
            else return -1;
        }
        /// <summary>
        /// Ejecuta operaciones de escritura (INSERT, UPDATE, DELETE, CREATE,...) en la base de datos. Devuelve el número de filas afectadas
        /// </summary>
        /// <param name="query">Cadena con la consulta de escritura a ejecutar en la base de datos</param>
        /// <returns>Entero con el número de filas a fectadas por la consulta ejecutada</returns>
        public Int32 ejecutarNoSelect(String query)
        {
            if (opMy != null) return opMy.ejecutarNoSelect(query);
            else if (opMs != null) return opMs.ejecutarNoSelect(query);
            else if (opEvo != null) return opEvo.ejecutarNoSelect(query);
            else return -1;
        }
        /// <summary>
        /// Inserta el DataRow pasado por parámetros en la tabla especificada. Las cabeceras de las columnas del DataRow han de coincidir con las de la tabla.
        /// </summary>
        /// <param name="row">DataRow que se insertará en la tabla de la base de datos.</param>
        /// <param name="table">Nombre de la tabla en la que se realizará la insercción.</param>
        /// <returns>Devuelve 1 en caso de insercción correcta o -1 en caso de error</returns>
        public Int32 dataRow2SQL(DataRow row, String table)
        {
            if (opMy != null) return opMy.DataRow2SQL(row, table);
            else if (opMs != null) return opMs.DataRow2SQL(row, table);
            else if (opEvo != null) return opEvo.DataRow2SQL(row, table);
            else return -1;
        }
        /// <summary>
        /// Actualiza la tabla de la base de datos con la información contenida en el DataRow de la fila con id indicado 
        /// </summary>
        /// <param name="row">DataRow con la información a actualizar</param>
        /// <param name="table">Tabla de la base de datos que se desea actualizar</param>
        /// <param name="idUpdatingRow">ID de la fila que se desea actualizar</param>
        /// <param name="idFieldName">Nombre del campo con la id sobre la que se realizará el UPDATE</param>
        /// <returns>Devuelve 1 en caso de insercción correcta o -1 en caso de error</returns>
        public Int32 dataRow2SQL(DataRow row, String table, String idUpdatingRow, String idFieldName)
        {
            if (opMy != null) return opMy.DataRow2SQL(row, table, idUpdatingRow, idFieldName);
            else if (opMs != null) return opMs.DataRow2SQL(row, table, idUpdatingRow, idFieldName);
            else if (opEvo != null) return opEvo.DataRow2SQL(row, table, idUpdatingRow, idFieldName);
            else return -1;
        }
        /// <summary>
        /// Actualiza la tabla de la base de datos con la información contenida en el DataRow. Esta fila contendrá el id. 
        /// </summary>
        /// <param name="row">DataRow con la información a actualizar</param>
        /// <param name="table">Tabla de la base de datos que se desea actualizar</param>
        /// <param name="idFieldName">Nombre del campo con la id sobre la que se realizará el UPDATE</param>
        /// <returns>Devuelve 1 en caso de insercción correcta o -1 en caso de error</returns>
        public Int32 dataRow2SQL(DataRow row, String table, String idFieldName)
        {
            if (opMy != null) return opMy.DataRow2SQL(row, table, idFieldName);
            else if (opMs != null) return opMs.DataRow2SQL(row, table, idFieldName);
            else if (opEvo != null) return opEvo.DataRow2SQL(row, table, idFieldName);
            else return -1;
        }
        /// <summary>
        /// Devuelve el último id afectado al realizar un NoSelect
        /// </summary>
        /// <returns></returns>
        public long ultimoIdAfectado()
        {
            if (opMy != null) return opMy.ultimoIdAfectado;
            else if (opMs != null) return opMs.ultimoIdAfectado;
            else if (opEvo != null) return opEvo.ultimoIdAfectado;
            else return -1;
        }
        /// <summary>
        /// Fuerza el cierre de la conexión con la base de datos
        /// </summary>
        public void cerrarConexion()
        {
            if (opMy != null) opMy.cerrarConexion();
            else if (opMs != null) opMs.cerrarConexion();
            else if (opEvo != null) opEvo.cerrarConexion();
        }
        
    }
    /// <summary>
    /// Clase para operaciones en la base de datos de Evolution. Hereda las operaciones con las bases de datos.
    /// </summary>
    public class operacionesEvolution : operacionesBbdd
    {
        /// <summary>
        /// Constructor para las operaciones con EvolutionDB
        /// </summary>
        /// <param name="conexion">Cadena de conexión a la base de datos Sql Server de Evolution</param>
        /// <param name="permanente">Indica si la aconexión con el servidor Sql Server de Evolution se quedará abierta (true) o se cerrará al finalizar la operación (método) solicitado (false)</param>
        public operacionesEvolution(cadenasConexionesSqlServer conexion, bool permanente)
        {
            opEvo = new operacionesSqlEvolution(conexion.ToString(), permanente);           
        }
        /// <summary>
        /// Constructor para las operaciones con EvolutionDB con commandTimeOut
        /// </summary>
        /// <param name="conexion">Cadena de conexión a la base de datos Sql Server de Evolution</param>
        /// <param name="cmdTimeout">Segundos a los que expirará la ejecución de una consulta</param>
        /// <param name="permanente">Indica si la aconexión con el servidor Sql Server de Evolution se quedará abierta (true) o se cerrará al finalizar la operación (método) solicitado (false)</param>
        public operacionesEvolution(cadenasConexionesSqlServer conexion, Int32 cmdTimeout, bool permanente)
        {
            opEvo = new operacionesSqlEvolution(conexion.ToString(), cmdTimeout, permanente);
        }
        /// <summary>
        /// Carga un nuevo cliente en una campaña de la base de datos de evolution con todos los parámetros especificados
        /// </summary>
        /// <param name="idCampanya">Id de la campaña en la que se cargará el cliente</param>
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
        /// <returns>Devuelve 1 si se ha insertado correctamente el cliente y -1 en caso de error</returns>
        public Int32 cargaClienteEvoParametrica(String idCampanya, String idOriginal, String nombre, String apellido1, String apellido2,
           String direccion, String poblacion, String codigoPostal, String provincia, String pais, String idioma, String email1, String email2,
           String movil, String movil2, String segmento, String telefono, String telefono2, String observaciones, String fax, String llamarDesde, String llamarHasta)
        {
            return opEvo.cargaClienteEvoParametrica(idCampanya, idOriginal, nombre, apellido1, apellido2,
                    direccion, poblacion, codigoPostal, provincia, pais, idioma, email1, email2, movil, movil2, segmento, telefono, telefono2, observaciones, fax, llamarDesde, llamarHasta);
        }
        /// <summary>
        /// Añade un nuevo Cliente en una campaña específica de Evolution
        /// </summary>
        /// <param name="idCampanya">Id de la campaña en la que se desea insertar el nuevo cliente</param>
        /// <param name="dr">DataRow que contiene la información necesaria para la insercción del cliente. Preparada para la carga en Cross y Renove.</param>
        /// <returns></returns>
        public Int32 cargaClienteEvo(String idCampanya, DataRow dr)
        {
            return opEvo.cargaClienteEvo(idCampanya, dr);
        }
        /// <summary>
        /// Método especial para cargas en Evolution del proyecto eON
        /// </summary>
        /// <param name="idCampanya"></param>
        /// <param name="dr">DataRow que contiene la información necesaria para la insercción del cliente. Preparada para la carga en eON</param>
        /// <returns>Devuelve 1 si se ha insertado correctamente el cliente y -1 en caso de error</returns>
        public Int32 cargaClienteEvoEon(String idCampanya, DataRow dr)
        {
            return opEvo.cargaClienteEvoEon(idCampanya, dr);
        }
        /// <summary>
        /// Carga un nuevo cliente en la campaña de Evolution con una mayor mapeo de campos
        /// </summary>
        /// <param name="idCampanya">Id de la campaña en la que se cargará el nuevo cliente.</param>
        /// <param name="dr">DataRow que contiene la información del cliente a cargar.</param>
        /// <returns></returns>
        public Int32 cargaCompletaClienteEvo(String idCampanya, DataRow dr)
        {
            return opEvo.cargaCompletaClienteEvo(idCampanya, dr);
        }
        /// <summary>
        /// Carga un nuevo cliente en la campaña de Evolution con un mayor mapeo de campos
        /// </summary>
        /// <param name="idCampanya">Id de la campaña en la que se cargará el nuevo cliente</param>
        /// <param name="dr">List de Strings con la información del nuevo cliente</param>
        /// <returns></returns>
        public Int32 cargaCompletaClienteEvo(String idCampanya, List<String> dr)
        {
            return opEvo.cargaCompletaClienteEvo(idCampanya, dr);
        }
        /// <summary>
        /// Importa un data table en una campaña de Evolution. Adaptado para cargas en Cross y Renove.
        /// </summary>
        /// <param name="idCampanya">Id de la campaña en la que se cargarán los registros</param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public Int32 cargaListadoEvo(String idCampanya, DataTable dt)
        {
            return opEvo.cargaListadoEvo(idCampanya, dt);
        }
        /// <summary>
        /// Limpia toda la información relacionada con una campaña concreta de Evolution
        /// </summary>
        /// <param name="idCampanya">Id de la campaña a limpiar</param>
        /// <returns>Devuelve True en caso de un limpiado correcto y False en caso de error.</returns>
        public Boolean limpiarCampanyaEvo(String idCampanya)
        {
            return opEvo.limpiarCampanyaEvo(idCampanya);
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
            return opEvo.altaUsuario(login, password, nombre, apellido1, apellido2, dni, loginACD, passwordACD, tipoAgente, idPuesto, idServicio, idSkill, valorSkill);
        }
    }


}
