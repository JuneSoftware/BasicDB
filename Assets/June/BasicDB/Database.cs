using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;

using Logger = UnityEngine.Debug;

namespace June.BasicDB {

	using SQLiteUnityKit;

	/// <summary>
	/// Database interface.
	/// </summary>
	public static partial class Database {
		
		#region Settings Table
		/// <summary>
		/// Settings Table Name
		/// </summary>
		#if UNITY_EDITOR
		public
		#else
		internal
		#endif
		static readonly string DB_SETTINGS_TABLE_NAME = "settings";

		/// <summary>
		/// Settings Table Create Statement
		/// </summary>
		private static readonly string DB_SETTINGS_CREATE_SQL = "CREATE TABLE IF NOT EXISTS " + DB_SETTINGS_TABLE_NAME + " ('key' VARCHAR PRIMARY KEY, 'value' VARCHAR NOT NULL)";
		
		#endregion
		
		#region Database Versions
		/// <summary>
		/// Array containing database version and path to the script for the migration
		/// </summary>
		#if UNITY_EDITOR
		public
		#else
		internal
		#endif
		static List<KeyValuePair<float, string>> DB_VERSIONS = new List<KeyValuePair<float, string>> () { 
			new KeyValuePair<float, string> (1.0f, "Schema1"),
			new KeyValuePair<float, string> (2.0f, "Schema2")
		};

		/// <summary>
		/// DB Version Settings Key
		/// </summary>
		#if UNITY_EDITOR
		public
		#else
		internal
		#endif
		static readonly string DB_VERSION_SETTING_KEY = "DB_VER";

		/// <summary>
		/// Latest DB Version
		/// </summary>
		#if UNITY_EDITOR
		public
		#else
		internal
		#endif
		static readonly float DB_VERSION_LATEST = 1.0f;

		/// <summary>
		/// Database filename
		/// </summary>
		public static string FILE_NAME = "BasicDB.db";

		/// <summary>
		/// Database connection string
		/// </summary>
		#if UNITY_EDITOR
		public
		#else
		internal
		#endif
		static readonly string DB_CONNECTION_STRING = Path.Combine(Application.persistentDataPath, FILE_NAME);

		#endregion
		
		
		/// <summary>
		/// Private connection/datbase object for SQLite db
		/// </summary>	
		private static SqliteDatabase _database = null;
		
		private static object _DbDeleteMutex = new object ();
		/// <summary>
		/// Delete this database.
		/// </summary>	
		public static void Delete () {
			lock (_DbDeleteMutex) {
				if (null != _database && _database.IsConnectionOpen) {
					_database.Close ();
				}
				if (System.IO.File.Exists (DB_CONNECTION_STRING)) {
					#if !UNITY_WEBPLAYER
					System.IO.File.Move (DB_CONNECTION_STRING, DB_CONNECTION_STRING + "_" + DateTime.Now.ToString ("yyyyMMddHHmmss"));
					#endif
				}
				_database = null;
			}
		}

		/// <summary>
		/// Initializes the <see cref="June.BasicDB.Database"/> class.
		/// </summary>
		static Database() {
			if(false == CheckIfTableExists(DB_SETTINGS_TABLE_NAME)) {
				CreateSettingsTable();
			}
		}

		#region Data Access Methods
		/// <summary>
		/// Get Database Connection Object using default connection string
		/// </summary>
		/// <returns></returns>
		private static SqliteDatabase GetConnection () {
			return GetConnection (DB_CONNECTION_STRING);
		}
		
		/// <summary>
		/// Gets the connection.
		/// </summary>
		/// <param name="connectionString">The connection string.</param>
		/// <returns></returns>
		private static SqliteDatabase GetConnection (string connectionString) {
			if (null == _database) {
				_database = new SqliteDatabase (connectionString);
			}
			return _database;
		}
		
		/// <summary>
		/// Gets the command object.
		/// </summary>
		/// <returns></returns>
		public static SqliteQuery GetCommand () {
			return new SqliteQuery (GetConnection ());
		}
		
		/// <summary>
		/// Get Database Command object
		/// </summary>
		/// <param name="sql"></param>
		/// <returns></returns>
		public static SqliteQuery GetCommand (string sql) {
			return new SqliteQuery (GetConnection (), sql);
		}
		
