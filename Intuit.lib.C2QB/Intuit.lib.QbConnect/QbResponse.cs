using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Intuit.lib.C2QB
{
    class QbResponse
    {      
        public string AccessToken { get; set; }
        public string AccessSecret { get; set; }
        public string RealmId { get; set; }
        public string DataSource { get; set; }
        public DateTime ExpirationDateTime { get; set; }
       
    }
}
