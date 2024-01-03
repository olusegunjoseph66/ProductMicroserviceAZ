namespace Product.Application.Interfaces.Services
{
    public interface IAuthenticatedUserService
    {
        public int UserId { get; set; }
        public string Role { get; set; }
    }
}
