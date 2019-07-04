using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bootstrap.Business.Models.ResponseModels
{
    public class DingTalkAccessTokenGetResponseModel : DingTalkResponseModel
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
