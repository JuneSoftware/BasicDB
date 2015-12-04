using System;
using System.Runtime.InteropServices;
using System.Collections;
using UnityEngine;

/*
 * please don't use this code for sell a asset
 * user for free 
 * developed by Poya  @  http://gamesforsoul.com/
 * BLOB support by Jonathan Derrough @ http://jderrough.blogspot.com/
 * Modify and structure by Santiago Bustamante @ busta117@gmail.com
 * Android compatibility by Thomas Olsen @ olsen.thomas@gmail.com
 *
 * */

using Logger = June.DebugLogger;

namespace SQLiteUnityKit {

	/// <summary>
	/// Sqlite exception.
	/// </summary>
	public class SqliteException : Exception {

		/// <summary>
		/// Initializes a new instance of the <see cref="SQLiteUnityKit.SqliteException"/> class.
		/// </summary>
		/// <param name="message">Message.</param>
		public SqliteException (string message) : base(message) { }
	}

	/// <summary>
	/// Sqlite database.
	/// </summary>
	public class SqliteDatabase : IDisposable {

		private bool CanExQuery = true;
		const int SQLITE_OK = 0;
		const int SQLITE_ROW = 100;
		const int SQLITE_DONE = 101;
		const int SQLITE_INTEGER = 1;
		const int SQLITE_FLOAT = 2;
		const int SQLITE_TEXT = 3;
		const int SQLITE_BLOB = 4;
		const int SQLITE_NULL = 5;
	        
		[DllImport("sqlite3", EntryPoint = "sqlite3_open")]
		private static extern int sqlite3_open (string filename, out IntPtr db);
	 
		[DllImport("sqlite3", EntryPoint = "sqlite3_close")]
		private static extern int sqlite3_close (IntPtr db);
	 
		[DllImport("sqlite3", EntryPoint = "sqlite3_prepare_v2")]
		private static extern int sqlite3_prepare_v2 (IntPtr db, string zSql, int nByte, out IntPtr ppStmpt, IntPtr pzTail);
	 
		[DllImport("sqlite3", EntryPoint = "sqlite3_step")]
		private static extern int sqlite3_step (IntPtr stmHandle);
	 
		[DllImport("sqlite3", EntryPoint = "sqlite3_finalize")]
		private static extern int sqlite3_finalize (IntPtr stmHandle);

		[DllImport("sqlite3", EntryPoint = "sqlite3_exec")]
		private static extern int sqlite3_exec (IntPtr db, string zSql, IntPtr callback, IntPtr firstArg, out IntPtr ErrorWrapper);
	 
		[DllImport("sqlite3", EntryPoint = "sqlite3_errmsg")]
		private static extern IntPtr sqlite3_errmsg (IntPtr db);
	 
		[DllImport("sqlite3", EntryPoint = "sqlite3_total_changes")]
		private static extern int sqlite3_total_changes (IntPtr db);

		[DllImport("sqlite3", EntryPoint = "sqlite3_changes")]
		private static extern int sqlite3_changes (IntPtr db);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_count")]
		private static extern int sqlite3_column_count (IntPtr stmHandle);
	 
		[DllImport("sqlite3", EntryPoint = "sqlite3_column_name")]
		private static extern IntPtr sqlite3_column_name (IntPtr stmHandle, int iCol);
	 
		[DllImport("sqlite3", EntryPoint = "sqlite3_column_type")]
		private static extern int sqlite3_column_type (IntPtr stmHandle, int iCol);
	 
		[DllImport("sqlite3", EntryPoint = "sqlite3_column_int")]
		private static extern int sqlite3_column_int (IntPtr stmHandle, int iCol);
	 
		[DllImport("sqlite3", EntryPoint = "sqlite3_column_text")]
		private static extern IntPtr sqlite3_column_text (IntPtr stmHandle, int iCol);
	 
		[DllImport("sqlite3", EntryPoint = "sqlite3_column_double")]
		private static extern double sqlite3_column_double (IntPtr stmHandle, int iCol);
	 
		[DllImport("sqlite3", EntryPoint = "sqlite3_column_blob")]
		private static extern IntPtr sqlite3_column_blob (IntPtr stmHandle, int iCol);

		[DllImport("sqlite3", EntryPoint = "sqlite3_column_bytes")]
		private static extern int sqlite3_column_bytes (IntPtr stmHandle, int iCol);
		
		private IntPtr _connection;

		/// <summary>
		/// Gets a value indicating whether this instance is connection open.
		/// </summary>
		/// <value><c>true</c> if this instance is connection open; otherwise, <c>false</c>.</value>
		public bool IsConnectionOpen { 
			get {
				return CanExQuery;
			}
		}

		/// <summary>
		/// Gets the connection string.
		/// </summary>
		/// <value>The connection string.</value>
		public string ConnectionString { 
			get {
				return pathDB;
			}
		}
		
		private string pathDB;
		
		
	    #region Public Methods
	    
		/// <summary>
		/// Initializes a new instance of the <see cref="SqliteDatabase"/> class.
		/// </summary>
		/// <param name='dbName'> 
		/// Data Base name. (the file needs exist in the streamingAssets folder)
		/// </param>
		public SqliteDatabase (string dbName) {
			this.pathDB = dbName;
			this.CanExQuery = false;
		}

		/// <summary>
		/// Open this instance.
		/// </summary>
		public void Open () {
			this.Open (pathDB);	
		}

		/// <summary>
		/// Open the specified path.
		/// </summary>
		/// <param name="path">Path.</param>
		public void Open (string path) {
			if (IsConnectionOpen) {
				throw new SqliteException ("There is already an open connection");
			}
	        
			if (sqlite3_open (path, out _connection) != SQLITE_OK) {
				throw new SqliteException ("Could not open database file: " + path);
			}
	        
			this.CanExQuery = true;
		}
	     
