using LabCollect.Models;

namespace LabCollect.Repository.Interface
{
    public interface IPatientService
    {
        Patient GetPatientByName(string name);
        Patient GetPatientById(int id);
        List<Patient> SearchPatientsByName(string name);
    }
}
