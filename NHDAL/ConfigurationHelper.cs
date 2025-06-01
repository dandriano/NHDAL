using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using System;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NHDAL
{
    public static class ConfigurationHelper
    {
        public static Configuration CreateConfiguration(UnitOfWorkFactoryOptions options)
        {
            return CreateConfiguration(options.Secret,
                                       options.Host,
                                       options.Port,
                                       options.Database,
                                       options.Username,
                                       options.ApplicationName);
        }
        public static Configuration CreateConfiguration(string secret,
                                                        string host = "127.0.0.1",
                                                        string port = "5432",
                                                        string database = "test",
                                                        string username = "postgres",
                                                        string applicationName = "")
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("Provide secret");

            var sb = new StringBuilder();

            sb.Append($"Host={host};");
            sb.Append($"Port={port};");
            sb.Append($"Database={database};");
            sb.Append($"Username={username};");
            sb.Append($"Password={secret};");
            sb.Append("SslMode=Disable");

            if (!string.IsNullOrWhiteSpace(applicationName))
                sb.Append($";ApplicationName={applicationName}");

            var currentAssembly = Assembly.GetExecutingAssembly();
            var cfg = new Configuration();
            var mapper = new ConventionModelMapper();

            mapper.AddMappings(currentAssembly.GetExportedTypes()
                                .Where(type => type.Namespace!.EndsWith("Maps")));
            var mapping = mapper.CompileMappingFor(currentAssembly.GetExportedTypes()
                                                    .Where(type => type.Namespace!.EndsWith("Entities")));
            cfg.AddMapping(mapping);

            cfg.DataBaseIntegration(db =>
            {
                db.ConnectionString = sb.ToString();
                db.Dialect<PostgreSQL83Dialect>();
            });

            return cfg;
        }
    }
}
