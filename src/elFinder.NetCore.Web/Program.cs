using System;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace elFinder.NetCore.Web
{
    public class Program
    {
        /// <summary>
        /// The main entry-point to the application.
        /// </summary>
        /// <param name="args">The arguments to the application.</param>
        /// <returns>
        /// The exit code from the application.
        /// </returns>
        public static int Main(string[] args) => Run(args);

        /// <summary>
        /// Runs ths application.
        /// </summary>
        /// <param name="args">The arguments to the application.</param>
        /// <param name="cancellationToken">The optional cancellation token to use.</param>
        /// <returns>
        /// The exit code from the application.
        /// </returns>
        public static int Run(string[] args, CancellationToken cancellationToken = default(CancellationToken))
        {
            try
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                    .AddJsonFile("hosting.json", true)
                    .AddEnvironmentVariables()
                    .Build();

                var builder = new WebHostBuilder()
                    .UseKestrel((p) => p.AddServerHeader = false)
                    .UseConfiguration(configuration)
                    .UseContentRoot(System.IO.Directory.GetCurrentDirectory())
                    .UseIISIntegration()
                    .UseStartup<Startup>()
                    .CaptureStartupErrors(true);

                using (var host = builder.Build())
                {
                    using (var tokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                    {
                        Console.CancelKeyPress += (_, e) =>
                        {
                            tokenSource.Cancel();
                            e.Cancel = true;
                        };

                        host.Run(tokenSource.Token);
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unhandled exception: {ex}");
                return -1;
            }
        }
    }
}