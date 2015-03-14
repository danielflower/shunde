Shunde
======

**Note:** This framework has not been updated for many years, but is here as an historical curiosty. It is however still in
use in many deployed projects.

An object-relational mapper for .NET. This framework was written in the early 2000s and is similar to frameworks such as
NHibernate, but with more emmphasis on storing objects where the classes are part of object hierarchies, and with much
less emphasis on abstracting away the Database. The framework assumes you are using SQL Server, and lets you write things
such as `WHERE` clauses using SQL directly, rather than in some abstract query language.

The unique feature is the way it maps class hierarchies to database tables. Imagine the following object hierarchy:

    Cat : Animal
    Dog : Animal
    Animal : DBObject

`DBObject` is a Shunde-provided class which means it is a class which is stored in the database.
There would be 4 tables in the database: `Cat`, `Dog`, `Animal` and `DBObject`. The `Cat` table would only have columns 
declared on the `Cat` class; the `Animal` table has only properties declared on the `Animal` class, etc. Every object
in the database has a row in the `DBObject` table (which stores a couple of other things like the Type of the object etc).

Mirroring hierarchies in database tables is an interesting approach to solving the Object/Relational DB impedence mismatch,
and also lets you target your foreign keys at any part of the hierarchy (e.g. a foreign key could point to any of the 4
tables, each giving a different meaning to it).
