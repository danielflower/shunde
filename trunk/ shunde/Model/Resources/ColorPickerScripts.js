


var cp_tableCols = 18;
var cp_colorArray = new Array('330000','333300','336600','339900','33CC00','33FF00','66FF00','66CC00','669900','666600','663300','660000','FF0000','FF3300','FF6600','FF9900','FFCC00','FFFF00','330033','333333','336633','339933','33CC33','33FF33','66FF33','66CC33','669933','666633','663333','660033','FF0033','FF3333','FF6633','FF9933','FFCC33','FFFF33','330066','333366','336666','339966','33CC66','33FF66','66FF66','66CC66','669966','666666','663366','660066','FF0066','FF3366','FF6666','FF9966','FFCC66','FFFF66','330099','333399','336699','339999','33CC99','33FF99','66FF99','66CC99','669999','666699','663399','660099','FF0099','FF3399','FF6699','FF9999','FFCC99','FFFF99','3300CC','3333CC','3366CC','3399CC','33CCCC','33FFCC','66FFCC','66CCCC','6699CC','6666CC','6633CC','6600CC','FF00CC','FF33CC','FF66CC','FF99CC','FFCCCC','FFFFCC','3300FF','3333FF','3366FF','3399FF','33CCFF','33FFFF','66FFFF','66CCFF','6699FF','6666FF','6633FF','6600FF','FF00FF','FF33FF','FF66FF','FF99FF','FFCCFF','FFFFFF','0000FF','0033FF','0066FF','0099FF','00CCFF','00FFFF','99FFFF','99CCFF','9999FF','9966FF','9933FF','9900FF','CC00FF','CC33FF','CC66FF','CC99FF','CCCCFF','CCFFFF','0000CC','0033CC','0066CC','0099CC','00CCCC','00FFCC','99FFCC','99CCCC','9999CC','9966CC','9933CC','9900CC','CC00CC','CC33CC','CC66CC','CC99CC','CCCCCC','CCFFCC','000099','003399','006699','009999','00CC99','00FF99','99FF99','99CC99','999999','996699','993399','990099','CC0099','CC3399','CC6699','CC9999','CCCC99','CCFF99','000066','003366','006666','009966','00CC66','00FF66','99FF66','99CC66','999966','996666','993366','990066','CC0066','CC3366','CC6666','CC9966','CCCC66','CCFF66','000033','003333','006633','009933','00CC33','00FF33','99FF33','99CC33','999933','996633','993333','990033','CC0033','CC3333','CC6633','CC9933','CCCC33','CCFF33','000000','003300','006600','009900','00CC00','00FF00','99FF00','99CC00','999900','996600','993300','990000','CC0000','CC3300','CC6600','CC9900','CCCC00','CCFF00','000000','111111','222222','333333','444444','555555','666666','777777','888888','999999','AAAAAA','BBBBBB','CCCCCC','DDDDDD','EEEEEE','FFFFFF');
var cp_colWidth = 10;
var cp_colHeight = 10;

var cp_pickerDiv;

var cp_currentTarget = null;
var cp_currentTarget_sample = null;
var cp_pickerHexTB = null;
var cp_pickerSample = null;
var cp_pickerInitialised = false;


// these are optional functions that can be used to handle events
var cp_onColourClicked = null;
var cp_onColourSelected = null;
var cp_onCancelled = null;

