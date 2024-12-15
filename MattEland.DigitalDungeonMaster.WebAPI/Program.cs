using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MattEland.DigitalDungeonMaster.ServiceDefaults;
using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Services.Azure;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Routes;
using MattEland.DigitalDungeonMaster.WebAPI.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Dependency Injection Configuration
builder.Services.AddScoped<IStorageService, AzureStorageService>();
builder.Services.AddScoped<IUserService, AzureTableUserService>();

// Set up AI resources
    /*
    services.AddScoped<AzureOpenAIClient>(s =>
    {
        IOptionsSnapshot<AzureResourceConfig> config = s.GetRequiredService<IOptionsSnapshot<AzureResourceConfig>>();
        Uri endpoint = new Uri(config.Value.AzureOpenAiEndpoint);
        AzureKeyCredential credential = new(config.Value.AzureOpenAiKey);
        
        return new(endpoint, credential);
    });        
    services.AddScoped<AzureOpenAIChatCompletionService>(s =>
    {
        AzureOpenAIClient client = s.GetRequiredService<AzureOpenAIClient>();
        IOptionsSnapshot<AzureResourceConfig> config = s.GetRequiredService<IOptionsSnapshot<AzureResourceConfig>>();
        AzureOpenAIChatCompletionService chat = new(
            config.Value.AzureOpenAiChatDeploymentName,
            client);
        
        return chat;
    });        
    services.AddScoped<IChatCompletionService>(s =>
    {
        AzureOpenAIChatCompletionService chat = s.GetRequiredService<AzureOpenAIChatCompletionService>();
        return chat;
    });    
    services.AddScoped<ITextGenerationService>(s =>
    {
        AzureOpenAIChatCompletionService chat = s.GetRequiredService<AzureOpenAIChatCompletionService>();
        return chat;
    });    
    services.AddScoped<ITextToImageService>(s =>
    {
        AzureOpenAIClient client = s.GetRequiredService<AzureOpenAIClient>();
        IOptionsSnapshot<AzureResourceConfig> config = s.GetRequiredService<IOptionsSnapshot<AzureResourceConfig>>();
        return new AzureOpenAITextToImageService(config.Value.AzureOpenAiImageDeploymentName, client, null);
    });
    services.AddScoped<Kernel>(s =>
    {
        // Set up Semantic Kernel
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.Services.AddScoped<IChatCompletionService>(r => s.GetRequiredService<IChatCompletionService>());
        builder.Services.AddScoped<ITextToImageService>(r => s.GetRequiredService<ITextToImageService>());
        builder.Services.AddScoped<ITextGenerationService>(r => s.GetRequiredService<ITextGenerationService>());
        builder.Services.AddScoped<ILoggerFactory>(r => s.GetRequiredService<ILoggerFactory>());

        return builder.Build();
    });
    */

// Add configuration settings
IConfiguration configuration = builder.Configuration;
builder.Services.Configure<RegistrationSettings>(c => configuration.Bind("Registration", c));
builder.Services.Configure<AzureResourceConfig>(c => configuration.Bind("AzureResources", c));

// JWT settings
JwtSettings jwtSettings = builder.Configuration.GetJwtSettings();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });
builder.Services.AddAuthorization();

WebApplication app = builder.Build();

app.UseAuthentication(); 
app.UseAuthorization(); 

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // TODO: Swagger documentation would be great here
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Routes
app.AddLoginAndRegister();
app.MapDefaultEndpoints();

app.Run();
