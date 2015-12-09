using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace SQLiteUnityKit {

	/// <summary>
	/// DataRow Class, this object can be accessed via two indexers i.e. ColumnName and ColumnNumber.
	/// </summary>
	public partial class DataRow : IDictionary<int, object>, IDictionary<string, object>, IDisposable {
		List<string> _Columns = null;
		Dictionary<int, object> _Row;

		#region IDictionary implementation

		void IDictionary<string, object>.Add (string key, object value) {
			_Row.Add(_Columns.IndexOf(key), value);
		}

		bool IDictionary<string, object>.ContainsKey (string key) {
			return _Columns.Contains(key);
		}

		bool IDictionary<string, object>.Remove (string key) {
			return _Row.Remove(_Columns.IndexOf(key));
		}

		bool IDictionary<string, object>.TryGetValue (string key, out object value) {
			return _Row.TryGetValue(_Columns.IndexOf(key), out value);
		}

		ICollection<string> IDictionary<string, object>.Keys {
			get {
				return _Columns;
			}
		}

		ICollection<object> IDictionary<string, object>.Values {
			get {
				return _Row.Values;
			}
		}
		
		#endregion

		#region ICollection implementation

		void ICollection<KeyValuePair<string, object>>.Add (KeyValuePair<string, object> item) {
			_Row.Add(_Columns.IndexOf(item.Key), item.Value);
		}

		void ICollection<KeyValuePair<string, object>>.Clear () {
			((ICollection<KeyValuePair<int, object>>)_Row).Clear();
		}

		bool ICollection<KeyValuePair<string, object>>.Contains (KeyValuePair<string, object> item) {
			return ((ICollection<KeyValuePair<int, object>>)_Row).Contains(
				new KeyValuePair<int, object>(_Columns.IndexOf(item.Key), item.Value));
		}

		void ICollection<KeyValuePair<string, object>>.CopyTo (KeyValuePair<string, object>[] array, int arrayIndex) {
			// TODO: This has not been implemented, will need to convert int -> string over entire collection
			throw new NotImplementedException ();
		}

		bool ICollection<KeyValuePair<string, object>>.Remove (KeyValuePair<string, object> item) {
			return ((ICollection<KeyValuePair<int, object>>)_Row).Remove(
				new KeyValuePair<int, object>(_Columns.IndexOf(item.Key), item.Value));
		}

		int ICollection<KeyValuePair<string, object>>.Count {
			get {
				return ((ICollection<KeyValuePair<int, object>>)_Row).Count;
			}
		}

		bool ICollection<KeyValuePair<string, object>>.IsReadOnly {
			get {
				return ((ICollection<KeyValuePair<int, object>>)_Row).IsReadOnly;
			}
		}

		
		#endregion

		#region IEnumerable implementation

		IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator () {
			foreach(var item in _Row) {
				yield return new KeyValuePair<string, object>(_Columns[item.Key], item.Value);
			}
		}

		#endregion

		#region IDictionary<int, object> Implementation
		#region IDictionary implementation

		void IDictionary<int, object>.Add (int key, object value) {
			_Row.Add(key, value);
		}

		bool IDictionary<int, object>.ContainsKey (int key) {
			return _Row.ContainsKey(key);
		}

		bool IDictionary<int, object>.Remove (int key) {
			return _Row.Remove(key);
		}

		bool IDictionary<int, object>.TryGetValue (int key, out object value) {
			return _Row.TryGetValue(key, out value);
		}

		ICollection<int> IDictionary<int, object>.Keys {
			get {
				return _Row.Keys;
			}
		}

		ICollection<object> IDictionary<int, object>.Values {
			get {
				return _Row.Values;
			}
		}

		#endregion

		#region ICollection implementation

		void ICollection<KeyValuePair<int, object>>.Add (KeyValuePair<int, object> item) {
			((ICollection<KeyValuePair<int, object>>)_Row).Add(item);
		}

		void ICollection<KeyValuePair<int, object>>.Clear () {
			((ICollection<KeyValuePair<int, object>>)_Row).Clear();
		}

		bool ICollection<KeyValuePair<int, object>>.Contains (KeyValuePair<int, object> item) {
			return _Row.Contains(item);
		}

		void ICollection<KeyValuePair<int, object>>.CopyTo (KeyValuePair<int, object>[] array, int arrayIndex) {
			((ICollection<KeyValuePair<int, object>>)_Row).CopyTo(array, arrayIndex);
		}

		bool ICollection<KeyValuePair<int, object>>.Remove (KeyValuePair<int, object> item) {
			return ((ICollection<KeyValuePair<int, object>>)_Row).Remove(item);
		}

		int ICollection<KeyValuePair<int, object>>.Count {
			get {
				return ((ICollection<KeyValuePair<int, object>>)_Row).Count;
			}
		}

		bool ICollection<KeyValuePair<int, object>>.IsReadOnly {
			get {
				return ((ICollection<KeyValuePair<int, object>>)_Row).IsReadOnly;
			}
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator<KeyValuePair<int, object>> IEnumerable<KeyValuePair<int, object>>.GetEnumerator () {
			return ((IEnumerator<KeyValuePair<int, object>>)_Row);
		}

		#endregion

		#region IEnumerable implementation

		IEnumerator IEnumerable.GetEnumerator () {
			return ((IEnumerable<KeyValuePair<int, object>>)_Row).GetEnumerator();
		}

		#endregion

		#endregion
		

		/// <summary>
		/// Initializes a new instance of the <see cref="DataRow"/> class.
		/// </summary>
		/// <param name='columns'>
		/// Columns.
		/// </param>
		public DataRow(List<string> columns) : base() {
			_Columns = columns;
			_Row = new Dictionary<int, object>();
		}
		
		/// <summary>
		/// Gets or sets the <see cref="DataRow"/> with the specified column.
		/// </summary>
		/// <param name='column'>
		/// Column.
		/// </param>
		public object this [int columnNumber] {
			get {
				if (_Row.ContainsKey (columnNumber)) {
					return _Row [columnNumber];
				}
	            
				return null;
			}
			set {
				if (_Row.ContainsKey (columnNumber)) {
					_Row [columnNumber] = value;
				} 
				else {
					_Row.Add (columnNumber, value);
				}
			}
		}
		
		/// <summary>
		/// Gets or sets the <see cref="DataRow"/> with the specified column.
		/// </summary>
		/// <param name='column'>
		/// Column.
		/// </param>
		public object this [string columnName] {
			get {
				if(_Columns.Contains(columnName)) {
					return this[_Columns.IndexOf(columnName)];
				}
				return null;
			}
			set {
				if(_Columns.Contains(columnName)) {
					this[_Columns.IndexOf(columnName)] = value;
				}
				else {
					throw new ApplicationException("Column name not found - " + columnName);
				}
			}
		}

		/// <summary>
		/// Gets the data.
		/// </summary>
		/// <value>The data.</value>
		public object[] Data {
			get {
				return this._Row.Values.ToArray();
			}
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="SQLiteUnityKit.DataRow"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="SQLiteUnityKit.DataRow"/>.</returns>
		public override string ToString () {
			return string.Join(", ", Array.ConvertAll<object, string>(_Row.Values.ToArray(), o => o.ToString()));
		}

		#region IDisposable implementation
		/// <summary>
		/// Releases all resource used by the <see cref="SQLiteUnityKit.DataRow"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="SQLiteUnityKit.DataRow"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="SQLiteUnityKit.DataRow"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="SQLiteUnityKit.DataRow"/> so the garbage
		/// collector can reclaim the memory that the <see cref="SQLiteUnityKit.DataRow"/> was occupying.</remarks>
		public void Dispose () {
			if(null != _Row) {
				_Row.Clear();
				_Row = null;
			}

			if(null != _Columns) {
				_Columns.Clear();
				_Columns = null;
			}
		}
		#endregion
	}

	/// <summary>
	/// Data table.
	/// </summary>
	public partial class DataTable : IDisposable {

		/// <summary>
		/// Initializes a new instance of the <see cref="SQLiteUnityKit.DataTable"/> class.
		/// </summary>
		public DataTable () {
			Columns = new List<string> ();
			Rows = new List<DataRow> ();
		}

		/// <summary>
		/// Gets or sets the columns.
		/// </summary>
		/// <value>The columns.</value>
		public List<string> Columns { get; set; }

		/// <summary>
		/// Gets or sets the rows.
		/// </summary>
		/// <value>The rows.</value>
		public List<DataRow> Rows { get; set; }
	    
		/// <summary>
		/// Gets the <see cref="SQLiteUnityKit.DataTable"/> with the specified row.
		/// </summary>
		/// <param name="row">Row.</param>
		public DataRow this [int row] {
			get {
				return Rows [row];
			}
		}
	    
		/// <summary>
		/// Adds the row.
		/// </summary>
		/// <param name="values">Values.</param>
		public void AddRow (object[] values) {
			if (values.Length != Columns.Count) {
				throw new IndexOutOfRangeException ("The number of values in the row must match the number of column");
			}
	        
			var row = new DataRow (this.Columns);
			for (int i = 0; i < values.Length; i++) {
				row [i] = values [i];
			}
	        
			AddRow(row);
		}

		/// <summary>
		/// Adds the row.
		/// </summary>
		/// <param name="row">Row.</param>
		public void AddRow(DataRow row) {
			Rows.Add (row);
		}
			
		/// <summary>
		/// Releases all resource used by the <see cref="SQLiteUnityKit.DataTable"/> object.
		/// </summary>
		/// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="SQLiteUnityKit.DataTable"/>. The
		/// <see cref="Dispose"/> method leaves the <see cref="SQLiteUnityKit.DataTable"/> in an unusable state. After calling
		/// <see cref="Dispose"/>, you must release all references to the <see cref="SQLiteUnityKit.DataTable"/> so the
		/// garbage collector can reclaim the memory that the <see cref="SQLiteUnityKit.DataTable"/> was occupying.</remarks>
		public void Dispose () {
			if(Columns != null) {
				Columns.Clear ();
				Columns = null;
			}
			
			if(Rows != null) {
				Rows.ForEach(row => row.Dispose());
				Rows.Clear ();
				Rows = null;
			}
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="SQLiteUnityKit.DataTable"/>.
		/// </summary>
		/// <returns>A <see cref="System.String"/> that represents the current <see cref="SQLiteUnityKit.DataTable"/>.</returns>
		public override string ToString () {
			System.Text.StringBuilder table = new System.Text.StringBuilder();

			//Header
			table.Append(string.Join(", ", Columns.ToArray()));
			table.AppendLine();

			//Rows
			foreach(var row in Rows) {
				table.AppendLine(row.ToString());
			}

			return table.ToString();
		}
	}
}