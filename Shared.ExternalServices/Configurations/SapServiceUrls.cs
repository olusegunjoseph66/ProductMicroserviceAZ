using Shared.ExternalServices.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.Configurations
{
    public class SapServiceUrls
    {
        public string BaseUrl { get; set; }
        public string GetProductsEndpoint { get; set; }
        public string GetAvailableProductsEndpoint { get; set; }
    }
}
