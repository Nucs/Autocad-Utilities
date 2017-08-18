using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MySql.Data.MySqlClient;

namespace nucs.Database {
    /// <summary>
    /// Manages all the Db manners.
    /// </summary>
    public static class Db {
        public static string IP { get; private set; } = "localhost";
        public static string ConnectionString { get; private set; } = $"Data Source=localhost; Initial Catalog=;User Id=sa;Password=;";
        private static string ConnectionStringSourceFormat { get; set; } = "Data Source=localhost; Initial Catalog={catalog};User Id=sa;Password=;";

        public static SimpleCRUD.Dialect Dialect {
            get { return SimpleCRUD.GetDialect(); }
            set { SimpleCRUD.SetDialect(value); }
        }

        static Db() {
            Dialect = SimpleCRUD.Dialect.MySQL;
            /*            string conString = Microsoft.Extensions.Configuration.ConfigurationExtensions.GetConnectionString(this.Configuration, "DefaultConnection");

                        var strings = ConfigurationManager.ConnectionStrings;
                        var @default = strings.Cast<ConnectionStringSettings>().FirstOrDefault(cs => cs.Name == "Default");
                        if (@default == null)
                            return;*/
            /*            
                        ChangeConnectionString(@default);*/
        }

        #region Methods

        #region Settings

        public static void ChangeConnectionString(SqlConnectionStringBuilder cs) {
            if (cs == null)
                throw new ArgumentNullException(nameof(cs));
            ChangeConnectionString(cs.ConnectionString);
        }

        public static void ChangeConnectionString(string connectionstring) {
            changestring(connectionstring);
        }

        private static void changestring(string connectionstring) {
            if (connectionstring == null)
                throw new ArgumentNullException(nameof(connectionstring));
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionstring);
            // Retrieve the DataSource property.    
            IP = builder.DataSource;
            ConnectionString = builder.ToString();
            builder.InitialCatalog = "{catalog}";
            ConnectionStringSourceFormat = builder.ToString();
        }

        #endregion

        #region GetConnection

        /// <summary>
        ///     Gets the default connection for the entire db. (No schema/catalog specified)
        /// </summary>
        public static MySqlConnection GetConnection() {
            var db = new MySqlConnection(ConnectionString);
            _retry:
            try {
                if (db.State == ConnectionState.Closed)
                    db.Open();
            } catch (MySqlException e) when (e.ToString().Contains("error: 40")) {
                goto _retry;
            } catch (MySqlException e) {
                db.Dispose();
                throw e;
            }
            return db;
        }

        /// <summary>
        ///     Gets the default connection for the entire db. (No schema/catalog specified)
        /// </summary>
        public static MySqlConnection GetConnection(string dbname) {
            if (string.IsNullOrEmpty(dbname))
                return GetConnection();
            var db = new MySqlConnection(ConnectionStringSourceFormat.Replace("{catalog}", dbname));
            _retry:
            try {
                if (db.State == ConnectionState.Closed)
                    db.Open();
            } catch (MySqlException e) when (e.ToString().Contains("error: 40")) {
                goto _retry;
            } catch (Exception e) {
                db.Dispose();
                throw e;
            }
            return db;
        }

        /// <summary>
        ///     Creates a using for dbconnection and calls sqlfunc inside on it.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="sqlfunc">Method that uses sql connection</param>
        /// <param name="db">Database to refer to</param>
        /// <returns></returns>
        public static T Call<T>(Func<MySqlConnection, T> sqlfunc, string db = null) {
            _retry:
            try {
                using (var conn = GetConnection(db)) {
                    try {
                        if (conn.State == ConnectionState.Closed)
                            conn.Open();
                        return sqlfunc(conn);
                    } finally {
                        conn.Close();
                    }
                }
            } catch (MySqlException e) when (e.ToString().Contains("error: 40")) {
                //TcpOpener.Open(IP, 1433);
                //Logger.LogDefault(LogLevel.Error, HttpContextProvider.Current.Request.Path, e);
                goto _retry;
            }
        }

