using System.Configuration;

namespace UserAccountApp.Config
{
    public static class ConnectionConfig
    {
        public static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"]?.ConnectionString
                ?? throw new ConfigurationErrorsException("Connection string 'DefaultConnection' not found in configuration.");
        }
    }
} 