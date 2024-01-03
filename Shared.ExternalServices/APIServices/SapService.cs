
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.ExternalServices.Configurations;
using Shared.ExternalServices.DTOs;
using Shared.ExternalServices.Enums;
using Shared.ExternalServices.Interfaces;
using Shared.ExternalServices.ViewModels.Request;
using Shared.ExternalServices.ViewModels.Response;
using System.Net;
using System.Net.Http;

namespace Shared.ExternalServices.APIServices
{
#pragma warning disable CS8604 // Possible null reference assignment.
    public class SapService : BaseService, ISapService
    {
        private readonly ILogger<SapService> _logger;

        public SapService(IHttpClientFactory httpClientFactory, IConfiguration _config, ILogger<SapService> logger) : base(httpClientFactory, _config)
        {
            _logger = logger;
        }

        public async Task<List<SapProductDto>> GetProducts(string companyCode, string countryCode)
        {
            ResponseDto? result = new();
            string? endpoint = string.Empty;
            try
            {
                endpoint = _config["SapServiceUrls:GetProductsEndpoint"].Replace("{countryCode}", countryCode).Replace("{companyCode}", companyCode);

                result = await this.SendAsync<ResponseDto>(new ApiRequest()
                {
                    ApiType = ApiTypeEnum.GET,
                    Url = $"{_config["SapServiceUrls:BaseUrl"]}/{endpoint}"
                });


                //_logger.LogInformation($"{_config["SapServiceUrls:BaseUrl"]}/{endpoint}");
                //if (result != null)
                //    _logger.LogInformation(JsonConvert.SerializeObject(result));
                //else
                //    _logger.LogInformation("SAP Data returned with NULL! Result not returned...Possible Timeout error.");

                if (result != null && (result?.StatusCode == "00"))
                {
                    var response = JsonConvert.DeserializeObject<SapProductResponse>(Convert.ToString(result.Data));
                    return response.SapProducts;
                }
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"{_config["SapServiceUrls:BaseUrl"]}/{endpoint}");
                _logger.LogInformation(JsonConvert.SerializeObject(result));
                _logger.LogError(ex, "Call to SAP ran into an issue!");
                return new List<SapProductDto>();
            }
            
            return new List<SapProductDto>();
        }

        public async Task<List<NameAndIdDto<string>>> GetAvailableProductsByPlantsAndCustomers(string companyCode, string countryCode, string plantCode, string distributorNumber)
        {
            ResponseDto? result = new();
            string? endpoint = string.Empty;
            try
            {
                endpoint = _config["SapServiceUrls:GetAvailableProductsEndpoint"].Replace("{countryCode}", countryCode).Replace("{companyCode}", companyCode).Replace("{plantCode}", plantCode).Replace("{distributorNumber}", distributorNumber);

                result = await this.SendAsync<ResponseDto>(new ApiRequest()
                {
                    ApiType = ApiTypeEnum.GET,
                    Url = $"{_config["SapServiceUrls:BaseUrl"]}/{endpoint}"
                });

                _logger.LogInformation($"{_config["SapServiceUrls:BaseUrl"]}/{endpoint}");
                if (result != null)
                    _logger.LogInformation(JsonConvert.SerializeObject(result));
                else
                    _logger.LogInformation("SAP Data returned with NULL! Result not returned...Possible Timeout error.");

                if (result != null && (result?.StatusCode == "00"))
                {
                    var response = JsonConvert.DeserializeObject<AvailableSapProductResponse>(Convert.ToString(result.Data));
                    return response.SapProducts;
                }
            }
            catch(Exception ex)
            {
                _logger.LogInformation($"{_config["SapServiceUrls:BaseUrl"]}/{endpoint}");
                _logger.LogInformation(JsonConvert.SerializeObject(result));
                _logger.LogError(ex, "Call to SAP ran into an issue!");
            }

            return new List<NameAndIdDto<string>>();
        }
    }
#pragma warning restore CS8604 // Possible null reference assignment.
}
