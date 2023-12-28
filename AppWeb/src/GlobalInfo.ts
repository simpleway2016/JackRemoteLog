import { ref, reactive } from "vue"
import type { WebSocketConnection } from "./WebSocketConnection";
import { POSITION, useToast } from "vue-toastification";
var toast = useToast();
export class GlobalInfo {
    static UserInfo = reactive({
        Token: "",
        Name: ""
    });

    static PublicInfo = reactive({
        AppContexts : <any[]>[],
        LevelObjs:[
            {name:"Trace" , value:0},
            {name:"Debug" , value:1},
            {name:"Information" , value:2},
            {name:"Warning" , value:3},
            {name:"Error" , value:4},
            {name:"Critical" , value:5}],
        SelectedAppContexts:<any[]>[],
        SelectedLevels:<any[]>[],
        SelectedTraceIds:<any[]>[],
        SearchKeys:<any[]>[],
        StartTime:"2020-1-1",
        EndTime:"",
    });

    static ServerUrl: string = <any>process.env.ServerUrl;
    static RefreshTokenInterval = 60000;

    static IsGoBack = false;
    static goBackAction: any = null;

    static showError = (err: any) => {
        if (err && typeof err === "string") {
            alert(err);
            return;
        }
        console.error("showError", err);
        if (err && err.statusCode == 401) {
            GlobalInfo.UserInfo.Token = "";
            return;
        }
        if (err.errCode) {
            toast(err.msg, {
                position: POSITION.BOTTOM_CENTER,
                closeOnClick: true,
                timeout: false
            });
        }
        else {
            toast(JSON.stringify(err), {
                position: POSITION.BOTTOM_CENTER,
                closeOnClick: true,
                timeout: false
            });
        }
    }

    static init = () => {
        if (GlobalInfo.ServerUrl == "..") {
            GlobalInfo.ServerUrl = `${location.protocol}//${location.host}`;
        }
        window.setTimeout(GlobalInfo.refreshToken, GlobalInfo.RefreshTokenInterval);
    }

    static async refreshToken() {
        //console.log("刷新token");
        try {
            if (GlobalInfo.UserInfo.Token) {
                var token = await GlobalInfo.get("/User/RefreshToken", undefined);
                //console.log(token);
                GlobalInfo.UserInfo.Token = token;
                localStorage.setItem("Token", token);
            }
        } catch (error: any) {
            if (error.statusCode == 401) {
                GlobalInfo.UserInfo.Token = "";
            }
        }
        finally {
            window.setTimeout(GlobalInfo.refreshToken, 10000);
        }
    }

    static arrayBufferToBase64 = (ab: ArrayBuffer) => {
        var binary = '';
        var bytes = new Uint8Array(ab);
        var len = bytes.byteLength;
        for (var i = 0; i < len; i++) {
            binary += String.fromCharCode(bytes[i]);
        }
        return window.btoa(binary); // 使用btoa()方法将字符串转换为base64编码
    }



    /**
     * 提交表单
     * @param url 根目录路径 如：/Doc
     * @param bodyObj 参数对象
     * @returns 服务器返回信息
     */
    static post = async (url: string, bodyObj: any): Promise<string> => {

        var bodyStr: string = "";

        if (bodyObj) {
            for (var pro in bodyObj) {
                var val = bodyObj[pro];
                if (val == undefined || val == null)
                    continue;
                if (bodyStr.length > 0)
                    bodyStr += "&";

                if (Array.isArray(val)) {
                    for (var i = 0; i < val.length; i++) {
                        if (i > 0)
                            bodyStr += "&";
                        bodyStr += pro + "=" + encodeURIComponent(val[i]);
                    }
                }
                else {
                    bodyStr += pro + "=" + encodeURIComponent(val);
                }
            }
        }

        var ret = await fetch(`${GlobalInfo.ServerUrl}${url}`, {
            method: 'POST',
            headers: {
                'Content-Type': "application/x-www-form-urlencoded",
                'Authorization': GlobalInfo.UserInfo.Token
            },
            body: bodyStr
        });
        var text = await ret.text();
        if (ret.status >= 300 || ret.status < 200) {
            if (text)
                throw text;
            else
                throw { statusCode: ret.status };
        }

        return text;


    };

