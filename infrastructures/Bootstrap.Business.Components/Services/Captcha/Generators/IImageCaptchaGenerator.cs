using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Bootstrap.Infrastructures.Models.ResponseModels;

namespace Bootstrap.Business.Components.Services.Captcha.Generators
{
    public interface IImageCaptchaGenerator
    {
        Task<SingletonResponse<Stream>> Generate(string code, int width = 104, int height = 36);
    }
}
