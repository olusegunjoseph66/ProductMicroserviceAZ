using Product.Application.DTOs.Filters;
using Product.Application.Enums;
using Shared.Data.Extensions;
using Shared.Utilities.Helpers;
using System.Security.Cryptography.X509Certificates;

namespace Product.Infrastructure.QueryObjects
{
    public class ProductQueryObject : QueryObject<Shared.Data.Models.Product>
    {
        public ProductQueryObject(ProductFilterDto filter)
        {
            if (filter is not null)
            {
                if (!string.IsNullOrWhiteSpace(filter.CompanyCode))
                    And(p => p.CompanyCode == filter.CompanyCode);

                if (!string.IsNullOrWhiteSpace(filter.SearchText))
                {
                    And(p => p.Name.ToLower().Contains(filter.SearchText.ToLower())
                      || p.Description.ToLower().Contains(filter.SearchText.ToLower())
                      || p.ProductSapNumber.Contains(filter.SearchText));
                }
            }

            And(p => p.ProductStatus.Code == ProductStatusEnum.Active.ToDescription());

            And(p => p.IsDeleted == false);
        }

        public ProductQueryObject(AvailableProductFilterDto filter)
        {
            if (filter is not null)
            {
                if (filter.SapNumbers.Any())
                    And(p => filter.SapNumbers.Contains(p.ProductSapNumber));

                if (!string.IsNullOrWhiteSpace(filter.CompanyCode))
                    And(p => p.CompanyCode == filter.CompanyCode);

                if (!string.IsNullOrWhiteSpace(filter.SearchText))
                {
                    And(p => p.Name.ToLower().Contains(filter.SearchText.ToLower())
                      || p.Description.ToLower().Contains(filter.SearchText.ToLower())
                      || p.ProductSapNumber.Contains(filter.SearchText));
                }
            }
            And(p => p.ProductStatus.Code == ProductStatusEnum.Active.ToDescription());

            And(p => p.IsDeleted == false);
        }
    }
}
