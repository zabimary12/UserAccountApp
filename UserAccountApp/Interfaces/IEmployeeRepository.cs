using System.Collections.Generic;
using System.Threading.Tasks;
using UserAccountApp.Model;

namespace UserAccountApp.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetEmployeesAsync(string departmentFilter = null, string positionFilter = null, string nameFilter = null);
        Task<Employee> GetByIdAsync(int id);
        Task<int> AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task DeleteAsync(int id);
    }
} 