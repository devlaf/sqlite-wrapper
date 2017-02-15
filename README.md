SQLiteWrapper
===

A simple wrapper for the ADO.NET SQLite libraries.  Manages connection settings and provides a slightly more useful interface on top of the lower-level SQLiteConection stuff.

Building
---
As of version 2.0, this library is designed to build against .net core.

Given the recent (and developing) situation with decision in the .net core effort to [transition to the csproj project format from the previous project.json format,](https://blogs.msdn.microsoft.com/dotnet/2016/11/16/announcing-net-core-tools-msbuild-alpha/) there are some restrictions on how code in this repo may be built.  This project has opted to support the new csproj format going forward; however, given the state of tooling at the current time what that means is that **building this project will require either Visual Studio 2017 RC or a version of the dotnet CLI tools greater than 1.0.0-RC3.**

Running Tests
---
To run the integration tests, you may either:
- Open the SQLiteWrapper.sln in VS2017RC, select "Build All", and then use the TestExplorer window
- Navigate to the test/SQLiteWrapperIntegrationTests directory and run ```dotnet restore``` and ```dotnet test```


Simple Code Sample: Make a table
---

```C#
const string tableName = "my_table_name";
IDatabase Database = new SQLiteDatabase(locationInfo);
string sql = string.Format("CREATE TABLE IF NOT EXISTS '{0}' (KEY, VALUE, TYPE);", tableName);
Database.ExecuteNonQuery(sql);
```

Simple Code Sample: Get data as a structured table
---

```C#
const string tableName = "my_table_name";
IDatabase Database = new SQLiteDatabase(locationInfo);
string sql = string.Format("SELECT * from '{0}';", tableName);
List<Dictionary<string, object>> table = db.GetTable(sql);
int numRows = table.Count;
```

License (BEER-WARE, Revision 42)
---
**Note:**
Thanks (and perhaps a beer) goes to [Poul-Henning Kamp](https://people.freebsd.org/~phk/) for authoring the following beerware license.

**License Text:**
```
----------------------------------------------------------------------------
THE BEER-WARE LICENSE" (Revision 42):
[devlaf] wrote this file.  As long as you retain this notice you
can do whatever you want with this stuff. If we meet some day, and you think
this stuff is worth it, you can buy me a beer in return devlaf
----------------------------------------------------------------------------
```
