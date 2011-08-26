var markCount = 0;
var isIE = navigator.userAgent.toLowerCase().indexOf("msie") != -1;
var isGecko = navigator.userAgent.toLowerCase().indexOf("gecko") != -1;

var CONTEXTPATH = "images/"; //设置图片路径

var Class = {
	create: function() {
		return function() { this.initialize.apply(this, arguments); }
	}
};

var Extend = function(destination, source) {
	for (var property in source) {
		destination[property] = source[property];
	}
};

var Bind = function(object, fun) {
	return function() {
		return fun.apply(object, arguments);
	}
};

var BindAsEventListener = function(object, fun) {
	return function(event) {
		return fun.call(object, (event || window.event));
	}
};

var CurrentStyle = function(element){
	return element.currentStyle || document.defaultView.getComputedStyle(element, null);
};

function $D(ele) {
  if (typeof(ele) == 'string'){
    var id = ele;
    ele = document.getElementById(id)
    if(!ele && parent) {
        ele = parent.document.getElementById(id);
    }
    if(!ele){
  		return null;
    }
  }
  if(ele){
  	Core.attachMethod(ele);
	}
  return ele;
};

function $T(tagName,ele){
	ele = $D(ele);
	ele = ele || document;
	var ts = ele.getElementsByTagName(tagName);//此处返回的不是数组
	var arr = [];
	var len = ts.length;
	for(var i=0;i<len;i++){
		arr.push($D(ts[i]));
	}
	return arr;
};

function toggleSelect(show) {
    var sels = $T("SELECT");
    for (var i = 0; i < sels.length; i++) {
        if (show)
            sels[i].style.display = '';
        else
            sels[i].style.display = 'none';
    }
};

function stopEvent(event){//阻止一切事件执行,包括浏览器默认的事件
	event = window.event||event;
	if(!event){
		return;
	}
	if(isGecko){
		event.preventDefault();
		event.stopPropagation();
	}
	event.cancelBubble = true
	event.returnValue = false;
};

function addEventHandler(oTarget, sEventType, fnHandler) {
	if (oTarget.addEventListener) {
		oTarget.addEventListener(sEventType, fnHandler, false);
	} else if (oTarget.attachEvent) {
		oTarget.attachEvent("on" + sEventType, fnHandler);
	} else {
		oTarget["on" + sEventType] = fnHandler;
	}
};

function removeEventHandler(oTarget, sEventType, fnHandler) {
    if (oTarget.removeEventListener) {
        oTarget.removeEventListener(sEventType, fnHandler, false);
    } else if (oTarget.detachEvent) {
        oTarget.detachEvent("on" + sEventType, fnHandler);
    } else { 
        oTarget["on" + sEventType] = null;
    }
};

function getPopup(id) {
    return frames["_DialogFrame_" + id].document.body;
};

Array.prototype.remove = function(s){
  for(var i=0;i<this.length;i++){
    if(s == this[i]){
    	this.splice(i, 1);
    }
  }
};

if(window.HTMLElement){//给FF添加IE专有的属性和方法
    HTMLElement.prototype.__defineGetter__("parentElement",function(){
        if(this.parentNode==this.ownerDocument)return null;
        return this.parentNode;
        });
	HTMLElement.prototype.__defineSetter__("outerHTML",function(sHTML){
        var r=this.ownerDocument.createRange();
        r.setStartBefore(this);
        var df=r.createContextualFragment(sHTML);
        this.parentNode.replaceChild(df,this);
        return sHTML;
        });
    HTMLElement.prototype.__defineGetter__("outerHTML",function(){
        var attr;
        var attrs=this.attributes;
        var str="<"+this.tagName;
        for(var i=0;i<attrs.length;i++){
            attr=attrs[i];
            if(attr.specified)
                str+=" "+attr.name+'="'+attr.value+'"';
            }
        if(!this.canHaveChildren)
            return str+">";
        return str+">"+this.innerHTML+"</"+this.tagName+">";
        });
    HTMLElement.prototype.__defineSetter__("innerText",function(sText){
        var parsedText=document.createTextNode(sText);
        this.innerHTML=parsedText;
        return parsedText;
        });
    HTMLElement.prototype.__defineGetter__("innerText",function(){
        var r=this.ownerDocument.createRange();
        r.selectNodeContents(this);
        return r.toString();
        });
}

