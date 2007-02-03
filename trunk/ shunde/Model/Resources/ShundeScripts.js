
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
	}
	

	

}
