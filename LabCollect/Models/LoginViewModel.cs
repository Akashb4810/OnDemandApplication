using System.ComponentModel.DataAnnotations;

namespace LabCollect.Models
{
    public class LoginViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public int SelectedAppType { get; set; } // ID selected from dropdown

        public List<AppTypeViewModel> AppTypes { get; set; } = new();
    }

    public class AppTypeViewModel
    {
        public int AppTypeId { get; set; }
        public string AppTypeName { get; set; }
    }

    public class UserLoginResult
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string RoleName { get; set; }
        public string AppTypeName { get; set; }
    }

}
