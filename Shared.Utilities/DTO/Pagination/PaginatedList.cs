namespace Shared.Utilities.DTO.Pagination
{
    public class PaginatedList<T>
    {
        public PaginatedList(IReadOnlyList<T> items, PaginationMetaData meta, object parameters)
        {
            Items = items;
            Pagination = meta;
            OriginParams = parameters;
        }

        public object? OriginParams { get; set; }
        public IReadOnlyList<T> Items { get; }
        public PaginationMetaData Pagination { get; }
    }
}
