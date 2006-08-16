using System;
using System.Data;
using System.Configuration;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Shunde.Framework;
using Shunde.Utilities;

public partial class EditObjects_ViewBinaryData : PageBase
{
	public override void Start()
	{


		DBObject obj = DBObject.GetObject(GetStringParam("objectId"));
		obj.PopulateBinaryData();
		String fieldName = GetStringParam("fieldName");

		BinaryData bd = new BinaryData(null, "", "");

		ObjectInfo oi = ObjectInfo.GetObjectInfo(obj.GetType());
		foreach (DBTable table in oi.Tables)
		{
			foreach (DBColumn col in table.Columns)
			{
				if (col.Name.Equals(fieldName))
				{

					FieldInfo dataFI = col.FieldInfo;
					bd = (BinaryData)dataFI.GetValue(obj);
					goto breakPoint;
				}
			}
		}

	breakPoint:

		Response.ContentType = bd.MimeType;
		Response.BinaryWrite(bd.Data);



	}
}
