A C# Serializer for storing objects to a SQLite Database in a human-readable format
-----------------------------------------------------------------------------------
Current Status: **BETA**<br />
Current Version: **1.0.4b**

The goal of this project is to be able to serialize an object to a SQLite database
and visually be able to inspect the tables and columns to see your type stored
inside.

The project handles simple and complex objects such as arrays, generics, or compound
classes. Each property of a class is looked at and if it is not a simple type
then it is given it's own table in the database and the process runs recursively.

Currently, handles all known objects passed to the serializer regardless of the
Serializable attribute being set on the class.

Known Issues
------------
- Doesn't handle abstract field types with sub-class types stored inside of them. Ends up trying to serialize as the abstract class.
- Multi-dimensional arrays are not supported, but will be soon.
- Dynamic objects are not supported and may not be for a long time.

* If you find any issues with objects or types that are not handled properly, please report them and provide a working example, if possible.