using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bootstrap.Business.Components.ResponseBuilders;
using Bootstrap.Business.Components.Sdks.LeanCloud.ResponseModels;
using Bootstrap.Business.Models.Constants;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bootstrap.Business.Components.Sdks.LeanCloud
{
    public class LeanCloudSmsService
    {
        private readonly IOptions<LeanCloudOptions> _options;
        private readonly HttpClient _client;

        public LeanCloudSmsService(IOptions<LeanCloudOptions> options)
        {
            _options = options;
            _client = new HttpClient()
            {
                DefaultRequestHeaders =
                {
                    {"X-LC-Id", _options.Value.LcId},
                    {"X-LC-Key", _options.Value.LcKey},
                }
            };
        }

        protected virtual async Task<TRsp> PostJson<TRsp>(string uri, object data)
        {
            var rspBody = await (await _client.PostAsync($"{_options.Value.Endpoint}{uri}",
                    new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"))).Content
                .ReadAsStringAsync();
            var rsp = JsonConvert.DeserializeObject<TRsp>(rspBody);
            return rsp;
        }

        public async Task<BaseResponse> Send(string mobile)
        {
            var rsp = await PostJson<LeanCloudResponseModel>("/1.1/requestSmsCode", new {MobilePhoneNumber = mobile});
            return rsp.Code == 0
                ? BaseResponseBuilder.Ok
                : BaseResponseBuilder.Build(ResponseCode.SystemError, rsp.Error);
        }

        public async Task<bool> Validate(string mobile, string code)
        {
            var rsp = await PostJson<LeanCloudResponseModel>($"/1.1/verifySmsCode/{code}",
                new {MobilePhoneNumber = mobile});
            return rsp.Code == 0;
        }
    }
}