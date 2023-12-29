const fs = require("fs");


//判断文件夹是否存在
if (fs.existsSync("../Jack.RemoteLog.WebApi/wwwroot/assets")) {
    //删除文件夹
    fs.rmSync("../Jack.RemoteLog.WebApi/wwwroot/assets", { recursive: true });
}

//创建文件夹
fs.mkdirSync("../Jack.RemoteLog.WebApi/wwwroot/assets");

//拷贝文件夹 要求nodejs>=16.7.0
fs.cpSync("./dist/assets", "../Jack.RemoteLog.WebApi/wwwroot/assets", { recursive: true });

//拷贝文件
fs.copyFileSync("./dist/index.html", "../Jack.RemoteLog.WebApi/wwwroot/index.html");

console.log("成功拷贝文件到../Jack.RemoteLog.WebApi/wwwroot/assets");
