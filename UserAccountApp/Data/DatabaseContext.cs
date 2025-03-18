using System;
using System.Data.SqlClient;
using System.Configuration;
using UserAccountApp.Config;
using UserAccountApp.Interfaces;

namespace UserAccountApp.Data
{
    public class DatabaseContext : IDatabase
    {
        private readonly string _connectionString;

        public DatabaseContext()
        {
            _connectionString = ConnectionConfig.GetConnectionString();
            ValidateConnection();
        }

        public DatabaseContext(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            _connectionString = connectionString;
            ValidateConnection();
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public void ValidateConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                }
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException($"Не вдалося підключитися до бази даних. Помилка: {ex.Message}", ex);
            }
        }
    }
} 