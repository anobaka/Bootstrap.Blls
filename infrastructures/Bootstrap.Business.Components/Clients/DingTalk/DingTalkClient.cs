using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bootstrap.Business.Models.ResponseModels;
using Bootstrap.Infrastructures.Components.HttpClient;
using Bootstrap.Infrastructures.Models.RequestModels;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Bootstrap.Business.Components.Clients.DingTalk
{
    public class DingTalkClient : ServiceHttpClient<DingTalkClientOptions>
    {
        private const string AccessTokenCacheKey = "DingTalkAccessToken";
        private readonly IDistributedCache _cache;
        private readonly List<int> _invalidAccessTokenErrCodes = new List<int> {40001, 40014, 41001, 42001, 43007};

        public DingTalkClient(IOptions<DingTalkClientOptions> options, IDistributedCache cache) : base(options)
        {
            _cache = cache;
        }

        protected async Task<string> GetAccessToken()
        {
            var rsp = await base.InvokeAsync<DingTalkAccessTokenGetResponseModel>(new ServiceHttpClientRequestModel
            {
                Method = HttpMethod.Get,
                QueryParameters = new Dictionary<string, List<object>>
                {
                    {"appKey", new List<object> {Options.Value.AppKey}},
                    {"appsecret", new List<object> {Options.Value.AppSecret}}
                }
            });
            if (rsp.ErrCode == 0)
            {
                await _cache.SetStringAsync(AccessTokenCacheKey, rsp.AccessToken, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(90)
                });
                return rsp.AccessToken;
            }
            else
            {
                throw new Exception(
                    $"Unable to get ding talk access token, response: {JsonConvert.SerializeObject(rsp)}");
            }
        }

        public async Task<DingTalkUserInfoGetResponseModel> GetUserInfo(string code)
        {
            return await InvokeAsync<DingTalkUserInfoGetResponseModel>(new ServiceHttpClientRequestModel
            {
                Method = HttpMethod.Get,
                QueryParameters = new Dictionary<string, List<object>>
                {
                    {nameof(code), new List<object> {code}}
                }
            });
        }

        protected override async Task<T> InvokeAsync<T>(ServiceHttpClientRequestModel request,
            CancellationToken? cancellationToken = null)
        {
            var accessToken = await _cache.GetStringAsync(AccessTokenCacheKey);
            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = await GetAccessToken();
            }

            if (request.QueryParameters == null)
            {
                request.QueryParameters = new Dictionary<string, List<object>>();
            }

            request.QueryParameters["access_token"] = new List<object> {accessToken};
            var rsp = await base.InvokeAsync<T>(request, cancellationToken);
            if (rsp is DingTalkResponseModel dsp)
            {
                if (_invalidAccessTokenErrCodes.Contains(dsp.ErrCode))
                {
                    // Again
                    accessToken = await GetAccessToken();
                    request.QueryParameters["access_token"] = new List<object> {accessToken};
                    rsp = await base.InvokeAsync<T>(request, cancellationToken);
                }
            }

            return rsp;
        }
    }
}