using Bootstrap.Infrastructures.Components.HttpClient;

namespace Bootstrap.Business.Components.Sdks.DingTalk
{
    public class DingTalkClientOptions : ServiceHttpClientOptions
    {
        public string AppKey { get; set; }
        public string AppSecret { get; set; }
    }
}
