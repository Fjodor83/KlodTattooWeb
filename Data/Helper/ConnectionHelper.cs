using Npgsql;
namespace KlodTattoo.Data.Helper
{
    public static class ConnectionHelper
    {
        public static string GetConnectionString(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
                if (!string.IsNullOrEmpty(databaseUrl))
                {
                    connectionString = BuildConnectionString(databaseUrl);
                }
            }

            if (string.IsNullOrEmpty(connectionString)) { 

                throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            }
            return connectionString;
        }

        private static string BuildConnectionString(string databaseUrl)
            {
                var databaseUri = new Uri(databaseUrl);
                var userInfo = databaseUri.UserInfo.Split(':');
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = databaseUri.Host,
                    Port = databaseUri.Port,
                    Username = userInfo[0],
                    Password = userInfo[1],
                    Database = databaseUri.AbsolutePath.TrimStart('/'),
                    SslMode = SslMode.Require,
                   // TrustServerCertificate = true
                };
                return builder.ToString();
            }
    }
}
