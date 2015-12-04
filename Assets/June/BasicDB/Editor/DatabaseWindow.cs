using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class DatabaseWindow : EditorWindow {

	/// <summary>
	/// The DB Query key.
	/// </summary>
	private const string DB_QUERY_KEY = "DatabaseWindow.Query";

	/// <summary>
	/// Opens the database window.
	/// </summary>
	[MenuItem("June/Database")]
	public static void OpenDatabaseWindow() {
		GetWindow<DatabaseWindow>("DatabaseWindow");
	}

	/// <summary>
	/// Gets or sets the query.
	/// </summary>
	/// <value>The query.</value>
	private string Query {
		get {
			return EditorPrefs.GetString(DB_QUERY_KEY);
		}
		set {
			EditorPrefs.SetString(DB_QUERY_KEY, value);
		}
	}
	private string _Result = null;
	private SQLiteUnityKit.DataTable _Table = null;
	private string[] _Tabs = { "Database", "Settings", "Migrations" };
	private Action[] _TabRenderer {
		get {
			return new Action[] {
				RenderDatabaseTab,	//"Database"
				() => { RenderSettingsTab(); RefreshSettingsTable(); },			//"Settings"
				() => { _CurrentSchemaVersion = -1; RenderMigrationsTab(); }	//"Migrations"
			};
		}
	}
	private int _TabIndex=0;

	/// <summary>
	/// GUI renderer.
	/// </summary>
	public void OnGUI() {
		EditorGUILayout.BeginVertical(); {

			EditorGUILayout.BeginHorizontal(); {
				EditorGUILayout.LabelField("Database : ", EditorStyles.boldLabel, GUILayout.MaxWidth(60f));
				EditorGUILayout.LabelField(June.BasicDB.Database.FILE_NAME, GUILayout.ExpandWidth(true));
			} EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal(); {
				EditorGUILayout.LabelField("Connection String : ", EditorStyles.boldLabel, GUILayout.MaxWidth(110f));
				EditorGUILayout.LabelField(June.BasicDB.Database.DB_CONNECTION_STRING);
			} EditorGUILayout.EndHorizontal();

			_TabIndex = GUILayout.Toolbar(_TabIndex, _Tabs);

			_TabRenderer[_TabIndex]();

		} EditorGUILayout.EndVertical();
	}

	#region Database Tab
	/// <summary>
	/// Renders the database tab.
	/// </summary>
	private void RenderDatabaseTab() {
		EditorGUILayout.BeginHorizontal(); {
			EditorGUILayout.LabelField("Query", EditorStyles.boldLabel, GUILayout.MaxWidth(50f));
			Query = EditorGUILayout.TextArea(Query, GUILayout.ExpandWidth(true), GUILayout.MaxHeight(100f));
		} EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal(); {
			EditorGUILayout.LabelField("Execute", EditorStyles.boldLabel, GUILayout.MaxWidth(50f));
			try {
				if(GUILayout.Button("Execute Reader")) {
					_Table = June.BasicDB.Database.ExecuteReader(Query);
					_Result = null == _Table ? "<NULL>" : null;
				}
				if(GUILayout.Button("Execute Scalar")) {
					_Result = June.BasicDB.Database.ExecuteScalar(Query).ToString();
				}
				if(GUILayout.Button("Execute Non Query")) {
					_Result = June.BasicDB.Database.ExecuteNonQuery(Query).ToString();
				}
				if(GUILayout.Button("Execute Script")) {
					_Result = June.BasicDB.Database.ExecuteScript(Query).ToString();
				}
			}
			catch(Exception ex) {
				_Result = ex.ToString();
			}
		} EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.BeginHorizontal(); {
			EditorGUILayout.LabelField("Result", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
			if(GUILayout.Button("Clear", GUILayout.MaxWidth(50f))) {
				_Result = string.Empty;
				_Table = null;
			}
		} EditorGUILayout.EndHorizontal();
		
		EditorGUILayout.Separator();
		
		if(!string.IsNullOrEmpty(_Result)) {
			EditorGUILayout.TextField(_Result, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		}
		else if(null != _Table) {
			RenderDataTable(_Table);
		}
	}


	Vector2 _TableScrollPosition;
	/// <summary>
	/// Renders the data table.
	/// </summary>
	/// <param name="table">Table.</param>
	private void RenderDataTable(SQLiteUnityKit.DataTable table, float widthPerCharacter = 6.8f) {
		if(null == table || null == table.Columns) {
			EditorGUILayout.LabelField("DataTable is NULL");
			return;
		}

		float[] columnWidths = new float[null != table.Columns ? table.Columns.Count : 0];

		//Calculate size of each table
		for(int i=0; i<table.Columns.Count; i++) {
			columnWidths[i] = Mathf.Max(20f, (float)table.Columns[i].Length * widthPerCharacter);
		}

		if(null != table.Rows) {
			foreach(var row in table.Rows) {
				for(int j=0; j<columnWidths.Length; j++) {
					if(null != row[j]) {
						columnWidths[j] = Mathf.Max(columnWidths[j], (float)(row[j].ToString()).Length * widthPerCharacter);
					}
				}
			}
		}

		EditorGUILayout.BeginVertical(); {

			_TableScrollPosition = EditorGUILayout.BeginScrollView(_TableScrollPosition); {
				// Render Table Header
				EditorGUILayout.BeginHorizontal(); {
					for(int i=0; i<table.Columns.Count; i++) {
						EditorGUILayout.LabelField(table.Columns[i], EditorStyles.boldLabel, GUILayout.MaxWidth(columnWidths[i]));
					}
				} EditorGUILayout.EndHorizontal();

				foreach(var row in table.Rows) {
					//Render row
					EditorGUILayout.BeginHorizontal(); {
						for(int i=0; i<table.Columns.Count; i++) {
							string item = null != row[i] ? row[i].ToString() : string.Empty;
							EditorGUILayout.LabelField(
								new GUIContent(item, item),
								GUILayout.MaxWidth(columnWidths[i]));
						}
					} EditorGUILayout.EndHorizontal();
				}
			} EditorGUILayout.EndScrollView();
		} EditorGUILayout.EndVertical();
	}
	#endregion

	#region Settings Tab

	private string _SettingsKeyGet;
	private string _SettingsValueGet;
	private string _SettingsKeySet;
	private string _SettingsValueSet;
	private Vector2 _SettingsScrollPosition;
	private SQLiteUnityKit.DataTable _SettingsTable;
	/// <summary>
	/// Renders the settings tab.
	/// </summary>
	private void RenderSettingsTab() {
		try {
			string tableName = June.BasicDB.Database.DB_SETTINGS_TABLE_NAME;

			EditorGUILayout.BeginHorizontal(); {
				EditorGUILayout.LabelField("Table : ", EditorStyles.boldLabel, GUILayout.MaxWidth(60f));
				EditorGUILayout.LabelField(tableName);
			} EditorGUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			EditorGUILayout.BeginHorizontal(); {
				EditorGUILayout.LabelField("Key : ", EditorStyles.boldLabel, GUILayout.MaxWidth(60f));
				_SettingsKeySet = EditorGUILayout.TextField(_SettingsKeySet, GUILayout.ExpandWidth(true));
				EditorGUILayout.LabelField("Value : ", EditorStyles.boldLabel, GUILayout.MaxWidth(60f));
				_SettingsValueSet = EditorGUILayout.TextField(_SettingsValueSet, GUILayout.ExpandWidth(true));
				if(GUILayout.Button("Set", GUILayout.MaxWidth(80f))) {
					June.BasicDB.Database.SetSettingsValue(_SettingsKeySet, _SettingsValueSet);
					RefreshSettingsTable();
				}
			} EditorGUILayout.EndHorizontal();

			EditorGUILayout.Separator();

			EditorGUILayout.BeginHorizontal(); {
				EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
				if(GUILayout.Button("Refresh", GUILayout.MaxWidth(80f))) {
					RefreshSettingsTable();
				}
			} EditorGUILayout.EndHorizontal();

			if(null != _SettingsTable) {
				RenderDataTable(_SettingsTable, 10f);
			}
		}
		catch(Exception ex) {
			Debug.Log(ex);
		}
	}

	/// <summary>
	/// Refresh's the settings table.
	/// </summary>
	private void RefreshSettingsTable() {
		_SettingsTable = June.BasicDB.Database.ExecuteReader("SELECT * FROM " + June.BasicDB.Database.DB_SETTINGS_TABLE_NAME);
	}

	#endregion

	#region Migrations Tab

	private float _CurrentSchemaVersion = -1f;
	private float _SelectedSchemaVersion = 0f;
	/// <summary>
	/// Renders the migrations tab.
	/// </summary>
	private void RenderMigrationsTab() {
		try {
			EditorGUILayout.BeginHorizontal(); {
				EditorGUILayout.LabelField("Current Schema Version : ", EditorStyles.boldLabel, GUILayout.MaxWidth(150f));
				if(-1 == _CurrentSchemaVersion) {
					_CurrentSchemaVersion = June.BasicDB.Database.GetCurrentSchemaVersion();
				}
				EditorGUILayout.LabelField(_CurrentSchemaVersion.ToString());
				EditorGUILayout.LabelField("Latest Schema Version : ", EditorStyles.boldLabel, GUILayout.MaxWidth(150f));
				EditorGUILayout.LabelField(June.BasicDB.Database.DB_VERSION_LATEST.ToString());
			} EditorGUILayout.EndHorizontal();

			EditorGUILayout.Separator();


			EditorGUILayout.BeginHorizontal(); {
				EditorGUILayout.LabelField("Version", EditorStyles.boldLabel, GUILayout.MaxWidth(100f));
				_SelectedSchemaVersion = EditorGUILayout.FloatField(_SelectedSchemaVersion, GUILayout.MaxWidth(100f));
				if(GUILayout.Button("Migrate To Version")) {
					if(EditorUtility.DisplayDialog("Are you sure", "Do you wish to migrate the current database to version " + _SelectedSchemaVersion + " ?", "Yes", "No")) {
						June.BasicDB.Database.MigrateSchemaToVersion(_SelectedSchemaVersion);
					}
				}
				if(GUILayout.Button("Migrate To Latest")) {
					if(EditorUtility.DisplayDialog("Are you sure", "Do you wish to migrate the current database to latest schema?", "Yes", "No")) {
						June.BasicDB.Database.MigrateSchemaToLatest();
					}
				}
			} EditorGUILayout.EndHorizontal();


			EditorGUILayout.Separator();

			// Print Migration List
			EditorGUILayout.BeginVertical(); {
				EditorGUILayout.LabelField("Database Versions", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));

				//Render Header
				EditorGUILayout.BeginHorizontal(); {
					EditorGUILayout.LabelField("Version", EditorStyles.boldLabel, GUILayout.MaxWidth(100f));
					EditorGUILayout.LabelField("Resource", EditorStyles.boldLabel, GUILayout.MaxWidth(100f));
				} EditorGUILayout.EndHorizontal();

				foreach(var kv in June.BasicDB.Database.DB_VERSIONS) {
					//Render row
					EditorGUILayout.BeginHorizontal(); {
						EditorGUILayout.LabelField(kv.Key.ToString(), GUILayout.MaxWidth(100f));
						//EditorGUILayout.LabelField(kv.Value, GUILayout.MaxWidth(100f));
						if(GUILayout.Button(kv.Value)) {
							EditorGUIUtility.PingObject(Resources.Load(kv.Value));
						}
					} EditorGUILayout.EndHorizontal();			
				}
			} EditorGUILayout.EndVertical();
		}
		catch(Exception ex) {
			Debug.Log(ex);
		}
	}

	#endregion
}