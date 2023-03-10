using System;
using System.Collections.Generic;
using System.Text;

namespace SSEAIRTRICITY.AzureAPIs
{

    public class BrowserstackResponse
    {
        public string os { get; set; }
        public string os_version { get; set; }
        public string browser { get; set; }
        public string device { get; set; }
        public string browser_version { get; set; }
        public bool? real_mobile { get; set; }

    }

}
