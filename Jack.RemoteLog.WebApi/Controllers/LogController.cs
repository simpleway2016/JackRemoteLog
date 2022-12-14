using Jack.RemoteLog.WebApi.Applications;
using Jack.RemoteLog.WebApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;
using System.Text;

namespace Jack.RemoteLog.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class LogController : ControllerBase
    {
        LogService _logService;
        public LogController(LogService logService)
        {
            this._logService = logService;

        }

        [HttpGet]
        public string GetVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        [HttpGet]
        public int GetPageSize()
        {
            return Global.PageSize;
        }

        public static async Task HandleWriteLog(HttpContext context,LogService logService)
        {

            byte[] postContent = new byte[(int)context.Request.ContentLength];
            await context.Request.Body.ReadAsync(postContent, 0, postContent.Length);
            var text = Encoding.UTF8.GetString(postContent);

            var writeLogModel = System.Text.Json.JsonSerializer.Deserialize<WriteLogModel>(text);
            if (writeLogModel.SourceContext.Contains("\r") || writeLogModel.SourceContext.Contains("\n"))
            {
                writeLogModel.SourceContext = writeLogModel.SourceContext.Replace("\r", "-").Replace("\n", "-");
            }
            logService.WriteLog(writeLogModel);
        }

        [HttpGet]
        public LogItem[] ReadLogs(string applicationContext, string? sourceContext, LogLevel? level, long startTimeStamp, long? endTimeStamp, string? keyWord)
        {
            if (sourceContext != null)
            {
                if (sourceContext.Contains("\r") || sourceContext.Contains("\n"))
                {
                    sourceContext = sourceContext.Replace("\r", "-").Replace("\n", "-");
                }
            }
            return _logService.ReadLogs(applicationContext, sourceContext, level, startTimeStamp, endTimeStamp, keyWord);
        }

        [HttpGet]
        public string[] GetSourceContextes(string applicationContext)
        {
            return _logService.GetSourceContextes(applicationContext).OrderBy(m=>m).ToArray();
        }

        [HttpGet]
        public string[] GetApplications()
        {
            return _logService.GetApplications().OrderBy(m => m).ToArray();
        }
    }
}