using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace opBbbd
{
    /// <summary>
    /// Proporciona las cadenas de conexión para MySQL
    /// </summary>
    public sealed class cadenasConexionesMySql
    {
        /// <summary>
        /// Cadena de conexión generada
        /// </summary>
        private readonly String cadena;
        /// <summary>
        /// ID de la cadena generada
        /// </summary>
        private readonly int value;
        /// <summary>
        /// Cadena de conexión de ejemplo. Puede copiar esta línea y contruirse sus cadenas de conexión personalizadas para sus aplicaciones
        /// </summary>
        public static readonly cadenasConexionesMySql connPersonalizada = new cadenasConexionesMySql(1, "server=<DIRECCION IP>;userid=<USUARIO>;password=<CONTRASEÑA>;database=<ESQUEMA PREDETERMINADO>;default command timeout=999");

        /// <summary>
        /// Constructor que permite realizar un pseudoenum
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        private cadenasConexionesMySql(int value, String name)
        {
            this.cadena = name;
            this.value = value;
        }
        /// <summary>
        /// Constructor para cadenas de conexión de MySql parametrizadas
        /// </summary>
        /// <param name="ip">IP del servidor MySql</param>
        /// <param name="user">Ususario para la conexión on el servidor</param>
        /// <param name="password">Contraseña del usuario en texto plano</param>
        /// <param name="defaultDatabase">Base de datos o esquema por defecto</param>
        /// <param name="commandTimeOut">Tiempo máximo de ejecución de una consulta</param>
        public cadenasConexionesMySql(String ip, String user, String password, String defaultDatabase, Int32 commandTimeOut)
        {
            this.cadena = "server=" + ip + ";userid=" + user + ";password=" + password + ";database=" + defaultDatabase + ";default command timeout=" + commandTimeOut.ToString();
            this.value = 0;
        }
        /// <summary>
        /// Constructor para cadenas de conexión de MySql parametrizadas
        /// </summary>
        /// <param name="ip">IP del servidor MySql</param>
        /// <param name="user">Ususario para la conexión on el servidor</param>
        /// <param name="password">Contraseña del usuario en texto plano</param>
        /// <param name="defaultDatabase">Base de datos o esquema por defecto</param>
        /// <param name="commandTimeOut">Tiempo máximo de ejecución de una consulta</param>
        /// <param name="timeoutConexion">TimeOut para la conexión con la base de datos</param>
        public cadenasConexionesMySql(String ip, String user, String password, String defaultDatabase, Int32 commandTimeOut, Int32 timeoutConexion)
        {
            this.cadena = "server=" + ip + ";userid=" + user + ";password=" + password + ";database=" + defaultDatabase + ";default command timeout=" + commandTimeOut.ToString() + ";Connect Timeout=" + timeoutConexion + ";";
            this.value = 0;
        }
        /// <summary>
        /// Constructor para cadenas de conexión de MySql parametrizadas
        /// </summary>
        /// <param name="ip">IP del servidor MySql</param>
        /// <param name="user">Ususario para la conexión on el servidor</param>
        /// <param name="password">Contraseña del usuario en texto plano</param>
        /// <param name="defaultDatabase">Base de datos o esquema por defecto</param>
        public cadenasConexionesMySql(String ip, String user, String password, String defaultDatabase)
        {
            this.cadena = "server=" + ip + ";userid=" + user + ";password=" + password + ";database=" + defaultDatabase + ";";
            this.value = 0;
        }
        /// <summary>
        /// Sobreescribe el método ToString para que devuelva la cadena de conexión necesaria
        /// </summary>
        /// <returns>Cadena de conexión a la base de datos</returns>
        public override String ToString()
        {
            return cadena;
        }

    }
    /// <summary>
    /// Proporciona las cadenas de conexión para bases de datos de Microsoft SQL Server
    /// </summary>
    public sealed class cadenasConexionesSqlServer
    {
        /// <summary>
        /// Cadena de conexión generada
        /// </summary>
        private readonly String cadena;
        /// <summary>
        /// ID de la cadena generada
        /// </summary>
        private readonly int value;
        /// <summary>
        /// Cadena de conexión de ejemplo. Puede copiar esta línea y contruirse sus cadenas de conexión personalizadas para sus aplicaciones
        /// </summary>
        public static readonly cadenasConexionesSqlServer connPersonalizada = new cadenasConexionesSqlServer(1, "Server=<DIRECCION IP>;User Id=<USUARIO>;Password=<CONTRASEÑA>;Database=<ESQUEMA PREDETERMINADO>;");


        /// <summary>
        /// Constructor que permite realizar un pseudoenum
        /// </summary>
        /// <param name="value"></param>
        /// <param name="name"></param>
        private cadenasConexionesSqlServer(int value, String name)
        {
            this.cadena = name;
            this.value = value;
        }
        /// <summary>
        /// Constructor para cadenas de conexión de SqlServer parametrizadas
        /// </summary>
        /// <param name="ip">IP del servidor SQL Server</param>
        /// <param name="user">Ususario para la conexión on el servidor</param>
        /// <param name="password">Contraseña del usuario en texto plano</param>
        /// <param name="defaultDatabase">Base de datos o esquema por defecto</param>
        public cadenasConexionesSqlServer(String ip, String user, String password, String defaultDatabase)
        {
            this.cadena = "Server=" + ip + ";Database=" + defaultDatabase + ";User Id=" + user + ";Password=" + password + ";";
            this.value = 0;
        }
        /// <summary>
        /// Constructor para cadenas de conexión de SqlServer parametrizadas
        /// </summary>
        /// <param name="ip">IP del servidor SQL Server</param>
        /// <param name="user">Ususario para la conexión on el servidor</param>
        /// <param name="password">Contraseña del usuario en texto plano</param>
        /// <param name="defaultDatabase">Base de datos o esquema por defecto</param>
        /// <param name="timeoutConexion">TimeOut para la conexión con la base de datos</param>
        public cadenasConexionesSqlServer(String ip, String user, String password, String defaultDatabase, Int32 timeoutConexion)
        {
            this.cadena = "Server=" + ip + ";Database=" + defaultDatabase + ";User Id=" + user + ";Password=" + password + ";" + ";Connect Timeout=" + timeoutConexion + ";";
            this.value = 0;
        }
        /// <summary>
        /// Sobreescribe el método ToString para que devuelva la cadena de conexión necesaria
        /// </summary>
        /// <returns>Cadena de conexión a la base de datos</returns>
        public override String ToString()
        {
            return cadena;
        }

    }
    /// <summary>
    /// Tipos de base de datos compatibles con la biblioteca
    /// </summary>
    public enum tipoBaseDeDatos
    {
        /// <summary>
        /// Microsoft SQLServer
        /// </summary>
        SqlServer,
        /// <summary>
        /// Oracle MySQL
        /// </summary>
        MySql
    }
}
