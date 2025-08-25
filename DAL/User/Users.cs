using System.ComponentModel.DataAnnotations;

namespace DAL.Models.User
{
    public class Users
    {
        [Required(ErrorMessage = "User ID is required")]
        public Guid USERID { get; set; }  // Matches alias "USERID"

        [Required(ErrorMessage = "Role ID is required")]
        public Guid RoleId { get; set; }  // Matches alias "RoleId"

        [Required(ErrorMessage = "Rolename is required")]
        public string RoleName { get; set; }  // Matches alias "RoleName"

        [Required(ErrorMessage = "Hospital ID is required")]
        public Guid HOSPITALID { get; set; }  // Matches alias "HOSPITALID"

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z0-9_]*$", ErrorMessage = "Username must contain only letters, numbers, or underscores")]
        public string UserName { get; set; }  // Matches alias "UserName"

        [Required(ErrorMessage = "Password is required")]
        [StringLength(50)]
        public string Password { get; set; }  // Matches alias "Password"

        [Range(1000000000, 9999999999, ErrorMessage = "Mobile number must be 10 digits")]
        public long? MobileNumber { get; set; }  // Matches alias "MobileNumber"

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Emailid { get; set; }  // Matches alias "Emailid"

        public DateTime? LastLoginDate { get; set; }  // Matches alias "LastLoginDate"
        public bool? ActiveFlag { get; set; }  // Matches alias "ActiveFlag"
        public int? CreationUID { get; set; }  // Matches alias "CreationUID"
        public DateTime? CreationDateTime { get; set; }  // Matches alias "CreationDateTime"
        public int? ModifyUID { get; set; }  // Matches alias "ModifyUID"
        public DateTime? ModifyDateTime { get; set; }  // Matches alias "ModifyDateTime"
    }
}
