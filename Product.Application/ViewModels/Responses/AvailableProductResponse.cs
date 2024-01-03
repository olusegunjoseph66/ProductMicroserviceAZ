using Product.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Product.Application.ViewModels.Responses
{
    public record AvailableProductResponse(int ProductId, string Name, string Description, string ProductType, string UnitOfMeasureCode, string? PrimaryProductImageUrl, int? PrimaryProductImageId, NameAndCode ProductStatus, DateTime? DateModified);
}
