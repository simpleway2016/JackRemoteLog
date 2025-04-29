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
            var title = Global.Configuration["Title"];
            var htmlContent = System.IO.File.ReadAllText(path, Encoding.UTF8);
            if (!string.IsNullOrEmpty(title))
                htmlContent = htmlContent.Replace("<title>Remote Log Viewer</title>", $"<title>{title}</title>");

            return Content(htmlContent, "text/html");
        }
    }
}
