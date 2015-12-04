# BasicDB
A simple database access interface for Unity3D

This framework provides a simple interface to SQLite. It also provides migration support and contains an Editor tool to query the database from the Unity3D editor.

## Download

You can download the [Unity Plugin](https://github.com/JuneSoftware/BasicDB/raw/master/June.BasicDB.unitypackage) and import it directly in your project.

Alternatively, you may clone this repository.

## Usage

The Database.cs file contains all the properties and methods to access the database. 

These are the 3 basic methods to interact with the database:

```csharp
// Executes a SQL statement against the connection and returns the number of rows affected.
public static int ExecuteNonQuery (string sql)

// Executes the query, and returns the first column of the first row in the result set returned by the query.
public static object ExecuteScalar (string sql)

// Executes the query, and returns the entire DataTable.
public static DataTable ExecuteReader (string sql)
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

