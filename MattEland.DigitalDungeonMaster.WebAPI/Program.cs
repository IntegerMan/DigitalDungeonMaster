using System.Text;
using Azure;
using Azure.AI.OpenAI;
using MattEland.DigitalDungeonMaster;
using MattEland.DigitalDungeonMaster.Agents.GameMaster;
using MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;
using MattEland.DigitalDungeonMaster.Agents.WorldBuilder;
using MattEland.DigitalDungeonMaster.ServiceDefaults;
using MattEland.DigitalDungeonMaster.Services;
using MattEland.DigitalDungeonMaster.Services.Azure;
using MattEland.DigitalDungeonMaster.WebAPI.Models;
using MattEland.DigitalDungeonMaster.WebAPI.Routes;
using MattEland.DigitalDungeonMaster.WebAPI.Services;
using MattEland.DigitalDungeonMaster.WebAPI.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.TextGeneration;
using Microsoft.SemanticKernel.TextToImage;

#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Auth
builder.Services.AddScoped<TokenService>();
builder.Services.AddTransient<AppUser>();
builder.Services.AddHttpContextAccessor();

// Dependency Injection Configuration
builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<IRecordStorageService, AzureTableStorageService>();
builder.Services.AddScoped<IUserService, AzureTableUserService>();
builder.Services.AddScoped<AdventuresService>();
builder.Services.AddScoped<ChatService>();

// Set up AI resources
builder.Services.AddScoped<AzureOpenAIClient>(s =>
{
    IOptionsSnapshot<AzureResourceConfig> config = s.GetRequiredService<IOptionsSnapshot<AzureResourceConfig>>();
    Uri endpoint = new Uri(config.Value.AzureOpenAiEndpoint);
    AzureKeyCredential credential = new(config.Value.AzureOpenAiKey);

    return new(endpoint, credential);
});
builder.Services.AddScoped<AzureOpenAIChatCompletionService>(s =>
{
    AzureOpenAIClient client = s.GetRequiredService<AzureOpenAIClient>();
    IOptionsSnapshot<AzureResourceConfig> config = s.GetRequiredService<IOptionsSnapshot<AzureResourceConfig>>();
    AzureOpenAIChatCompletionService chat = new(
        config.Value.AzureOpenAiChatDeploymentName,
        client);

    return chat;
});
builder.Services.AddScoped<IChatCompletionService>(s =>
{
    AzureOpenAIChatCompletionService chat = s.GetRequiredService<AzureOpenAIChatCompletionService>();
    return chat;
});
builder.Services.AddScoped<ITextGenerationService>(s =>
{
    AzureOpenAIChatCompletionService chat = s.GetRequiredService<AzureOpenAIChatCompletionService>();
    return chat;
});
builder.Services.AddScoped<ITextToImageService>(s =>
{
    AzureOpenAIClient client = s.GetRequiredService<AzureOpenAIClient>();
    IOptionsSnapshot<AzureResourceConfig> config = s.GetRequiredService<IOptionsSnapshot<AzureResourceConfig>>();
    return new AzureOpenAITextToImageService(config.Value.AzureOpenAiImageDeploymentName, client, null);
});
builder.Services.AddScoped<Kernel>(s =>
{
    // Set up Semantic Kernel
    IKernelBuilder kb = Kernel.CreateBuilder();
    kb.Services.AddScoped<IChatCompletionService>(_ => s.GetRequiredService<IChatCompletionService>());
    kb.Services.AddScoped<ITextToImageService>(_ => s.GetRequiredService<ITextToImageService>());
    kb.Services.AddScoped<ITextGenerationService>(_ => s.GetRequiredService<ITextGenerationService>());
    kb.Services.AddScoped<ILoggerFactory>(_ => s.GetRequiredService<ILoggerFactory>());

    return kb.Build();
});
builder.Services.AddScoped<GameMasterAgent>();
builder.Services.AddScoped<WorldBuilderAgent>();
builder.Services.AddScoped<AgentConfigurationService>();
builder.Services.AddScoped<RequestContextService>();
builder.Services.AddScoped<LocationGenerationService>();
builder.Services.AddScoped<RandomService>();
builder.Services.AddScoped<RulesetService>();

// Add middleware to log every request and response
builder.Services.AddLogging(b =>
{
    b.AddConsole();
    b.AddDebug();
});

// Add configuration settings
IConfiguration configuration = builder.Configuration;
builder.Services.Configure<RegistrationSettings>(c => configuration.Bind("Registration", c));
builder.Services.Configure<AzureResourceConfig>(c => configuration.Bind("AzureResources", c));
builder.Services.Configure<AgentConfig>("Game Master", c => configuration.Bind("Agents:GameMaster", c));
builder.Services.Configure<AgentConfig>("World Builder", c => configuration.Bind("Agents:WorldBuilder", c));
builder.Services.Configure<JwtSettings>(c => configuration.Bind("JwtSettings", c));

// Authentication - TODO: This is ugly and belongs in an extension method somewhere
JwtSettings jwtSettings = builder.Configuration.GetRequiredSection("JwtSettings").Get<JwtSettings>()!;
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
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Routes
app.AddLoginAndRegisterEndpoints();
app.AddWorldBuilderEndpoints();
app.AddAdventureEndpoints();
app.AddRulesetsEndpoints();
app.MapDefaultEndpoints();

app.Run();