import { GlobalInfo } from "./GlobalInfo";

export class WebSocketConnection {
    private _websocket: WebSocket = <any>null;
    Url: string;
    private _serverUrl: string = "";
    onMessage: (sender: any, data: any) => void = <any>null;
    onDisconnect: () => void = <any>null;
    onConnect: () => void = <any>null;

    constructor(url: string) {
        var serverUrl = GlobalInfo.ServerUrl;
        serverUrl = serverUrl.replace("http://", "ws://");
        this._serverUrl = serverUrl.replace("https://", "wss://");


        this.Url = url;
    }

    private keepAliveTimer = 0;
    keepAlive = ()=>{
        if(!this._websocket)
            return;

        this._websocket?.send("{}");
        this.keepAliveTimer = window.setTimeout(() => this.keepAlive(), 1000);
    }


    listen = () => {
        this._websocket = new WebSocket(`${this._serverUrl}${this.Url}`);
        this._websocket.onopen = () => {
            console.log("websocket已连接");
            if (this.onConnect) {
                this.onConnect();
            }
        };
        this._websocket.onclose = () => {
            console.log("websocket已关闭");
            window.clearTimeout(this.keepAliveTimer);
            if (this._websocket) {
                this._websocket = <any>null;
                if (this.onDisconnect) {
                    this.onDisconnect();
                }
                window.setTimeout(() => this.listen(), 1000);
            }
        };
        this._websocket.onerror = () => {
            console.log("websocket发生错误");
            window.clearTimeout(this.keepAliveTimer);
            if (this._websocket) {
                this._websocket = <any>null;
                if (this.onDisconnect) {
                    this.onDisconnect();
                }
                window.setTimeout(() => this.listen(), 1000);
            }
        };

        this._websocket.onmessage = (e: MessageEvent) => {
            if (this._websocket && this.onMessage) {
                this.onMessage(this, e.data);
            }
        };
    }

    stop = () => {
        var socket = this._websocket;
        this._websocket = <any>null;
        socket?.close();
    }

    send = (text: string) => {
        this._websocket?.send(text);
    }
}