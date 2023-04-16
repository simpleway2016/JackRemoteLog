namespace Jack.RemoteLog.WebApi.Dtos
{
    public class UserInfo
    {
        public string Name { get; set; }
        public string Password { get; set; }
        /// <summary>
        /// 是否有写日志权限
        /// </summary>
        public bool Writeable { get; set; }
    }
}
