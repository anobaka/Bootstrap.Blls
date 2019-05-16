using System;
using System.Threading.Tasks;
using Bootstrap.Infrastructures.Extensions;
using Microsoft.Extensions.Caching.Distributed;

namespace Bootstrap.Business.Components.Services.Captcha
{
    public class AbstractCaptchaService
    {
        private const string CaptchaCacheKeyTemplate = "{0}_{1}";
        private readonly IDistributedCache _cache;

        public AbstractCaptchaService(IDistributedCache cache)
        {
            _cache = cache;
        }

        protected async Task<string> Create(int purpose, string key, TimeSpan absoluteExpirationRelativeToNow)
        {
            var code = StringUtils.GetRandomNumber(6);
            var cacheKey = string.Format(CaptchaCacheKeyTemplate, purpose, key);
            await _cache.SetStringAsync(cacheKey, code, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow
            });
            return code;
        }

        protected async Task<bool> Validate(int purpose, string key, string code)
        {
            var cacheKey = string.Format(CaptchaCacheKeyTemplate, purpose, key);
            var validCode = await _cache.GetStringAsync(cacheKey);
            if (validCode?.Equals(code, StringComparison.OrdinalIgnoreCase) == true)
            {
                await _cache.RemoveAsync(cacheKey);
                return true;
            }

            return false;
        }

        protected async Task<string> Create<TType>(TType purpose, string key, TimeSpan absoluteExpirationRelativeToNow)
            where TType : struct
        {
            return await Create(Convert.ToInt32(purpose), key, absoluteExpirationRelativeToNow);
        }

        protected async Task<bool> Validate<TType>(TType purpose, string key, string code) where TType : struct
        {
            return await Validate(Convert.ToInt32(purpose), key, code);
        }
    }
}