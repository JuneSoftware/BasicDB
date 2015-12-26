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

You can also use the the Database Window to interact with the database in Unity3d, the editor window can be opened from the `June => Database` menu.

Here is a screenshot of the Demo scene and the Database Editor window which lets you interact with the database:

![BasicDB Demo Scene](https://raw.githubusercontent.com/JuneSoftware/BasicDB/master/screenshots/1UnityDemo.png)

In the above image we can see there are 3 sections in the Database Window

- **Database**
  : The Database tab allows the user to execute queries on the database.

![Editor Database Tab](https://raw.githubusercontent.com/JuneSoftware/BasicDB/master/screenshots/2DatabaseTab.png)
  
- **Settings**
  : The Settings tab allows the user to create/update/remove keys from the settings table in the database.


![Editor Settings Tab](https://raw.githubusercontent.com/JuneSoftware/BasicDB/master/screenshots/3SettingsTab.png)

- **Migrations** 
  : The Migrations tab allows the user to check the current schema version and migrate the database to newer schemas.

![Editor Migrations Tab](https://raw.githubusercontent.com/JuneSoftware/BasicDB/master/screenshots/4MigrationsTab.png)


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

### Settings

The [Database.cs](https://github.com/JuneSoftware/BasicDB/raw/master/Assets/June/BasicDB/Database.cs) class also provides a rudimentary key-value store in the form of a `settings` table.

There are two constants in Database.cs file which control the settings table.

```csharp
// This specifies the name of the settings table
static readonly string DB_SETTINGS_TABLE_NAME = "settings";

// This generates the create statement for the settings table
static readonly string DB_SETTINGS_CREATE_SQL = "CREATE TABLE IF NOT EXISTS " + DB_SETTINGS_TABLE_NAME + " ('key' VARCHAR PRIMARY KEY, 'value' VARCHAR NOT NULL)";	
```

These are the methods which interact with this table:

```csharp
// Determines whether Settings table is present.
private static bool IsSettingsTablePresent ()
		
// Creates the settings table.
private static bool CreateSettingsTable ()
		
// Gets the settings value.
public static string GetSettingsValue (string key)
		
// Sets the settings value.
public static bool SetSettingsValue (string key, string value)
```

The settings table is used by the Migration functionality to keep a track of the current database schema version.

### Migrations

The [Database.cs](https://github.com/JuneSoftware/BasicDB/raw/master/Assets/June/BasicDB/Database.cs) class also provides a basic forward only migration system.

To setup the migration you need to provide the following information

```csharp
// Schema Scripts
static List<KeyValuePair<float, string>> DB_VERSIONS = new List<KeyValuePair<float, string>> () { 
			new KeyValuePair<float, string> (1.0f, "Schema1"),
			new KeyValuePair<float, string> (2.0f, "Schema2")
		};

// DB Version Settings Key
static readonly string DB_VERSION_SETTING_KEY = "DB_VER";

// The latest schema version which this application expects
static readonly float DB_VERSION_LATEST = 2.0f;
```
**DB_VERSIONS : **
This contains a list of key-pairs of _( Version No & Resource Name )_, the resource name is the name of the file present in the [Resources] (https://github.com/JuneSoftware/BasicDB/tree/master/Assets/Resources) folder containing the corresponding migration script

**DB_VERSION_SETTING_KEY : **
This is the key used to store the current schema version in the settings table in the database

**DB_VERSION_LATEST : **
If the current schema version in the settings table is lesser than this then the migration system will execute the migration scripts till it reaches this value

If the above values have be specified, the developer only needs to call `June.BasicDB.Database.MigrateSchemaToLatest ()` when the app starts to check and migrate the current database to the latest schema.

Here are a few examples of the migration scripts:

This first schema version defines the basic table structure for the current game

**Schema1**

```sql
CREATE TABLE IF NOT EXISTS players (
	'id' INTEGER PRIMARY KEY,
	'name' VARCHAR NOT NULL
);

CREATE TABLE IF NOT EXISTS items (
	'id' INTEGER PRIMARY KEY,
	'name' VARCHAR NOT NULL
);

---

INSERT OR REPLACE INTO settings VALUES ('DB_VER','1');
```
The next release might add a player level feature.

_**NOTE :** After successful execution of migration script the `DB_VER` value in the settings table should be updated to reflect the schema version the script belongs to_

**Schema2**

```sql
ALTER TABLE players ADD COLUMN level INTEGER NOT NULL DEFAULT 1;

---

INSERT OR REPLACE INTO settings VALUES ('DB_VER','2');
```
The migration scripts can be used to change the structure of the database schema as well as the data present in the schema.

These are the other methods which can be used to interact with the migration system:

```csharp
// Gets the current schema version.
public static float GetCurrentSchemaVersion ()	

// Determines whether database schema is up to date.
public static bool IsSchemaUpToDate ()
		
// Migrates the schema to version.
public static bool MigrateSchemaToVersion (float versionNo)
		
// Gets the next schema version.
public static float GetNextSchemaVersion (float currentVersion)
```


## Advanced Querying

** TODO: NEED TO ADD LINKS TO June.Core & EXPLAIN BaseModel **

You can also use a generalized form of the ExecuteQuery method which can return objects in your specified type.

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

