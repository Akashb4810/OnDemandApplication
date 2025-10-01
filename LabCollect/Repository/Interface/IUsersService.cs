using LabCollect.Models;

namespace LabCollect.Repository.Interface
{
    public interface IUserService
    {
        List<AppTypeViewModel> GetAppTypes();
        UserLoginResult GetUserByCredentials(string username, string password, int appTypeId);
        UserLoginResult ValidateUser(string username, string password, int appTypeId);
    }
}
