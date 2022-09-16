using Jack.RemoteLog.WebApi.Applications;
using Jack.RemoteLog.WebApi.Dtos;
using Microsoft.AspNetCore.Mvc;

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
        public int GetPageSize()
        {
            return Global.PageSize;
        }

        [HttpPost]
        public void WriteLog(WriteLogModel writeLogModel)
        {
            if (writeLogModel.SourceContext.Contains("\r") || writeLogModel.SourceContext.Contains("\n"))
            {
                writeLogModel.SourceContext = writeLogModel.SourceContext.Replace("\r", "-").Replace("\n", "-");
            }
            _logService.WriteLog(writeLogModel);
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
            return _logService.GetApplications();
        }
    }
}