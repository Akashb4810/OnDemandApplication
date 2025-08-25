namespace DAL.Models.User
{
    public class LoginResponse
    {
        public string HOSPITALID { get; set; }
        public long? MobileNumber { get; set; }
        public string Emailid { get; set; }
        public string Connection { get; set; }
        public Guid USERID { get; set; }
        public Guid RoleId { get; set; }
    }
}
