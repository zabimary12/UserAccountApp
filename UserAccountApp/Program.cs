using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using UserAccountApp.Data;
using UserAccountApp.Interfaces;
using UserAccountApp.Repositories;
using UserAccountApp.ViewModels;
using UserAccountApp.Factories;
using UserAccountApp.Views;
using Microsoft.Extensions.Logging;

namespace UserAccountApp
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder()
        {
            return new HostBuilder()
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureServices((context, services) =>
                {
                    // Database
                    services.AddSingleton<DatabaseContext>();
                    services.AddSingleton<IDatabase>(sp => sp.GetRequiredService<DatabaseContext>());

                    // Repositories
                    services.AddScoped<IEmployeeRepository, EmployeeRepository>();
                    services.AddScoped<IEmployeeReportRepository, EmployeeReportRepository>();
                    services.AddScoped<IDepartmentRepository, DepartmentRepository>();
                    services.AddScoped<IPositionRepository, PositionRepository>();

                    // Factories
                    services.AddSingleton<RepositoryFactory>();

                    // ViewModels
                    services.AddTransient<MainViewModel>();
                    services.AddTransient<EmployeesViewModel>();
                    services.AddTransient<SalaryReportViewModel>();
                    services.AddTransient<CompanyInfoViewModel>();
                    services.AddTransient<EmployeeDialogViewModel>();

                    // Views
                    services.AddTransient<MainWindow>();
                    services.AddTransient<EmployeeDialog>();
                    services.AddTransient<SalaryReportView>();
                });
        }
    }
} 