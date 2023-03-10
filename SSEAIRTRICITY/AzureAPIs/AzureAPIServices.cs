using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSEAIRTRICITY.Utilities
{
    public class AzureAPIServices
    {
        public string ProjectName { get; set; }
        public string EnvironmentURL { get; set; }
        public int TestPlanId { get; set; }
        public int ReleaseSuiteID { get; set; }
        public string AccessToken { get; set; }
        public string ReleaseName { get; set; }
    }

    
}
