using System;
using System.Collections.Generic;
using System.Text;
using Bootstrap.Business.Models.Constants;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bootstrap.Business.Models.ResponseModels
{
    public class DingTalkUserInfoGetResponseModel : DingTalkResponseModel
    {
        [JsonProperty("userid")] public string UserId { get; set; }
        [JsonProperty("is_sys")] public bool IsAdministrator { get; set; }
        [JsonProperty("sys_level")] public DingSysLevel SysLevel { get; set; }
        [JsonProperty("deviceId")] public string DeviceId { get; set; }
    }
}