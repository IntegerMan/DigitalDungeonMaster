// See https://aka.ms/new-console-template for more information

using MattEland.BasementsAndBasilisks;
using Microsoft.Extensions.Configuration;

// Read the configuration from appsettings.json and user secrets
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddUserSecrets<Program>()
    .Build();

// Get the BasiliskConfig from the configuration using Binder
var basiliskConfig = new BasiliskConfig();
configuration.Bind(basiliskConfig);

Console.WriteLine("Hello, World: " + basiliskConfig.AzureOpenAiKey);

BasiliskKernel kernel = new(basiliskConfig.AzureOpenAiDeploymentName, 
    basiliskConfig.AzureOpenAiEndpoint, 
    basiliskConfig.AzureOpenAiKey);

string response = await kernel.ChatAsync("Hello, Basilisk!");

Console.WriteLine(response);