		/// <summary>
		/// Close this instance.
		/// </summary>
		public void Close () {
			if (IsConnectionOpen) {
				sqlite3_close (_connection);
			}
			this.CanExQuery = false;
		}
	 
		/// <summary>
		/// Executes a Update, Delete, etc  query.
		/// </summary>
		/// <param name='query'>
		/// Query.
		/// </param>
		/// <exception cref='SqliteException'>
		/// Is thrown when the sqlite exception.
		/// </exception>
		public int ExecuteNonQuery (string query) {
			int changes = 0;

			if (!CanExQuery) {
				Logger.Log ("ERROR: Can't execute the query, verify DB origin file");
				return 0;
			}

			if(!IsConnectionOpen) {
				this.Open ();
			}

			if (!IsConnectionOpen) {
				throw new SqliteException ("SQLite database is not open.");
			}

			IntPtr stmHandle = Prepare (query);
	 
			if (sqlite3_step (stmHandle) != SQLITE_DONE) {
				throw new SqliteException ("Could not execute SQL statement.");
			}

			changes = sqlite3_changes(_connection);

			Finalize (stmHandle);

			this.Close ();

			return changes;
		}
		
		/// <summary>
		/// Executes a query that requires a response (SELECT, etc).
		/// </summary>
		/// <returns>
		/// Dictionary with the response data
		/// </returns>
		/// <param name='query'>
		/// Query.
		/// </param>
		/// <exception cref='SqliteException'>
		/// Is thrown when the sqlite exception.
		/// </exception>
		public DataTable ExecuteQuery (string query) {
			if (!CanExQuery) {
				Logger.Log ("ERROR: Can't execute the query, verify DB origin file");
				return null;
			}

			if(!IsConnectionOpen) {
				this.Open ();
			}

			if (!IsConnectionOpen) {
				throw new SqliteException ("SQLite database is not open.");
			}
	        
			IntPtr stmHandle = Prepare (query);
	 
			int columnCount = sqlite3_column_count (stmHandle);
	 
			var dataTable = new DataTable ();
			for (int i = 0; i < columnCount; i++) {
				string columnName = Marshal.PtrToStringAnsi (sqlite3_column_name (stmHandle, i));
				dataTable.Columns.Add (columnName);
			}
	        
			//populate datatable
			while (sqlite3_step(stmHandle) == SQLITE_ROW) {
				object[] row = new object[columnCount];
				for (int i = 0; i < columnCount; i++) {
					switch (sqlite3_column_type (stmHandle, i)) {
					case SQLITE_INTEGER:
						row [i] = sqlite3_column_int (stmHandle, i);
						break;
	                
					case SQLITE_TEXT:
						IntPtr text = sqlite3_column_text (stmHandle, i);
						row [i] = Marshal.PtrToStringAnsi (text);
						break;

					case SQLITE_FLOAT:
						row [i] = sqlite3_column_double (stmHandle, i);
						break;
	                    
					case SQLITE_BLOB:
						IntPtr blob = sqlite3_column_blob (stmHandle, i);
						int size = sqlite3_column_bytes (stmHandle, i);
						byte[] data = new byte[size];
						Marshal.Copy (blob, data, 0, size);
						row [i] = data;
						break;
						
					case SQLITE_NULL:
						row [i] = null;
						break;
					}
				}
	        
				dataTable.AddRow (row);
			}
	        
			Finalize (stmHandle);
			this.Close ();
			return dataTable;
		}
	    
		public int ExecuteScript (string script) {
			int changes = 0;

			try {
				if (!CanExQuery) {
					Logger.Log ("ERROR: Can't execute the query, verify DB origin file");
					return 0;
				}
				
				if(!IsConnectionOpen) {
					this.Open ();
				}
				
				if (!IsConnectionOpen) {
					throw new SqliteException ("SQLite database is not open.");
				}

				IntPtr hError;
				int result = sqlite3_exec(_connection, script, IntPtr.Zero, IntPtr.Zero, out hError);
				
				if (result != SQLITE_OK || result != SQLITE_DONE) {
					throw new SqliteException ("Could not execute SQL script.");
				}

				changes = sqlite3_changes(_connection);

				this.Close();
			}
			catch(Exception ex) {
				Logger.Log(ex);
			}

			return changes;
		}
	    
	    #endregion
	    
	    #region Private Methods
	 
		private IntPtr Prepare (string query) {
			IntPtr stmHandle;
	        
			if (sqlite3_prepare_v2 (_connection, query, query.Length, out stmHandle, IntPtr.Zero) != SQLITE_OK) {
				IntPtr errorMsg = sqlite3_errmsg (_connection);
				throw new SqliteException (Marshal.PtrToStringAnsi (errorMsg));
			}
	        
			return stmHandle;
		}
	 
		private void Finalize (IntPtr stmHandle) {
			if (sqlite3_finalize (stmHandle) != SQLITE_OK) {
				throw new SqliteException ("Could not finalize SQL statement.");
			}
		}
	    
	    #endregion

		#region IDisposable implementation
		/// <summary>
		/// Releases all resource used by the <see cref="SQLiteUnityKit.SqliteDatabase"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="SQLiteUnityKit.SqliteDatabase"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="SQLiteUnityKit.SqliteDatabase"/> in an unusable state. After
		/// calling <see cref="Dispose"/>, you must release all references to the <see cref="SQLiteUnityKit.SqliteDatabase"/>
		/// so the garbage collector can reclaim the memory that the <see cref="SQLiteUnityKit.SqliteDatabase"/> was occupying.</remarks>
		public void Dispose () {
			if(IsConnectionOpen) {
				Close();
			}
		}
		#endregion
	}
}
