using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Bootstrap.Business.Models.ResponseModels
{
    public class DingTalkResponseModel
    {
        [JsonProperty("errcode")] public int ErrCode { get; set; }
        [JsonProperty("errmsg")] public string ErrMsg { get; set; }
    }
}
