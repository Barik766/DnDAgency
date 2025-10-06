using DnDAgency.EmailWorker;
using DnDAgency.EmailWorker.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.AddSingleton<IEmailService, EmailService>();
    })
    .Build();

await host.RunAsync();