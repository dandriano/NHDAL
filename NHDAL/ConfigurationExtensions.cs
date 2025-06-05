using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NHDAL
{
    public static class ConfigurationExtensions
    {
        public static Configuration CreateConfiguration(UnitOfWorkFactoryOptions options)
        {
            return CreateConfiguration(options.Secret,
                                       options.Host,
                                       options.Port,
                                       options.Database,
                                       options.Username,
                                       options.ApplicationName,
                                       options.AssemblyName);
        }
        public static Configuration CreateConfiguration(string secret,
                                                        string host = "127.0.0.1",
                                                        string port = "5432",
                                                        string database = "test",
                                                        string username = "postgres",
                                                        string applicationName = "",
                                                        string assemblyName = "")
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

            var cfg = new Configuration();
            var mappings = new List<Type>();
            var entities = new List<Type>();

            if (string.IsNullOrEmpty(assemblyName))
            {
                var mappingAssembly = Assembly.GetExecutingAssembly();
                mappings.AddRange(mappingAssembly.GetExportedTypes()
                                                 .Where(type => type.Namespace?.EndsWith("Maps") ?? false));
                entities.AddRange(mappingAssembly.GetExportedTypes()
                                                 .Where(type => type.Namespace?.EndsWith("Entities") ?? false));
            }
            else
            {
                var mappingAssembly = AppDomain.CurrentDomain
                                        .GetAssemblies()
                                        .First(a => a.GetName().Name == assemblyName);
                mappings.AddRange(mappingAssembly.GetTypes()
                                                 .Where(type => type.Namespace?.EndsWith("Maps") ?? false));
                entities.AddRange(mappingAssembly.GetTypes()
                                                 .Where(type => type.Namespace?.EndsWith("Entities") ?? false));
            }

            cfg.AddMappings(mappings, entities);
            cfg.SetupDataBaseIntegration<PostgreSQL83Dialect>(sb.ToString());

            return cfg;
        }
        public static Configuration SetupDataBaseIntegration<TDialect>(this Configuration config, string connectionString) where TDialect : Dialect
        {
            config.DataBaseIntegration(db =>
            {
                db.ConnectionString = connectionString;
                db.Dialect<TDialect>();
            });

            return config;
        }
        public static Configuration AddMappings(this Configuration config, IEnumerable<Type> maps, IEnumerable<Type> entities)
        {
            var mapper = new ConventionModelMapper();

            mapper.AddMappings(maps);
            config.AddMapping(mapper.CompileMappingFor(entities));

            return config;
        }
    }
}
