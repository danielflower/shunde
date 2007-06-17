using System;
using System.Collections.Generic;
using System.Text;
using Shunde.Framework;
using Shunde.Utilities;
using Shunde.Framework.Columns;

namespace Shunde.Common
{

	/// <summary>
	/// A relation between an object and an attribute
	/// </summary>
	public class AttribRelation : DBObject, IAttribRelation
	{

		private Attrib attrib;

		/// <summary>
		/// The attribute in the relation
		/// </summary>
		public Attrib Attrib
		{
			get { return attrib; }
			set { attrib = value; }
		}

		private DBObject dbObject;

		/// <summary>
		/// The object related to the attribute
		/// </summary>
		public DBObject DBObject
		{
			get { return dbObject; }
			set { dbObject = value; }
		}

		
		/// <summary>Sets up the <see cref="Shunde.Framework.ObjectInfo" /> for this class</summary>
		static AttribRelation()
		{

			DBTable tbl = new DBTable("AttribRelation", new DBColumn[] {
				new DBObjectColumn( "attrib", typeof(Attrib), false ),
				new DBObjectColumn( "dbObject", typeof(DBObject), false )
			});

			tbl.UniqueIndexColumns = "attribId,dbObjectId";

			ObjectInfo.RegisterObjectInfo(typeof(AttribRelation), tbl);

		}


		/// <summary>Gets and populates all the Attribs that are related (by an <see cref="AttribRelation" />) to the given Object</summary>
		/// <returns>Returns an array of 0 or more Attrib objects</returns>
		public static Attrib[] GetAttribsRelatedTo(DBObject relatedObject)
		{

			Type t = typeof(Attrib);
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			string sql = oi.GetSelectStatement() + " WHERE DBObject.isDeleted = 0 AND Attrib.Id IN (SELECT [AttribRelation].[AttribId] FROM AttribRelation INNER JOIN DBObject ON AttribRelation.[id] = DBObject.[id] WHERE DBObject.isDeleted = 0 AND AttribRelation.dbObjectId = " + relatedObject.Id + ")";
			return (Attrib[])GetObjects(sql, t);

			/*
			string where = 

			Attrib[] objs = (Attrib[])TreeNode.GetTreeNodesAsForest(where, typeof(Attrib), new Type[0]);

			return (Attrib[])TreeNode.ConvertToFlatArray(objs, typeof(Attrib));
			 * */
		}

		/// <summary>Gets and populates all the Attribs that are related (by an <see cref="AttribRelation" />) to the given Object and which are one of the given types</summary>
		/// <returns>Returns an array of 0 or more Attrib objects</returns>
		public static Attrib[] GetAttribsRelatedTo(DBObject relatedObject, AttribType[] attribTypes)
		{

			if (attribTypes.Length == 0)
			{
				return new Attrib[0];
			}

			Type t = typeof(Attrib);
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			string sql = oi.GetSelectStatement() + " WHERE DBObject.isDeleted = 0 AND Attrib.Id IN (SELECT [AttribRelation].[AttribId] FROM AttribRelation INNER JOIN DBObject ON AttribRelation.[id] = DBObject.[id] WHERE DBObject.isDeleted = 0 AND AttribRelation.dbObjectId = " + relatedObject.Id + ") AND [Attrib].[attribTypeId] IN (" + ObjectUtils.GetIDsAsCSV(attribTypes) + ")";
			return (Attrib[])GetObjects(sql, t);

			/*
			string where = 

			Attrib[] objs = (Attrib[])TreeNode.GetTreeNodesAsForest(where, typeof(Attrib), new Type[0]);

			return (Attrib[])TreeNode.ConvertToFlatArray(objs, typeof(Attrib));
			 * */
		}

		/// <summary>
		/// Gets the relations for a given attribute type and object
		/// </summary>
		/// <remarks>Returns deleted relations also</remarks>
		public static AttribRelation[] GetAttribRelations(DBObject relatedObject)
		{
			Type t = typeof(AttribRelation);
			ObjectInfo oi = ObjectInfo.GetObjectInfo(t);
			string sql = oi.GetSelectStatement() + " WHERE dbObjectId = " + relatedObject.Id;
			return (AttribRelation[])GetObjects(sql, t);
		}

	}

}
