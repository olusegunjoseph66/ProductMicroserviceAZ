using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ExternalServices.DTOs
{
    public class SapProductDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } 
        public decimal Price { get; set; }
        public NameAndCodeDto ProductType { get; set; }
        public NameAndCodeDto ProductStatus { get; set; }
        public NameAndCodeDto SalesUnitOfMeasure { get; set; }
    }
}
