using System.Threading.Tasks;
using Bootstrap.Business.Components.ResponseBuilders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Bootstrap.Business.Components.Authentication
{
    public static class AuthenticationBuilderExtensions
    {
        public static AuthenticationBuilder AddCookieWithBootstrapBehavior(this AuthenticationBuilder builder,
            string scheme)
        {
            return builder.AddCookie(scheme, t =>
            {
                t.Cookie.Name = scheme;
                t.Cookie.SameSite = SameSiteMode.None;
                t.Events.OnValidatePrincipal = context => Task.CompletedTask;
                t.Events.OnRedirectToLogout = async context => { };
                t.Events.OnRedirectToReturnUrl = async context => { };
                t.Events.OnRedirectToLogin = async context =>
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(
                            JsonConvert.SerializeObject(BaseResponseBuilder.Unauthenticated));
                    }
                };
                t.Events.OnRedirectToAccessDenied = async context =>
                {
                    if (!context.Response.HasStarted)
                    {
                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(
                            JsonConvert.SerializeObject(BaseResponseBuilder.Unauthorized));
                    }
                };
            });
        }
    }
}