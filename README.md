SQLiteWrapper
===

A simple wrapper for the ADO.NET SQLite libraries.  Manages connection settings and provides a slightly more useful interface on top of the lower-level SQLiteConection stuff.


Simple Code Sample: Make a table
---

```C#
const string configTableName = "my_table_name";
IDatabase Database = new SQLiteDatabase(locationInfo);
string sql = string.Format("CREATE TABLE IF NOT EXISTS '{0}' (KEY, VALUE, TYPE);", configTableName);
Database.ExecuteNonQuery(sql);
```

License (BEER-WARE, Revision 42)
---
**Note:** 
A tip of the hat (and perhaps a beer) goes to [Poul-Henning Kamp](https://people.freebsd.org/~phk/) for authoring the following beerware license.  

**License Text:** 
```
THE BEER-WARE LICENSE" (Revision 42):
[devlaf](devlaf@users.noreply.github.com) wrote this file.  As long as you retain this notice you can do whatever you want with this stuff. If we meet some day, and you think this stuff is worth it, you can buy me a beer in return devlaf
```