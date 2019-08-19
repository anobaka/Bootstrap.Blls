using Bootstrap.Business.Models.Constants;
using Bootstrap.Infrastructures.Extensions;
using Bootstrap.Infrastructures.Models.ResponseModels;

namespace Bootstrap.Business.Components.ResponseBuilders
{
    public static class SearchResponseBuilder<T>
    {
        public static SearchResponse<T> Ok = Build(ResponseCode.Success);
        public static SearchResponse<T> BadRequest = Build(ResponseCode.InvalidPayload);

        public static SearchResponse<T> InvalidOperation = Build(ResponseCode.InvalidOperation);

        public static SearchResponse<T> SystemError = Build(ResponseCode.SystemError);
        public static SearchResponse<T> Unauthorized = Build(ResponseCode.Unauthorized);
        public static SearchResponse<T> NotFound = Build(ResponseCode.NotFound);
        public static SearchResponse<T> Conflict = Build(ResponseCode.Conflict);
        public static SearchResponse<T> Unauthenticated = Build(ResponseCode.Unauthenticated);
        public static SearchResponse<T> Timeout = Build(ResponseCode.Timeout);

        public static SearchResponse<T> Build(ResponseCode code, string message = null) =>
            new SearchResponse<T>((int) code, message ?? SpecificEnumUtils<ResponseCode>.GetDisplayName(code));

        public static SearchResponse<T> BuildBadRequest(string message) =>
            Build(ResponseCode.InvalidPayload, message);
    }
}