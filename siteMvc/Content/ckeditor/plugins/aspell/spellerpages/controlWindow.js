﻿/*
Copyright (c) 2003-2010, CKSource - Frederico Knabben. All rights reserved.
For licensing, see LICENSE.html or http://ckeditor.com/license
*/

(function(){window.controlWindow=function(a){var b=this;b._form=a;b.windowType='controlWindow';b.noSuggestionSelection='- No suggestions -';b.suggestionList=b._form.sugg;b.evaluatedText=b._form.misword;b.replacementText=b._form.txtsugg;b.undoButton=b._form.btnUndo;};window.controlWindow.prototype={resetForm:function(){if(this._form)this._form.reset();},setSuggestedText:function(){var a=this.suggestionList,b=this.replacementText,c='';if(a.options[0].text&&a.options[0].text!=this.noSuggestionSelection)c=a.options[a.selectedIndex].text;b.value=c;},selectDefaultSuggestion:function(){var c=this;var a=c.suggestionList,b=c.replacementText;if(a.options.length==0)c.addSuggestion(c.noSuggestionSelection);else a.options[0].selected=true;c.setSuggestedText();},addSuggestion:function(a){var b=this.suggestionList;if(a){var c=b.options.length,d=new Option(a,'sugg_text'+c);b.options[c]=d;}},clearSuggestions:function(){var a=this.suggestionList;for(var b=a.length-1;b>-1;b--){if(a.options[b])a.options[b]=null;}},enableUndo:function(){if(this.undoButton)if(this.undoButton.disabled==true)this.undoButton.disabled=false;},disableUndo:function(){if(this.undoButton)if(this.undoButton.disabled==false)this.undoButton.disabled=true;}};})();
