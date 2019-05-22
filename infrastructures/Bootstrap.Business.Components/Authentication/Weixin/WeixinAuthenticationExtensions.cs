using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Bootstrap.Business.Components.Authentication.Weixin
{
    /// <summary> 
    /// </summary>
    public static class WeixinAuthenticationExtensions
    {
        /// <summary> 
        /// </summary>
        public static AuthenticationBuilder AddWeixinAuthentication(this AuthenticationBuilder builder)
        {
            return builder.AddWeixinAuthentication(WeixinAuthenticationDefaults.AuthenticationScheme, WeixinAuthenticationDefaults.DisplayName, options => { });
        }

        /// <summary> 
        /// </summary>
        public static AuthenticationBuilder AddWeixinAuthentication(this AuthenticationBuilder builder, Action<WeixinAuthenticationOptions> configureOptions)
        {
            return builder.AddWeixinAuthentication(WeixinAuthenticationDefaults.AuthenticationScheme, WeixinAuthenticationDefaults.DisplayName, configureOptions);
        }

        /// <summary> 
        /// </summary>
        public static AuthenticationBuilder AddWeixinAuthentication(this AuthenticationBuilder builder, string authenticationScheme, Action<WeixinAuthenticationOptions> configureOptions)
        {
            return builder.AddWeixinAuthentication(authenticationScheme, WeixinAuthenticationDefaults.DisplayName, configureOptions);
        }

        /// <summary> 
        /// </summary>
        public static AuthenticationBuilder AddWeixinAuthentication(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<WeixinAuthenticationOptions> configureOptions)
        {
            return builder.AddOAuth<WeixinAuthenticationOptions, WeixinAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}