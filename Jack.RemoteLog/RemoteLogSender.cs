using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Way.Lib;

namespace Jack.RemoteLog
{
    internal class RemoteLogSender : ILogSender
    {
        public RemoteLogSender()
        {
        }
        public async Task Send(LogItem logitem, System.Net.Http.HttpClient httpClient)
        {
            if (Global.Authorization != null && httpClient.DefaultRequestHeaders.Authorization == null)
            {
                httpClient.DefaultRequestHeaders.Authorization = Global.Authorization;
            }
            var url = Global.ServerUrl;
            if (!string.IsNullOrEmpty(url))
            {
                var serverUrl = $"{url}/Log/WriteLog";
               
                HttpContent content = new StringContent(logitem.ToJsonString());
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                
                HttpResponseMessage response = await httpClient.PostAsync(serverUrl, content);//改成自己的
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception("");
            }
        }
    }
}
