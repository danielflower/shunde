var lastLength = -1;
var currentNameBox = null;
var currentIdBox = null;
var req;
var mouseIsOverResultsBox = false;





function oe_mouseOver(resultsBox, mouseIsOver) {
	mouseIsOverResultsBox = mouseIsOver;
	if (!mouseIsOver && currentNameBox == null) {
		oe_unselectResultsBox();
	}
}

function oe_doubleClick(resultsBox) {
	oe_selectCurrentlySelected();
	oe_unselectResultsBox(true);
}


function oe_popup(url, name, width, height) {
	var tempWin = window.open(url, name, 'width=' + width + ',height=' + height + ',toolbar=no,location=no,directories=no,status=no,menubar=no,scrollbars=yes,copyhistory=yes,resizable=yes,left=50,top=50');
	tempWin.focus();
}


function oe_searchBoxUpdate(nameBox, idBox, xmlUrl) {
	var val = nameBox.value;

	if (lastLength == val.length) {
		return;
	}
	
	lastLength = val.length;
	
	xmlUrl += '&name=' + escape(val);

	currentNameBox = nameBox;
	currentIdBox = idBox;

	idBox.value = '';

	oe_loadXMLDoc(xmlUrl);

}




function oe_loadXMLDoc(url) {
	req = false;
    // branch for native XMLHttpRequest object
    if(window.XMLHttpRequest) {
    	try {
			req = new XMLHttpRequest();
        } catch(e) {
			req = false;
        }
    // branch for IE/Windows ActiveX version
    } else if(window.ActiveXObject) {
       	try {
        	req = new ActiveXObject('Msxml2.XMLHTTP');
      	} catch(e) {
        	try {
          		req = new ActiveXObject('Microsoft.XMLHTTP');
        	} catch(e) {
          		req = false;
        	}
		}
    }
	if(req) {
		req.onreadystatechange = oe_processReqChange;
		req.open('GET', url, true);
		req.send('');
	}
}

function oe_processReqChange() {
    // only if req shows 'loaded'
    if (req.readyState == 4) {
        // only if 'OK'
        if (req.status == 200) {
            // ...processing statements go here...
            
			oe_showSuggestions( req.responseXML );

        } else {
            alert('There was a problem retrieving the XML data:\n' +
                req.statusText);
        }
    }
}

function oe_showSuggestions( xml ) {
    var items = req.responseXML.getElementsByTagName('DBObject');

	var resultsBox = document.getElementById( 'oe_searchListBox' );

	var currentIndex = resultsBox.selectedIndex;
	resultsBox.options.length = 0;

	if (items.length == 0) {
		resultsBox.style.visibility = 'hidden';
		return;
	}

	resultsBox.style.visibility = 'visible';

    for (var i = 0; i < items.length; i++) {
        var name = oe_getElementTextNS('', 'FriendlyName', items[i], 0);
		var id = oe_getElementTextNS('', 'Id', items[i], 0);
		resultsBox.options[i] = new Option( name, id );
    }

	
	resultsBox.style.width = parseInt(currentNameBox.offsetWidth) + 'px';
	

	var newTop = parseInt(oe_findPosY(currentNameBox)) + parseInt(currentNameBox.offsetHeight);
	resultsBox.style.top = newTop + 'px';

	var newLeft = parseInt(oe_findPosX(currentNameBox));
	resultsBox.style.left = newLeft + 'px';
	

}


function oe_findPosX(obj)
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
}

function oe_findPosY(obj)
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
}



// retrieve text of an XML document element, including
// elements using namespaces
function oe_getElementTextNS(prefix, local, parentElem, index) {
    var result = '';
    if (prefix && isIE) {
        // IE/Windows way of handling namespaces
        result = parentElem.getElementsByTagName(prefix + ':' + local)[index];
    } else {
        // the namespace versions of this method 
        // (getElementsByTagNameNS()) operate
        // differently in Safari and Mozilla, but both
        // return value with just local name, provided 
        // there aren't conflicts with non-namespace element
        // names
        result = parentElem.getElementsByTagName(local)[index];
    }
    if (result) {
        // get text, accounting for possible
        // whitespace (carriage return) text nodes 
        if (result.childNodes.length > 1) {
            return result.childNodes[1].nodeValue;
        } else {
            return result.firstChild.nodeValue;    		
        }
    } else {
        return 'n/a';
    }
}

function oe_unselectResultsBox() {
	oe_unselectResultsBox(false);
}

function oe_unselectResultsBox(forceClose) {
	
	if (!forceClose && mouseIsOverResultsBox) {
		return;
	}

	if (currentNameBox != null) {
		currentNameBox.blur();
	}
	currentNameBox = null;
	currentIdBox = null;
	var resultsBox = document.getElementById( 'oe_searchListBox' );
	resultsBox.options.length = 0;
	resultsBox.style.visibility = 'hidden';
	lastLength = -1;
}


function oe_selectCurrentlySelected() {
	var resultsBox = document.getElementById( 'oe_searchListBox' );
	if (resultsBox.selectedIndex >= 0) {
		currentNameBox.value = resultsBox.options[resultsBox.selectedIndex].text;
		currentIdBox.value = resultsBox.options[resultsBox.selectedIndex].value;
	}
	oe_unselectResultsBox();
}

function oe_searchKeyDown(e) {

	if (e.keyCode == 13) { // pressing enter
		oe_selectCurrentlySelected();
		return false;
	} else if (e.keyCode == 38) { // pressing up
		var resultsBox = document.getElementById( 'oe_searchListBox' );
		if (resultsBox.selectedIndex > 0) {
			resultsBox.selectedIndex = resultsBox.selectedIndex - 1;
		}
		return false;
	} else if (e.keyCode == 40) { // pressing down

		var resultsBox = document.getElementById( 'oe_searchListBox' );
		if (resultsBox.selectedIndex < resultsBox.options.length - 1) {
			resultsBox.selectedIndex = resultsBox.selectedIndex + 1;
		}
		return false;
	}
	
	return true;

}