        /// <summary>
        ///     Creates a using for dbconnection and calls sqlfunc inside on it.
        /// </summary>
        /// <typeparam name="T">Return type</typeparam>
        /// <param name="sqlfunc">Method that uses sql connection</param>
        /// <returns></returns>
        public static void CallForget(Action<MySqlConnection> sqlfunc) {
            _retry:
            try {
                using (var conn = Db.GetConnection()) {
                    try {
                        if (conn.State == ConnectionState.Closed)
                            conn.Open();
                        sqlfunc(conn);
                    } finally {
                        conn.Close();
                    }
                }
            } catch (MySqlException e) when (e.ToString().Contains("error: 40")) {
                //TcpOpener.Open(IP, 1433);
                //Logger.LogDefault(LogLevel.Error, HttpContextProvider.Current.Request.Path, e);
                goto _retry;
            }
        }

        #endregion

        #region Wrappers

        private static T Wrap<T>(Func<IDbConnection, T> action) {
            _retry:
            using (var conn = GetConnection()) {
                try {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    return action(conn);
                } catch (MySqlException e) when (e.ToString().Contains("error: 40")) {
                    //TcpOpener.Open(IP, 1433);
                    //Logger.LogDefault(LogLevel.Error, HttpContextProvider.Current.Request.Path, e);
                    goto _retry;
                } finally {
                    conn.Close();
                } /* catch (InvalidOperationException e) when (e.Message.EndsWith("was reached.")) {
                    
                }*/
            }
        }

        private static async Task<T> WrapAsync<T>(Func<IDbConnection, Task<T>> action) {
            _retry:
            using (var conn = GetConnection()) {
                try {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    return await action(conn);
                } catch (MySqlException e) when (e.ToString().Contains("error: 40")) {
                    //TcpOpener.Open(IP, 1433);
                    //Logger.LogDefault(LogLevel.Error, HttpContextProvider.Current.Request.Path, e);
                    goto _retry;
                } finally {
                    conn.Close();
                }
            }
        }

        private static void Wrap(Action<IDbConnection> action) {
            _retry:
            using (var conn = GetConnection()) {
                try {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    action(conn);
                } catch (MySqlException e) when (e.ToString().Contains("error: 40")) {
                    //TcpOpener.Open(IP, 1433);
                    //Logger.LogDefault(LogLevel.Error, HttpContextProvider.Current.Request.Path, e);
                    goto _retry;
                } finally {
                    conn.Close();
                }
            }
        }

        private static async Task WrapAsync(Func<IDbConnection, Task> action) {
            _retry:
            using (var conn = GetConnection()) {
                try {
                    if (conn.State == ConnectionState.Closed)
                        conn.Open();
                    await action(conn);
                } catch (MySqlException e) when (e.ToString().Contains("error: 40")) {
                    //TcpOpener.Open(IP, 1433);
                    //Logger.LogDefault(LogLevel.Error, HttpContextProvider.Current.Request.Path, e);
                    goto _retry;
                } finally {
                    conn.Close();
                }
            }
        }

        #endregion

        #endregion

        #region Delegating

        #region Dapper

