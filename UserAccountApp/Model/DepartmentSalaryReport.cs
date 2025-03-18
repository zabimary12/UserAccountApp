using System;

namespace UserAccountApp.Model
{
    public class DepartmentSalaryReport
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public int EmployeeCount { get; set; }
        public decimal TotalSalary { get; set; }
        public decimal AverageSalary => EmployeeCount > 0 ? TotalSalary / EmployeeCount : 0;
    }
} 