		/// <summary>
		/// This command creates a connection and command and then it calls handler which matches the signature
		/// of the delegate CommandHandler
		/// </summary>
		/// <typeparam name="T">The return type can be anything</typeparam>
		/// <param name="sql">The query to eb executed</param>
		/// <param name="handler">The delegate to be followed</param>
		/// <returns></returns>
		public static TResult ExecuteCommand<TResult> (SqliteQuery cmd, Func<SqliteQuery, TResult> handler) {
			//If connection object exists check if its open
			if (false == cmd.Connection.IsConnectionOpen) {
				Logger.Log ("[Database] Opening Connection - " + cmd.Connection.ConnectionString);
				cmd.Connection.Open ();
			}
			
			//execute query, make sure only one query is fired on the database, hence making it thread safe
			lock (cmd.Connection) {
				Logger.Log ("[Database] Executing \n" + cmd.CommandText);
				return handler (cmd);
			}
		}
		
		/// <summary>
		/// This method calls ExecuteCommand whose handler calls the SQLite method ExecuteNonQuery
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public static int ExecuteNonQuery (SqliteQuery command) {
			return ExecuteCommand<int> (command, cmd => cmd.ExecuteNonQuery ());
		}
		
		/// <summary>
		/// This method calls ExecuteCommand whose handler calls the SQLite method ExecuteNonQuery
		/// </summary>
		/// <param name="sql">The query</param>
		/// <returns>No. of rows updated</returns>
		public static int ExecuteNonQuery (string sql) {
			using (var cmd = GetCommand(sql)) {
				return ExecuteNonQuery (cmd);
			}
		}
		
		/// <summary>
		/// This method calls ExecuteCommand whose handler calls the SQLite method ExecuteScalar
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public static object ExecuteScalar (SqliteQuery command) {
			return ExecuteCommand<object> (command, cmd => cmd.ExecuteScalar ());
		}
		
		/// <summary>
		/// This method calls ExecuteCommand whose handler calls the SQLite method ExecuteScalar
		/// </summary>
		/// <param name="sql">Query</param>
		/// <returns>Value of any type</returns>
		public static object ExecuteScalar (string sql) {
			using (var cmd = GetCommand(sql)) {
				return ExecuteScalar (cmd);
			}
		}
		
		/// <summary>
		/// This method calls ExecuteCommand whose handler calls the SQLite method ExecuteScalar
		/// </summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="sql">The SQL.</param>
		/// <param name="converter">The converter.</param>
		/// <returns></returns>
		public static TResult ExecuteScalar<TResult> (string sql, Converter<object, TResult> converter) {
			using (var cmd = GetCommand(sql)) {
				return converter (ExecuteScalar (cmd));
			}
		} 
		
		/// <summary>
		/// Execute Reader
		/// </summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="command">The command.</param>
		/// <param name="handler">The handler.</param>
		/// <returns></returns>
		public static TResult ExecuteReader<TResult> (SqliteQuery command, Func<DataTable, TResult> handler) {
			return ExecuteCommand<TResult> (command, cmd => {
				using(var reader = cmd.ExecuteReader ()) {
					return handler (reader);
				}
			});
		}
		
		/// <summary>
		/// Execute Reader
		/// </summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="sql">The SQL.</param>
		/// <param name="handler">The handler.</param>
		/// <returns></returns>
		public static TResult ExecuteReader<TResult> (string sql, Func<DataTable, TResult> handler) {
			using(var cmd = GetCommand (sql)) {
				return ExecuteReader<TResult> (cmd, handler);
			}
		}
		
		/// <summary>
		/// This method calls the overloaded method with the type DataTable
		/// </summary>
		/// <param name="sql">Query</param>
		/// <returns>DataTable object</returns>
		public static DataTable ExecuteReader (string sql) {
			using(var command = GetCommand (sql)) {
				return ExecuteCommand<DataTable> (command, cmd => cmd.ExecuteReader ());
			}
		}
		
		/// <summary>
		/// Executes the script.
		/// </summary>
		/// <returns>
		/// The script.
		/// </returns>
		/// <param name='command'>
		/// Command.
		/// </param>
		public static int ExecuteScript (SqliteQuery command) {
			return ExecuteCommand<int> (command, cmd => cmd.ExecuteScript ());
		}
		
		/// <summary>
		/// Executes the script.
		/// </summary>
		/// <returns>
		/// The script.
		/// </returns>
		/// <param name='sql'>
		/// Sql.
		/// </param>
		public static int ExecuteScript (string sql) {
			using (var cmd = GetCommand(sql)) {
				return ExecuteScript (cmd);
			}
		}
		#endregion
		
