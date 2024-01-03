using Product.Application.DTOs;

namespace Product.Application.ViewModels.Responses
{
    public record ProductResponse(int ProductId, string Name, string Description, string ProductType, string UnitOfMeasure, string? PrimaryProductImageUrl, int? PrimaryProductImageId, NameAndCode ProductStatus, DateTime? DateModified, decimal? Price);
}
