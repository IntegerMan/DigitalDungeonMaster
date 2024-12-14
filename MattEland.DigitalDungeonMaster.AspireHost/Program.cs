var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.MattEland_DigitalDungeonMaster_WebAPI>("WebAPI");

builder.Build().Run();