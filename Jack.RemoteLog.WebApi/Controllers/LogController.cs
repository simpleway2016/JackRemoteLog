using Jack.RemoteLog.WebApi.Applications;
using Jack.RemoteLog.WebApi.Dtos;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost]
        public LogItem[] ReadLogs([FromBody]SearchRequestBody body)
        {
            if (body.Sources != null)
            {
                for(int i = 0; i < body.Sources.Length; i ++)
                {
                    var source = body.Sources[i];
                    if (source.Contains("\r") || source.Contains("\n"))
                    {
                        body.Sources[i] = source.Replace("\r", "-").Replace("\n", "-");
                    }
                }
            }
            return _logService.ReadLogs(body);
        }

        [Authorize]
        [HttpGet]
        public string[] GetSourceContexts(string applicationContext)
        {
            return _logService.GetSourceContexts(applicationContext).OrderBy(m=>m).ToArray();
        }

        [Authorize]
        [HttpGet]
        public string[] GetApplications()
        {
            return _logService.GetApplications().OrderBy(m => m).ToArray();
        }
    }
}