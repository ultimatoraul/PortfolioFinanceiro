using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace PortfolioFinanceiro.Data
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Registra o DataContext com banco de dados em memória (InMemory)
        /// </summary>
        public static IServiceCollection AddInMemoryDataContext(
            this IServiceCollection services,
            string databaseName = "PortfolioFinanceiroDb")
        {
            services.AddDbContext<DataContext>(options =>
            {
                options.UseInMemoryDatabase(databaseName);
                options.EnableSensitiveDataLogging();
            });

            return services;
        }

        /// <summary>
        /// Inicializa o banco de dados e carrega os dados de seed
        /// </summary>
        public static async Task<IServiceProvider> InitializeDatabaseAsync(
            this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                await context.InitializeAsync();
            }

            return serviceProvider;
        }
    }
}
