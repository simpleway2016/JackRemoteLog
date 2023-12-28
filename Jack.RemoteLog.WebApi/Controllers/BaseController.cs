using Jack.RemoteLog.WebApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Jack.RemoteLog.WebApi.Controllers
{
    public class BaseController : Controller
    {
        private readonly ILogger _logger;

        public BaseController(ILogger logger)
        {
            _logger = logger;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                context.ExceptionHandled = true;
                context.Result = new ContentResult()
                {
                    StatusCode = 500,
                    Content = context.Exception.Message
                };
                if(!(context.Exception is ServiceException))
                {
                    _logger.LogError(context.Exception, null);
                }
            }
            base.OnActionExecuted(context);
        }
    }
}