function cp_setupPicker() {
		
	cp_pickerDiv = document.createElement('div');
	cp_pickerDiv.id = 'cp_pickerDiv';	
	cp_pickerDiv.style.position = 'absolute';
	cp_pickerDiv.style.left = '-1000px';
	cp_pickerDiv.style.top = '-1000px';
	document.body.appendChild(cp_pickerDiv);		


	var html = '<table style=\"table-layout:fixed;empty-cells:show;border-collapse:collapse;border:1px solid black;\">';

	// make all the spare columns in the final row white
	var remainder = cp_tableCols - (cp_colorArray.length % cp_tableCols);

	
	for (var i = 0; i < cp_colorArray.length + remainder; i++) {
		if (i % cp_tableCols == 0) {
			html += '<tr>';
		}
		
		var c = (i >= cp_colorArray.length) ? 'FFFFFF' : cp_colorArray[i];
		html += '<td style=\"background-color:#' + c + ';width:' + cp_colWidth + 'px;cursor:pointer;height:' + cp_colHeight + 'px;\" onclick=\"cp_setColour(\'' + c + '\');\"></td>';

		if ( (i + 1) % cp_tableCols == 0) {
			html += '</tr>';
		}
	}
	
	var closeImageWidth = 21;
	var colsNeededForCloseImage = parseInt((closeImageWidth / cp_colWidth) + 1);

	var selectImageWidth = 21;
	var colsNeededForSelectImage = parseInt((selectImageWidth / cp_colWidth) + 1);
	
	html += '<tr style=\"background-color:White;\"><td align=\"right\" colspan=\"' + (cp_tableCols - colsNeededForCloseImage - colsNeededForSelectImage) + '\">#<input id=\"cp_pickerHexTB\" type=\"text\" style=\"height:16px;width:60px;border:1px solid black;\" /> <span style=\"padding-left:30px;border:1px solid black;font-size:15px;margin-right:3px;\" id=\"cp_pickerSample\">&nbsp;</span></td><td colspan=\"' + colsNeededForSelectImage + '\"><a href=\"javascript:cp_selectColour();\"><img src=\"/Site/Images/select.gif\" style=\"border:none;display:block;\" alt=\"select\" /></a></td><td colspan=\"' + colsNeededForCloseImage + '\" align=\"right\"><a href=\"javascript:cp_cancel();\"><img src=\"/Site/Images/close.gif\" style=\"border:none;display:block;\" alt=\"cancel\" /></a></td></tr></table>';
	
	cp_pickerDiv.innerHTML = html;
	
	cp_pickerSample = document.getElementById('cp_pickerSample');
	cp_pickerHexTB = document.getElementById('cp_pickerHexTB');
	
	cp_pickerInitialised = true;
	
}

function cp_cancel() {
	if (cp_onCancelled) {
		cp_onCancelled(cp_currentTarget);
	}
	cp_closePicker();
}

function cp_closePicker() {
	unhideSelectObjects(null);
	cp_pickerDiv.style.left = '-1000px';
	cp_pickerDiv.style.top = '-1000px';
	cp_currentTarget = null;
	cp_currentTarget_sample = null;
}

function cp_selectColour() {
	var c = cp_pickerHexTB.value;
	var cssValue = (c == '') ? '' : '#' + c;
	
	cp_currentTarget.value = cssValue;
	cp_currentTarget_sample.style.backgroundColor = cssValue;
	cp_closePicker();
}

function cp_setColour(c) {
	cp_pickerHexTB.value = c;
	cp_pickerSample.style.backgroundColor = '#' + c;
	if (cp_onColourClicked) {
		cp_onColourClicked(cp_currentTarget, '#' + c);
	}
}

function cp_pick(link, textboxId) {
	hideSelectObjects(null);
	if (!cp_pickerInitialised) {
		cp_setupPicker();
	}
	var tb = document.getElementById(textboxId);
	if (tb == null) {
		alert('Text box with id ' + textboxId + ' not found');
	}
	cp_pickerHexTB.value = tb.value.replace('#', '');
	cp_pickerSample.style.backgroundColor = tb.value;
	cp_currentTarget = tb;
	cp_currentTarget_sample = document.getElementById(textboxId + '_sample');

	cp_pickerDiv.style.left = oe_getXPositionToPlace( oe_findPosX(link), cp_pickerDiv.offsetWidth) + 'px';
	
	var newTop = oe_getYPositionToPlace(parseInt(oe_findPosY(link)) + parseInt(link.offsetHeight), cp_pickerDiv.offsetHeight);
	cp_pickerDiv.style.top = newTop + 'px';
	
}
