using Product.Application.Enums;

namespace Product.Application.ViewModels.QueryFilters
{
    public class ProductQueryFilter
    {
        public string? CompanyCode { get; set; } = null!;
        public string? ProductStatusCode { get; set; } = null!;
    }
}
