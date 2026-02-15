using LabCollect.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LabCollect.Repository.Interface
{
    public interface ITestsService
    {
        Task<bool> InsertAsync(LaboratoryTestModel model);
        Task<bool> UpdateAsync(LaboratoryTestModel model);
        Task<bool> DeleteAsync(int testId);
        Task<List<LaboratoryTestModel>> GetAllAsync();
        Task<LaboratoryTestModel> GetByIdAsync(int? testId);
        Task<List<SelectListItem>> GetCategories();
    }
}