        // ReSharper disable InvokeAsExtensionMethod
        public static int Execute(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {
            return Wrap(conn => SqlMapper.Execute(conn, sql, param, transaction, commandTimeout, commandType));
        }

        public static Task ExecuteAsync(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {
            return WrapAsync(conn => SqlMapper.ExecuteAsync(conn, sql, param, transaction, commandTimeout, commandType));
        }

        public static IEnumerable<object> Query(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null) {
            return Wrap(cnn => SqlMapper.Query(cnn, sql, param, transaction, buffered, commandTimeout, commandType));
        }

        public static IEnumerable<T> Query<T>(string sql, object param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null) {
            return Wrap(cnn => SqlMapper.Query<T>(cnn, sql, param, transaction, buffered, commandTimeout, commandType));
        }

        public static SqlMapper.GridReader QueryMultiple(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {
            return Wrap(cnn => SqlMapper.QueryMultiple(cnn, sql, param, transaction, commandTimeout, commandType));
        }

        public static TReturn QueryMultiple<TReturn>(string sql, object param, Func<SqlMapper.GridReader, TReturn> handler, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {
            return Wrap(cnn => {
                using (var grid = SqlMapper.QueryMultiple(cnn, sql, param, transaction, commandTimeout, commandType)) {
                    return handler(grid);
                }
            });
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return Wrap(cnn => SqlMapper.Query(cnn, sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return Wrap(cnn => SqlMapper.Query(cnn, sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return Wrap(cnn => SqlMapper.Query(cnn, sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return Wrap(cnn => SqlMapper.Query(cnn, sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return Wrap(cnn => SqlMapper.Query(cnn, sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return Wrap(cnn => SqlMapper.Query(cnn, sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));
        }

        public static Task<IEnumerable<T>> QueryAsync<T>(string sql, object param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null) {
            return WrapAsync(async cnn => await SqlMapper.QueryAsync<T>(cnn, sql, param, transaction, commandTimeout, commandType));
        }

        public static async Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(string sql, Func<TFirst, TSecond, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return await WrapAsync(async cnn => await SqlMapper.QueryAsync(cnn, sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));
        }

        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return WrapAsync(async cnn => await SqlMapper.QueryAsync(cnn, sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));
        }

        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return WrapAsync(async cnn => await SqlMapper.QueryAsync(cnn, sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));
        }

        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return WrapAsync(async cnn => await SqlMapper.QueryAsync(cnn, sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));
        }

        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return WrapAsync(async cnn => await SqlMapper.QueryAsync(cnn, sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));
        }

        public static Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, object param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null) {
            return WrapAsync(async cnn => await SqlMapper.QueryAsync(cnn, sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));
        }

        #endregion

        #region Dapper.Contrib

        /*
                        public static Task<T> GetAsync<T>(object id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class {
                            return WrapAsync(async connection => await Dapper.Contrib.Extensions.SqlMapperExtensions.GetAsync<T>(connection, id, transaction, commandTimeout));
                        }
        
                        public static Task<IEnumerable<T>> GetAllAsync<T>(IDbTransaction transaction = null, int? commandTimeout = null) where T : class {
                            return WrapAsync(async connection => await Dapper.Contrib.Extensions.SqlMapperExtensions.GetAllAsync<T>(connection, transaction, commandTimeout));
                        }
        
                        public static Task<int> InsertAsync<T>(T entityToInsert, IDbTransaction transaction = null, int? commandTimeout = null, ISqlAdapter sqlAdapter = null) where T : class {
                            return WrapAsync(async connection => await Dapper.Contrib.Extensions.SqlMapperExtensions.InsertAsync(connection, entityToInsert, transaction, commandTimeout, sqlAdapter));
                        }
        
                        public static Task<bool> UpdateAsync<T>(T entityToUpdate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class {
                            return WrapAsync(async connection => await Dapper.Contrib.Extensions.SqlMapperExtensions.UpdateAsync(connection, entityToUpdate, transaction, commandTimeout));
                        }
        
                        public static Task<bool> DeleteAsync<T>(T entityToDelete, IDbTransaction transaction = null, int? commandTimeout = null) where T : class {
                            return WrapAsync(async connection => await Dapper.Contrib.Extensions.SqlMapperExtensions.DeleteAsync(connection, entityToDelete, transaction, commandTimeout));
                        }
        
                        public static Task<bool> DeleteAllAsync<T>(IDbTransaction transaction = null, int? commandTimeout = null) where T : class {
                            return WrapAsync(async connection => await Dapper.Contrib.Extensions.SqlMapperExtensions.DeleteAllAsync<T>(connection, transaction, commandTimeout));
                        }
        
                /*
                                public static T Get<T>(object id, IDbTransaction transaction = null, int? commandTimeout = null) where T : class {
                                    return Wrap(connection => Dapper.Contrib.Extensions.SqlMapperExtensions.Get<T>(connection, id, transaction, commandTimeout));
                                }#1#
        
                        public static IEnumerable<T> GetAll<T>(IDbTransaction transaction = null, int? commandTimeout = null) where T : class {
                            return Wrap(connection => Dapper.Contrib.Extensions.SqlMapperExtensions.GetAll<T>(connection, transaction, commandTimeout));
                        }
        
                        public static long Insert<T>(T entityToInsert, IDbTransaction transaction = null, int? commandTimeout = null) where T : class {
                            return Wrap(connection => Dapper.Contrib.Extensions.SqlMapperExtensions.Insert(connection, entityToInsert, transaction, commandTimeout));
                        }
        
                        public static bool Update<T>(T entityToUpdate, IDbTransaction transaction = null, int? commandTimeout = null) where T : class {
                            return Wrap(connection => Dapper.Contrib.Extensions.SqlMapperExtensions.Update(connection, entityToUpdate, transaction, commandTimeout));
                        }
        
                /*
                                public static bool Delete<T>(T entityToDelete, IDbTransaction transaction = null, int? commandTimeout = null) where T : class {
                                    return Wrap(connection => Dapper.Contrib.Extensions.SqlMapperExtensions.Delete(connection, entityToDelete, transaction, commandTimeout));
                                }#1#
        
                        public static bool DeleteAll<T>(IDbTransaction transaction = null, int? commandTimeout = null) where T : class {
                            return Wrap(connection => Dapper.Contrib.Extensions.SqlMapperExtensions.DeleteAll<T>(connection, transaction, commandTimeout));
                        }*/

        #endregion

        #region SimpleCRUD

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>By default filters on the Id column</para>
        /// <para>-Id column name can be overridden by adding an attribute on your primary key property [Key]</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns a single entity by a single id from table T</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Returns a single entity by a single id from table T.</returns>
        public static T Get<T>(object id, IDbTransaction transaction = null, int? commandTimeout = null) {
            return Wrap(connection => SimpleCRUD.Get<T>(connection, id, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>whereConditions is an anonymous type to filter the results ex: new {Category = 1, SubCategory=2}</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns a list of entities that match where conditions</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="whereConditions"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Gets a list of entities with optional exact match where conditions</returns>
        public static IEnumerable<T> GetList<T>(object whereConditions, IDbTransaction transaction = null, int? commandTimeout = null) {
            return Wrap(connection => SimpleCRUD.GetList<T>(connection, whereConditions, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>conditions is an SQL where clause and/or order by clause ex: "where name='bob'" or "where age>=@Age"</para>
        /// <para>parameters is an anonymous type to pass in named parameter values: new { Age = 15 }</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns a list of entities that match where conditions</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="conditions"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Gets a list of entities with optional SQL where conditions</returns>
        public static IEnumerable<T> GetList<T>(string conditions, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null) {
            return Wrap(connection => SimpleCRUD.GetList<T>(connection, conditions, parameters, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Returns a list of all entities</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <returns>Gets a list of all entities</returns>
        public static IEnumerable<T> GetList<T>() {
            return Wrap(connection => SimpleCRUD.GetList<T>(connection));
        }

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>conditions is an SQL where clause ex: "where name='bob'" or "where age>=@Age" - not required </para>
        /// <para>orderby is a column or list of columns to order by ex: "lastname, age desc" - not required - default is by primary key</para>
        /// <para>parameters is an anonymous type to pass in named parameter values: new { Age = 15 }</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns a list of entities that match where conditions</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="pageNumber"></param>
        /// <param name="rowsPerPage"></param>
        /// <param name="conditions"></param>
        /// <param name="orderby"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Gets a paged list of entities with optional exact match where conditions</returns>
        public static IEnumerable<T> GetListPaged<T>(int pageNumber, int rowsPerPage, string conditions, string @orderby, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null) {
            return Wrap(connection => SimpleCRUD.GetListPaged<T>(connection, pageNumber, rowsPerPage, conditions, @orderby, parameters, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>Inserts a row into the database</para>
        /// <para>By default inserts into the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Insert filters out Id column and any columns with the [Key] attribute</para>
        /// <para>Properties marked with attribute [Editable(false)] and complex types are ignored</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns the ID (primary key) of the newly inserted record if it is identity using the int? type, otherwise null</para>
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entityToInsert"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The ID (primary key) of the newly inserted record if it is identity using the int? type, otherwise null</returns>
        public static int? Insert(object entityToInsert, IDbTransaction transaction = null, int? commandTimeout = null) {
            return Wrap(connection => SimpleCRUD.Insert(connection, entityToInsert, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>Inserts a row into the database</para>
        /// <para>By default inserts into the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Insert filters out Id column and any columns with the [Key] attribute</para>
        /// <para>Properties marked with attribute [Editable(false)] and complex types are ignored</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns the ID (primary key) of the newly inserted record if it is identity using the defined type, otherwise null</para>
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entityToInsert"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The ID (primary key) of the newly inserted record if it is identity using the defined type, otherwise null</returns>
        public static TKey Insert<TKey>(object entityToInsert, IDbTransaction transaction = null, int? commandTimeout = null) {
            return Wrap(connection => SimpleCRUD.Insert<TKey>(connection, entityToInsert, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>Updates a record or records in the database</para>
        /// <para>By default updates records in the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Updates records where the Id property and properties with the [Key] attribute match those in the database.</para>
        /// <para>Properties marked with attribute [Editable(false)] and complex types are ignored</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns number of rows effected</para>
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entityToUpdate"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The number of effected records</returns>
        public static int Update(object entityToUpdate, IDbTransaction transaction = null, int? commandTimeout = null) {
            return Wrap(connection => SimpleCRUD.Update(connection, entityToUpdate, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>Deletes a record or records in the database that match the object passed in</para>
        /// <para>-By default deletes records in the table matching the class name</para>
        /// <para>Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns the number of records effected</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="entityToDelete"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The number of records effected</returns>
        public static int Delete<T>(T entityToDelete, IDbTransaction transaction = null, int? commandTimeout = null) {
            return Wrap(connection => SimpleCRUD.Delete(connection, entityToDelete, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>Deletes a record or records in the database by ID</para>
        /// <para>By default deletes records in the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Deletes records where the Id property and properties with the [Key] attribute match those in the database</para>
        /// <para>The number of records effected</para>
        /// <para>Supports transaction and command timeout</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The number of records effected</returns>
        public static int Delete<T>(object id, IDbTransaction transaction = null, int? commandTimeout = null) {
            return Wrap(connection => SimpleCRUD.Delete<T>(connection, id, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>Deletes a list of records in the database</para>
        /// <para>By default deletes records in the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Deletes records where that match the where clause</para>
        /// <para>whereConditions is an anonymous type to filter the results ex: new {Category = 1, SubCategory=2}</para>
        /// <para>The number of records effected</para>
        /// <para>Supports transaction and command timeout</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="whereConditions"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The number of records effected</returns>
        public static int DeleteList<T>(object whereConditions, IDbTransaction transaction = null, int? commandTimeout = null) {
            return Wrap(connection => SimpleCRUD.DeleteList<T>(connection, whereConditions, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>Deletes a list of records in the database</para>
        /// <para>By default deletes records in the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Deletes records where that match the where clause</para>
        /// <para>conditions is an SQL where clause ex: "where name='bob'" or "where age>=@Age"</para>
        /// <para>parameters is an anonymous type to pass in named parameter values: new { Age = 15 }</para>
        /// <para>Supports transaction and command timeout</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="conditions"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The number of records effected</returns>
        public static int DeleteList<T>(string conditions, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null) {
            return Wrap(connection => SimpleCRUD.DeleteList<T>(connection, conditions, parameters, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Returns a number of records entity by a single id from table T</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>conditions is an SQL where clause ex: "where name='bob'" or "where age>=@Age" - not required </para>
        /// <para>parameters is an anonymous type to pass in named parameter values: new { Age = 15 }</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="conditions"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Returns a count of records.</returns>
        public static int RecordCount<T>(string conditions = "", object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null) {
            return Wrap(connection => SimpleCRUD.RecordCount<T>(connection, conditions, parameters, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Returns a number of records entity by a single id from table T</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>whereConditions is an anonymous type to filter the results ex: new {Category = 1, SubCategory=2}</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="whereConditions"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Returns a count of records.</returns>
        public static int RecordCount<T>(object whereConditions, IDbTransaction transaction = null, int? commandTimeout = null) {
            return Wrap(connection => SimpleCRUD.RecordCount<T>(connection, whereConditions, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>By default queries the table matching the class name asynchronously </para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>By default filters on the Id column</para>
        /// <para>-Id column name can be overridden by adding an attribute on your primary key property [Key]</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns a single entity by a single id from table T</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Returns a single entity by a single id from table T.</returns>
        public static Task<T> GetAsync<T>(object id, IDbTransaction transaction = null, int? commandTimeout = null) {
            return WrapAsync(async connection => await SimpleCRUD.GetAsync<T>(connection, id, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>By default queries the table matching the class name asynchronously</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>whereConditions is an anonymous type to filter the results ex: new {Category = 1, SubCategory=2}</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns a list of entities that match where conditions</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="whereConditions"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Gets a list of entities with optional exact match where conditions</returns>
        public static Task<IEnumerable<T>> GetListAsync<T>(object whereConditions, IDbTransaction transaction = null, int? commandTimeout = null) {
            return WrapAsync(async connection => await SimpleCRUD.GetListAsync<T>(connection, whereConditions, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>conditions is an SQL where clause and/or order by clause ex: "where name='bob'" or "where age>=@Age"</para>
        /// <para>parameters is an anonymous type to pass in named parameter values: new { Age = 15 }</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns a list of entities that match where conditions</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="conditions"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Gets a list of entities with optional SQL where conditions</returns>
        public static Task<IEnumerable<T>> GetListAsync<T>(string conditions, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null) {
            return WrapAsync(async connection => await SimpleCRUD.GetListAsync<T>(connection, conditions, parameters, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>By default queries the table matching the class name asynchronously</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Returns a list of all entities</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <returns>Gets a list of all entities</returns>
        public static Task<IEnumerable<T>> GetListAsync<T>() {
            return WrapAsync(async connection => await SimpleCRUD.GetListAsync<T>(connection));
        }

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>conditions is an SQL where clause ex: "where name='bob'" or "where age>=@Age" - not required </para>
        /// <para>orderby is a column or list of columns to order by ex: "lastname, age desc" - not required - default is by primary key</para>
        /// <para>parameters is an anonymous type to pass in named parameter values: new { Age = 15 }</para>
        /// <para>Returns a list of entities that match where conditions</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="pageNumber"></param>
        /// <param name="rowsPerPage"></param>
        /// <param name="conditions"></param>
        /// <param name="orderby"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Gets a list of entities with optional exact match where conditions</returns>
        public static Task<IEnumerable<T>> GetListPagedAsync<T>(int pageNumber, int rowsPerPage, string conditions, string @orderby, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null) {
            return WrapAsync(async connection => await SimpleCRUD.GetListPagedAsync<T>(connection, pageNumber, rowsPerPage, conditions, @orderby, parameters, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>Inserts a row into the database asynchronously</para>
        /// <para>By default inserts into the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Insert filters out Id column and any columns with the [Key] attribute</para>
        /// <para>Properties marked with attribute [Editable(false)] and complex types are ignored</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns the ID (primary key) of the newly inserted record if it is identity using the int? type, otherwise null</para>
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entityToInsert"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The ID (primary key) of the newly inserted record if it is identity using the int? type, otherwise null</returns>
        public static Task<int?> InsertAsync(object entityToInsert, IDbTransaction transaction = null, int? commandTimeout = null) {
            return WrapAsync(async connection => await SimpleCRUD.InsertAsync(connection, entityToInsert, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>Inserts a row into the database asynchronously</para>
        /// <para>By default inserts into the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Insert filters out Id column and any columns with the [Key] attribute</para>
        /// <para>Properties marked with attribute [Editable(false)] and complex types are ignored</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns the ID (primary key) of the newly inserted record if it is identity using the defined type, otherwise null</para>
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="entityToInsert"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The ID (primary key) of the newly inserted record if it is identity using the defined type, otherwise null</returns>
        public static Task<TKey> InsertAsync<TKey>(object entityToInsert, IDbTransaction transaction = null, int? commandTimeout = null) {
            return WrapAsync(async connection => await SimpleCRUD.InsertAsync<TKey>(connection, entityToInsert, transaction, commandTimeout));
        }

        /// <summary>
        ///  <para>Updates a record or records in the database asynchronously</para>
        ///  <para>By default updates records in the table matching the class name</para>
        ///  <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        ///  <para>Updates records where the Id property and properties with the [Key] attribute match those in the database.</para>
        ///  <para>Properties marked with attribute [Editable(false)] and complex types are ignored</para>
        ///  <para>Supports transaction and command timeout</para>
        ///  <para>Returns number of rows effected</para>
        ///  </summary>
        ///  <param name="connection"></param>
        ///  <param name="entityToUpdate"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The number of effected records</returns>
        public static Task<int> UpdateAsync(object entityToUpdate, IDbTransaction transaction = null, int? commandTimeout = null, CancellationToken? token = null) {
            return WrapAsync(async connection => await SimpleCRUD.UpdateAsync(connection, entityToUpdate, transaction, commandTimeout, token));
        }

        /// <summary>
        /// <para>Deletes a record or records in the database that match the object passed in asynchronously</para>
        /// <para>-By default deletes records in the table matching the class name</para>
        /// <para>Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>Returns the number of records effected</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="entityToDelete"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The number of records effected</returns>
        public static Task<int> DeleteAsync<T>(T entityToDelete, IDbTransaction transaction = null, int? commandTimeout = null) {
            return WrapAsync(async connection => await SimpleCRUD.DeleteAsync(connection, entityToDelete, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>Deletes a record or records in the database by ID asynchronously</para>
        /// <para>By default deletes records in the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Deletes records where the Id property and properties with the [Key] attribute match those in the database</para>
        /// <para>The number of records effected</para>
        /// <para>Supports transaction and command timeout</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="id"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The number of records effected</returns>
        public static Task<int> DeleteAsync<T>(object id, IDbTransaction transaction = null, int? commandTimeout = null) {
            return WrapAsync(async connection => await SimpleCRUD.DeleteAsync<T>(connection, id, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>Deletes a list of records in the database</para>
        /// <para>By default deletes records in the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Deletes records where that match the where clause</para>
        /// <para>whereConditions is an anonymous type to filter the results ex: new {Category = 1, SubCategory=2}</para>
        /// <para>The number of records effected</para>
        /// <para>Supports transaction and command timeout</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="whereConditions"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The number of records effected</returns>
        public static Task<int> DeleteListAsync<T>(object whereConditions, IDbTransaction transaction = null, int? commandTimeout = null) {
            return WrapAsync(async connection => await SimpleCRUD.DeleteListAsync<T>(connection, whereConditions, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>Deletes a list of records in the database</para>
        /// <para>By default deletes records in the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Deletes records where that match the where clause</para>
        /// <para>conditions is an SQL where clause ex: "where name='bob'" or "where age>=@Age"</para>
        /// <para>parameters is an anonymous type to pass in named parameter values: new { Age = 15 }</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="conditions"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>The number of records effected</returns>
        public static Task<int> DeleteListAsync<T>(string conditions, object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null) {
            return WrapAsync(async connection => await SimpleCRUD.DeleteListAsync<T>(connection, conditions, parameters, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>conditions is an SQL where clause ex: "where name='bob'" or "where age>=@Age" - not required </para>
        /// <para>parameters is an anonymous type to pass in named parameter values: new { Age = 15 }</para>   
        /// <para>Supports transaction and command timeout</para>
        /// /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="conditions"></param>
        /// <param name="parameters"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Returns a count of records.</returns>
        public static Task<int> RecordCountAsync<T>(string conditions = "", object parameters = null, IDbTransaction transaction = null, int? commandTimeout = null) {
            return WrapAsync(async connection => await SimpleCRUD.RecordCountAsync<T>(connection, conditions, parameters, transaction, commandTimeout));
        }

        /// <summary>
        /// <para>By default queries the table matching the class name</para>
        /// <para>-Table name can be overridden by adding an attribute on your class [Table("YourTableName")]</para>
        /// <para>Returns a number of records entity by a single id from table T</para>
        /// <para>Supports transaction and command timeout</para>
        /// <para>whereConditions is an anonymous type to filter the results ex: new {Category = 1, SubCategory=2}</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <param name="whereConditions"></param>
        /// <param name="transaction"></param>
        /// <param name="commandTimeout"></param>
        /// <returns>Returns a count of records.</returns>
        public static Task<int> RecordCountAsync<T>(object whereConditions, IDbTransaction transaction = null, int? commandTimeout = null) {
            return WrapAsync(async connection => await SimpleCRUD.RecordCountAsync<T>(connection, whereConditions, transaction, commandTimeout));
        }

        // ReSharper restore InvokeAsExtensionMethod

        #endregion

        #endregion

        /// <summary>
        ///     Performs a basic Select query, will throw if fails.
        /// </summary>
        public static Exception TestConnection() {
            try {
                var r=Db.Query<int>("Select 1")?.ToArray();
                if (r==null || r.Length!=1)
                    throw new IndexOutOfRangeException("Supposed to receive 1 object, did not receive anything.");
                return null;
            } catch (Exception e) {
                return e;
            }
        }
    }
}