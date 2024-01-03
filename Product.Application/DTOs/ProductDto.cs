using Shared.ExternalServices.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Application.DTOs
{
    public class ProductDto 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public NameAndCodeDto ProductType { get; set; }
        public NameAndId<byte> ProductStatus { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public string CompanyCode { get; set; } = null!;
        public string UnitOfMeasureCode { get; set; } = null!;
    }
}
