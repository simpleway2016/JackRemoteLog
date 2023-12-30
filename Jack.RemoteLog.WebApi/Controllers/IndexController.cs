using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace Jack.RemoteLog.WebApi.Controllers
{
    [ApiController]
    [Route("/")]
    public class IndexController:ControllerBase
    {
        [HttpGet]
        public ContentResult Get()
        {
            var path = Path.Combine("wwwroot", "index.html");
            var htmlContent = System.IO.File.ReadAllText(path, Encoding.UTF8);
            return Content(htmlContent, "text/html");
        }
    }
}
