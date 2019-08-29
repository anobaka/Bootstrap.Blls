using System.IO;
using System.Threading.Tasks;
using Bootstrap.Infrastructures.Models.ResponseModels;

namespace Bootstrap.Business.Components.Services.CommonServices.Captcha.Generators
{
    public interface IImageCaptchaGenerator
    {
        Task<SingletonResponse<Stream>> Generate(string code, int width = 104, int height = 36);
    }
}