		#region Setting Table Methods
		
		/// <summary>
		/// Determines whether Settings table is present.
		/// </summary>
		/// <returns>
		///   <c>true</c> if Settings table is present; otherwise, <c>false</c>.
		/// </returns>
		private static bool IsSettingsTablePresent () {
			return CheckIfTableExists (DB_SETTINGS_TABLE_NAME);
		}
		
		/// <summary>
		/// Creates the settings table.
		/// </summary>
		/// <returns></returns>
		private static bool CreateSettingsTable () {
			return Database.ExecuteNonQuery (DB_SETTINGS_CREATE_SQL) != 0;
		}
		
		/// <summary>
		/// Gets the settings value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <returns>The settings value for the key, or null if setting isnt present</returns>
		public static string GetSettingsValue (string key) {
			Logger.Log("[Database] Fetching Setting for Key: " + key);
			string query = string.Format ("SELECT value FROM {0} WHERE key = '{1}'", DB_SETTINGS_TABLE_NAME, key);
			return Database.ExecuteScalar<string> (query,
			                                       obj => (null != obj) ? obj.ToString () : null);
		}
		
		/// <summary>
		/// Sets the settings value.
		/// </summary>
		/// <param name="key">The key.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public static bool SetSettingsValue (string key, string value) {
			//if (false == Database.IsSettingsTablePresent ()) {
			//	Database.CreateSettingsTable ();
			//}
			return 1 == Database.ExecuteNonQuery (
				string.Format("INSERT OR REPLACE INTO {0} VALUES ('{1}','{2}')",
				//string.Format ("UPDATE {0} SET value = '{1}' WHERE key = '{2}'",
			               DB_SETTINGS_TABLE_NAME,
			               key,
			               value));
		}
		
		#endregion
		
		#region Migration Methods
		
		/// <summary>
		/// Gets the current schema version.
		/// </summary>
		/// <returns>Returns current schema version, or 0 if no schema is present.</returns>
		public static float GetCurrentSchemaVersion () {
			string ver = null;
			try {
				ver = Database.GetSettingsValue (DB_VERSION_SETTING_KEY);
			} catch {
				ver = null;
			}
			Logger.Log ("[Database] Settings, Current Schema Version - " + ver);
			return (null != ver) ? float.Parse (ver) : 0;
		}
		
