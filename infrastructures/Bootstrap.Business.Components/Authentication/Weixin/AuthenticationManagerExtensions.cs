using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Bootstrap.Business.Components.Authentication.Weixin
{
    public static class HttpContextExtensions
    {
        /// <summary> 
        ///  Get the external login information from weixin provider.
        /// </summary> 
        public static async Task<Dictionary<string, string>> GetExternalWeixinLoginInfoAsync(
            this HttpContext httpContext, string expectedXsrf = null)
        {
            var auth = await httpContext.AuthenticateAsync(WeixinAuthenticationDefaults.AuthenticationScheme);

            var items = auth?.Properties?.Items;
            if (auth?.Principal == null || items == null || !items.ContainsKey("LoginProvider"))
            {
                return null;
            }

            if (expectedXsrf != null)
            {
                if (!items.ContainsKey("XsrfId"))
                {
                    return null;
                }

                var userId = items["XsrfId"] as string;
                if (userId != expectedXsrf)
                {
                    return null;
                }
            }

            var userInfo = auth.Principal.FindFirst("urn:weixin:user_info");
            if (userInfo == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(userInfo.Value))
            {
                var jObject = JObject.Parse(userInfo.Value);

                Dictionary<string, string> dict = new Dictionary<string, string>();

                foreach (var item in jObject)
                {
                    dict[item.Key] = item.Value?.ToString();
                }

                return dict;
            }

            return null;

        }
    }
}