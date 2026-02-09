using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using workplace.Application.Dtos;
using workplace.Application.Interfaces;
using workplace.Application.Services;
using workplace.Domain.Interfaces;
using workplace.Infrastructure.Interfaces;
using workplace.Infrastructure.Repositories;
using workplace.Infrastructure.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureOpenApi() 
    .ConfigureServices(services =>
        {
            services.AddApplicationInsightsTelemetryWorkerService();
            services.ConfigureFunctionsApplicationInsights();

            services.AddHttpContextAccessor();

            // AutoMapper
            services.AddAutoMapper(typeof(MapProfile));
            
            // Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPlaceRepository, PlaceRepository>();
            
            // Service
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IPlaceService, PlaceService>();
            services.AddScoped<IPlaceServiceInfrastructure, PlaceServiceInfrastructure>();
            
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