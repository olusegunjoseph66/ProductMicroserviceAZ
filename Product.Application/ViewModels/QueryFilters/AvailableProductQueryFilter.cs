using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Application.ViewModels.QueryFilters
{
    public class AvailableProductQueryFilter
    {
        public string CompanyCode { get; set; } 
        public string CountryCode { get; set; } 
        public string PlantCode { get; set; }
        public string DistributorSapNumber { get; set; } 
        public string? DeliveryMethodCode { get; set; } = null!;
    }
}
