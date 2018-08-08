using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bootstrap.Service.NotificationService.Models.RequestModels
{
    public class EmailMessageSearchRequestModel : SearchRequestModel
    {
        public string Email { get; set; }
    }
}
