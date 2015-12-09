# BasicDB
A simple database access interface for Unity3D

This framework provides a simple interface to SQLite. It also provides migration support and contains an Editor tool to query the database from the Unity3D editor.

## Download

You can download the [Unity Plugin](https://github.com/JuneSoftware/BasicDB/raw/master/June.BasicDB.unitypackage) and import it directly in your project.

Alternatively, you may clone this repository.

## Usage

The Database.cs file contains all the properties and methods to access the database. 

These are the 3 fundamental methods to interact with the database:

```csharp
// Executes a SQL statement against the connection and returns the number of rows affected.
public static int ExecuteNonQuery (string sql)

// Executes the query, and returns the first column of the first row in the result set returned by the query.
public static object ExecuteScalar (string sql)

// Executes the query, and returns the entire DataTable.
public static DataTable ExecuteReader (string sql)
```

### Querying a table

Assume the following table `Scores` in the database:

| id | name  | score |
|----|---------|-----|
| 1  | gaurang | 52  |  
| 2  | avinash | 54  |  
| 3  | nikhil  | 53  |  

```csharp
var dataTable = June.BasicDB.Database.ExecuteQuery("SELECT * FROM Scores");

var row = dataTable[2];		// This gets the DataRow object
var name1 = row["name"];	// name will contain "nikhil"

// These statements get the values in the cells directly
var score1Col = dataTable[0]["score"];	// score1Col will contain 52
var score1Idx = dataTable[0][2];		// score1Idx will contain 52
var name2Col = dataTable[1]["name"];	// name2 will contain "avinash"
var name2Idx = dataTable[1][1];		// name2Idx will contain "avinash"
```
The DataTable object will allow access to the DataRow using the indexer,
Values from the DataRow object can then be fetched by using a column number or the column name as seen from the examples above.



#### Advanced Querying

You can also use a generalized form of the ExecuteQuery method which can return objects in your specified format.

```csharp

class PlayerScore : BaseModel {
	public int Id { get { return GetInt("id"); } }
	
	public string Name { get { return GetString("name"); } }
	
	public int Score { get { return GetInt("score"); } }
	
	public PlayerScore(IDictionary<string, object> doc) : base(doc) { }
		
	public static List<PlayerScore> GetPlayerScoresFromDataTable(DataTable table) {
		List<PlayerScore> scores = new List<PlayerScore>();
		for(int row=0; row<table.Rows.Count; row++) {
			scores.Add(new PlayerScore(table[row]));		}
		return scores;	}}


var scores = June.BasicDB.Database.ExecuteQuery<List<PlayerScore>>(
				sql: "SELECT * FROM Scores",
				handler: PlayerScore.GetPlayerScoresFromDataTable);


```


Here are the properties that can be modified:

```csharp
// Database file name
public static string FILE_NAME = "BasicDB.db";

// Database file path
internal static readonly string DB_CONNECTION_STRING = Path.Combine(Application.persistentDataPath, FILE_NAME);

// The latest version of the schema
internal static readonly float DB_VERSION_LATEST = 1.0f;

// The schema versions with their migration scripts
// The migration script needs to be present in the Resources folder
internal static List<KeyValuePair<float, string>> DB_VERSIONS = new List<KeyValuePair<float, string>> () { 
			new KeyValuePair<float, string> (1.0f, "Schema1"),
			new KeyValuePair<float, string> (2.0f, "Schema2")
		};
```

