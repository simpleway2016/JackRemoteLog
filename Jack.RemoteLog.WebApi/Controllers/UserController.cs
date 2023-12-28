using Jack.RemoteLog.WebApi.Dtos;
using Jack.RemoteLog.WebApi.Exceptions;
using JMS.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Jack.RemoteLog.WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]/[action]")]
    public class UserController: BaseController
    {
        private readonly TokenClient _tokenClient;

        public UserController(TokenClient tokenClient , ILogger<UserController> logger):base(logger)
        {
            _tokenClient = tokenClient;
        }

        [AllowAnonymous]
        [HttpGet]
        public int NeedLogin()
        {
            if (Global.UserInfos.Current == null || Global.UserInfos.Current.Length == 0)
                return 0;
            return 1;
        }

        [HttpGet]
        public UserInfo GetUserInfo()
        {
            var name = this.User.FindFirstValue("Content");
            return new UserInfo() { Name = name};
        }

        [AllowAnonymous]
        [HttpPost]
        public string Login([FromBody] LoginRequestDto request)
        {
            if (Global.UserInfos.Current == null || Global.UserInfos.Current.Length == 0)
                return _tokenClient.Build("Admin", DateTime.Now.AddMinutes(10000));

            if( Global.UserInfos.Current.Any(m=>string.Equals(m.Name , request.Name , StringComparison.OrdinalIgnoreCase) && m.Password == request.Password) )
            {
                return _tokenClient.Build(request.Name, DateTime.Now.AddMinutes(10000));
            }
            throw new ServiceException("用户名密码错误");
        }

        [HttpGet]
        public string RefreshToken()
        {
            var name = this.User.FindFirstValue("Content");
            return _tokenClient.Build(name, DateTime.Now.AddMinutes(30));
        }
    }
}
