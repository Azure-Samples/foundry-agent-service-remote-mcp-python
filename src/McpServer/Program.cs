using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Azure;
using Azure.Identity;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        // Add Azure Blob Storage client
        services.AddAzureClients(builder =>
        {
            builder.AddBlobServiceClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? "UseDevelopmentStorage=true");
            builder.UseCredential(new DefaultAzureCredential());
        });
    })
    .Build();

host.Run();
