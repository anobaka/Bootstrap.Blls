using Bootstrap.Business.Models.Constants;
using Bootstrap.Infrastructures.Extensions;
using Bootstrap.Infrastructures.Models.ResponseModels;

namespace Bootstrap.Business.Components.ResponseBuilders
{
    public static class SingletonResponseBuilder<T>
    {
        public static SingletonResponse<T> Ok = Build(ResponseCode.Success);
        public static SingletonResponse<T> BadRequest = Build(ResponseCode.InvalidPayload);

        public static SingletonResponse<T> InvalidOperation = Build(ResponseCode.InvalidOperation);

        public static SingletonResponse<T> SystemError = Build(ResponseCode.SystemError);
        public static SingletonResponse<T> Unauthorized = Build(ResponseCode.Unauthorized);
        public static SingletonResponse<T> NotFound = Build(ResponseCode.NotFound);
        public static SingletonResponse<T> Conflict = Build(ResponseCode.Conflict);
        public static SingletonResponse<T> Unauthenticated = Build(ResponseCode.Unauthenticated);
        public static SingletonResponse<T> Timeout = Build(ResponseCode.Timeout);

        public static SingletonResponse<T> Build(ResponseCode code, string message = null) =>
            new SingletonResponse<T>((int) code, message ?? SpecificEnumUtils<ResponseCode>.GetDisplayName(code));

        public static SingletonResponse<T> BuildBadRequest(string message) =>
            Build(ResponseCode.InvalidPayload, message);
    }
}