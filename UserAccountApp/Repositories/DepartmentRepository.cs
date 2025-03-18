using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using UserAccountApp.Model;
using UserAccountApp.Data;
using UserAccountApp.Interfaces;

namespace UserAccountApp.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly DatabaseContext _context;

        public DepartmentRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<Department>> GetAllAsync()
        {
            var departments = new List<Department>();
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT Id, Name FROM Departments ORDER BY Name", connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        departments.Add(new Department
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
            }
            return departments;
        }

        public async Task<Department> GetByIdAsync(int id)
        {
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT Id, Name FROM Departments WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Department
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            };
                        }
                        return null;
                    }
                }
            }
        }

        public async Task<int> AddAsync(Department department)
        {
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("INSERT INTO Departments (Name) VALUES (@Name); SELECT SCOPE_IDENTITY();", connection))
                {
                    command.Parameters.AddWithValue("@Name", department.Name);
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task UpdateAsync(Department department)
        {
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("UPDATE Departments SET Name = @Name WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", department.Id);
                    command.Parameters.AddWithValue("@Name", department.Name);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("DELETE FROM Departments WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
} 