﻿using Jp.Api.Management.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.Threading.Tasks;

namespace Jp.Api.Management
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "JP Project - Api Management";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(@"jpProject_sso_log-.txt", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 5)
                .WriteTo.Console()
                .CreateLogger();

            var host = CreateHostBuilder(args).Build();

            Task.WaitAll(DatabaseChecker.EnsureDatabaseIsReady(host.Services.CreateScope()));

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureLogging(builder =>
                    {
                        builder.ClearProviders();
                        builder.AddSerilog();
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}

