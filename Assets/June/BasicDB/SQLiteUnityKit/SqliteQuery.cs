using UnityEngine;
using System.Collections;
using System;

namespace SQLiteUnityKit {
	/// <summary>
	/// Sqlite query.
	/// </summary>
	public class SqliteQuery : IDisposable {

		/// <summary>
		/// The text.
		/// </summary>
		public string CommandText;

		/// <summary>
		/// The connection.
		/// </summary>
		public SqliteDatabase Connection;

		/// <summary>
		/// Initializes a new instance of the <see cref="SQLiteUnityKit.SqliteQuery"/> class.
		/// </summary>
		/// <param name="connection">Connection.</param>
		/// <param name="query">Query.</param>
		public SqliteQuery(SqliteDatabase connection, string query) : this(connection) {
			this.CommandText = query;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SQLiteUnityKit.SqliteQuery"/> class.
		/// </summary>
		public SqliteQuery(SqliteDatabase connection) { 
			this.Connection = connection;
		}

		/// <summary>
		/// Executes the non query.
		/// </summary>
		/// <returns>The non query.</returns>
		public int ExecuteNonQuery () {
			return this.Connection.ExecuteNonQuery(this.CommandText);
		}

		/// <summary>
		/// Executes the scalar.
		/// </summary>
		/// <returns>The scalar.</returns>
		public object ExecuteScalar() {
			using(var table = this.Connection.ExecuteQuery(this.CommandText)) {
				if(null != table && null != table.Rows && table.Rows.Count > 0 
			   	&& null != table.Columns && table.Columns.Count > 0) {
					return table[0][0];
				}
				return null;
			}
		}

		/// <summary>
		/// Executes the reader.
		/// </summary>
		/// <returns>The reader.</returns>
		public DataTable ExecuteReader() {
			return this.Connection.ExecuteQuery(this.CommandText);
		}

		/// <summary>
		/// Executes the script.
		/// </summary>
		/// <returns>The script.</returns>
		public int ExecuteScript() {
			return this.Connection.ExecuteScript(this.CommandText);
		}

		#region IDisposable implementation

		/// <summary>
		/// Releases all resource used by the <see cref="SQLiteUnityKit.SqliteQuery"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="SQLiteUnityKit.SqliteQuery"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="SQLiteUnityKit.SqliteQuery"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="SQLiteUnityKit.SqliteQuery"/> so
		/// the garbage collector can reclaim the memory that the <see cref="SQLiteUnityKit.SqliteQuery"/> was occupying.</remarks>
		public void Dispose () {
			if(null != this.Connection) {
				this.Connection = null;
			}
			this.CommandText = null;
		}

		#endregion
	}
}