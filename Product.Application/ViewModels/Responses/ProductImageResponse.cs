namespace Product.Application.ViewModels.Responses
{
    public class ProductImageResponse
    {
        public int Id { get; set; }
        public string PublicUrl { get; set; }
        public bool IsPrimaryImage { get; set; }
    }
}
