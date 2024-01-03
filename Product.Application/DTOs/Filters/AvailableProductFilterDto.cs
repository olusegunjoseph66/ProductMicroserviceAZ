using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Application.DTOs.Filters
{
    public class AvailableProductFilterDto
    {
        public string? SearchText { get; set; }
        public string CompanyCode { get; set; }
        public List<string> SapNumbers { get; set; }
    }
}
