using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sprocit;
using Useit;

Generator.GetImplementationCode<IMySprocitTest>();


//var host = CreateHostBuilder(args).Build();

//// Resolve the service and run the application
//var app = host.Services.GetRequiredService<App>();
//app.Run();

//static IHostBuilder CreateHostBuilder(string[] args) =>
//            Host.CreateDefaultBuilder(args)
//                .ConfigureServices((context, services) =>
//                {
//                    // Register your services here
//                    services.AddTransient<App>();
//                    services.AddSprocit<IMySprocitTest>();
//                    // Add other services as needed
//                });