using System;
using System.Collections.Generic;
using System.Text;
using Bootstrap.Infrastructures.Components.HttpClient;

namespace Bootstrap.Business.Components.Clients.DingTalk
{
    public class DingTalkClientOptions : ServiceHttpClientOptions
    {
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
    }
}
