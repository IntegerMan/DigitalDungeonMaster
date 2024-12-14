using MattEland.DigitalDungeonMaster;
using MattEland.DigitalDungeonMaster.GameManagement.Services;
using MattEland.DigitalDungeonMaster.ServiceDefaults;
using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using Microsoft.AspNetCore.Mvc;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Load Configuration - TODO: Might not be best way of doing this with ASP .NET
IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .AddUserSecrets<Program>()
    .Build();

// Add services
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency Injection Configuration
builder.Services.AddScoped<StorageDataService>();
builder.Services.AddScoped<UserService>();

// Setting configuration options
builder.Services.Configure<AzureResourceConfig>(c => configuration.Bind("AzureResources", c));

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // TODO: Swagger documentation would be great here
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// API Endpoints
app.MapPost("/login", async ([FromBody] LoginBody login, [FromServices] UserService userService) =>
    {
        bool result = await userService.LoginAsync(login.Username, login.Password);   
        
        if (result)
        {
            return Results.Ok(); // TODO: Return a token
        }

        return Results.Unauthorized();
    })
    .WithName("Login")
    .AllowAnonymous()
    .WithOpenApi();

app.MapPost("/register", async ([FromBody] RegisterBody login, [FromServices] UserService userService) =>
    {
        try
        {
            await userService.RegisterAsync(login.Username, login.Password);

            // TODO: This would be better if we returned a token
            LoginBody routeValues = new LoginBody
            {
                Username = login.Username, 
                Password = login.Password
            };
            return Results.CreatedAtRoute("Login", routeValues);
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(ex.Message);
        }
    })
    .WithName("Register")
    .AllowAnonymous()
    .WithOpenApi();

app.MapDefaultEndpoints();

app.Run();
