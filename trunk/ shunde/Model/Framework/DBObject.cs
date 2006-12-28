using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using Shunde.Utilities;

namespace Shunde.Framework
{
	/// <summary>Represents a Database object.</summary>
	/// <remarks>
	/// 	<para>This is the base type of a framework used for applications that save object info in SQL Server. Each object extends this object, with that objects columns specified by an array of <see cref="DBColumn" /> objects. This base class implements Code that will save and populate objects.</para>
	/// </remarks>
	public abstract class DBObject
	{


		private int id = -1;

		/// <summary>A unique (to the whole database) ID to identify this object</summary>
		/// <remarks>Shunde automatically generates a new ID for your object when you save a new object.</remarks>
		public int Id
		{
			get { return id; }
			set { id = value; }
		}

		/// <summary>
		/// Gets a Name that can be displayed to the end user for this object, if such a Name exists
		/// </summary>
		public virtual string FriendlyName
		{
			get { return ToString(); }
		}

		/// <summary>The full class Name of this object, including namespace, eg. Shunde.DBObject</summary>
		/// <remarks>This is used to know which object type to create when creating a new DBObject. The full class Name can be gotten by getting the FullName property of a Type object</remarks>
		private string className;

		/// <summary>Specifies that this object is deleted</summary>
		/// <remarks>Rather than deleting an object from the database, it is just marked as deleted. The lastUpdate field of the object should be the date that it was deleted on, and lastUpdatedBy should hold the Name of the user that deleted it.</remarks>
		public bool IsDeleted
		{
			get
			{
				return isDeleted;
			}
			set
			{
				isDeleted = value;
			}
		}

		private bool isDeleted = false;


		private int displayOrder;

		/// <summary>The relative (to other DBObjects) display order that this DBObject should appear in</summary>
		/// <remarks>The use of this is optional, but if you want to order some objects in a specific way, then you can give different objects different display orders, and then when retrieving objects use "ORDER BY DBObject.displayOrder ASC" in your SQL.</remarks>
		public int DisplayOrder
		{
			get { return displayOrder; }
			set { displayOrder = value; }
		}
	


		/// <summary>The update id, used to see if an object can be updated without concurrency issues.</summary>
		/// <remarks>This is incremented each time an object is saved.</remarks>
		private int updateId;

		/// <summary>The last time that this object was updated.</summary>
		/// <remarks>This is automatically updated each time the object is saved.</remarks>
		public DateTime LastUpdate
		{
			get
			{
				return lastUpdate;
			}
		}

		private DateTime lastUpdate;



		private string lastUpdatedBy;

		/// <summary>The person or user who last updated this object</summary>
		public string LastUpdatedBy
		{
			get { return lastUpdatedBy; }
			set { lastUpdatedBy = value; }
		}



		private bool hasBeenPopulated = false;


		/// <summary>The object info for the class that this object is an instance of</summary>
		/// <remarks>This is just a reference to the <see cref="ObjectInfo" /> in the objectInfoHT HastTable</remarks>
		private ObjectInfo objectInfo;



		/// <summary>Creates a new DBObject</summary>
		/// <remarks>This Constructor loads the reference to the <see cref="ObjectInfo" /> of the class that this type belongs to.</remarks>
		public DBObject()
		{

			string hashName = this.GetType().FullName;
			objectInfo = ObjectInfo.objectInfoHT[hashName];

		}


		/// <summary>Sets up the <see cref="ObjectInfo">ObjectInfo</see> for this class. Each class has an ObjectInfo, and each instance of the class has a reference to it.</summary>
		static DBObject()
		{

			DBColumn cnCol = new DBColumn("className", typeof(string), 1, 100);
			cnCol.Constraints = "className LIKE '_%._%'";

			DBTable tbl = new DBTable("DBObject", new DBColumn[] {
				cnCol,
				new DBColumn( "isDeleted", typeof(bool), false),
				new DBColumn( "displayOrder", typeof(int), false, -320000, 32000 ),
				new DBColumn( "updateId", typeof(int), false),
				new DBColumn( "lastUpdate", typeof(DateTime), false ),
				new DBColumn( "lastUpdatedBy", typeof(string), 0, 100 ),
			});

			ObjectInfo.RegisterObjectInfo(typeof(DBObject), tbl);

		}

