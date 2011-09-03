//html编辑器
var HTMLEditorPath = '/htmleditor/';

//id为frame对应的iframe对象id
var HTMLEditor = function(id, width, height) {

    //把当前frameid赋值给变量id
    this.id = id;
    this.width = width || '100%';
    this.height = height || '320px';

    this.exists = function() {
        var frameid = this.id + '___Frame';
        var frame = document.getElementById(frameid);
        if (frame) {
            return true;
        }
    };

    this.create = function(container) {
        var frameid = this.id + '___Frame';
        var frame = document.getElementById(frameid);
        if (frame) {
            alert('ID为【' + this.id + '】的HTML编辑器已经存在！');
            return;
        }

        if (container) {
            if (typeof (container) == 'string')
                container = document.getElementById(container);
        }
        else {
            container = document.getElementsByTagName('body')[0];
        }

        var html = '<iframe id="' + this.id + '___Frame" name="' + this.id + '___Frame" src="' + HTMLEditorPath + 'editor.htm" width="' +
            this.width + '" height="' + this.height + '" frameborder="no" scrolling="no"></iframe>'
        var div = document.createElement('div');
        div.innerHTML = html;

        if (!container) {
            alert('传入的容器id不存在，请查证！');
            return;
        }
        container.appendChild(div);
    };

    //设置焦点
    this.setFocus = function() {
        var frameid = this.id + '___Frame';
        var frame = window.frames[frameid];
        if (frame) {
            var htmlframe = frame.window.frames['HtmlEditor'];
            if (htmlframe) {
                htmlframe.focus();
            }
        }
    };

    //给编辑器赋值
    this.setHTML = function(text) {
        var frameid = this.id + '___Frame';
        var iframeload = false;
        var sid = null;
        var setEditorValue = function() {
            var frame = window.frames[frameid];
            if (frame) {
                var htmlframe = frame.window.frames['HtmlEditor'];
                if (htmlframe) {
                    var body = htmlframe.document.getElementsByTagName('body')[0];
                    if (body) {
                        if (sid) {
                            clearInterval(sid);
                        }
                        text = text.replace(/&lt;/ig, '<')
                        text = text.replace(/&gt;/ig, '>')
                        //text = text.replace(/&amp;/ig,'&')

                        body.innerHTML = text;
                        iframeload = true;
                    }
                }
            }
        }

        //先试着设置一下值
        setEditorValue();

        if (!iframeload) {
            var iframe = document.getElementById(frameid);
            if (iframe.attachEvent) {
                iframe.attachEvent("onload", function() {
                    //alert("IE Local iframe is now loaded."); 
                    sid = setInterval(setEditorValue, 100);
                });
            }
            else {
                iframe.onload = function() {
                    //alert("FireFox Local iframe is now loaded."); 
                    setEditorValue();
                };
            }
        }
    };

    //从编辑器取值
    this.getHTML = function() {

        var text = this.getText();

        //text = text.replace(/\&/ig,'&amp;')
        text = text.replace(/\</ig, '&lt;')
        text = text.replace(/\>/ig, '&gt;')

        return text;
    };

    //获取原始内容
    this.getText = function() {
        var text = "";
        var frameid = this.id + '___Frame';
        var frame = window.frames[frameid];
        if (frame) {
            var editor = frame.document.getElementById("sourceEditor");
            if (editor && editor.style.display != 'none') {
                text = editor.value;
            }
            else {
                var htmlframe = frame.window.frames['HtmlEditor'];
                if (htmlframe) {
                    var body = htmlframe.document.getElementsByTagName('body')[0];
                    if (body) {
                        text = body.innerHTML;
                    }
                }
            }
        }

        return text;
    };

    //给编辑器追加值
    this.appendHTML = function(text) {
        var html = this.getHTML();
        this.setHTML(html + text);
    };
}