using Shared.ExternalServices.DTOs;

namespace Shared.ExternalServices.Interfaces
{
    public interface ISapService
    {
        Task<List<SapProductDto>> GetProducts(string companyCode, string countryCode);
        Task<List<NameAndIdDto<string>>> GetAvailableProductsByPlantsAndCustomers(string companyCode, string countryCode, string plantCode, string distributorNumber);
    }
}