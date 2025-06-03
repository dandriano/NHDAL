using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;


namespace NHDAL.Tests
{
    /// <summary>
    /// Base class for registrars.
    /// </summary>
    internal class RegistrarBase
    {
        private PostgreSqlContainer _db;

        protected ServiceProvider _serviceProvider;

        [OneTimeSetUp]
        public async Task OneTimeSetup()
        {
            var services = new ServiceCollection();

            // use inline options
            /*
            services.Configure<UnitOfWorkFactoryOptions>(options =>
            {
                ...
            });
            */

            // or use configure file
            var config = new ConfigurationBuilder()
                            .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                            .AddJsonFile("db.json", optional: false, reloadOnChange: true)
                            .Build();

            // start postgresql container
            var cn = config.GetSection(nameof(UnitOfWorkFactoryOptions)).Get<UnitOfWorkFactoryOptions>()!;
            _db = new PostgreSqlBuilder()
                                    .WithImage("postgres:alpine")
                                    .WithHostname(cn.Host)
                                    .WithPortBinding(cn.Port)
                                    .WithDatabase(cn.Database)
                                    .WithUsername(cn.Username)
                                    .WithPassword(cn.Secret)
                                    .Build();
            await _db.StartAsync();

            // null logger
            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

            services.Configure<UnitOfWorkFactoryOptions>(config.GetSection(nameof(UnitOfWorkFactoryOptions)));
            services.AddSingleton<UnitOfWorkFactory>();

            _serviceProvider = services.BuildServiceProvider();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _db.DisposeAsync();
            _serviceProvider.Dispose();
        }
    }
}
