using System;
using System.IO;
using System.Threading.Tasks;
using Bootstrap.Business.Components.Services.CommonServices.Captcha.Generators;
using Bootstrap.Infrastructures.Models.ResponseModels;
using Microsoft.Extensions.Caching.Distributed;

namespace Bootstrap.Business.Components.Services.CommonServices.Captcha
{
    public class DefaultCaptchaService : AbstractCaptchaService
    {
        private readonly IImageCaptchaGenerator _imageCaptchaGenerator;
        private readonly ISmsCaptchaSender _smsCaptchaSender;

        public DefaultCaptchaService(IDistributedCache cache, ISmsCaptchaSender smsCaptchaSender,
            IImageCaptchaGenerator imageCaptchaGenerator) : base(cache)
        {
            _smsCaptchaSender = smsCaptchaSender;
            _imageCaptchaGenerator = imageCaptchaGenerator;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="key"></param>
        /// <param name="absoluteExpirationRelativeToNow"></param>
        /// <returns>PNG file</returns>
        public async Task<SingletonResponse<Stream>> CreateImageCaptcha(int purpose, string key,
            TimeSpan absoluteExpirationRelativeToNow)
        {
            var code = await Create(purpose, key, absoluteExpirationRelativeToNow);
            var image = await _imageCaptchaGenerator.Generate(code);
            return image;
        }

        public async Task<BaseResponse> CreateSmsCaptcha(string mobile, int purpose, string key,
            TimeSpan absoluteExpirationRelativeToNow)
        {
            var code = await Create(purpose, key, absoluteExpirationRelativeToNow);
            return await _smsCaptchaSender.Send(mobile, code);
        }
    }
}