		/// <summary>
		/// Determines whether [database schema is up to date].
		/// </summary>
		/// <returns>
		///   <c>true</c> if [database schema is up to date]; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsSchemaUpToDate () {
			return GetCurrentSchemaVersion () == DB_VERSION_LATEST;
		}
		
		/// <summary>
		/// Migrates the schema to version.
		/// </summary>
		/// <param name="versionNo">The version no.</param>
		/// <returns></returns>
		public static bool MigrateSchemaToVersion (float versionNo) {
			var version = Util.FirstOrDefault<KeyValuePair<float, string>> (DB_VERSIONS, kv => kv.Key == versionNo);
			return ExecuteScriptFromResource (version.Value);
		}
		
		/// <summary>
		/// Executes the script from resource.
		/// </summary>
		/// <returns>
		/// The script from resource.
		/// </returns>
		/// <param name='resourceName'>
		/// If set to <c>true</c> resource name.
		/// </param>
		public static bool ExecuteScriptFromResource (string resourceName) {
			bool status = true;
			string text = Util.ReadTextFromResource (resourceName);
			if (null != text) {
				status = ExecuteScript (text) > 0;
			}
			return status;
		}
		
		/// <summary>
		/// Migrates the schema to latest.
		/// </summary>
		/// <returns></returns>
		public static bool MigrateSchemaToLatest () {
			bool status = true;
			if (!IsSchemaUpToDate ()) {
				float currentVersion = GetCurrentSchemaVersion ();
				while (currentVersion < DB_VERSION_LATEST) {
					var nextVersion = GetNextSchemaVersion (currentVersion);
					status &= MigrateSchemaToVersion (nextVersion);
					currentVersion = nextVersion;
				}
			}
			return status;
		}
		
		/// <summary>
		/// Gets the next schema version.
		/// </summary>
		/// <returns>
		/// The next schema version.
		/// </returns>
		/// <param name='currentVersion'>
		/// Current version.
		/// </param>
		public static float GetNextSchemaVersion (float currentVersion) {
			foreach (var kv in DB_VERSIONS) {
				if (kv.Key > currentVersion) {
					return kv.Key;
				}
			}
			return currentVersion;
		}
		
		#endregion
		
		/// <summary>
		/// Checks if table exists.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <returns></returns>
		public static bool CheckIfTableExists (string tableName) {
			return Database.ExecuteScalar ("SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='" + tableName + "'",
			                               obj => {
				int count;
				if (null != obj && int.TryParse (obj.ToString (), out count)) {
					Logger.Log("[Database] CheckIfTableExists " + tableName + " : " + (count == 1));
					return count == 1;
				} else {
					return false;
				}
			});
		}
		
		/// <summary>
		/// Gets all records.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <returns></returns>
		public static DataTable GetAllRecords (string tableName) {
			return GetAllRecords<DataTable> (tableName, dt => dt);
		}
		
		/// <summary>
		/// Gets the reocrd count.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <returns></returns>
		public static int GetReocrdCount (string tableName) {
			return Database.ExecuteScalar<int> ("SELECT COUNT(*) FROM " + tableName, Convert.ToInt32);
		}
		
		/// <summary>
		/// Get all records in a table
		/// </summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="tableName">Table Name</param>
		/// <param name="handler"> DataTable handler</param>
		/// <returns></returns>
		public static TResult GetAllRecords<TResult> (string tableName, Func<DataTable, TResult> handler) {
			return Database.ExecuteReader<TResult> ("SELECT * FROM " + tableName, handler);
		}
		
		/// <summary>
		/// Get Record By Id
		/// </summary>
		/// <typeparam name="T">Return Type</typeparam>
		/// <param name="handler"></param>
		/// <param name="tableName">Database table name</param>
		/// <param name="id">id</param>
		/// <returns>Result Object</returns>
		public static TResult GetRecordById<TResult> (string tableName, int id, Func<DataTable, TResult> handler) {
			return GetRecordBy<TResult> (tableName, "id", id.ToString (), handler);
		}
		
		/// <summary>
		/// Get Record By Column and Value
		/// </summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="tableName">Table Name</param>
		/// <param name="columnName">Column Name</param>
		/// <param name="columnValue">Column Value</param>
		/// <param name="handler"> DataTable handler</param>
		/// <returns></returns>
		public static TResult GetRecordBy<TResult> (string tableName, string columnName, string columnValue, Func<DataTable, TResult> handler) {
			return Database.ExecuteReader<TResult> (
				string.Format ("SELECT * FROM {0} WHERE {1} = {2}", tableName, columnName, columnValue), 
				handler);
		}
		
		/// <summary>
		/// Get Ordered Records
		/// </summary>
		/// <typeparam name="TResult">The type of the result.</typeparam>
		/// <param name="tableName">Table Name</param>
		/// <param name="columnName">Column Name</param>
		/// <param name="columnValue">Column Value</param>
		/// <param name="orderByColumn">Sort by this column name</param>
		/// <param name="orderDirection">Sort direction</param>
		/// <param name="handler"> DataTable handler</param>
		/// <returns></returns>
		public static TResult GetOrderedRecordBy<TResult> (string tableName, string columnName, string columnValue, string orderByColumn, string orderDirection, Func<DataTable, TResult> handler) {
			return Database.ExecuteReader<TResult> (
				string.Format ("SELECT * FROM {0} WHERE {1} = {2} ORDER BY {3} {4}", tableName, columnName, columnValue, orderByColumn, orderDirection), 
				handler);
		}
		
		/// <summary>
		/// Updates the record.
		/// </summary>
		/// <param name="tableName">Name of the table.</param>
		/// <param name="updateColumnName">Name of the update column.</param>
		/// <param name="updateColumnValue">The update column value.</param>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		public static bool UpdateRecord (string tableName, string updateColumnName, string updateColumnValue, int id) {
			return 1 == Database.ExecuteNonQuery (
				string.Format ("UPDATE {0} SET {1} = {2} WHERE id = {3}", tableName, updateColumnName, updateColumnValue, id));
		}
		
		/// <summary>
		/// Delete record by id
		/// </summary>
		/// <param name="tableName">Table Name</param>
		/// <param name="id">Record Id</param>
		/// <returns>Delete Status</returns>
		public static bool DeleteRecord (string tableName, int id) {
			return (1 == Database.ExecuteNonQuery ("DELETE FROM " + tableName + " WHERE id = " + id.ToString ()));
		}
	}
}
