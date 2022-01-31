﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Common.WebAPI.Auth
{

    public class JwtSettings
    {
        public string SymetricKey { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ExpireTimeSec { get; set; } = 60 * 5; //5MIN

        public void ConfigureJwt(JwtBearerOptions options)
        {
            options.Audience = Audience;
            options.TokenValidationParameters = TokenValidationParameters;
            options.Events = new JwtBearerEvents()
            {
                OnMessageReceived = context =>
                {
                    if (!context.HttpContext.EndpointRequiresAuthorization())
                    {
                        return Task.CompletedTask;
                    }

                    if (context.HttpContext.IsIdTokenDeactivated())
                    {
                        context.Token = null;
                    }
                    else
                    {
                        context.Token = context.Request.Cookies["IdToken"]; //TODO is bearer header ignored??
                    }
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    context.Response.Cookies.Delete("IdToken");
                    return Task.CompletedTask;
                }
            };
        }

        public TokenValidationParameters TokenValidationParameters =>
            new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidAudience = Audience,
                ValidateLifetime = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(SymetricKey)),
                ClockSkew = TimeSpan.Zero,
            };
    }
}