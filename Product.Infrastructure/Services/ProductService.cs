using Shared.Data.Repository;
using Shared.Data.Models;
using Product.Application.Interfaces.Services;
using Product.Application.DTOs.APIDataFormatters;
using Shared.Utilities.DTO.Pagination;
using Product.Application.ViewModels.Responses;
using Product.Application.ViewModels.QueryFilters;
using Product.Application.DTOs.Sortings;
using Product.Application.DTOs.Filters;
using Product.Infrastructure.QueryObjects;
using Microsoft.EntityFrameworkCore;
using Shared.Data.Extensions;
using Product.Application.Constants;
using Microsoft.Extensions.Configuration;
using Product.Application.Enums;
using Shared.Utilities.Helpers;
using Product.Application.ViewModels.Requests;
using Shared.ExternalServices.Interfaces;
using Shared.ExternalServices.DTOs;
using Product.Application.Exceptions;
using Product.Application.DTOs.Events;
using Product.Application.DTOs;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Shared.ExternalServices.APIServices;

namespace Product.Infrastructure.Services
{
    public class ProductService : BaseService, IProductService
    {
        private readonly IAsyncRepository<Shared.Data.Models.Product> _productRepository;
        public readonly IMessagingService _messageBus;
        private readonly ISapService _sapService;
        private readonly IDmsService _dmsService;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IAsyncRepository<Shared.Data.Models.Product> productRepository, ILogger<ProductService> logger, ISapService sapService, IDmsService dmsService, IMessagingService messageBus, IAuthenticatedUserService authenticatedUserService) : base(authenticatedUserService)
        {
            _logger = logger;
            _messageBus = messageBus;
            _productRepository = productRepository;
            _sapService = sapService;
            _dmsService = dmsService;
        }


