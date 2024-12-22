var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.MattEland_DigitalDungeonMaster_WebAPI>("WebAPI");

builder.AddProject<Projects.MattEland_DigitalDungeonMaster_ConsoleApp>("ConsoleApp")
    .WithArgs("--aspire") // Tells the console app to use the Aspire API endpoint instead of the server
    .WithReference(api);

builder.AddProject<Projects.MattEland_DigitalDungeonMaster_AvaloniaApp>("AvaloniaApp")
    .WithArgs("--aspire") // Tells the Avalonia app to use the Aspire API endpoint instead of the server
    .WithReference(api);

builder.Build().Run();