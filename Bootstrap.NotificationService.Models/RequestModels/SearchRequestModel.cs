using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bootstrap.Service.NotificationService.Models.RequestModels
{
    public abstract class SearchRequestModel
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
