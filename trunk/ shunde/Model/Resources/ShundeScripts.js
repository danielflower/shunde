
var ShundeUtils = {

	findPosX: function(obj)
	{
		var curleft = 0;
		
		if (obj.offsetParent)
		{
			while (obj.offsetParent)
			{
				curleft += obj.offsetLeft
				obj = obj.offsetParent;
			}
		}
		else if (obj.x)
			curleft += obj.x;
		return curleft;
	},

	findPosY: function(obj)
	{
		var curtop = 0;

		if (obj.offsetParent)
		{
			while (obj.offsetParent)
			{
				curtop += obj.offsetTop
				obj = obj.offsetParent;
			}
		}
		else if (obj.y)
			curtop += obj.y;

		

		return curtop;
	},
	
	

	
	// Gets the X position which is visible, based on the user's current scroll position
	getVisibleXPosition: function(preferredX, elementWidth) {
		return preferredX; // todo make this work
	},
	
	// Gets the X position which is visible, based on the user's current scroll position
	getVisibleYPosition: function(preferredY, elementHeight) {
		return preferredY; // todo make this work
	},
	

	// Gets an elements X, Y, Width and Height positions (copied from ASP.NET js files)
	getElementPosition: function(element) {
		var result = new Object();
		result.x = 0;
		result.y = 0;
		result.width = 0;
		result.height = 0;
		if (element.offsetParent) {
			result.x = element.offsetLeft;
			result.y = element.offsetTop;
			var parent = element.offsetParent;
			while (parent) {
				result.x += parent.offsetLeft;
				result.y += parent.offsetTop;
				var parentTagName = parent.tagName.toLowerCase();
				if (parentTagName != "table" &&
					parentTagName != "body" && 
					parentTagName != "html" && 
					parentTagName != "div" && 
					parent.clientTop && 
					parent.clientLeft) {
					result.x += parent.clientLeft;
					result.y += parent.clientTop;
				}
				parent = parent.offsetParent;
			}
		}
		else if (element.left && element.top) {
			result.x = element.left;
			result.y = element.top;
		}
		else {
			if (element.x) {
				result.x = element.x;
			}
			if (element.y) {
				result.y = element.y;
			}
		}
		if (element.offsetWidth && element.offsetHeight) {
			result.width = element.offsetWidth;
			result.height = element.offsetHeight;
		}
		else if (element.style && element.style.pixelWidth && element.style.pixelHeight) {
			result.width = element.style.pixelWidth;
			result.height = element.style.pixelHeight;
		}
		return result;
	}
	

}
