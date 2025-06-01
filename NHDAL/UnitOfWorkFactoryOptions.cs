namespace NHDAL
{
    public class UnitOfWorkFactoryOptions
    {
        public string Secret { get; set; } = string.Empty;
        public string Username { get; set; } = "postgres";
        public string Host { get; set; } = "127.0.0.1";
        public string Port { get; set; } = "5432";
        public string Database { get; set; } = "test";
        public string ApplicationName { get; set; } = string.Empty;
    }
}