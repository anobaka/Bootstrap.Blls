using Bootstrap.Business.Models.Constants;
using Bootstrap.Infrastructures.Extensions;
using Bootstrap.Infrastructures.Models.ResponseModels;

namespace Bootstrap.Business.Extensions.ResponseBuilders
{
    public static class BaseResponseBuilder
    {
        public static BaseResponse Ok = Build(ResponseCode.Success);
        public static BaseResponse BadRequest = Build(ResponseCode.InvalidPayload);

        public static BaseResponse InvalidOperation = Build(ResponseCode.InvalidOperation);

        public static BaseResponse SystemError = Build(ResponseCode.SystemError);
        public static BaseResponse Unauthorized = Build(ResponseCode.Unauthorized);
        public static BaseResponse NotFound = Build(ResponseCode.NotFound);
        public static BaseResponse Conflict = Build(ResponseCode.Conflict);
        public static BaseResponse Unauthenticated = Build(ResponseCode.Unauthenticated);

        private static BaseResponse Build(ResponseCode code, string message = null) =>
            new BaseResponse((int) code, message ?? SpecificEnumUtils<ResponseCode>.GetDisplayName(code));
    }
}