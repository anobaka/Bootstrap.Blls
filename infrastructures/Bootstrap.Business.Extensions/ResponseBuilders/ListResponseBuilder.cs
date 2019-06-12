using System;
using System.Collections.Generic;
using System.Text;
using Bootstrap.Business.Models.Constants;
using Bootstrap.Infrastructures.Extensions;
using Bootstrap.Infrastructures.Models.ResponseModels;

namespace Bootstrap.Business.Extensions.ResponseBuilders
{
    public static class ListResponseBuilder<T>
    {
        public static ListResponse<T> Ok = Build(ResponseCode.Success);
        public static ListResponse<T> BadRequest = Build(ResponseCode.InvalidPayload);

        public static ListResponse<T> InvalidOperation = Build(ResponseCode.InvalidOperation);

        public static ListResponse<T> SystemError = Build(ResponseCode.SystemError);
        public static ListResponse<T> Unauthorized = Build(ResponseCode.Unauthorized);
        public static ListResponse<T> NotFound = Build(ResponseCode.NotFound);
        public static ListResponse<T> Conflict = Build(ResponseCode.Conflict);
        public static ListResponse<T> Unauthenticated = Build(ResponseCode.Unauthenticated);
        public static ListResponse<T> Timeout = Build(ResponseCode.Timeout);

        public static ListResponse<T> Build(ResponseCode code, string message = null) =>
            new ListResponse<T>((int) code, message ?? SpecificEnumUtils<ResponseCode>.GetDisplayName(code));

        public static ListResponse<T> BuildBadRequest(string message) =>
            Build(ResponseCode.InvalidPayload, message);
    }
}