using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;

namespace Bootstrap.Business.Components.Authentication.Weixin
{
    /// <summary>
    /// Defines a set of options used by <see cref="WeixinAuthenticationHandler"/>.
    /// </summary>
    public class WeixinAuthenticationOptions : OAuthOptions
    {
        public WeixinAuthenticationOptions()
        {
            ClaimsIssuer = WeixinAuthenticationDefaults.Issuer;
            CallbackPath = new PathString(WeixinAuthenticationDefaults.CallbackPath);

            AuthorizationEndpoint = WeixinAuthenticationDefaults.AuthorizationEndpoint;
            TokenEndpoint = WeixinAuthenticationDefaults.TokenEndpoint;
            UserInformationEndpoint = WeixinAuthenticationDefaults.UserInformationEndpoint;

            Scope.Add("snsapi_login");
            Scope.Add("snsapi_userinfo");
        }
    }
}