		/// <summary>
		/// Resets the Id of the object so that when saved again, a new object will be created in the database.
		/// </summary>
		/// <remarks>This is useful for those times when you would like to clone an existing object. Simply populate it, call ResetId, and save</remarks>
		public void ResetId()
		{
			this.id = -1;
			this.hasBeenPopulated = false;
		}

		/// <summary>Populates this object from the database</summary>
		/// <exception cref="ObjectDoesNotExistException">Thrown if the specified Id is not found in the database.</exception>
		public virtual void Populate()
		{

			if (hasBeenPopulated)
			{
				return;
			}

			Type objType = GetType();

			string sql = objectInfo.GetSelectStatement() + " WHERE [DBObject].[id] = " + id;


			SqlDataReader sdr = DBUtils.ExecuteSqlQuery(sql);
			if (sdr.Read())
			{

				PopulateBaseProperties(sdr);

				sdr.Close();
			}
			else
			{
				sdr.Close();
				throw new ObjectDoesNotExistException("Tried to populate a " + objType + " with the id " + id + ", however this does not exist in the database. " + sql);
			}

			hasBeenPopulated = true;

		}

		/// <summary>Populates the data of all the <see cref="BinaryData" /> objects in this class</summary>
		/// <remarks>When <see cref="Populate" /> is called on an object, any Binary Data is not fully populated. This must be therefore called before trying to access the binary data.</remarks>
		public void PopulateBinaryData()
		{
			Type objType = GetType();
			foreach (DBTable table in objectInfo.Tables)
			{
				
				foreach (DBColumn col in table.Columns)
				{
					
					if (!(col.Type.Equals(typeof(BinaryData))))
					{
						continue;
					}

					FieldInfo fi = col.FieldInfo;
					SqlDataReader sdr = DBUtils.ExecuteSqlQuery("SELECT [" + col.Name + "] AS binaryData, [" + col.Name + "MimeType] AS mimeType, [" + col.Name + "Filename] AS filename FROM [" + table.Name + "] WHERE [id] = " + id);
					sdr.Read();

					string mimeType = sdr["mimeType"].ToString();
					string filename = sdr["filename"].ToString();


					object bd;
					if (mimeType.Length == 0)
					{
						// no data has been uploaded
						bd = null;
					}
					else
					{
						bd = new BinaryData((byte[])sdr["binaryData"], mimeType, filename);
					}
					fi.SetValue(this, bd);
					sdr.Close();

				}
			}
		}


		/// <summary>Populates the other DB objects in this class</summary>
		/// <remarks>This will populate all DBOBjects in this object, that are in the columns of the <see cref="DBTable" /> for this class. Any DBObjects that are null are not populated.</remarks>
		/// <exception cref="ObjectDoesNotExistException">Thrown if one of the DBObjects in this class has an invalid id.</exception>
		public void PopulateObjects()
		{
			Type objType = GetType();
			foreach (DBTable table in objectInfo.Tables)
			{
				
				foreach (DBColumn col in table.Columns)
				{
					
					if (col.isDBObjectType)
					{

						DBObject obj = (DBObject) col.FieldInfo.GetValue(this);
						if (obj != null)
						{
							obj.Populate();
						}
					}
				}
			}
		}

		/// <summary>Populates the properties of this class given a populated <see cref="SqlDataReader" /> object</summary>
		private void PopulateBaseProperties(SqlDataReader sdr)
		{
			PopulateBaseProperties(sdr, objectInfo, true);
		}

