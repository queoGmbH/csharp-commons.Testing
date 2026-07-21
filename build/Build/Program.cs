using Build.Common.Extensions;

using Cake.Frosting;

using Microsoft.Extensions.DependencyInjection;

namespace Build
{
    public class Program : IFrostingStartup
    {
        /// <summary>Configures services used by Cake.</summary>
        /// <param name="services">The services to configure.</param>
        public void Configure(IServiceCollection services)
        {
            // Register the tools
            services.UseTool("nuget:?package=NuGet.CommandLine".AsUri());
            services.UseTool("dotnet:?package=GitVersion.Tool".AsUri());
            services.UseTool("nuget:?package=NUnit.ConsoleRunner".AsUri());
            services.UseTool("nuget:?package=ReportGenerator&version=5.1.26".AsUri());
        }

        public static int Main(string[] args) =>
            // Create the host.
            new CakeHost()
                .UseStartup<Program>()
                .UseContext<Context>()
                .UseLifetime<Lifetime>()
                .UseWorkingDirectory("../..")
                .Run(args);
    }
}
