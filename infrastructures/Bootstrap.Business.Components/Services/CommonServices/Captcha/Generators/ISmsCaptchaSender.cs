using System.Threading.Tasks;
using Bootstrap.Infrastructures.Models.ResponseModels;

namespace Bootstrap.Business.Components.Services.CommonServices.Captcha.Generators
{
    public interface ISmsCaptchaSender
    {
        Task<BaseResponse> Send(string mobile, string code);
    }
}
