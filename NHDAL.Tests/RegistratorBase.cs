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
        private PostgreSqlContainer _postgresContainer;

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
            var cn = new UnitOfWorkFactoryOptions();
            config.GetSection(nameof(UnitOfWorkFactoryOptions)).Bind(cn);

            _postgresContainer = new PostgreSqlBuilder()
                .WithImage("postgres:alpine")
                .WithHostname(cn.Host)
                .WithPortBinding(cn.Port)
                .WithDatabase(cn.Database)
                .WithUsername(cn.Username)
                .WithPassword(cn.Secret)
                .Build();
            await _postgresContainer.StartAsync();

            // null configuration
            services.AddSingleton<IConfiguration>(config);

            services.AddSingleton<ILoggerFactory>(NullLoggerFactory.Instance);
            services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));

            services.Configure<UnitOfWorkFactoryOptions>(config.GetSection(nameof(UnitOfWorkFactoryOptions)));
            services.AddSingleton<UnitOfWorkFactory>();

            _serviceProvider = services.BuildServiceProvider();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            await _postgresContainer.DisposeAsync();
            _serviceProvider.Dispose();
        }
    }
}
