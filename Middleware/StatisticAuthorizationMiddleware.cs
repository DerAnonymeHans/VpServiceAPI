using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace VpServiceAPI.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class StatisticAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public StatisticAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if(Environment.GetEnvironmentVariable("MODE") == "Testing")
            {
                await _next.Invoke(httpContext);
                return;
            }

            if (httpContext.Request.Path.Value is null)
            {
                await _next.Invoke(httpContext);
                return;
            }
            var path = httpContext.Request.Path.Value.Split('/');
            if (path.Length > 2)
            {
                if((path[1] == "Statistic" && path[2] == "Login") || path[1] != "Statistic")
                {
                    await _next.Invoke(httpContext);
                    return;
                }
            }
            else
            {
                await _next.Invoke(httpContext);
                return;
            }
            if (httpContext.Request.Cookies["statAuth"] == Environment.GetEnvironmentVariable("SITE_STAT_AUTH"))
            {
                await _next.Invoke(httpContext);
                return;
            }
            httpContext.Response.StatusCode = 401;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class StatisticAuthorizationMiddlewareExtensions
    {
        public static IApplicationBuilder UseStatisticAuthorizationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<StatisticAuthorizationMiddleware>();
        }
    }
}
