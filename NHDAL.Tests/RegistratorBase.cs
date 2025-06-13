using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Threading.Tasks;
using Testcontainers.PostgreSql;


namespace NHDAL.Tests
{
    /// <summary>
    /// Base class for registrars.
    /// </summary>
    public class RegistrarBase
    {
        private PostgreSqlContainer _db = null!;
        protected ServiceProvider _serviceProvider = null!;

        public virtual async Task OneTimeSetup()
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
            // to-do dynamic ports
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
            // transient between tests, testing dispose
            services.AddTransient<UnitOfWorkFactory>();

            _serviceProvider = services.BuildServiceProvider();
        }
        public virtual async Task OneTimeTearDown()
        {
            await _db.DisposeAsync();
            _serviceProvider.Dispose();
        }
    }
}
