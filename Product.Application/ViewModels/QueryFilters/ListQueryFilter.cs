using Product.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Application.ViewModels.QueryFilters
{
    public class ListQueryFilter
    {
        public string? SearchKeyword { get; set; } = null!;
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public ProductSortingEnum? Sort { get; set; }
    }
}
