using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Shunde.Framework;
using System.Web.Caching;

namespace Shunde.Utilities
{
	/// <summary>A Utility class for the DBObject Class</summary>
	public class ObjectUtils
	{

		/// <summary>Prevents creation of this class</summary>
		private ObjectUtils() { }


		/// <summary>
		/// Writes a list of DBObjects to a stream as XML
		/// </summary>
		/// <param name="outputStream">The stream to write to</param>
		/// <param name="objects">The DBObjects to write</param>
		public static void WriteObjectsToXml(System.IO.Stream outputStream, IList<DBObject> objects)
		{
			XmlTextWriter writer = new XmlTextWriter(outputStream, Encoding.UTF8);

			writer.WriteStartDocument();
			writer.WriteStartElement("DBObjects");

			foreach (DBObject obj in objects)
			{
				writer.WriteStartElement("DBObject");
				writer.WriteElementString("FriendlyName", obj.FriendlyName);
				writer.WriteElementString("Id", obj.Id.ToString());
				writer.WriteEndElement();
			}

			writer.WriteEndElement();
			writer.WriteEndDocument();
			writer.Close();

		}


		/// <summary>Adds a collection of DBObjects to the cache, which can later be accessed using <see cref="GetObjectFromCache" />.</summary>
		/// <param name="cache">The cache to use</param>
		/// <param name="objectsToAdd">The objects to add</param>
		public static void AddObjectsToCache(Cache cache, IEnumerable<DBObject> objectsToAdd)
		{
			foreach (DBObject dbObject in objectsToAdd)
			{
				string cacheName = "dbobject_" + dbObject.Id.ToString();
				cache[cacheName] = dbObject;
			}

		}

		/// <summary>Populates an object with the given id, and saves it to the cache. In future instances, the cached version is used.</summary>
		/// <param name="cache">The Cache to use</param>
		/// <param name="objectId">The ID of the object to populate</param>
		/// <exception cref="Shunde.ObjectDoesNotExistException">Thrown when the object does not exist in the database</exception>
		/// <returns>A fully populated DBObject</returns>
		public static DBObject GetObjectFromCache(Cache cache, int objectId)
		{
			string cacheName = "dbobject_" + objectId.ToString();
			DBObject dbObject = (DBObject)cache[cacheName];
			if (dbObject == null)
			{
				dbObject = DBObject.GetObject(objectId.ToString());
				cache[cacheName] = dbObject;
			}
			return dbObject;
		}

		/// <summary>Removes the object with the given id from the cache</summary>
		/// <remarks>No exception is thrown if the object is not already in the cache</remarks>
		/// <param name="cache">The cache to use</param>
		/// <param name="objectId">The id of the object to remove</param>
		public static void RemoveObjectFromCache(Cache cache, int objectId)
		{
			string cacheName = "dbobject_" + objectId.ToString();
			cache.Remove(cacheName);
		}



		/// <summary>Saves an array of <see cref="DBObject" />s</summary>
		/// <exception cref="Shunde.ValidationException">Thrown if an object in the array fails the database constraints. The Message property contains detailed information on why it failed, which may or may not be suitable to show on the end user, depending on the situation.</exception>
		/// <exception cref="ConcurrencyException">Thrown if an object is read and updated by more than one user at the same time. The first to save will have no problem, but if anyone tries to save subsequent to another save without re-reading the data, then this exception will be thrown. The Message property contains detailed information suitable to be shown to the end user.</exception>
		/// <param Name="objects">An array of <see cref="DBObject" />s to be saved</param>
		public static void SaveListOfObjects(IEnumerable<DBObject> objects)
		{
			foreach (DBObject obj in objects)
			{
				obj.Save();
			}
		}


		/// <summary>Gets a DBObject with the given id from an array of DBObjects, or null if the DBObject is not in the array</summary>
		public static DBObject GetFromList(IEnumerable<DBObject> objects, int objectId)
		{
			foreach (DBObject obj in objects)
			{
				if (obj.Id == objectId)
				{
					return obj;
				}
			}
			return null;
		}

		/// <summary>Deletes an DBObject from an array of DBObjects</summary>
		public static void DeleteFromList(IEnumerable<DBObject> objects, DBObject objectToBeDeleted)
		{
			foreach (DBObject obj in objects)
			{
				if (obj.Equals(objectToBeDeleted))
				{
					obj.IsDeleted = true;
					return;
				}
			}
		}


		/// <summary>
		/// Adds an object to an array, or replaces it if it's already in the array
		/// </summary>
		/// <param name="array">An array of objects</param>
		/// <param name="toAdd">The object to be updated or added</param>
		public static void AddToArray(DBObject[] array, DBObject toAdd)
		{
			int index = 0;
			foreach (DBObject obj in array)
			{
				if (obj.Equals(toAdd))
				{
					array[index] = toAdd;
					return;
				}
			}
			index++;
			DBObject[] newArray = (DBObject[])Array.CreateInstance(array.GetType().GetElementType(), array.Length + 1);
			array.CopyTo(newArray, 0);
			newArray[array.Length] = toAdd;
		}


		/// <summary>
		/// Returns a comma separated list of object IDs
		/// </summary>
		/// <param Name="objects">The objects whose IDs will be returned</param>
		/// <returns>A comma separated list as a string</returns>
		public static String GetIDsAsCSV(IEnumerable<DBObject> objects)
		{
			StringBuilder result = new StringBuilder();
			foreach (DBObject single in objects)
			{
				if (result.Length > 0)
				{
					result.Append( "," );
				}
				result.Append( single.Id.ToString() );
			}
			return result.ToString();
		}


	}
}
