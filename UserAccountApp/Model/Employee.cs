using System;

namespace UserAccountApp.Model
{
    public class Employee
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime HireDate { get; set; }
        public decimal Salary { get; set; }
        
        public int DepartmentId { get; set; }
        public Department Department { get; set; }
        
        public int PositionId { get; set; }
        public Position Position { get; set; }
        
        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
} 