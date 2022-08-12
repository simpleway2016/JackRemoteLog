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

        [HttpPost]
        public void WriteLog(WriteLogModel writeLogModel)
        {
            _logService.WriteLog(writeLogModel);
        }

        [HttpGet]
        public LogItem[] ReadLogs(string applicationContext, string? sourceContext, LogLevel? level, long startTimeStamp, long? endTimeStamp, string? keyWord)
        {
            return _logService.ReadLogs(applicationContext,sourceContext,level, startTimeStamp,endTimeStamp,keyWord);
        }

        [HttpGet]
        public string[] GetSourceContextes(string applicationContext)
        {
            return _logService.GetSourceContextes(applicationContext);
        }

        [HttpGet]
        public string[] GetApplications()
        {
            return _logService.GetApplications();
        }
    }
}