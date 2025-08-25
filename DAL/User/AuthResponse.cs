namespace DAL.Models.User
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? Expires { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpires { get; set; }
    }
}
