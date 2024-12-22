var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.MattEland_DigitalDungeonMaster_WebAPI>("WebAPI");

builder.AddProject<Projects.MattEland_DigitalDungeonMaster_ConsoleApp>("ConsoleApp")
    .WithReference(api);

builder.AddProject<Projects.MattEland_DigitalDungeonMaster_AvaloniaApp>("AvaloniaApp")
    .WithReference(api);

builder.Build().Run();