		/// <summary>Populates the properties of this class given a populated SqlDataReader object, using the <see href="ObjectInfo" /> to specify which fields should be populated</summary>
		/// <remarks>The <see href="ObjectInfo" /> is used to specify which fields of a class should be populated.</remarks>
		private void PopulateBaseProperties(SqlDataReader sdr, ObjectInfo objectInfo, bool useSdrIndex)
		{

			id = sdr.GetInt32(0);

			Type objType = GetType();
			foreach (DBTable table in objectInfo.Tables)
			{
				
				foreach (DBColumn col in table.Columns)
				{

					FieldInfo fi = col.FieldInfo;
					object value = (useSdrIndex) ? sdr[col.sdrIndex] : sdr[col.GetColumnName()];

					if (!col.isDBObjectType && !col.Type.Equals(typeof(BinaryData)))
					{

						if (value == DBNull.Value)
						{
							if (col.Type.Equals(typeof(short)))
							{
								value = DBColumn.ShortNullValue;
							}
							else if (col.Type.Equals(typeof(int)))
							{
								value = DBColumn.IntegerNullValue;
							}
							else if (col.Type.Equals(typeof(long)))
							{
								value = DBColumn.LongNullValue;
							}
							else if (col.Type.Equals(typeof(DateTime)))
							{
								value = DBColumn.DateTimeNullValue;
							}
							else if (col.Type.Equals(typeof(float)))
							{
								value = DBColumn.FloatNullValue;
							}
							else if (col.Type.Equals(typeof(double)))
							{
								value = DBColumn.DoubleNullValue;
							}
							else if (col.Type.Equals(typeof(string)))
							{
								value = "";
							}
						}

					}
					else if (col.isDBObjectType)
					{

						if (value == DBNull.Value)
						{
							value = null;
						}
						else
						{

							string foreignClassName = (useSdrIndex) ? sdr[col.sdrIndex + 1].ToString() : sdr[col.Name + "ClassName"].ToString();
							DBObject foreignKeyObject;
							foreignKeyObject = CreateObject(objType.Assembly, foreignClassName);
							foreignKeyObject.id = Convert.ToInt32(value);
							value = foreignKeyObject;

						}
					}
					else
					{
						if (value == DBNull.Value)
						{
							value = null;
						}
						else
						{
							string mimeType = (useSdrIndex) ? sdr[col.sdrIndex + 1].ToString() : sdr[col.Name + "MimeType"].ToString();
							string filename = (useSdrIndex) ? sdr[col.sdrIndex + 2].ToString() : sdr[col.Name + "Filename"].ToString();
							// the returned Value will not have the data, rather the data size
							BinaryData bd = new BinaryData((int)value, mimeType, filename);
							value = bd;
						}
					}

					try
					{
						fi.SetValue(this, value);
					}
					catch
					{
						throw;
//						throw new ShundeException("index: " + col.sdrIndex + ", fi: " + fi + " , this: " + this + " , Value: " + Value);
					}

				}
			}
		}

		/// <summary>Creates a new DBObject which belongs in the given Assembly with the classname, where the classname includes the namespace</summary>
		/// <remarks>First checks the <see cref="ObjectInfo.constructors" /> dictionary for the <see cref="ConstructorInfo" /> object. If not, it uses the Assembly to create a ConstructorInfo.</remarks>
		public static DBObject CreateObject(Assembly callingAssembly, String className)
		{



			// check the constructor hash Table to see if constructor is in there
			ConstructorInfo ci = null;
			if (ObjectInfo.constructors.ContainsKey(className))
			{
				ci = ObjectInfo.constructors[className];
			} else
			{

				// try to get the type from the assembly that the object is in
				Type type = callingAssembly.GetType(className);

				if (type == null)
				{
					// now try getting it from the Shunde assembly

					type = Type.GetType(className);


					if (type == null)
					{
						// go through all the loaded assemblies
						Assembly[] asms = System.AppDomain.CurrentDomain.GetAssemblies();
						foreach (Assembly ass in asms)
						{
							type = ass.GetType(className);
							if (type != null)
							{
								break;
							}
						}
					}

					if (type == null)
					{
						// try to load the assembly using the beginning of the class Name
						String assemblyName = className.Substring(0, className.IndexOf("."));
						Assembly ass = Assembly.Load(assemblyName);
						type = ass.GetType(className);

					}

				}

				// if type is still null, then guess the assembly that it is in
				// and load it from there
				if (type == null)
				{
					string[] temp = className.Split(new char[] { '.' });
					for (int i = temp.Length - 1; i >= 0; i--)
					{
						string guess = temp[i];
						try
						{
							Assembly asm = Assembly.Load(guess);
							type = asm.GetType(className);
							break;
						}
						catch (System.IO.FileNotFoundException)
						{
							// the guess was wrong, I guess
						}
					}
				}

				// if it's still null then i don't know what to do
				if (type == null)
				{
					throw new ShundeException("Could not load type: " + className);
				}

				ci = type.GetConstructors()[0];
				ObjectInfo.constructors[className] = ci;


			}
			return (DBObject)ci.Invoke(null);
		}

