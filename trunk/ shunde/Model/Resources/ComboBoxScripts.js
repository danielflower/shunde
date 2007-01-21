
var ShundeComboBox = {

	mouseIsOverListbox : Boolean,

	initialise: function(tb, lb) {
		
		lb.style.width = parseInt(tb.offsetWidth) + 'px';
		lb.style.top = (parseInt(ShundeUtils.findPosY(tb)) + parseInt(tb.offsetHeight)) + 'px';
		
		mouseIsOverListbox = false;
		
	},

	showListBox: function(tb, lb) {
		
		lb.style.visibility = 'visible';
		
	},
	
	updateBox: function(tb, lb) {
	
		if (tb.value.length == 0) {
			lb.selectedIndex = -1;
			return;
		}
	
		var cur = tb.value.toLowerCase();
		for (var i = 0; i < lb.options.length; i++) {
			var op = lb.options[i];
			if (op.text.toLowerCase().indexOf(cur) == 0) {
				lb.selectedIndex = i;
				return;
			}
		}
		lb.selectedIndex = -1;
	
	},
	
	selectCurrentlySelected: function(tb, lb) {
		if (lb.selectedIndex == -1) {
			return;
		}
		tb.value = lb.options[lb.selectedIndex].text;
	},
	
	onKeyDown: function(tb, lb, keyCode) {
		
		if (keyCode == 13) { // pressing enter
			ShundeComboBox.selectCurrentlySelected(tb, lb);
			ShundeComboBox.hideListBox(tb, lb);
			return false;
		} else if (keyCode == 38) { // pressing up
			if (lb.selectedIndex > 0) {
				lb.selectedIndex = lb.selectedIndex - 1;
				ShundeComboBox.selectCurrentlySelected(tb, lb);
			}
			return false;
		} else if (keyCode == 40) { // pressing down

			if (lb.selectedIndex < lb.options.length - 1) {
				lb.selectedIndex = lb.selectedIndex + 1;
				ShundeComboBox.selectCurrentlySelected(tb, lb);
			}
			return false;
		}
		
		return true;
	},
	
	onMouseOverListbox : function(tb, lb) {
		ShundeComboBox.mouseIsOverListbox = true;
	},
	
	onMouseOutListbox : function(tb, lb) {
		ShundeComboBox.mouseIsOverListbox = false;	
	},
	
	onMouseDoubleClickListbox : function(tb, lb) {
		ShundeComboBox.selectCurrentlySelected(tb, lb);
		ShundeComboBox.mouseIsOverListbox = false;
		ShundeComboBox.hideListBox(tb, lb);
	},
	
	hideListBox: function(tb, lb) {
		if (!ShundeComboBox.mouseIsOverListbox || lb.selectedIndex == -1) {
			lb.style.visibility = 'hidden';
			tb.blur();
		}
	}

}
