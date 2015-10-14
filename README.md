A C# Serializer for storing objects to a SQLite Database in a human-readable format
-----------------------------------------------------------------------------------
Current Status: **BETA**<br />
Current Version: **1.0.1b**

The goal of this project was to be able to serialize an object to a SQLite database
and visually be able to inspect the tables and columns to see your type stored
inside.

The project handles simple and complex objects such as arrays, generics, or compound
classes. Each property of a class is looked at and if it is not a simple type
then it is given it's own table in the database and the process runs recusively.