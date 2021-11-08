﻿namespace Exemplum.Application;

using Common.Behaviour;
using Common.Exceptions;
using Common.Exceptions.Converters;
using Common.Security;
using FluentValidation;
using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using WeatherForecasts;
using WeatherForecasts.Models;
using WeatherForecasts.Query;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddMediatR(Assembly.GetExecutingAssembly());

        services.AddTransient<IExceptionToErrorConverter, ExceptionToErrorConverter>();
        services.AddTransient<ICustomExceptionErrorConverter, RefitApiExceptionConverter>();
        services.AddTransient<ICustomExceptionErrorConverter, UnauthorizedAccessExceptionConverter>();

        services.AddTransient(typeof(IRequestPreProcessor<>), typeof(LoggingBehaviour<>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        services.AddTransient(typeof(IPipelineBehavior<GetWeatherForecastQuery, WeatherForecast>),
            typeof(CacheForecastBehaviour));

        services.Configure<WeatherForecastOptions>(configuration.GetSection(WeatherForecastOptions.Section));

        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy(Security.Policy.TodoWriteAccess,
                policy => policy.RequireClaim(Security.ClaimTypes.Permission, Security.Permissions.WriteTodo));
            options.AddPolicy(Security.Policy.TodoDeleteAccess,
                policy => policy.RequireClaim(Security.ClaimTypes.Permission, Security.Permissions.DeleteTodo));
        });

        return services;
    }
}