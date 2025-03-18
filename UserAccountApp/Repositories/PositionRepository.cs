using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using UserAccountApp.Model;
using UserAccountApp.Data;
using UserAccountApp.Interfaces;

namespace UserAccountApp.Repositories
{
    public class PositionRepository : IPositionRepository
    {
        private readonly DatabaseContext _context;

        public PositionRepository(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<List<Position>> GetAllPositionsAsync()
        {
            var positions = new List<Position>();
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT Id, Name FROM Positions ORDER BY Name";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        positions.Add(new Position
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name"))
                        });
                    }
                }
            }
            return positions;
        }

        public async Task<Position> GetByIdAsync(int id)
        {
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("SELECT Id, Name FROM Positions WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new Position
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

        public async Task<int> AddAsync(Position position)
        {
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("INSERT INTO Positions (Name) VALUES (@Name); SELECT SCOPE_IDENTITY();", connection))
                {
                    command.Parameters.AddWithValue("@Name", position.Name);
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task UpdateAsync(Position position)
        {
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("UPDATE Positions SET Name = @Name WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", position.Id);
                    command.Parameters.AddWithValue("@Name", position.Name);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteAsync(int id)
        {
            using (var connection = _context.GetConnection())
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand("DELETE FROM Positions WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
} 