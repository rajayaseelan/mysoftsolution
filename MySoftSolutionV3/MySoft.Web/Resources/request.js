/*****
* 属性 
* url //content //method //async //timeout //ontimeout //oncomplete //onexception 
* 方法 
* setcharset(charset) 
* get([url],[callback]) 
* post([url],[content],[callback]) 
* postf(form_obj,[callback],[url],[method]) 
* update([update_obj],[url],[update_interval],[update_times]) 
*****/

function AJAXRequest() {
    var xmlPool = [], AJAX = this, ac = arguments.length, av = arguments;
    var xmlVersion = ["Msxml2.XMLHTTP.6.0", "Msxml2.XMLHTTP.3.0", "MSXML2.XMLHTTP", "Microsoft.XMLHTTP"];
    var emptyFun = function() { };
    var av = ac > 0 ? typeof (av[0]) == "object" ? av[0] : {} : {};
    var encode = av.charset ? av.charset.toUpperCase() == "UTF-8" ? encodeURIComponent : escape : encodeURIComponent;
    this.header = getp(av.header, null);
    this.url = getp(av.url, null);
    this.oncomplete = getp(av.oncomplete, emptyFun);
    this.content = getp(av.content, null);
    this.method = getp(av.method, "POST");
    this.async = av.async == undefined ? true : av.async;
    this.onexception = getp(av.onexception, emptyFun);
    this.ontimeout = getp(av.ontimeout, emptyFun);
    this.timeout = getp(av.timeout, 3600000);
    this.onrequeststart = getp(av.onstartrequest, emptyFun);
    this.onrequestend = getp(av.onendrequest, emptyFun);
    if (!createRequest()) return false;
    function getp(p, d) { return p ? p : d; };
    function createRequest() {
        var i, j, tmpObj;
        for (i = 0, j = xmlPool.length; i < j; i++) if (xmlPool[i].readyState == 0 || xmlPool[i].readyState == 4) return xmlPool[i];
        try { tmpObj = new XMLHttpRequest; }
        catch (e) {
            for (i = 0, j = xmlVersion.length; i < j; i++) {
                try { tmpObj = new ActiveXObject(xmlVersion[i]); } catch (e2) { continue; }
                break;
            }
        }
        if (!tmpObj) return false;
        else { xmlPool[xmlPool.length] = tmpObj; return xmlPool[xmlPool.length - 1]; }
    };
    function $(id) { return document.getElementById(id); }
    function $N(n, d) { n = parseInt(n); return (isNaN(n) ? d : n); }
    function $VO(v) {
        if (typeof (v) == "string") {
            if (v = $(v)) return v;
            else return false;
        }
        else return v;
    };
    function $ST(obj, text) {
        var nn = obj.nodeName.toUpperCase();
        if ("INPUT|TEXTAREA".indexOf(nn) > -1) obj.value = text;
        else try { obj.innerHTML = text; } catch (e) { };
    };
    function $CB(cb) {
        if (typeof (cb) == "function") return cb;
        else {
            cb = $VO(cb);
            if (cb) return (function(obj) { $ST(cb, obj.responseText); });
            else return emptyFun;
        }
    };
    function send(purl, pc, pcbf, pm, pa) {
        var purl, pc, pcbf, pm, pa, ct, ctf = false, xmlHttp = createRequest(), ac = arguments.length, av = arguments;
        if (!xmlHttp) return false;
        if (!pm || !purl || (pa == undefined)) return false;
        var ev = { url: purl, content: pc, method: pm };
        purl+=(purl.indexOf("?")>-1?"&":"?")+Math.random();
        xmlHttp.open(pm, purl, pa);
        AJAX.onrequeststart(ev);
        xmlHttp.setRequestHeader("X-Requested-With", "XMLHttpRequest");
        xmlHttp.setRequestHeader("If-Modified-Since", "Thu, 01 Jan 1970 00:00:00 GMT");
        if (pm == "POST") xmlHttp.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        if (AJAX.header) {
            for (var i = 0; i < AJAX.header.length; i++) {
                xmlHttp.setRequestHeader(AJAX.header[i][0], AJAX.header[i][1]);
            }
        }
        ct = setTimeout(function() { ctf = true; xmlHttp.abort(); }, AJAX.timeout);
        if (window.ActiveXObject) {
            xmlHttp.onreadystatechange = callbackRequest;
        } else {
            xmlHttp.onload = callbackRequest;
        }
        function callbackRequest() {
            if (ctf) { AJAX.ontimeout(ev); AJAX.onrequestend(ev); }
            else if (xmlHttp.readyState == 4) {
                ev.status = xmlHttp.status;
                ev.statusText = xmlHttp.statusText;
                if (xmlHttp.status != 200) ev.errorText = xmlHttp.responseText;
                try { clearTimeout(ct); } catch (e) { };
                try { if (xmlHttp.status == 200 && xmlHttp.statusText == 'OK') pcbf(xmlHttp); else AJAX.onexception(ev); }
                catch (e) { AJAX.onexception(ev); }
                AJAX.onrequestend(ev);
            }
        }
        if (pm == "POST") xmlHttp.send(pc); else xmlHttp.send(null);
    };
    this.setcharset = function(cs) {
        if (cs.toUpperCase() == "UTF-8") encode = encodeURIComponent; else encode = escape;
    };
    this.setcallback = function() {
        var ac = arguments.length, av = arguments;
        if (ac <= 0) return;
        this.onrequeststart = av[0];
        this.onrequestend = av[1];
        this.onexception = av[2];
        this.ontimeout = av[3];
        this.timeout = av[4]
    };
    this.setheader = function(header) {
        this.header = getp(header, null);
    };
    this.call = function() {
        var ac = arguments.length, av = arguments;
        var av = ac > 0 ? typeof (av[0]) == "object" ? av[0] : {} : {};
        this.url = getp(av.url, null);
        this.content = getp(av.content, null);
        this.method = getp(av.method, "POST");
        this.async = av.async == undefined ? true : av.async;
        this.timeout = getp(av.timeout, 3600000);
        this.oncomplete = getp(av.oncomplete, emptyFun);
        send(this.url, this.content, this.oncomplete, this.method, this.async);
    };
    this.get = function() {
        var purl, pcbf, pa, ac = arguments.length, av = arguments;
        purl = ac > 0 ? av[0] : this.url;
        pcbf = ac > 1 ? $CB(av[1]) : this.oncomplete;
        pa = ac > 2 ? av[2] : this.async;
        if (!purl && !pcbf) return false;
        send(purl, null, pcbf, "GET", pa);
    };
    this.post = function() {
        var purl, pcbf, pa, pc, ac = arguments.length, av = arguments;
        purl = ac > 0 ? av[0] : this.url;
        pc = ac > 1 ? av[1] : null;
        pcbf = ac > 2 ? $CB(av[2]) : this.oncomplete;
        pa = ac > 3 ? av[3] : this.async;
        if (!purl && !pcbf) return false;
        send(purl, pc, pcbf, "POST", pa);
    };
    this.postf = function() {
        var fo, vaf, pcbf, purl, pc, pm, ac = arguments.length, av = arguments;
        fo = ac > 0 ? $VO(av[0]) : false;
        if (!fo || (fo && fo.nodeName != "FORM")) return false;
        vaf = fo.getAttribute("onsubmit");
        vaf = vaf ? (typeof (vaf) == "string" ? new Function(vaf) : vaf) : null;
        if (vaf && !vaf()) return false;
        pcbf = ac > 1 ? $CB(av[1]) : this.oncomplete;
        purl = ac > 2 ? av[2] : (fo.action ? fo.action : this.url);
        pm = ac > 3 ? av[3] : (fo.method ? fo.method.toUpperCase() : "POST");
        if (!pcbf && !purl) return false;
        pc = this.formToStr(fo);
        if (!pc) return false;
        if (pm) {
            if (pm == "POST") send(purl, pc, pcbf, "POST", true);
            else if (purl.indexOf("?") > 0) send(purl + "&" + pc, null, pcbf, "GET", true);
            else send(purl + "?" + pc, null, pcbf, "GET", true);
        }
        else send(purl, pc, pcbf, "POST", true);
    };
    this.update = function() {
        var purl, puo, pinv, pcnt, ac = arguments.length, av = arguments;
        puo = ac > 0 ? $CB(av[0]) : emptyFun;
        purl = ac > 1 ? av[1] : this.url;
        pinv = ac > 2 ? $N(av[2], 1000) : null;
        pcnt = ac > 3 ? $N(av[3], 0) : null;
        if (pinv) {
            send(purl, null, puo, "GET", true);
            if (pcnt && --pcnt) {
                var cf = function(cc) {
                    send(purl, null, puo, "GET", true);
                    if (cc < 1) return; else cc--;
                    setTimeout(function() { cf(cc); }, pinv);
                }
                setTimeout(function() { cf(--pcnt); }, pinv);
            }
            else return (setInterval(function() { send(purl, null, puo, "GET", true); }, pinv));
        }
        else send(purl, null, puo, "GET", true);
    };
    this.formToStr = function(fc) {
        var i, qs = '', and = '', ev = '';
        for (i = 0; i < fc.length; i++) {
            e = fc[i];
            if (e.name != '') {
                if (e.type == 'select-one' && e.selectedIndex > -1) ev = e.options[e.selectedIndex].value;
                else if (e.type == 'checkbox' || e.type == 'radio') {
                    if (e.checked == false) continue;
                    ev = e.value;
                }
                else if (e.type == 'hidden' || e.type == 'text' || e.type == 'textarea') ev = e.value;
                else continue;
                ev = encode(ev);
                qs += and + e.name + '=' + ev;
                and = '&';
            }
        }
        return qs;
    };
}
