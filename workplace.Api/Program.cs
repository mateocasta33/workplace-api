using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using workplace.Application.Dtos;
using workplace.Application.Interfaces;
using workplace.Application.Services;
using workplace.Domain.Interfaces;
using workplace.Infrastructure.Repositories;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
        {
            services.AddApplicationInsightsTelemetryWorkerService();
            services.ConfigureFunctionsApplicationInsights();

            services.AddHttpContextAccessor();

            // AutoMapper
            services.AddAutoMapper(typeof(MapProfile));
            
            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            
            // Service
            services.AddScoped<IUserService, UserService>();
            
            // CORS
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
            
        })
    .Build();
    
host.Run();