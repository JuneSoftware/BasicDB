using UnityEngine;
using System.Collections;
using System;
using System.Text;

public class BasicDBDemo : MonoBehaviour {
	
	const float BUTTON_WIDTH = 150f;
	const float TEXT_HEIGHT = 150;

	static GUIStyle TITLE_STYLE = new GUIStyle() {
		fontSize = 20
	};

	private string _Text = string.Empty;
	private System.Text.StringBuilder _log;

	/// <summary>
	/// Use this for initialization
	/// </summary>
	void Start () {
		_Text = string.Empty;
		_log = new System.Text.StringBuilder();
	}

	/// <summary>
	/// Update is called once per frame
	/// </summary>
	void Update () {
		
	}

	/// <summary>
	/// Renders the GUI
	/// </summary>
	void OnGUI() {
		RenderDemoScene();
	}

	/// <summary>
	/// Renders the demo scene.
	/// </summary>
	void RenderDemoScene() {
		GUILayout.BeginVertical();
		RenderTitle();
		RenderUserInputs();
		RenderLog();
		GUILayout.EndVertical();
	}

	/// <summary>
	/// Renders the title.
	/// </summary>
	void RenderTitle() {
		GUILayout.BeginHorizontal();
		GUILayout.Label("BasicDB Demo", TITLE_STYLE);
		GUILayout.EndHorizontal();
	}

	/// <summary>
	/// Renders the user inputs.
	/// </summary>
	void RenderUserInputs() {
		GUILayout.BeginVertical();
		GUILayout.Label("Database Connection String: ");
		GUILayout.Label(June.BasicDB.Database.DB_CONNECTION_STRING);
		_Text = GUILayout.TextArea(_Text, GUILayout.ExpandWidth(true), GUILayout.Height(TEXT_HEIGHT));
		GUILayout.BeginHorizontal();
		RenderExecuteNonQuery();
		RenderExecuteScalar();
		RenderExecuteReader();
		GUILayout.EndHorizontal();
		GUILayout.EndVertical();
	}

	/// <summary>
	/// Renders the execute non query.
	/// </summary>
	void RenderExecuteNonQuery() {
		RenderExecuteButton("Execute Non Query", 
            () => Log("Rows modified : " + June.BasicDB.Database.ExecuteNonQuery(_Text)));
	}

	/// <summary>
	/// Renders the execute scalar.
	/// </summary>
	void RenderExecuteScalar() {
		RenderExecuteButton("Execute Scalar", 
            () => { 
				object obj = June.BasicDB.Database.ExecuteScalar(_Text);
				Log(null != obj ? obj.ToString() : "<null>"); 
			});
	}

	/// <summary>
	/// Renders the execute reader.
	/// </summary>
	void RenderExecuteReader() {
		RenderExecuteButton("Execute Reader",
            () => RenderDataTable(June.BasicDB.Database.ExecuteReader(_Text)));
	}

	/// <summary>
	/// Renders the execute button.
	/// </summary>
	/// <param name="buttonText">Button text.</param>
	/// <param name="action">Action.</param>
	void RenderExecuteButton(string buttonText, Action action) {
		if(GUILayout.Button(buttonText, GUILayout.Width(BUTTON_WIDTH))) {
			Log (_Text);
			try {
				action();
			}
			catch(Exception ex) {
				Log ("ERROR:\n" + ex.ToString());
			}
		}
	}

	Vector2 _TableScrollPosition;
	/// <summary>
	/// Renders the data table.
	/// </summary>
	/// <param name="table">Table.</param>
	/// <param name="widthPerCharacter">Width per character.</param>
	void RenderDataTable(SQLiteUnityKit.DataTable table) {
		if(null == table || null == table.Columns) {
			GUILayout.Label("DataTable is NULL");
			return;
		}
		
		int[] columnWidths = new int[null != table.Columns ? table.Columns.Count : 0];
		
		//Calculate size of each table
		for(int i=0; i<table.Columns.Count; i++) {
			columnWidths[i] = Mathf.Max(2, table.Columns[i].Length);
		}
		foreach(var row in table.Rows) {
			for(int j=0; j<columnWidths.Length; j++) {
				columnWidths[j] = Mathf.Max(columnWidths[j], row[j].ToString().Length);
			}
		}

		//Create string format
		string rowFormat = string.Empty;
		for(int i=0 ; i<columnWidths.Length; i++) {
			rowFormat += string.Format("{{{0},-{1}}}", i, columnWidths[i] + 1);
		}

		StringBuilder tableStr = new StringBuilder();
		tableStr.AppendLine("Rows: " + table.Rows.Count + " Columns:" + table.Columns.Count);
		tableStr.AppendFormat(rowFormat, table.Columns.ToArray());
		tableStr.AppendLine();

		foreach(var row in table.Rows) {
			tableStr.AppendFormat(rowFormat, row.Data);
			tableStr.AppendLine();
		}

		string str = tableStr.ToString();
		Log (str);
		Debug.Log(str);
	}

	Vector2 _LogScroll;
	/// <summary>
	/// Renders the log.
	/// </summary>
	void RenderLog() {
		GUILayout.Label("--- --- --- LOG --- --- ---", GUILayout.ExpandWidth(true));
		_LogScroll = GUILayout.BeginScrollView(_LogScroll, false, true);
		GUILayout.Label(_log.ToString(), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		GUILayout.EndScrollView();
	}

	/// <summary>
	/// Log the specified message.
	/// </summary>
	/// <param name="message">Message.</param>
	void Log(string message) {
		_log.AppendFormat("[{0:HH:mm:ss}] {1}", System.DateTime.Now, message);
		_log.AppendLine();
	}
}
