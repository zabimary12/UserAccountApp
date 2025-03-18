using System.Collections.Generic;
using System.Threading.Tasks;
using UserAccountApp.Model;

namespace UserAccountApp.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<List<Department>> GetAllAsync();
        Task<Department> GetByIdAsync(int id);
        Task<int> AddAsync(Department department);
        Task UpdateAsync(Department department);
        Task DeleteAsync(int id);
    }
} 