        public async Task<ApiResponse> GetProducts(ProductQueryFilter filter, ListQueryFilter paginationFilter, CancellationToken cancellationToken)
        {
            GetUserCredentials();

            BasePageFilter pageFilter = new(paginationFilter.PageSize, paginationFilter.PageIndex);
            ProductSortingDto sorting = new();
            if (paginationFilter.Sort == ProductSortingEnum.NameDescending)
                sorting.IsNameDescending = true;
            else if (paginationFilter.Sort == ProductSortingEnum.NameAscending)
                sorting.IsNameAscending = true;


            ProductFilterDto productFilter = new()
            {
                CompanyCode = filter.CompanyCode,
                ProductStatusCode = filter.ProductStatusCode,
                SearchText = paginationFilter.SearchKeyword, 
                UserRole = LoggedInUserRole
            };

            var expression = new ProductQueryObject(productFilter).Expression;
            var orderExpression = ProcessOrderFunc(sorting);
            var query = _productRepository.Table.AsNoTrackingWithIdentityResolution()
                    .OrderByWhere(expression, orderExpression);

            var totalCount = await query.CountAsync(cancellationToken);
            query = query.Select(x => new Shared.Data.Models.Product
            {
                CompanyCode = x.CompanyCode,
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                ProductType = x.ProductType,
                UnitOfMeasureCode = x.UnitOfMeasureCode,
                ProductImages = x.ProductImages.Select(i => new ProductImage
                {
                    Id = i.Id,
                    IsPrimaryImage = i.IsPrimaryImage,
                    PublicUrl = i.PublicUrl
                }).ToList(),
                ProductStatus = new ProductStatus
                {
                    Id = x.ProductStatus.Id,
                    Name = x.ProductStatus.Name,
                    Code = x.ProductStatus.Code
                },
                DateCreated = x.DateCreated,
                DateModified = x.DateModified, 
                Price = x.Price
            }).Paginate(pageFilter.PageNumber, pageFilter.PageSize);
            
            var products = await query.ToListAsync(cancellationToken);
            var totalPages = NumberManipulator.PageCountConverter(totalCount, pageFilter.PageSize);
            var response = new PaginatedList<ProductResponse>(ProcessQuery(products), new PaginationMetaData(pageFilter.PageNumber, pageFilter.PageSize, totalPages, totalCount), new());

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_PRODUCT_LIST_RETRIEVAL, response);
        }

        public async Task<ApiResponse> GetAvailableProducts(AvailableProductQueryFilter filter, ListQueryFilter paginationFilter, CancellationToken cancellationToken)
        {
            GetUserCredentials();

            var availablePlantProducts = await _sapService.GetAvailableProductsByPlantsAndCustomers(filter.CompanyCode, filter.CountryCode, filter.PlantCode, filter.DistributorSapNumber);

            _logger.LogInformation($"Total Available Plant Products is {JsonConvert.SerializeObject(availablePlantProducts)}");

            List<Shared.Data.Models.Product> plantProducts = new();
            List<Shared.Data.Models.Product> products = new();
            int totalCount = 0, totalPages = 0;

            BasePageFilter pageFilter = new(paginationFilter.PageSize, paginationFilter.PageIndex);
            PaginatedList<AvailableProductResponse> response = new(new List<AvailableProductResponse>(), new PaginationMetaData(0,0,0,0),filter);

            if (availablePlantProducts.Any()) 
            {
                var availablePlatProductIds = availablePlantProducts.Select(x => x.Id).ToList();

                ProductSortingDto sorting = new();
                if (paginationFilter.Sort == ProductSortingEnum.NameDescending)
                    sorting.IsNameDescending = true;
                else if (paginationFilter.Sort == ProductSortingEnum.NameAscending)
                    sorting.IsNameAscending = true;

                AvailableProductFilterDto productFilter = new()
                {
                    CompanyCode = filter.CompanyCode,
                    SearchText = paginationFilter.SearchKeyword,
                    SapNumbers = availablePlatProductIds
                };

                var expression = new ProductQueryObject(productFilter).Expression;
                var orderExpression = ProcessOrderFunc(sorting);
                var query = _productRepository.Table.AsNoTrackingWithIdentityResolution()
                        .OrderByWhere(expression, orderExpression);

                totalCount = await query.CountAsync(cancellationToken);
                _logger.LogInformation($"Total Company Product Count = {totalCount}");

                query = query.Select(x => new Shared.Data.Models.Product
                {
                    CompanyCode = x.CompanyCode,
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description, 
                    ProductSapNumber = x.ProductSapNumber,
                    ProductType = x.ProductType,
                    UnitOfMeasureCode = x.UnitOfMeasureCode,
                    ProductImages = x.ProductImages.Select(i => new ProductImage
                    {
                        Id = i.Id,
                        IsPrimaryImage = i.IsPrimaryImage,
                        PublicUrl = i.PublicUrl
                    }).ToList(),
                    ProductStatus = new ProductStatus
                    {
                        Id = x.ProductStatus.Id,
                        Name = x.ProductStatus.Name,
                        Code = x.ProductStatus.Code
                    },
                    DateCreated = x.DateCreated,
                    DateModified = x.DateModified,
                    Price = x.Price
                }).Paginate(pageFilter.PageNumber, pageFilter.PageSize);

                products = await query.ToListAsync(cancellationToken);
                _logger.LogInformation($"Total Company Products is {JsonConvert.SerializeObject(products)}");

                var distinctProducts = products.GroupBy(x => new { x.ProductSapNumber, x.CompanyCode}).ToList();

                List<Shared.Data.Models.Product> uniqueProducts = new();
                distinctProducts.ForEach(x =>
                {
                    var selectedProduct = x.FirstOrDefault();
                    if(selectedProduct != null)
                        uniqueProducts.Add(selectedProduct);
                });

                _logger.LogInformation($"Transformed Distinct Products is {JsonConvert.SerializeObject(uniqueProducts)}");
                var newCount = products.Count - uniqueProducts.Count;
                int newTotalCount = 1;
                if (newCount > 0)
                {
                    newTotalCount = totalCount - newCount;
                }
                totalPages = NumberManipulator.PageCountConverter(newTotalCount, pageFilter.PageSize);

                response = new PaginatedList<AvailableProductResponse>(ProcessAvailableProductQuery(uniqueProducts), new PaginationMetaData(pageFilter.PageNumber, pageFilter.PageSize, totalPages, newTotalCount), filter);
            }
            
            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_PRODUCT_LIST_RETRIEVAL, response);
        }

        public async Task<ApiResponse> GetProductById(int productId, CancellationToken cancellationToken)
        {
            var productDetail = await _productRepository.Table.Where(p => p.Id == (short)productId && p.IsDeleted == false).Select(x => new Shared.Data.Models.Product
            {
                Id = x.Id, 
                Name = x.Name, 
                Description = x.Description, 
                ProductType = x.ProductType, 
                ProductStatus = new ProductStatus
                {
                    Id = x.ProductStatus.Id, 
                    Name = x.ProductStatus.Name, 
                    Code = x.ProductStatus.Code
                }, 
                UnitOfMeasureCode = x.UnitOfMeasureCode,
                Price = x.Price, 
                ProductSapNumber = x.ProductSapNumber,
                DateModified = x.DateModified, 
                ProductImages = x.ProductImages.Select(i => new ProductImage
                {
                    Id = i.Id, 
                    PublicUrl = i.PublicUrl, 
                    IsPrimaryImage = i.IsPrimaryImage
                }).ToList()
            }).FirstOrDefaultAsync(cancellationToken);

            var productResponse = ProcessQuery(productDetail);

            string responseMessage = "";
            if (productResponse is null)
                responseMessage = ErrorMessages.PRODUCT_NOT_FOUND;
            else
                responseMessage = SuccessMessages.SUCCESSFUL_PRODUCT_RETRIEVAL;


            return ResponseHandler.SuccessResponse(responseMessage, productResponse);
        }

        public async Task<ApiResponse> ActivateDeactivateProduct(ActivateProductRequest request, CancellationToken cancellationToken)
        {
            var userId = GetUserId();

            var productDetail = await _productRepository.Table.AsNoTrackingWithIdentityResolution().Where(p => p.Id == (short)request.ProductId).Include(x => x.ProductStatus).FirstOrDefaultAsync(cancellationToken);

            if (productDetail == null)
                throw new NotFoundException(ErrorMessages.PRODUCT_NOT_FOUND, ErrorCodes.PRODUCT_NOTFOUND);

            var responseMessage = string.Empty;
            var oldProduct = new Shared.Data.Models.Product
            {
                Id = productDetail.Id,
                Name = productDetail.Name,
                ProductSapNumber = productDetail.ProductSapNumber,
                DateCreated = productDetail.DateCreated,
                Description = productDetail.Description,
                UnitOfMeasureCode = productDetail.UnitOfMeasureCode,
                ProductStatus = new ProductStatus
                {
                    Code = productDetail.ProductStatus.Code,
                    Name = productDetail.ProductStatus.Name,
                    Id = productDetail.ProductStatus.Id
                }
            };

            if (request.Activate)
            {
                productDetail.ProductStatusId = (int)ProductStatusEnum.Active;
                productDetail.ProductStatus = new ProductStatus 
                {
                    Id = (byte)ProductStatusEnum.Active, 
                    Code = ProductStatusEnum.Active.ToString(), 
                    Name = ProductStatusEnum.Active.ToDescription() 
                };
                responseMessage = SuccessMessages.SUCCESSFUL_PRODUCT_ACTIVATION;
            }
            else
            {
                productDetail.ProductStatusId = (int)ProductStatusEnum.InActive;
                productDetail.ProductStatus = new ProductStatus 
                { 
                    Id = (byte)ProductStatusEnum.InActive, 
                    Code = ProductStatusEnum.InActive.ToString(),
                    Name = ProductStatusEnum.InActive.ToDescription() 
                };
                responseMessage = SuccessMessages.SUCCESSFUL_PRODUCT_DEACTIVATION;
            }

            productDetail.DateModified = DateTime.UtcNow;
            productDetail.ModifiedByUserId = userId;

            _productRepository.Update(productDetail);
            await _productRepository.CommitAsync(cancellationToken);

            //Publish to Azure ServiceBus
            ProductUpdatedMessage updatedProductMessage = new()
            {
                ProductId = oldProduct.Id,
                ProductSapNumber = oldProduct.ProductSapNumber,
                Name = oldProduct.Name,
                DateCreated = oldProduct.DateCreated,
                Description = oldProduct.Description,
                UnitOfMeasureCode = oldProduct.UnitOfMeasureCode,
                ProductStatus = new NameAndCode(oldProduct.ProductStatus.Code, oldProduct.Name),
                DateModified = DateTime.UtcNow,
                ModifiedByUserId = userId
            };

            await _messageBus.PublishTopicMessage(updatedProductMessage, EventMessages.PRODUCTS_PRODUCT_UPDATED);

            return ResponseHandler.SuccessResponse(responseMessage);
        }

        public async Task<ApiResponse> AutoRefreshProducts(CancellationToken cancellationToken)
        {
            var companies = await _dmsService.GetCompanies();
            var countries = await _dmsService.GetCountries();
            var statuses = Enum.GetValues(typeof(ProductStatusEnum)).Cast<ProductStatusEnum>()
                   .Select(e => new NameAndId<byte>((byte)e, e.ToDescription())).ToList();

            var companyCodes = companies.Select(x => x.Code).ToList();
            var countryCodes = countries.Select(x => x.Code).ToList();

            List<ProductDto> productlist = new();
            foreach(var companyCode in companyCodes)
            {
                foreach(var countryCode in countryCodes)
                {
                    var sapCompanyProducts = await _sapService.GetProducts(companyCode, countryCode);
                    List<SapProductDto> sapProducts = sapCompanyProducts.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.ProductStatus is not null && x.ProductType is not null && x.SalesUnitOfMeasure is not null).ToList();

                    if (sapProducts.Any())
                    {
                        var products = sapProducts.GroupBy(x => x.Id).Select(g => g.OrderBy(o => o.Price).First()).ToList();
                        products.ForEach(x =>
                        {
                            var productStatus = statuses.FirstOrDefault(s => s.Name == x.ProductStatus.Name);

                            if (productStatus is not null)
                            {
                                productlist.Add(new ProductDto
                                {
                                    CompanyCode = companyCode,
                                    CountryCode = countryCode,
                                    Description = x.Description,
                                    Id = x.Id,
                                    Name = x.Name,
                                    Price = x.Price,
                                    ProductType = x.ProductType,
                                    UnitOfMeasureCode = x.SalesUnitOfMeasure.Code,
                                    ProductStatus = new NameAndId<byte>(productStatus.Id, productStatus.Name)
                                });
                            }
                        });
                    }
                }
            }

            List<Shared.Data.Models.Product> newProducts = new();
            List<Shared.Data.Models.Product> productsToUpdate = new();

            if (productlist.Any())
            {
                var productsSapNumbers = productlist.Select(x => x.Id).ToList();
                var products = await _productRepository.Table.Where(x => productsSapNumbers.Contains(x.ProductSapNumber)).ToListAsync(cancellationToken);

                productlist.ForEach(c =>
                {
                    var productSelected = products.FirstOrDefault(p => p.ProductSapNumber == c.Id && p.CompanyCode == c.CompanyCode);
                    if (productSelected is null)
                    {
                        Shared.Data.Models.Product product = new()
                        {
                            CompanyCode = c.CompanyCode,
                            CountryCode = c.CountryCode,
                            Name = c.Name,
                            DateCreated = DateTime.UtcNow,
                            DateRefreshed = DateTime.UtcNow,
                            Description = c.Description,
                            Price = c.Price,
                            ProductSapNumber = c.Id,
                            ProductType = c.ProductType.Name,
                            UnitOfMeasureCode = c.UnitOfMeasureCode,
                            ProductStatusId = c.ProductStatus.Id,
                            IsDeleted = false
                        };
                        newProducts.Add(product);
                    }
                    else
                    {
                        productSelected.Name = c.Name;
                        productSelected.Description = c.Description;
                        productSelected.Price = c.Price;
                        productSelected.DateRefreshed = DateTime.UtcNow;
                        productSelected.CompanyCode = c.CompanyCode;
                        productSelected.ProductType = c.ProductType.Name;
                        productSelected.CountryCode = c.CountryCode;
                        productSelected.UnitOfMeasureCode = c.UnitOfMeasureCode;
                        productSelected.ProductStatusId = c.ProductStatus.Id;
                        productsToUpdate.Add(productSelected);
                    }
                });

                if (newProducts.Any())
                    await _productRepository.AddRangeAsync(newProducts, cancellationToken);

                if (productsToUpdate.Any())
                    _productRepository.UpdateRange(productsToUpdate);

                if (newProducts.Any() || productsToUpdate.Any())
                    await _productRepository.CommitAsync(cancellationToken);

                List<Shared.Data.Models.Product> productsCommited = new();
                if (newProducts.Any())
                    productsCommited.AddRange(newProducts);

                if (productsToUpdate.Any())
                    productsCommited.AddRange(productsToUpdate);

                if (productsCommited.Any())
                {
                    List<string> productRefreshMessages = new();
                    foreach(var x in productsCommited)
                    {
                        var productStatuses = Enum.GetValues(typeof(ProductStatusEnum)).Cast<ProductStatusEnum>()
                  .Select(e => new NameAndId<byte>((byte)e, e.ToDescription())).ToList();

                        var productStatus = productStatuses.FirstOrDefault(p => p.Id == x.ProductStatusId);
                        ProductRefreshedMessage productRefreshMessage = new()
                        {
                            Description = x.Description,
                            CompanyCode = x.CompanyCode,
                            CountryCode = x.CountryCode,
                            DateRefreshed = x.DateRefreshed,
                            DateCreated = x.DateCreated,
                            Name = x.Name,
                            Price = x.Price,
                            ProductId = x.Id,
                            UnitOfMeasureCode = x.UnitOfMeasureCode,
                            ProductSapNumber = x.ProductSapNumber,
                            ProductStatus = new NameAndCode(productStatus.Name, productStatus.Name), 
                            Id = Guid.NewGuid()
                        };
                        await _messageBus.PublishTopicMessage(productRefreshMessage, EventMessages.PRODUCTS_PRODUCT_REFRESHED);
                    }
                }
            }

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_PRODUCT_REFRESH);
        }

        #region Private Methods
        private static Func<IQueryable<Shared.Data.Models.Product>, IOrderedQueryable<Shared.Data.Models.Product>> ProcessOrderFunc(ProductSortingDto? orderExpression = null)
        {
            IOrderedQueryable<Shared.Data.Models.Product> orderFunction(IQueryable<Shared.Data.Models.Product> queryable)
            {
                if (orderExpression == null)
                    return queryable.OrderByDescending(p => p.DateCreated);

                var orderQueryable = orderExpression.IsNameAscending
                   ? queryable.OrderBy(p => p.Name).ThenByDescending(p => p.DateCreated)
                   : orderExpression.IsNameDescending
                       ? queryable.OrderByDescending(p => p.Name).ThenByDescending(p => p.DateCreated)
                       : queryable.OrderByDescending(p => p.DateCreated);
                return orderQueryable;
            }
            return orderFunction;
        }

        private static IReadOnlyList<ProductResponse> ProcessQuery(IReadOnlyList<Shared.Data.Models.Product> products)
        {
            return products.Select(p =>
            {
                var primaryImage = p.ProductImages.Any() ? p.ProductImages.FirstOrDefault(x => x.IsPrimaryImage) : null;
                var item = new ProductResponse(p.Id, p.Name, p.Description, p.ProductType, p.UnitOfMeasureCode, primaryImage?.PublicUrl, primaryImage?.Id, new NameAndCode(p.ProductStatus.Code, p.ProductStatus.Name), p.DateModified, p.Price);
                return item;
            }).ToList();
        }

        private static IReadOnlyList<AvailableProductResponse> ProcessAvailableProductQuery(IReadOnlyList<Shared.Data.Models.Product> products)
        {
            return products.Select(p =>
            {
                var primaryImage = p.ProductImages.Any() ? p.ProductImages.FirstOrDefault(x => x.IsPrimaryImage) : null;
                var item = new AvailableProductResponse(p.Id, p.Name, p.Description, p.ProductType, p.UnitOfMeasureCode, primaryImage?.PublicUrl, primaryImage?.Id, new NameAndCode(p.ProductStatus.Code, p.ProductStatus.Name), p.DateModified);
                return item;
            }).ToList();
        }

        private static ProductDetailResponse ProcessQuery(Shared.Data.Models.Product? product)
        {
            if (product == null)
                return null;

            var productResponse = new ProductDetailDto()
            {
                ProductId = product.Id,
                DateModified = product.DateModified,
                Description = product.Description,
                Name = product.Name,
                ProductType = product.ProductType, 
                Price = product.Price, 
                ProductSapNumber = product.ProductSapNumber,
                UnitOfMeasure = new NameAndCode(product.UnitOfMeasureCode, product.UnitOfMeasureCode),
                ProductStatus = new NameAndCode(product.ProductStatus.Code, product.ProductStatus.Name),
                ProductImages = !product.ProductImages.Any() ? new List<ProductImageResponse>() : product.ProductImages.Select(x => new ProductImageResponse
                {
                    Id = x.Id,
                    IsPrimaryImage = x.IsPrimaryImage,
                    PublicUrl = x.PublicUrl
                }).ToList()
            };
            return new ProductDetailResponse(productResponse);
        }

        #endregion
    }
}
