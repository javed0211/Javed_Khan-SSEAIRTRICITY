using System;
using System.Collections.Generic;
using System.Text;

namespace SSEAIRTRICITY.Utilities
{
    public class SSE
    {
        public class Time
        {
            public string name { get; set; }
            public string time { get; set; }
        }

        public class Cost
        {
            public string appliance { get; set; }
            public string daily { get; set; }
            public string weekly { get; set; }
            public string monthly { get; set; }
            public string yearly { get; set; }
        }
    }
}
