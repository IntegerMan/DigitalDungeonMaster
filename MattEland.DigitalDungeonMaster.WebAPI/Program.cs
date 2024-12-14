using MattEland.DigitalDungeonMaster;
using MattEland.DigitalDungeonMaster.GameManagement.Services;
using MattEland.DigitalDungeonMaster.ServiceDefaults;
using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Services.Azure;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency Injection Configuration
builder.Services.AddScoped<IStorageService, AzureStorageService>();
builder.Services.AddScoped<IUserService, AzureTableUserService>();

// Add configuration settings
IConfiguration configuration = builder.Configuration;
builder.Services.Configure<RegistrationSettings>(c => configuration.Bind("Registration", c));
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
app.MapPost("/login", async ([FromBody] LoginBody login, [FromServices] IUserService userService) =>
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

app.MapPost("/register", async ([FromBody] RegisterBody login, 
        [FromServices] IUserService userService, 
        [FromServices] IOptionsSnapshot<RegistrationSettings> registrationSettings) =>
    {
        try
        {
            // Prevent registration if it's disabled
            if (registrationSettings.Value.AllowRegistration == false)
            {
                return Results.BadRequest("Registration is currently disabled. Please contact the administrator for access.");
            }
            
            // Actually register
            await userService.RegisterAsync(login.Username, login.Password);

            return Results.CreatedAtRoute("Login");
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
