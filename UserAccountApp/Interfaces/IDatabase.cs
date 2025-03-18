using System.Data.SqlClient;

namespace UserAccountApp.Interfaces
{
    public interface IDatabase
    {
        SqlConnection GetConnection();
        void ValidateConnection();
    }
} 