		/// <summary>Checks to see if this object exists in the database</summary>
		/// <remarks>Currently, if the id is less than 1 then it is considered to not exist. There is no check in the database for performance reasons. This may change later on.</remarks>
		public bool Exists()
		{
			return id > 0;
		}

		/// <summary>Saves this object to the database</summary>
		/// <exception cref="ValidationException">Thrown if the object fails the database constraints. The Message property contains detailed information on why it failed, which is suitable to be shown to the end user.</exception>
		/// <exception cref="ConcurrencyException">Thrown if an object is read and updated by more than one user at the same time. The first to save will have no problem, but if anyone tries to save subsequent to another save without re-reading the data, then this exception will be thrown. The Message property contains detailed information suitable to be shown to the end user.</exception>
		public virtual void Save()
		{

			lastUpdate = DateTime.Now;
			if (lastUpdatedBy != null && lastUpdatedBy.Length > 99)
			{
				lastUpdatedBy = lastUpdatedBy.Substring(0, 99);
			}

			Type objType = GetType();

			bool isNew = !Exists();
			bool isOk = true;
			String errorMessage = "";
			StringBuilder sql = new StringBuilder( "BEGIN TRANSACTION\n\n" );


			// the sql command that will run the statement
			SqlCommand sqlCommand = new SqlCommand();


			if (isNew)
			{
				sql.Append( "DECLARE @newId int\nSET @newId = (SELECT ISNULL(MAX([id]), 0) + 1 FROM DBObject)\n\n" );
				className = objType.FullName;
			}
			else
			{
				sql.Append( @"
IF (SELECT updateId FROM DBObject WHERE [id] = " + id + @") <> " + updateId + @"
BEGIN
   RAISERROR ('CONCURRENCY', 16, 1)
   RETURN
END
					");
				updateId++;
			}



			foreach (DBTable table in objectInfo.Tables)
			{
				

				StringBuilder columnNames = new StringBuilder();



				if (isNew)
				{
					sql.Append( "INSERT INTO [" + table.Name + "] ([id]{columnnames}) VALUES (\n\t@newId" );
					// if this Table has more than one column, then add a comma, ready to add more values on the end
					if (table.Columns.Length > 0)
					{
						sql.Append(  "," );
					}
				}
				else
				{
					// if this Table has only an ID, then don't update it
					if (table.Columns.Length == 0)
					{
						continue;
					}
					sql.Append( "UPDATE [" + table.Name + "] SET " );
				}

				int currentTableColumnCount = 0;
				foreach (DBColumn col in table.Columns)
				{

					Object value = col.FieldInfo.GetValue(this);


					try
					{
						col.Validate(this, value);
					}
					catch (ValidationException ve)
					{
						isOk = false;
						errorMessage += ve.Message + "\n";
					}
					catch (Exception)
					{
						if (!isNew)
						{
							updateId--;
						}
						throw new ShundeException("Error while validating column " + col.Name + " with value " + value);
					}

					if (DBColumn.IsColumnNull(value))
					{
						value = DBNull.Value;
					}

					// add the Value as a parameter to the statement
					SqlParameter param = new SqlParameter("@" + col.Name, DBUtils.GetSqlDbType(col));
					param.IsNullable = col.AllowNulls;
					sqlCommand.Parameters.Add(param);

					if (isNew)
					{
						sql.Append( "\n\t@" + col.Name );
						columnNames.Append( ", [" + col.GetColumnName() + "]" );
						if (col.isDBObjectType)
						{
							columnNames.Append( ", [" + col.Name + "ClassName]" );
							sql.Append( "\n\t, @" + col.Name + "ClassName" );
						}
						else if (col.Type.Equals(typeof(BinaryData)))
						{
							columnNames.Append( ", [" + col.Name + "MimeType], [" + col.Name + "Filename]" );
							sql.Append( "\n\t, @" + col.Name + "MimeType\n\t, @" + col.Name + "Filename" );
						}
					}
					else
					{
						sql.Append( "\n\t" + col.GetColumnName() + " = @" + col.Name);
						if (col.isDBObjectType)
						{
							sql.Append( ",\n\t" + col.Name + "ClassName = @" + col.Name + "ClassName" );
						}
						else if (col.Type.Equals(typeof(BinaryData)))
						{
							sql.Append(",\n\t" + col.Name + "MimeType = @" + col.Name + "MimeType");
							sql.Append( ",\n\t" + col.Name + "Filename = @" + col.Name + "Filename" );
						}
					}


					if (col.isDBObjectType)
					{
						SqlParameter classNameParam = new SqlParameter("@" + col.Name + "ClassName", SqlDbType.NVarChar);
						classNameParam.IsNullable = col.AllowNulls;
						sqlCommand.Parameters.Add(classNameParam);

						if (DBColumn.IsColumnNull(value))
						{
							classNameParam.Value = value;
							param.Value = value;
						}
						else
						{
							classNameParam.Value = value.GetType().FullName;
							param.Value = ((DBObject)value).Id;
						}
					}
					else if (col.Type.Equals(typeof(BinaryData)))
					{

						SqlParameter mimeTypeParam = new SqlParameter("@" + col.Name + "MimeType", SqlDbType.NVarChar);
						mimeTypeParam.IsNullable = col.AllowNulls;
						sqlCommand.Parameters.Add(mimeTypeParam);

						SqlParameter filenameParam = new SqlParameter("@" + col.Name + "Filename", SqlDbType.NVarChar);
						filenameParam.IsNullable = col.AllowNulls;
						sqlCommand.Parameters.Add(filenameParam);

						if (DBColumn.IsColumnNull(value))
						{
							mimeTypeParam.Value = value;
							filenameParam.Value = value;
							param.Value = value;
						}
						else
						{
							BinaryData bd = (BinaryData)value;
							mimeTypeParam.Value = bd.MimeType;
							filenameParam.Value = bd.Filename;
							param.Value = bd.Data;
						}

					}
					else
					{
						param.Value = value;
					}

					if (currentTableColumnCount < (table.Columns.Length - 1))
					{
						sql.Append( "," );
					}

					currentTableColumnCount++;

				}

				if (isNew)
				{
					sql = sql.Replace("{columnnames}", columnNames.ToString());
					sql.Append( "\n)\n\n" );
				}
				else
				{
					sql.Append( "\nWHERE [id] = " + id + "\n\n" );
				}


			}

			if (!isOk)
			{
				if (!isNew)
				{
					updateId--;
				}
				throw new ValidationException(errorMessage);
			}

			if (isNew)
			{
				sql.Append( "SELECT @newId AS intValue\n\n" );
			}

			sql.Append( "COMMIT\n\n" );


			

			if (isNew)
			{

				// In the case where two objects are being saved at the same time, there will
				// be a primary key violation.
				// When saving a new object, we catch Primary Key Violations, get a new id, and
				// try again. It keeps trying until it works (or times out - which it shouldn't do
				// really).

				/*
				if (isNew)
				{
					string h = "";
					foreach (SqlParameter p in sqlCommand.Parameters)
					{
						h += p.SourceColumn + ": " + p.Value + "\n";
					}
					throw new ShundeException(h);
				}
				*/

				bool completed = false;
				while (!completed)
				{

					try
					{
						sqlCommand.CommandText = sql.ToString();
						id = DBUtils.GetIntFromSqlSelect(sqlCommand);
						completed = true;
					}
					catch (ShundeSqlException sqlEx)
					{
						if (sqlEx.Message.StartsWith("Violation of PRIMARY KEY constraint"))
						{
							// do nothing, stay in while loop
						}
						else if (sqlEx.Message.IndexOf("conflicted with COLUMN CHECK constraint") > -1)
						{
							String colName = sqlEx.Message.Substring(sqlEx.Message.IndexOf(", column '"));
							colName = colName.Substring(colName.IndexOf("'") + 1);
							colName = colName.Substring(0, colName.IndexOf("'"));
							throw new ValidationException("You have entered an invalid value for " + TextUtils.MakeFriendly(colName) + ". Please try again.");
						}
						else
						{
							throw;
						}
					}
				}

			}
			else
			{
				try
				{
					sqlCommand.CommandText = sql.ToString();
					DBUtils.ExecuteSqlCommand(sqlCommand);
				}
				catch (ShundeSqlException sqlEx)
				{
					updateId--;
					if (sqlEx.Message.Equals("CONCURRENCY"))
					{
						throw new ConcurrencyException();
					}
					else if (sqlEx.Message.IndexOf("conflicted with COLUMN CHECK constraint") > -1)
					{
						String colName = sqlEx.Message.Substring(sqlEx.Message.IndexOf(", column '"));
						colName = colName.Substring(colName.IndexOf("'") + 1);
						colName = colName.Substring(0, colName.IndexOf("'"));
						throw new ValidationException("You have entered an invalid value for " + TextUtils.MakeFriendly(colName) + ". Please try again.");
					}
					else
					{
						throw;
					}
				}
				catch
				{
					updateId--;
					throw;
				}
			}



		}



		/// <summary>Gets a String representation of this object</summary>
		public override string ToString()
		{
			return "[" + GetType().FullName + ":" + id + "]";
		}

		/// <summary>Gets the hash key for this object</summary>
		public override int GetHashCode()
		{
			return id.GetHashCode();
		}

		/// <summary>
		/// Specifies whether this DBObject is equal to another
		/// </summary>
		/// <returns>True if the <see cref="Id" />s of each object are equal, and greater than 0</returns>
		public virtual bool Equals(DBObject another)
		{
			return this.id == another.id && this.id > 0;
		}

		/// <summary>Specifies whether this object is equal to another</summary>
		public override bool Equals(Object another)
		{
			DBObject otherDBO = another as DBObject;
			if (otherDBO != null)
			{
				return this.Equals(otherDBO);
			}
			return false;
		}









		/// <summary>Gets and (possibly partially) populates all the DBObjects for a given SQL Statement</summary>
		/// <remarks>The base type is the type that all the objects being retrieved extend. For example, if Notebook and Desktop both extend Computer, then the base type is Computer if getting all Notebooks and Desktops. The returned objects are then created as the type that they should be (ie. Notebooks and Desktops), however they are only partially populated up to the level of the base type (ie. Computer).</remarks>
		/// <param Name="query">The SQL query used to retrieve the objects</param>
		/// <param Name="baseType">The type of the lowest level DBObject.</param>
		public static DBObject[] GetObjects(string query, Type baseType)
		{

			SqlDataReader sdr = DBUtils.ExecuteSqlQuery(query);
			List<DBObject> objs = new List<DBObject>();

			ObjectInfo oi = ObjectInfo.GetObjectInfo(baseType);

			while (sdr.Read())
			{
				DBObject dbObj = CreateObject(baseType.Assembly, sdr["className"].ToString());
				dbObj.PopulateBaseProperties(sdr, oi, true);
				objs.Add(dbObj);
			}
			sdr.Close();

			DBObject[] objectArray = (DBObject[]) Array.CreateInstance(baseType, objs.Count);

			objs.CopyTo(objectArray);

			return objectArray;

		}




		/// <summary>Gets and populates all the DBObjects for a given SQL Statement using the Types given in extendingTypes</summary>
		/// <remarks>The base type is the type that all the objects being retrieved extend. For example, if Notebook and Desktop both extend Computer, then the base type is Computer if getting all Notebooks and Desktops. The returned objects are then created as the type that they should be (ie. Notebooks and Desktops), and fully populated if the type is included in the extendingTypes array. However, if the type is not included then it is only partially populated up to the level of the base type (ie. Computer).</remarks>
		/// <param Name="baseType">The type of the lowest level DBObject.</param>
		/// <param Name="extendingTypes">An optional array containing the extended types of objects that may be returned - if included then the returned objects will be fully populated.</param>
		/// <param Name="where">The SQL WHERE query</param>
		public static DBObject[] GetObjects(Type baseType, Type[] extendingTypes, string where)
		{

			ObjectInfo oi = ObjectInfo.GetObjectInfo(baseType);


			string query = "SELECT " + oi.GetJoinedColumnClause(extendingTypes) + " FROM " + oi.GetJoinedFromClause(extendingTypes) + " " + where;

			SqlDataReader sdr = DBUtils.ExecuteSqlQuery(query);
			List<DBObject> objects = new List<DBObject>();

			while (sdr.Read())
			{
				DBObject dbObj = CreateObject(baseType.Assembly, sdr["className"].ToString());

				ObjectInfo oiToUse = oi;
				Type curType = dbObj.GetType();

				if (extendingTypes != null)
				{
					foreach (Type extType in extendingTypes)
					{
						
						if (extType.Equals(curType))
						{
							oiToUse = ObjectInfo.GetObjectInfo(extType);
							break;
						}
					}
				}

				dbObj.PopulateBaseProperties(sdr, oiToUse, false);
				objects.Add(dbObj);
			}

			sdr.Close();

			DBObject[] objectArray = (DBObject[])Array.CreateInstance(baseType, objects.Count);
			objects.CopyTo(objectArray);

			return objectArray;

		}





		/// <summary>Gets and populates a single DBObject</summary>
		/// <remarks>This is a useful method when you do not know the type of object you will get back. Just pass in a SQL statement that returns one id.</remarks>
		/// <param Name="query">An SQL query that returns a single int which is the ID of the object</param>
		/// <exception cref="ValidationException">Thrown if the query returns more than one row.</exception>
		/// <exception cref="ObjectDoesNotExistException">Thrown if the specified ID is not found in the database.</exception>
		/// <returns>Returns a fully populated DBObject</returns>
		public static DBObject GetObject(string query)
		{

			if (query.Length == 0 || query.IndexOf("--") > -1 || query.IndexOf(')') < query.IndexOf('('))
			{
				throw new ObjectDoesNotExistException();
			}

			SqlDataReader sdr = DBUtils.ExecuteSqlQuery("SELECT [id], [className] FROM [DBObject] WHERE [id] IN (" + query + ")");

			if (!sdr.Read())
			{
				sdr.Close();
				throw new ObjectDoesNotExistException("The query " + query + " did not return any records.");
			}

			int id = System.Convert.ToInt32(sdr["id"]);
			string className = sdr["className"].ToString();

			if (sdr.Read())
			{
				sdr.Close();
				throw new ValidationException("The query " + query + " returned more than one row.");
			}

			sdr.Close();

			DBObject obj = CreateObject(Assembly.GetCallingAssembly(), className);
			obj.id = id;
			obj.Populate();

			return obj;


		}




		/// <summary>Gets and populates a single DBObject with the supplied ID</summary>
		/// <remarks>This is a useful method when you do not know the type of object you will get back.</remarks>
		/// <exception cref="ObjectDoesNotExistException">Thrown if the specified ID is not found in the database.</exception>
		/// <returns>Returns a fully populated DBObject</returns>
		public static DBObject GetObject(int id)
		{

			if (id < 1)
			{
				throw new ObjectDoesNotExistException();
			}

			SqlDataReader sdr = DBUtils.ExecuteSqlQuery("SELECT [className] FROM [DBObject] WHERE [id] = " + id);

			if (!sdr.Read())
			{
				sdr.Close();
				throw new ObjectDoesNotExistException("An object with the Id " + id + " does not exist.");
			}

			string className = sdr[0].ToString();


			sdr.Close();

			DBObject obj = CreateObject(Assembly.GetCallingAssembly(), className);
			obj.id = id;
			obj.Populate();

			return obj;

		}


		/// <summary>Gets and populates a single DBObject of the given type with the supplied ID</summary>
		/// <remarks>Use this method when you know the type as it is faster than <see cref="GetObject(int)" /></remarks>
		/// <exception cref="ObjectDoesNotExistException">Thrown if the specified ID is not found in the database.</exception>
		/// <returns>Returns a fully populated DBObject</returns>
		public static DBObject GetObject(int id, Type objectType)
		{

			if (id < 1)
			{
				throw new ObjectDoesNotExistException();
			}

			DBObject obj = CreateObject(Assembly.GetCallingAssembly(), objectType.FullName);
			obj.id = id;
			obj.Populate();

			return obj;

		}


	}
}
