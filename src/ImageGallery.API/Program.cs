using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;

namespace ImageGallery.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
                //.Enrich.With(new LogEnricher())
                .WriteTo.Seq("http://192.168.99.100:5341"/*, apiKey: "MyAppsApiKey"*/)
                .CreateLogger();

            Log.Information("Starting - Image API Web Host");
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();
    }
}