     /**
     * 提交表单
     * @param url 根目录路径 如：/Doc
     * @param bodyObj 参数对象
     * @returns 服务器返回信息
     */
     static postForBlob = async (url: string, bodyObj: any): Promise<Blob> => {

        var bodyStr: string = "";

        if (bodyObj) {
            for (var pro in bodyObj) {
                var val = bodyObj[pro];
                if (val == undefined || val == null)
                    continue;
                if (bodyStr.length > 0)
                    bodyStr += "&";

                if (Array.isArray(val)) {
                    for (var i = 0; i < val.length; i++) {
                        if (i > 0)
                            bodyStr += "&";
                        bodyStr += pro + "=" + encodeURIComponent(val[i]);
                    }
                }
                else {
                    bodyStr += pro + "=" + encodeURIComponent(val);
                }
            }
        }

        var ret = await fetch(`${GlobalInfo.ServerUrl}${url}`, {
            method: 'POST',
            headers: {
                'Content-Type': "application/x-www-form-urlencoded",
                'Authorization': GlobalInfo.UserInfo.Token
            },
            body: bodyStr
        });

        if (ret.status >= 300 || ret.status < 200) {
            var text = await ret.text();
            if (text)
                throw text;
            else
                throw { statusCode: ret.status };
        }

        return await ret.blob();


    };

    static postJson = async (url: string, bodyObj: any): Promise<string> => {


        var ret = await fetch(`${GlobalInfo.ServerUrl}${url}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': GlobalInfo.UserInfo.Token
            },
            body: JSON.stringify(bodyObj)
        });

        var text = await ret.text();
        if (ret.status >= 300 || ret.status < 200) {
            if (text)
                throw text;
            else
                throw { statusCode: ret.status };
        }

        return text;


    };

    static postFormData = async (url: string, formdata: FormData): Promise<string> => {


        var ret = await fetch(`${GlobalInfo.ServerUrl}${url}`, {
            method: 'POST',
            headers: {
                'Authorization': GlobalInfo.UserInfo.Token
            },
            body: formdata
        });
        var text = await ret.text();
        if (ret.status >= 300 || ret.status < 200) {
            if (text)
                throw text;
            else
                throw { statusCode: ret.status };
        }

        return text;


    };

    /**
     * 
     * @param url 
     * @param bodyObj 
     * @returns 
     */
    static get = async (url: string, bodyObj: any, token: any = null): Promise<any> => {

        var bodyStr: string = "";

        if (bodyObj) {
            for (var pro in bodyObj) {
                var val = bodyObj[pro];
                if (val == undefined || val == null)
                    continue;
                if (bodyStr.length > 0)
                    bodyStr += "&";

                if (Array.isArray(val)) {
                    for (var i = 0; i < val.length; i++) {
                        if (i > 0)
                            bodyStr += "&";
                        bodyStr += pro + "=" + encodeURIComponent(val[i]);
                    }
                }
                else {
                    bodyStr += pro + "=" + encodeURIComponent(val);
                }
            }
        }

        if (url.indexOf("?") > 0)
            url += "&" + bodyStr;
        else
            url += "?" + bodyStr;

        var ret = await fetch(`${GlobalInfo.ServerUrl}${url}`, {
            method: 'GET',
            headers: {
                'Authorization': token ? token : GlobalInfo.UserInfo.Token
            }
        });

        var text = await ret.text();
        if (ret.status >= 300 || ret.status < 200) {
            if (text)
                throw text;
            else
                throw { statusCode: ret.status };
        }

        return text;

    };

     /**
     * 
     * @param url 
     * @param bodyObj 
     * @returns 
     */
     static getForBlob = async (url: string, bodyObj: any, token: any = null): Promise<Blob> => {

        var bodyStr: string = "";

        if (bodyObj) {
            for (var pro in bodyObj) {
                var val = bodyObj[pro];
                if (val == undefined || val == null)
                    continue;
                if (bodyStr.length > 0)
                    bodyStr += "&";

                if (Array.isArray(val)) {
                    for (var i = 0; i < val.length; i++) {
                        if (i > 0)
                            bodyStr += "&";
                        bodyStr += pro + "=" + encodeURIComponent(val[i]);
                    }
                }
                else {
                    bodyStr += pro + "=" + encodeURIComponent(val);
                }
            }
        }

        if (url.indexOf("?") > 0)
            url += "&" + bodyStr;
        else
            url += "?" + bodyStr;

        var ret = await fetch(`${GlobalInfo.ServerUrl}${url}`, {
            method: 'GET',
            headers: {
                'Authorization': token ? token : GlobalInfo.UserInfo.Token
            }
        });

       
        if (ret.status >= 300 || ret.status < 200) {
            var text = await ret.text();
            if (text)
                throw text;
            else
                throw { statusCode: ret.status };
        }

        return await ret.blob();

    };
}