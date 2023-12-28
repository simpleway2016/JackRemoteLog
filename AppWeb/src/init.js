Date.prototype.Format = function (fmt) { //author: meizz   
    var o = {
        "M+": this.getMonth() + 1,                 //月份   
        "d+": this.getDate(),                    //日   
        "h+": this.getHours(),                   //小时   
        "H+": this.getHours(),                   //小时   
        "m+": this.getMinutes(),                 //分   
        "s+": this.getSeconds(),                 //秒   
        "q+": Math.floor((this.getMonth() + 3) / 3), //季度   
        "S": this.getMilliseconds()             //毫秒   
    };
    if (/(y+)/.test(fmt))
        fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
    for (var k in o)
        if (new RegExp("(" + k + ")").test(fmt))
            fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
    return fmt;
};
String.prototype.toFixed = function () {
    return this;
};

//防止低版本浏览器不支持以下函数
if (!"".endsWith) {
    String.prototype.endsWith = function (str) {
        var index = this.indexOf(str);
        return index + str.length === this.length;
    };
}
if (!"".startsWith) {
    String.prototype.endsWith = function (str) {
        var index = this.indexOf(str);
        return index === 0;
    };
}

if (![].forEach) {
    Array.prototype.forEach = function (action) {
        for (var i = 0; i < this.length; i++) {
            action(this[i], i, this);
        }
    }
}

if (![].find) {
    Array.prototype.find = function (action) {
        for (var i = 0; i < this.length; i++) {
            if (action(this[i], i, this)) {
                return this[i];
            }
        }
        return undefined;
    }
}
if (![].findIndex) {
    Array.prototype.findIndex = function (action) {
        for (var i = 0; i < this.length; i++) {
            if (action(this[i], i, this)) {
                return i;
            }
        }
        return -1;
    }
}
if (![].map) {
    Array.prototype.map = function (fn) {
        var newArr = []; var arr = this;
        for (var i = 0; i < arr.length; i++) {
            var s = fn.call(this, arr[i], i, arr);
            if (s) {
                newArr.push(s);
            }
        }
        return newArr;
    }
}
if (![].filter) {
    Array.prototype.filter = function (fn) {
        var newArr = []; var arr = this;
        for (var i = 0; i < arr.length; i++) {
            var s = fn.call(this, arr[i], i, arr);
            if (s) {
                newArr.push(s);
            }
        }
        return newArr;
    }
}

if (!NodeList.prototype.forEach) {
    NodeList.prototype.forEach = function (action) {
        for (var i = 0; i < this.length; i++) {
            action(this[i], i, this);
        }
    }
}

if (!NodeList.prototype.find) {
    NodeList.prototype.find = function (action) {
        for (var i = 0; i < this.length; i++) {
            if (action(this[i], i, this)) {
                return this[i];
            }
        }
        return undefined;
    }
}
if (!NodeList.prototype.findIndex) {
    NodeList.prototype.findIndex = function (action) {
        for (var i = 0; i < this.length; i++) {
            if (action(this[i], i, this)) {
                return i;
            }
        }
        return -1;
    }
}
if (!NodeList.prototype.map) {
    NodeList.prototype.map = function (fn) {
        var newArr = [];
        var arr = this;
        for (var i = 0; i < arr.length; i++) {
            var s = fn.call(this, arr[i], i, arr);
            if (s) {
                newArr.push(s);
            }
        }
        return newArr;
    }
}
if (!NodeList.prototype.filter) {
    NodeList.prototype.filter = function (fn) {
        var newArr = [];
        var arr = this;
        for (var i = 0; i < arr.length; i++) {
            var s = fn.call(this, arr[i], i, arr);
            if (s) {
                newArr.push(s);
            }
        }
        return newArr;
    }
}

function hidePhone(phone) {
    try {
        if (phone.length === 3) {
            return phone.substr(0, 1) + "*" + phone.substr(2, 1);
        }
        else if (phone.length === 2) {
            return "*" + phone.substr(1, 1);
        }
        else if (phone.length > 3 && phone.length < 5) {
            return phone.substr(0, 1) + "*" + phone.substr(phone.length - 1, 1);
        }
        return "****" + phone.substr(phone.length - 4, 4);
    }
    catch (e) {
        return "****" + phone;
    }
}

String.prototype.version = function () {
    if (!this)
        return "";
        
    if (this.endsWith(".0"))
        return this.substr(0,this.length - 2);

    return this;
};

export default {}