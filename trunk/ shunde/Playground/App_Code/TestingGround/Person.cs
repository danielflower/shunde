using System;
using Shunde.Framework;

/// <summary>
/// Summary description for Person
/// </summary>

namespace TestingGround
{
	public class Person : DBObject
	{
		private string firstName;

		public string FirstName
		{
			get { return firstName; }
			set { firstName = value; }
		}

		private string surname;

		public string Surname
		{
			get { return surname; }
			set { surname = value; }
		}

		private string password;

		/// <summary>
		/// password
		/// </summary>
		public string Password
		{
			get { return password; }
			set { password = value; }
		}
	

		public override string FriendlyName
		{
			get
			{
				return firstName + " " + surname;
			}
		}

		/// <summary>Sets up the <see cref="ObjectInfo" /> for this class</summary>
		static Person()
		{

			DBTable tbl = new DBTable("Person", new DBColumn[] {
				new DBColumn( "firstName", typeof(string), 1, 200 ),
				new DBColumn( "surname", typeof(string), 1, 200 ),
				new DBColumn( "password", typeof(string), 1, 100 )
			});

			ObjectInfo.RegisterObjectInfo(typeof(Person), tbl);

		}

		/// <summary>Searchs for people</summary>
		public static Person[] SearchPeople(string name)
		{

			Type t = typeof(Person);

			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);

			String sql = oi.GetSelectStatement() + " WHERE DBObject.isDeleted = 0 AND (firstName + ' ' + surname) LIKE '%" + Shunde.Utilities.DBUtils.ParseSql(name) + "%'";

			return (Person[])DBObject.GetObjects(sql, t);

		}

	}
}