using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Bootstrap.Infrastructures.Models.ResponseModels;

namespace Bootstrap.Business.Components.Services.Captcha.Generators
{
    public interface ISmsCaptchaSender
    {
        Task<BaseResponse> Send(string mobile, string code);
    }
}
