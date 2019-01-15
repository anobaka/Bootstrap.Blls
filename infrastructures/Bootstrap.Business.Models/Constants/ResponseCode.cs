
using System.ComponentModel;

namespace Bootstrap.Business.Models.Constants
{
    public enum ResponseCode
    {
        [Description("成功")]
        Success = 0,

        [Description("数据无任何更新")]
        NotModified = 304,

        [Description("请求参数有误")]
        InvalidPayload = 400,

        [Description("操作非法")]
        InvalidOperation = 400,

        [Description("请登录后再试，正在为您跳转至登录页")]
        Unauthenticated = 401,

        [Description("权限不足，请联系管理员添加权限并重新登录后再尝试，如果问题依旧存在请联系开发人员")]
        Unauthorized = 403,

        [Description("数据不存在")]
        NotFound = 404,

        [Description("数据冲突或已存在")]
        Conflict = 409,

        [Description("请求异常，请稍后再试")]
        SystemError = 500,

        [Description("请求超时，请稍后再试")]
        Timeout = 504,

        [Description("验证码有误")]
        InvalidCaptcha = 100400
    }
}