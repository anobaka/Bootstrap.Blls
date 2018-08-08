using System;
using System.Collections.Generic;
using System.Text;

namespace Bootstrap.Service.NotificationService.Models.Options
{
    public class NotificationServiceOptions
    {
        public string DbConnectionString { get; set; }
        public string MigrationsAssembly { get; set; }
    }
}
