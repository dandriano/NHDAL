using NHibernate.Cfg;
using NHibernate.Extensions.Npgsql;
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
            Assembly mappingAssembly = null!;
            if (string.IsNullOrEmpty(assemblyName))
            {
                mappingAssembly = Assembly.GetExecutingAssembly();
            }
            else
            {
                mappingAssembly = AppDomain.CurrentDomain
                                        .GetAssemblies()
                                        .First(a => a.GetName().Name == assemblyName);
            }

            mappings.AddRange(mappingAssembly.GetExportedTypes()
                                 .Where(type => type.Namespace?.EndsWith("Maps") ?? false));
            entities.AddRange(mappingAssembly.GetExportedTypes()
                                             .Where(type => type.Namespace?.EndsWith("Entities") ?? false));

            cfg.AddMappings(mappings, entities);
            cfg.SetupDataBaseIntegration<NpgsqlDialect, NpgsqlDriver>(sb.ToString());
            cfg.IntegrateWithEnvers();

            return cfg;
        }
        public static Configuration SetupDataBaseIntegration<TDialect, TDriver>(this Configuration config, string connectionString)
            where TDialect : NHibernate.Dialect.Dialect
            where TDriver : NHibernate.Driver.IDriver
        {
            config.DataBaseIntegration(db =>
            {
                db.ConnectionString = connectionString;
                db.Dialect<TDialect>();
                db.Driver<TDriver>();
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
