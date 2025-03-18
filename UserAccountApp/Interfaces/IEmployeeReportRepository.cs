using System.Collections.Generic;
using System.Threading.Tasks;
using UserAccountApp.Model;

namespace UserAccountApp.Interfaces
{
    public interface IEmployeeReportRepository
    {
        Task<IEnumerable<DepartmentSalaryReport>> GetDepartmentSalaryReportAsync(int? departmentId = null);
        Task ExportToExcelAsync(IEnumerable<DepartmentSalaryReport> reportData, string filePath);
    }
} 