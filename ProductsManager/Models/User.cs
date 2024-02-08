namespace ProductsManager.Models
{
    public class User
    {
        public string UserName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int phone { get; set; }


    }
}
