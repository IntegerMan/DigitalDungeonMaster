IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<ProjectResource> api = builder.AddProject<Projects.MattEland_DigitalDungeonMaster_WebAPI>("WebAPI");

// This lets you choose the console app over the Avalonia app if you prefer
bool useConsoleApp = false;

if (useConsoleApp)
{
    builder.AddProject<Projects.MattEland_DigitalDungeonMaster_ConsoleApp>("ConsoleApp")
        .WithArgs("--aspire") // Tells the console app to use the Aspire API endpoint instead of the server
        .WithReference(api);
}
else
{
    builder.AddProject<Projects.MattEland_DigitalDungeonMaster_AvaloniaApp>("AvaloniaApp")
        .WithArgs("--aspire") // Tells the Avalonia app to use the Aspire API endpoint instead of the server
        .WithReference(api);
}

builder.Build().Run();