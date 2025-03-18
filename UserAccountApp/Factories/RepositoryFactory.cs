using UserAccountApp.Data;
using UserAccountApp.Interfaces;
using UserAccountApp.Repositories;

namespace UserAccountApp.Factories
{
    public class RepositoryFactory
    {
        private readonly IDatabase _database;

        public RepositoryFactory(IDatabase database)
        {
            _database = database;
        }

        public IEmployeeRepository CreateEmployeeRepository()
        {
            return new EmployeeRepository((DatabaseContext)_database);
        }

        public IEmployeeReportRepository CreateEmployeeReportRepository()
        {
            return new EmployeeReportRepository((DatabaseContext)_database);
        }

        public IDepartmentRepository CreateDepartmentRepository()
        {
            return new DepartmentRepository((DatabaseContext)_database);
        }

        public IPositionRepository CreatePositionRepository()
        {
            return new PositionRepository((DatabaseContext)_database);
        }
    }
} 