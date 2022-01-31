﻿using Common.Application;
using Common.WebAPI.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Common.WebAPI
{
    public static class CommonWebApiInstaller
    {
        public static void AddCacheServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
                options.InstanceName = "AuctionhouseCache";
            });
        }

        public static void UseIdTokenManager(this WebApplication webApplication)
        {
            webApplication.UseMiddleware<IdTokenManagerMiddleware>();
        }

        public static void UseIdTokenSlidingExpiration(this WebApplication webApplication)
        {
            webApplication.UseMiddleware<IdTokenSlidingExpirationMiddleware>();
        }

        public static void AddCommonJwtAuth(this IServiceCollection services, JwtSettings jwtConfig, AuthenticationBuilder authenticationBuilder)
        {
            services.AddTransient<IIdTokenManager, IdTokenManager>();
            services.AddSingleton(jwtConfig);
            services.AddTransient<JwtService>();
            services.AddTransient<IUserIdentityService, UserIdentityService>();
            authenticationBuilder.AddJwtBearer(jwtConfig.ConfigureJwt);
        }

        public static void AddSerilogLogging(this IServiceCollection services, IConfiguration configuration, string appName)
        {
            var config = new LoggerConfiguration();
            Log.Logger = config
                .ReadFrom.Configuration(configuration)
                .WriteTo.Console(outputTemplate: "[{SourceContext}] [{Level:u3}] {Message:lj}{Properties:j}{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}{NewLine}{Exception}")
                .Enrich.WithProperty("AppName", appName)
                .CreateLogger();

            services.AddLogging(b => b.AddSerilog(dispose: true));
        }
    }
}