var $E = {};
$E.hide = function(ele) {
	ele = ele || this;
	ele = $D(ele);
  ele.style.display = 'none';
};
$E.show = function(ele) {
	ele = ele || this;
	ele = $D(ele);
  ele.style.display = '';
};

var Core = {};
Core.attachMethod = function(ele){
	if(!ele||ele["$A"]){
		return;
	}
	if(ele.nodeType==9){
		return;
	}
	var win;
	try{
		if(isGecko){		
			win = ele.ownerDocument.defaultView;
		}else{
			win = ele.ownerDocument.parentWindow;
		}
		for(var prop in $E){
			ele[prop] = win.$E[prop];
		}
	}catch(ex){
		//alert("Core.attachMethod:"+ele)//有些对象不能附加属性，如flash
	}
};

var Dialog = Class.create();

Dialog.prototype = {

    //初始化
    initialize: function(strID,options) {
    
		if(!strID){
			alert("Dialog id error!");
			return;
		}
		
		//设置传入的options
		this.setOptions(options);
		
		this.ID = strID;
		this.isModal = !!this.options.isModal;
		this.Width = parseInt(this.options.Width);
		this.Height =  parseInt(this.options.Height);
		this.Top = parseInt(this.options.Top);
		this.Left = parseInt(this.options.Left);
		this.ParentWindow = this.options.ParentWindow;
		this.onLoad = this.options.onLoad;
		this.Window = this.options.Window;
		
		this.Title = this.options.Title;
		this.URL = this.options.URL;
		this.DialogArguments = this.options.DialogArguments;
		this.WindowFlag = !!this.options.WindowFlag;
		this.Message = this.options.Message;
		this.MessageTitle = this.options.MessageTitle;
		this.ShowMessageRow = !!this.options.ShowMessageRow;
		this.ShowButtonRow = !!this.options.ShowButtonRow;
		this.Icon = this.options.Icon;
		this.bgdivID = this.options.bgdivID;
		
		this.AllowDrag = !!this.options.AllowDrag;
    },
    
    //设置默认属性
    setOptions : function(options) {
        this.options = {
		    isModal : true,
		    Width : 400,
		    Height : 300,
		    Top : 0,
		    Left : 0,
		    ParentWindow : null,
		    onLoad : null,
		    Window : null,
    		
		    Title : "",
		    URL : null,
		    DialogArguments : {},
		    WindowFlag : false,
		    Message : null,
		    MessageTitle : null,
		    ShowMessageRow : false,
		    ShowButtonRow : true,
		    Icon : null,
		    bgdivID : null,
    		
		    AllowDrag : true
        };
        Extend(this.options, options || {});
    },
    
    showWindow : function() {
        if (isIE) {
            this.ParentWindow.showModelessDialog(this.URL, this.DialogArguments, "dialogWidth:" + this.Width + ";dialogHeight:" + this.Height + ";help:no;scroll:no;status:no");
        }
        if (isGecko) {
            var sOption = "location=no,menubar=no,status=no;toolbar=no,dependent=yes,dialog=yes,minimizable=no,modal=yes,alwaysRaised=yes,resizable=no";
            this.Window = this.ParentWindow.open('', this.URL, sOption, true);
            var w = this.Window;
            if (!w) {
                alert("browser prevent pop-up window, please change your browser settings!");
                return;
            }
            w.moveTo(this.Left, this.Top);
            w.resizeTo(this.Width, this.Height + 30);
            w.focus();
            w.location.href = this.URL;
            w.Parent = this.ParentWindow;
            w.dialogArguments = this.DialogArguments;
        }
    },

    show : function() {
        markCount++;
        toggleSelect(false);
        var pw = window;
        var doc = pw.document;
        var cw = doc.compatMode == "BackCompat" ? doc.body.clientWidth : doc.documentElement.clientWidth;
        var ch = doc.compatMode == "BackCompat" ? doc.body.clientHeight : doc.documentElement.clientHeight; //必须考虑文本框处于页面边缘处，控件显示不全的问题
        var sl = Math.max(doc.documentElement.scrollLeft, doc.body.scrollLeft);
        var st = Math.max(doc.documentElement.scrollTop, doc.body.scrollTop); //考虑滚动的情况
        var sw = Math.max(doc.documentElement.scrollWidth, doc.body.scrollWidth);
        var sh = Math.max(doc.documentElement.scrollHeight, doc.body.scrollHeight); //考虑滚动的情况
        sw = Math.max(sw, cw);
        sh = Math.max(sh, ch);
        
        if(this.Top < st || this.Top > ch) this.Top = 0;
        if(this.Left < sl || this.Left > cw) this.Left = 0;

        if (!this.ParentWindow) {
            this.ParentWindow = window;
        }
        this.DialogArguments._DialogInstance = this;
        this.DialogArguments.ID = this.ID;

        if (!this.Height) {
            this.Height = this.Width / 2;
        }

        if (this.Top == 0) {
            this.Top = (ch - this.Height - 30) / 2 + st - 8;
        }
        if (this.Left == 0) {
            this.Left = (cw - this.Width - 12) / 2 + sl;
        }
        if (this.ShowButtonRow) {//按钮行高36
            this.Top -= 18;
        }
        if (this.WindowFlag) {
            this.showWindow();
            return;
        }
        var arr = [];
        arr.push("<table style='-moz-user-select:none;' oncontextmenu='stopEvent(event);' onselectstart='stopEvent(event);' border='0' cellpadding='0' cellspacing='0' width='" + (this.Width + 26) + "'>");
        arr.push("  <tr style='cursor:;'>");
        arr.push("    <td width='13' height='33' style=\"background-image:url(" + CONTEXTPATH + "dialog_lt.png) !important;background-image: none;filter:progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + CONTEXTPATH + "dialog_lt.png', sizingMethod='crop');\"><div style='width:13px;'></div></td>");
        arr.push("    <td id='_Drag_" + this.ID + "' height='33' style=\"background-image:url(" + CONTEXTPATH + "dialog_ct.png) !important;background-image: none;filter:progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + CONTEXTPATH + "dialog_ct.png', sizingMethod='crop');\"><div style=\"font-size:9pt;float:left;font-weight:bold; color:#FFFFFF; padding:9px 0 0 4px;\"><img src=\"" + CONTEXTPATH + "icon_dialog.gif\" align=\"absmiddle\">&nbsp;" + this.Title + "</div>");
        arr.push("      <div style=\"position: relative;cursor:pointer; float:right; margin:5px 0 0; _margin:4px 0 0;height:17px; width:28px; background-image:url(" + CONTEXTPATH + "dialog_closebtn.gif)\" onMouseOver=\"this.style.backgroundImage='url(" + CONTEXTPATH + "dialog_closebtn_over.gif)'\" onMouseOut=\"this.style.backgroundImage='url(" + CONTEXTPATH + "dialog_closebtn.gif)'\" drag='false' onClick=\"Dialog.getInstance('" + this.ID + "').CancelButton.onclick.apply(Dialog.getInstance('" + this.ID + "').CancelButton,[]);\"></div></td>");
        arr.push("    <td width='13' height='33' style=\"background-image:url(" + CONTEXTPATH + "dialog_rt.png) !important;background-image: none;filter:progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + CONTEXTPATH + "dialog_rt.png', sizingMethod='crop');\"><div style=\"width:13px;\"></div></td>");
        arr.push("  </tr>");
        arr.push("  <tr drag='false'><td width='13' style=\"background-image:url(" + CONTEXTPATH + "dialog_mlm.png) !important;background-image: none;filter:progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + CONTEXTPATH + "dialog_mlm.png', sizingMethod='crop');\"></td>");
        arr.push("    <td align='center' valign='top'>");
        arr.push("    <table width='100%' border='0' cellpadding='0' cellspacing='0' bgcolor='#FFFFFF'>");
        arr.push("        <tr id='_MessageRow_" + this.ID + "' style='display:none'>");
        arr.push("          <td height='50' valign='top'><table id='_MessageTable_" + this.ID + "' width='100%' border='0' cellspacing='0' cellpadding='8' style=\" background:#EAECE9 url(" + CONTEXTPATH + "dialog_bg.jpg) no-repeat right top;\">");
        arr.push("              <tr><td width='25' height='50' align='right'><img id='_MessageIcon_" + this.ID + "' src='" + CONTEXTPATH + "window.gif' width='32' height='32'></td>");
        arr.push("                <td align='left' style='line-height:16px;'>");
        arr.push("                <h2 class='fb' id='_MessageTitle_" + this.ID + "'>&nbsp;</h2>");
        arr.push("                <div id='_Message_" + this.ID + "'>&nbsp;</div></td>");
        arr.push("              </tr></table></td></tr>");
        arr.push("        <tr><td align='center' valign='top'>");
        arr.push("          <iframe src='");
        if (this.URL.indexOf(":") == -1) {
            //arr.push(CONTEXTPATH+this.URL);
            arr.push(this.URL);
        } else {
            arr.push(this.URL);
        }
        arr.push("' id='_DialogFrame_" + this.ID + "' name='_DialogFrame_" + this.ID + "' allowTransparency='true'  width='" + this.Width + "' height='" + this.Height + "' frameborder='0' style=\"background-color: #transparent; border:none;\"></iframe></td></tr>");
        arr.push("        <tr drag='false' id='_ButtonRow_" + this.ID + "'><td height='36'>");
        arr.push("            <div id='_DialogButtons_" + this.ID + "' style='text-align:right; border-top:#dadee5 1px solid; padding:8px 20px; background-color:#f6f6f6;'>");
        arr.push("           	<input id='_ButtonOK_" + this.ID + "'  type='button' value='确&nbsp;定'>");
        arr.push("           	<input id='_ButtonCancel_" + this.ID + "'  type='button' onclick=\"Dialog.getInstance('" + this.ID + "').close();\" value='取&nbsp;消'>");
        arr.push("            </div></td></tr>");
        arr.push("      </table></td>");
        arr.push("    <td width='13' style=\"background-image:url(" + CONTEXTPATH + "dialog_mrm.png) !important;background-image: none;filter:progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + CONTEXTPATH + "dialog_mrm.png', sizingMethod='crop');\"></td></tr>");
        arr.push("  <tr><td width='13' height='13' style=\"background-image:url(" + CONTEXTPATH + "dialog_lb.png) !important;background-image: none;filter:progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + CONTEXTPATH + "dialog_lb.png', sizingMethod='crop');\"></td>");
        arr.push("    <td style=\"background-image:url(" + CONTEXTPATH + "dialog_cb.png) !important;background-image: none;filter:progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + CONTEXTPATH + "dialog_cb.png', sizingMethod='crop');\"></td>");
        arr.push("    <td width='13' height='13' style=\"background-image:url(" + CONTEXTPATH + "dialog_rb.png) !important;background-image: none;filter:progid:DXImageTransform.Microsoft.AlphaImageLoader(src='" + CONTEXTPATH + "dialog_rb.png', sizingMethod='crop');\"></td>");
        arr.push("  </tr></table>");
        this.TopWindow = pw;

        var bgdiv = pw.$D("_DialogBGDiv_" + this.ID);
        if (!bgdiv) {
            bgdiv = document.createElement("div");
            bgdiv.id = "_DialogBGDiv_" + this.ID;
            $E.hide(bgdiv);
            pw.$T("body")[0].appendChild(bgdiv);
        }

        var div = pw.$D("_DialogDiv_" + this.ID);
        if (!div) {
            div = document.createElement("div");
            $E.hide(div);
            div.id = "_DialogDiv_" + this.ID;
            div.className = "dialogdiv";
            div.setAttribute("dragStart", "Dialog.dragStart");
            pw.$T("body")[0].appendChild(div);
        }

        this.DialogDiv = div;
        div.innerHTML = arr.join('\n');

        pw.$D("_DialogFrame_" + this.ID).DialogInstance = this;

        this.OKButton = pw.$D("_ButtonOK_" + this.ID);
        this.CancelButton = pw.$D("_ButtonCancel_" + this.ID);

        //显示标题图片
        if (this.ShowMessageRow) {
            $E.show(pw.$D("_MessageRow_" + this.ID));
            if (this.MessageTitle) {
                pw.$D("_MessageTitle_" + this.ID).innerHTML = this.MessageTitle;
            }
            if (this.Message) {
                pw.$D("_Message_" + this.ID).innerHTML = this.Message;
            }
        }

        //显示按钮栏
        if (!this.ShowButtonRow) {
            pw.$D("_ButtonRow_" + this.ID).hide();
        }
        if (this.CancelEvent) {
            this.CancelButton.onclick = this.CancelEvent;
        }
        if (this.OKEvent) {
            this.OKButton.onclick = this.OKEvent;
        }
        if (!this.AlertFlag) {
            $E.show(bgdiv);
            this.bgdivID = "_DialogBGDiv_" + this.ID;
        } else {
            bgdiv = pw.$D("_AlertBGDiv_" + this.ID);
            if (!bgdiv) {
                bgdiv = document.createElement("div");
                bgdiv.id = "_AlertBGDiv_" + this.ID;
                $E.hide(bgdiv);
                pw.$T("body")[0].appendChild(bgdiv);
                bgdiv.style.cssText = "background-color:#333;position:absolute;left:0px;top:0px;opacity:0.4;filter:alpha(opacity=40);width:" + sw + "px;height:" + sh + "px;z-index:991";
            }
            $E.show(bgdiv);
            this.bgdivID = "_AlertBGDiv_" + this.ID;
        }
        this.DialogDiv.style.cssText = "position:absolute; display:block;z-index:" + (this.AlertFlag ? 992 : 990) + ";left:" + this.Left + "px;top:" + this.Top + "px";

        //判断当前窗口是否是对话框，如果是，则将其置在bgdiv之后
        if (!this.AlertFlag) {
            var win = window;
            var flag = false;
            while (win != win.parent) {//需要考虑父窗口是弹出窗口中的一个iframe的情况
                if (win._DialogInstance) {
                    win._DialogInstance.DialogDiv.style.zIndex = 959;
                    flag = true;
                    break;
                }
                win = win.parent;
            }
            if (!flag) {
                bgdiv.style.cssText = "background-color:#333;position:absolute;left:0px;top:0px;opacity:0.4;filter:alpha(opacity=40);width:" + sw + "px;height:" + sh + "px;z-index:960";
            }
        }
        if (this.ShowButtonRow)   
            this.OKButton.focus();
            
        //判断是否支持拖放
        if (this.AllowDrag) {
            var drag = new Drag("_DialogDiv_" + this.ID, { Handle: "_Drag_" + this.ID ,Limit: true });
        }
        
        //放入队列中，以便于ESC时正确关闭
        pw.Dialog._Array.push(this.ID);
    },

    addParam : function(paramName,paramValue){
		    this.DialogArguments[paramName] = paramValue;
    },

    close : function() {
        markCount--;
        if (this.WindowFlag) {
            this.ParentWindow.$D = null;
            this.ParentWindow.$DW = null;
            this.Window.opener = null;
            this.Window.close();
            this.Window = null;
        } else {
            //如果上级窗口是对话框，则将其置于bgdiv前		
            var pw = window;
            var win = window;
            var flag = false;
            while (win != win.parent) {
                if (win._DialogInstance) {
                    flag = true;
                    win._DialogInstance.DialogDiv.style.zIndex = 960;
                    break;
                }
                win = win.parent;
            }
            if (this.AlertFlag) {
                $E.hide(pw.$D("_AlertBGDiv_" + this.ID));
            }
            if (!flag && !this.AlertFlag) {//此处是为处理弹出窗口被关闭后iframe立即被重定向时背景层不消失的问题
                if (markCount <= 0) {
                    pw.eval('window._OpacityFunc = function(){var w = window;$E.hide(w.$D("_DialogBGDiv_' + this.ID + '"));}');
                    pw._OpacityFunc();
                }
            }
            this.DialogDiv.outerHTML = "";
            pw.Dialog._Array.remove(this.ID);
        }
        toggleSelect(true);
    },

    addButton : function(id,txt,func){
	    var html = "<input id='_Button_"+this.ID+"_"+id+"' type='button' value='"+txt+"'> ";
	    var pw = window;
	    pw.$D("_DialogButtons_"+this.ID).$T("input")[0].getParent("a").insertAdjacentHTML("beforeBegin",html);
	    pw.$D("_Button_"+this.ID+"_"+id).onclick = func;
    },

    setPosition : function(){
	    var pw = window;
	    var doc = pw.document;
	    var cw = doc.compatMode == "BackCompat"?doc.body.clientWidth:doc.documentElement.clientWidth;
	    var ch = doc.compatMode == "BackCompat"?doc.body.clientHeight:doc.documentElement.clientHeight;//必须考虑文本框处于页面边缘处，控件显示不全的问题
	    var sl = Math.max(doc.documentElement.scrollLeft, doc.body.scrollLeft);
	    var st = Math.max(doc.documentElement.scrollTop, doc.body.scrollTop);//考虑滚动的情况
	    var sw = Math.max(doc.documentElement.scrollWidth, doc.body.scrollWidth);
	    var sh = Math.max(doc.documentElement.scrollHeight, doc.body.scrollHeight);
	    sw=Math.max(sw,cw);
	    sh=Math.max(sh,ch);
	    this.Top = (ch - this.Height - 30) / 2 + st - 8;//有8像素的透明背景
	    this.Left = (cw - this.Width - 12) / 2 +sl;
	    if(this.ShowButtonRow){//按钮行高36
		    this.Top -= 18;
	    }
	    this.DialogDiv.style.top=this.Top+"px";
	    this.DialogDiv.style.left=this.Left+"px";
	    pw.$D(this.bgdivID).style.width= sw + "px";
	    pw.$D(this.bgdivID).style.height= sh + "px";
    }
};

Dialog._Array = [];

Dialog.close = function(evt){
	window.Args._DialogInstance.close();
};

Dialog.getInstance = function(id){
	var pw = window
	var f = pw.$D("_DialogFrame_"+id);
	if(!f){
		return null;
	}
	return f.DialogInstance;
};

Dialog.AlertNo = 0;
Dialog.alert = function(msg,func,w,h){
	var pw = window
	var diag = new Dialog("_DialogAlert"+Dialog.AlertNo++);
	diag.ParentWindow = pw;
	diag.Width = w?w:300;
	diag.Height = h?h:120;
	diag.Title = "系统消息";
	diag.URL = "javascript:void(0);";
	diag.AlertFlag = true;
	diag.CancelEvent = function(){
		diag.close();
		if(func){
			func();
		}
	};
	diag.show();
	pw.$D("_AlertBGDiv_" + diag.ID).style.display="";
	$E.hide(pw.$D("_ButtonOK_"+diag.ID));
	var win = pw.$D("_DialogFrame_"+diag.ID).contentWindow;
	var doc = win.document;
	doc.open();
	doc.write("<body oncontextmenu='return false;'></body>") ;
	var arr = [];
	arr.push("<table height='100%' border='0' align='center' cellpadding='10' cellspacing='0'>");
	arr.push("<tr><td align='right'><img id='Icon' src='"+CONTEXTPATH+"icon_alert.gif' width='34' height='34' align='absmiddle'></td>");
	arr.push("<td align='left' id='Message' style='font-size:9pt'>"+msg+"</td></tr></table>");
	var div = doc.createElement("div");
	div.innerHTML = arr.join('');
	doc.body.appendChild(div);
	doc.close();
	var h = Math.max(doc.documentElement.scrollHeight, doc.body.scrollHeight);
	var w = Math.max(doc.documentElement.scrollWidth, doc.body.scrollWidth);
	if(w>300){
		win.frameElement.width = w;
	}
	if(h>120){
		win.frameElement.height = h;
	}
	
	diag.CancelButton.value = "确定";
	diag.CancelButton.focus();
	pw.$D("_DialogButtons_"+diag.ID).style.textAlign = "center";
};

Dialog.confirm = function(msg,func1,func2,w,h){
	var pw = window
	var diag = new Dialog("_DialogAlert"+Dialog.AlertNo++);
	diag.Width = w?w:300;
	diag.Height = h?h:120;
    diag.Title = "系统提示";
	diag.URL = "javascript:void(0);";
	diag.AlertFlag = true;
	diag.CancelEvent = function(){
		diag.close();
		if(func2){
			func2();
		}
	};
	diag.OKEvent = function(){
		diag.close();
		if(func1){
			func1();
		}
	};
	diag.show();
	pw.$D("_AlertBGDiv_" + diag.ID).style.dispaly="";
	var win = pw.$D("_DialogFrame_"+diag.ID).contentWindow;
	var doc = win.document;
	doc.open();
	doc.write("<body oncontextmenu='return false;'></body>") ;
	var arr = [];
	arr.push("<table height='100%' border='0' align='center' cellpadding='10' cellspacing='0'>");
	arr.push("<tr><td align='right'><img id='Icon' src='"+CONTEXTPATH+"icon_query.gif' width='34' height='34' align='absmiddle'></td>");
	arr.push("<td align='left' id='Message' style='font-size:9pt'>"+msg+"</td></tr></table>");
	var div = doc.createElement("div");
	div.innerHTML = arr.join('');
	doc.body.appendChild(div);
	doc.close();
	diag.OKButton.focus();
	pw.$D("_DialogButtons_"+diag.ID).style.textAlign = "center";
};

var _DialogInstance = window.frameElement?window.frameElement.DialogInstance:null;
var Page={};
Page.onDialogLoad = function(){
	if(_DialogInstance){
		if(_DialogInstance.Title){
			document.title = _DialogInstance.Title;
		}
		window.Args = _DialogInstance.DialogArguments;
		_DialogInstance.Window = window;
		window.Parent = _DialogInstance.ParentWindow;
	}
};

Page.onDialogLoad();

PageOnLoad=function (){
	var d = _DialogInstance;
	if(d){
		try{
			if(d.AlertFlag){
				$E.show(window.$D("_AlertBGDiv_" + d.ID));
			}
			if(d.ShowButtonRow&&$E.visible(d.CancelButton)){
				d.CancelButton.focus();
			}
			if(d.onLoad){
				d.onLoad();
			}
		}catch(ex){alert("DialogOnLoad:"+ex.message+"\t("+ex.fileName+" "+ex.lineNumber+")");}
	}
};

Dialog.onKeyUp = function(event){
	if(event.keyCode==9){
		var pw = window;
		if(pw.Dialog._Array.length>0){
			stopEvent(event);
		}
	}
	if(event.keyCode==27){
		var pw = window;
		if(pw.Dialog._Array.length>0){
			//Page.mousedown();
			//Page.click();
			var diag = pw.Dialog.getInstance(pw.Dialog._Array[pw.Dialog._Array.length-1]);
			diag.CancelButton.onclick.apply(diag.CancelButton,[]);
		}
	}
};

Dialog.setPosition=function(){
	if(window.parent!=window)return;
	var pw = window;
	var DialogArr=pw.Dialog._Array;
	if(DialogArr==null||DialogArr.length==0)return;
	
	for(i=0;i<DialogArr.length;i++)
	{
	    if(pw.$D("_DialogFrame_"+DialogArr[i])) {
		    pw.$D("_DialogFrame_"+DialogArr[i]).DialogInstance.setPosition();
		}
	}
};

//按ESC关闭对话框
if(isIE){
	document.attachEvent("onkeydown",Dialog.onKeyUp);
	window.attachEvent("onload",PageOnLoad);
	window.attachEvent('onresize',Dialog.setPosition);
}else{
	document.addEventListener("keydown",Dialog.onKeyUp,false);
	window.addEventListener("load",PageOnLoad,false);
	window.addEventListener('resize',Dialog.setPosition,false);
}

//Drag拖拽效果
var Drag = Class.create();

Drag.prototype = {

  //拖放对象
  initialize: function(drag, options) {
	this.Drag = $D(drag);//拖放对象
	this._x = this._y = 0;//记录鼠标相对拖放对象的位置
	this._marginLeft = this._marginTop = 0;//记录margin
	//事件对象(用于绑定移除事件)
	this._fM = BindAsEventListener(this, this.Move);
	this._fS = Bind(this, this.Stop);
	
	//设置传入的options
	this.SetOptions(options);
	
	this.Limit = !!this.options.Limit;
	this.mxLeft = parseInt(this.options.mxLeft);
	this.mxRight = parseInt(this.options.mxRight);
	this.mxTop = parseInt(this.options.mxTop);
	this.mxBottom = parseInt(this.options.mxBottom);
	
	this.LockX = !!this.options.LockX;
	this.LockY = !!this.options.LockY;
	this.Lock = !!this.options.Lock;
	
	this.onStart = this.options.onStart;
	this.onMove = this.options.onMove;
	this.onStop = this.options.onStop;
	
	this._Handle = $D(this.options.Handle) || this.Drag;
	this._Handle.style.cursor = 'move';
	this._mxContainer = $D(this.options.mxContainer) || null;
	
	this.Drag.style.position = "absolute";
	//透明
	if(isIE && !!this.options.Transparent){
		//填充拖放对象
		with(this._Handle.appendChild(document.createElement("div")).style){
			width = height = "100%"; backgroundColor = "#fff"; filter = "alpha(opacity:0)"; fontSize = 0;
		}
	}
	//修正范围
	this.Repair();
	addEventHandler(this._Handle, "mousedown", BindAsEventListener(this, this.Start));
  },
  //设置默认属性
  SetOptions: function(options) {
	this.options = {//默认值
		Handle:			"",//设置触发对象（不设置则使用拖放对象）
		Limit:			false,//是否设置范围限制(为true时下面参数有用,可以是负数)
		mxLeft:			0,//左边限制
		mxRight:		9999,//右边限制
		mxTop:			0,//上边限制
		mxBottom:		9999,//下边限制
		mxContainer:	"",//指定限制在容器内
		LockX:			false,//是否锁定水平方向拖放
		LockY:			false,//是否锁定垂直方向拖放
		Lock:			false,//是否锁定
		Transparent:	false,//是否透明
		onStart:		function(){},//开始移动时执行
		onMove:			function(){},//移动时执行
		onStop:			function(){}//结束移动时执行
	};
	Extend(this.options, options || {});
  },
  //准备拖动
  Start: function(oEvent) {
	if(this.Lock){ return; }
	this.Repair();
	//记录鼠标相对拖放对象的位置
	this._x = oEvent.clientX - this.Drag.offsetLeft;
	this._y = oEvent.clientY - this.Drag.offsetTop;
	//记录margin
	this._marginLeft = parseInt(CurrentStyle(this.Drag).marginLeft) || 0;
	this._marginTop = parseInt(CurrentStyle(this.Drag).marginTop) || 0;
	//mousemove时移动 mouseup时停止
	addEventHandler(document, "mousemove", this._fM);
	addEventHandler(document, "mouseup", this._fS);
	if(isIE){
		//焦点丢失
		addEventHandler(this._Handle, "losecapture", this._fS);
		//设置鼠标捕获
		this._Handle.setCapture();
	}else{
		//焦点丢失
		addEventHandler(window, "blur", this._fS);
		//阻止默认动作
		oEvent.preventDefault();
	};
	//附加程序
	this.onStart();
  },
  //修正范围
  Repair: function() {
	if(this.Limit){
		//修正错误范围参数
		this.mxRight = Math.max(this.mxRight, this.mxLeft + this.Drag.offsetWidth);
		this.mxBottom = Math.max(this.mxBottom, this.mxTop + this.Drag.offsetHeight);
		//如果有容器必须设置position为relative或absolute来相对或绝对定位，并在获取offset之前设置
		!this._mxContainer || CurrentStyle(this._mxContainer).position == "relative" || CurrentStyle(this._mxContainer).position == "absolute" || (this._mxContainer.style.position = "relative");
	}
  },
  //拖动
  Move: function(oEvent) {
	//判断是否锁定
	if(this.Lock){ this.Stop(); return; };
	//清除选择
	window.getSelection ? window.getSelection().removeAllRanges() : document.selection.empty();
	//设置移动参数
	var iLeft = oEvent.clientX - this._x, iTop = oEvent.clientY - this._y;
	//设置范围限制
	if(this.Limit){
		//设置范围参数
		var mxLeft = this.mxLeft, mxRight = this.mxRight, mxTop = this.mxTop, mxBottom = this.mxBottom;
		//如果设置了容器，再修正范围参数
		if(!!this._mxContainer){
			mxLeft = Math.max(mxLeft, 0);
			mxTop = Math.max(mxTop, 0);
			mxRight = Math.min(mxRight, this._mxContainer.clientWidth);
			mxBottom = Math.min(mxBottom, this._mxContainer.clientHeight);
		};
		//修正移动参数
		iLeft = Math.max(Math.min(iLeft, mxRight - this.Drag.offsetWidth), mxLeft);
		iTop = Math.max(Math.min(iTop, mxBottom - this.Drag.offsetHeight), mxTop);
	}
	//设置位置，并修正margin
	if(!this.LockX){ this.Drag.style.left = iLeft - this._marginLeft + "px"; }
	if(!this.LockY){ this.Drag.style.top = iTop - this._marginTop + "px"; }
	//附加程序
	this.onMove();
  },
  //停止拖动
  Stop: function() {
	//移除事件
	removeEventHandler(document, "mousemove", this._fM);
	removeEventHandler(document, "mouseup", this._fS);
	if(isIE){
		removeEventHandler(this._Handle, "losecapture", this._fS);
		this._Handle.releaseCapture();
	}else{
		removeEventHandler(window, "blur", this._fS);
	};
	//附加程序
	this.onStop();
  }
};