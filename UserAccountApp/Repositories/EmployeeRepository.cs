using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using UserAccountApp.Model;
using UserAccountApp.Data;
using UserAccountApp.Interfaces;

namespace UserAccountApp.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly DatabaseContext _context;

        public EmployeeRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetEmployeesAsync(string departmentFilter = null, string positionFilter = null, string nameFilter = null)
        {
            var employees = new List<Employee>();
            var query = @"SELECT e.*, d.Name as DepartmentName, p.Name as PositionName, c.Name as CompanyName 
                         FROM Employees e
                         JOIN Departments d ON e.DepartmentId = d.Id
                         JOIN Positions p ON e.PositionId = p.Id
                         JOIN Companies c ON e.CompanyId = c.Id
                         WHERE (@DepartmentFilter IS NULL OR d.Name LIKE @DepartmentFilter)
                         AND (@PositionFilter IS NULL OR p.Name LIKE @PositionFilter)
                         AND (@NameFilter IS NULL OR 
                              e.FirstName LIKE @NameFilter OR 
                              e.LastName LIKE @NameFilter OR 
                              e.MiddleName LIKE @NameFilter)";

            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DepartmentFilter", departmentFilter == null ? DBNull.Value : $"%{departmentFilter}%");
                    command.Parameters.AddWithValue("@PositionFilter", positionFilter == null ? DBNull.Value : $"%{positionFilter}%");
                    command.Parameters.AddWithValue("@NameFilter", nameFilter == null ? DBNull.Value : $"%{nameFilter}%");

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            employees.Add(new Employee
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                MiddleName = reader.IsDBNull(reader.GetOrdinal("MiddleName")) ? null : reader.GetString(reader.GetOrdinal("MiddleName")),
                                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                                BirthDate = reader.GetDateTime(reader.GetOrdinal("BirthDate")),
                                HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate")),
                                Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                                Department = new Department { Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")), Name = reader.GetString(reader.GetOrdinal("DepartmentName")) },
                                Position = new Position { Id = reader.GetInt32(reader.GetOrdinal("PositionId")), Name = reader.GetString(reader.GetOrdinal("PositionName")) }
                            });
                        }
                    }
                }
            }
            return employees;
        }

        public async Task<decimal> GetTotalSalaryByDepartmentAsync(int departmentId)
        {
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                var query = "SELECT SUM(Salary) FROM Employees WHERE DepartmentId = @DepartmentId";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DepartmentId", departmentId);
                    var result = await command.ExecuteScalarAsync();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            }
        }

        public async Task<List<Employee>> GetByDepartmentAsync(int departmentId)
        {
            var employees = new List<Employee>();
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT e.Id, e.FirstName, e.LastName, e.MiddleName, e.BirthDate, 
                           e.PhoneNumber, e.Address, e.HireDate, e.Salary,
                           d.Id as DepartmentId, d.Name as DepartmentName,
                           p.Id as PositionId, p.Name as PositionName
                    FROM Employees e
                    INNER JOIN Departments d ON e.DepartmentId = d.Id
                    INNER JOIN Positions p ON e.PositionId = p.Id
                    WHERE e.DepartmentId = @DepartmentId";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DepartmentId", departmentId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var employee = new Employee
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                MiddleName = !reader.IsDBNull(reader.GetOrdinal("MiddleName")) 
                                    ? reader.GetString(reader.GetOrdinal("MiddleName")) 
                                    : null,
                                BirthDate = reader.GetDateTime(reader.GetOrdinal("BirthDate")),
                                PhoneNumber = !reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) 
                                    ? reader.GetString(reader.GetOrdinal("PhoneNumber")) 
                                    : null,
                                Address = !reader.IsDBNull(reader.GetOrdinal("Address")) 
                                    ? reader.GetString(reader.GetOrdinal("Address")) 
                                    : null,
                                HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate")),
                                Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                                Department = new Department
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                    Name = reader.GetString(reader.GetOrdinal("DepartmentName"))
                                },
                                Position = new Position
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("PositionId")),
                                    Name = reader.GetString(reader.GetOrdinal("PositionName"))
                                }
                            };
                            employees.Add(employee);
                        }
                    }
                }
            }
            return employees;
        }

        public async Task<Employee> GetByIdAsync(int id)
        {
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                var query = @"SELECT e.*, d.Name as DepartmentName, p.Name as PositionName 
                            FROM Employees e
                            JOIN Departments d ON e.DepartmentId = d.Id
                            JOIN Positions p ON e.PositionId = p.Id
                            WHERE e.Id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Employee
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                MiddleName = reader.IsDBNull(reader.GetOrdinal("MiddleName")) ? null : reader.GetString(reader.GetOrdinal("MiddleName")),
                                Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString(reader.GetOrdinal("Address")),
                                PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PhoneNumber")) ? null : reader.GetString(reader.GetOrdinal("PhoneNumber")),
                                BirthDate = reader.GetDateTime(reader.GetOrdinal("BirthDate")),
                                HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate")),
                                Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                                Department = new Department 
                                { 
                                    Id = reader.GetInt32(reader.GetOrdinal("DepartmentId")), 
                                    Name = reader.GetString(reader.GetOrdinal("DepartmentName")) 
                                },
                                Position = new Position 
                                { 
                                    Id = reader.GetInt32(reader.GetOrdinal("PositionId")), 
                                    Name = reader.GetString(reader.GetOrdinal("PositionName")) 
                                }
                            };
                        }
                        return null;
                    }
                }
            }
        }

        public async Task<int> AddAsync(Employee employee)
        {
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                var query = @"INSERT INTO Employees (FirstName, LastName, MiddleName, Address, PhoneNumber, 
                                                   BirthDate, HireDate, Salary, DepartmentId, PositionId)
                            VALUES (@FirstName, @LastName, @MiddleName, @Address, @PhoneNumber, 
                                    @BirthDate, @HireDate, @Salary, @DepartmentId, @PositionId);
                            SELECT SCOPE_IDENTITY();";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                    command.Parameters.AddWithValue("@LastName", employee.LastName);
                    command.Parameters.AddWithValue("@MiddleName", (object)employee.MiddleName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object)employee.Address ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PhoneNumber", (object)employee.PhoneNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@BirthDate", employee.BirthDate);
                    command.Parameters.AddWithValue("@HireDate", employee.HireDate);
                    command.Parameters.AddWithValue("@Salary", employee.Salary);
                    command.Parameters.AddWithValue("@DepartmentId", employee.Department.Id);
                    command.Parameters.AddWithValue("@PositionId", employee.Position.Id);

                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task UpdateAsync(Employee employee)
        {
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                var query = @"UPDATE Employees 
                            SET FirstName = @FirstName,
                                LastName = @LastName,
                                MiddleName = @MiddleName,
                                Address = @Address,
                                PhoneNumber = @PhoneNumber,
                                BirthDate = @BirthDate,
                                HireDate = @HireDate,
                                Salary = @Salary,
                                DepartmentId = @DepartmentId,
                                PositionId = @PositionId
                            WHERE Id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", employee.Id);
                    command.Parameters.AddWithValue("@FirstName", employee.FirstName);
                    command.Parameters.AddWithValue("@LastName", employee.LastName);
                    command.Parameters.AddWithValue("@MiddleName", (object)employee.MiddleName ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Address", (object)employee.Address ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PhoneNumber", (object)employee.PhoneNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@BirthDate", employee.BirthDate);
                    command.Parameters.AddWithValue("@HireDate", employee.HireDate);
                    command.Parameters.AddWithValue("@Salary", employee.Salary);
                    command.Parameters.AddWithValue("@DepartmentId", employee.Department.Id);
                    command.Parameters.AddWithValue("@PositionId", employee.Position.Id);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                var query = "DELETE FROM Employees WHERE Id = @Id";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
} 