using System.Net.Http.Headers;
using MattEland.DigitalDungeonMaster.GameManagement.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace MattEland.DigitalDungeonMaster.ConsoleApp;

public class ApiClient
{
    private readonly ILogger<ApiClient> _logger;
    private readonly HttpClient _client;

    public ApiClient(IHttpClientFactory client, ILogger<ApiClient> logger)
    {
        _logger = logger;
        _client = client.CreateClient();
        _client.BaseAddress = new("https+http://WebAPI");
        _client.DefaultRequestHeaders.Accept.Add(new("application/json"));
    }

    public async Task<ApiResult> LoginAsync(string username, string password)
    {
        string? errorMessage = null;
        bool success = false;
        try
        {
            // Create HttpContent with JSON Content for the user
            HttpResponseMessage response = await _client.PostAsync("login", CreateJsonContent(new
            {
                Username = username,
                Password = password
            }));

            string content = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync())!;
            
            _logger.LogDebug("Login response: {Response} {Content}", response, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Logged in successfully as user {Username}", username);
                success = true;
                
                // Set the JWT into the client
                StoreJwt(content);
            }
            else
            {
                errorMessage = content;
                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"Failed to log in. Server returned status code {response.StatusCode}";
                }
                _logger.LogWarning("Failed to log in user {Username}", username);
            }
        }
        catch (HttpRequestException ex)
        {
            errorMessage = "Network error occurred trying to log in user. Please try again.";
            _logger.LogError(ex, "Network error occurred trying to log in user {Username}", username);
        }
        catch (TaskCanceledException ex)
        {
            errorMessage = "Timed out trying to log in user. Please try again.";
            _logger.LogError(ex, "Timed out trying to log in user {Username}", username);
        }

        return new ApiResult
        {
            Success = success,
            ErrorMessage = errorMessage
        };
    }

    private void StoreJwt(string content)
    {
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content);
    }

    public async Task<ApiResult> RegisterAsync(string username, string password)
    {
        bool success = false;
        string? errorMessage = null;
        try
        {
            HttpResponseMessage response = await _client.PostAsync("register", CreateJsonContent(new
            {
                Username = username,
                Password = password
            }));

            string content = JsonConvert.DeserializeObject<string>(await response.Content.ReadAsStringAsync())!;
            
            _logger.LogDebug("Register response: {Response} {Content}", response, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Registered successfully as user {Username}", username);
                
                success = true;
                
                StoreJwt(content);
            }
            else
            {
                _logger.LogWarning("Failed to register user {Username}", username);
                errorMessage = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    errorMessage = $"Failed to create account. Server returned status code {response.StatusCode}";
                }
            }
        }
        catch (HttpRequestException ex)
        {
            errorMessage = "Network error occurred trying to register user. Please try again.";
            _logger.LogError(ex, "Network error occurred trying to register user {Username}", username);
        }                
        catch (TaskCanceledException ex)
        {
            errorMessage = "Timed out trying to register user. Please try again.";
            _logger.LogError(ex, "Timed out trying to register user {Username}", username);
        }
        
        return new ApiResult
        {
            Success = success,
            ErrorMessage = errorMessage
        };
    }
    
    private static HttpContent CreateJsonContent(object payload)
    {
        HttpContent content = new StringContent(JsonConvert.SerializeObject(payload));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        
        return content;
    }

    public async Task<IEnumerable<AdventureInfo>> LoadAdventuresAsync(string username)
    {
        try
        {
            string json = await _client.GetStringAsync("adventures");
            
            return JsonConvert.DeserializeObject<IEnumerable<AdventureInfo>>(json) ?? Enumerable.Empty<AdventureInfo>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error occurred trying to register user {Username}", username);
            throw; // TODO: Better error handling needed
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timed out trying to register user {Username}", username);
            throw; // TODO: Better error handling needed
        }
    }

    public async Task<IEnumerable<Ruleset>> LoadRulesetsAsync(string username)
    {
        throw new NotImplementedException();
        
        await Task.CompletedTask;
    }

    public async Task<ChatResult> StartWorldBuilderConversationAsync()
    {
        throw new NotImplementedException();
        
        await Task.CompletedTask;
    }

    public async Task<ChatResult> ChatWithWorldBuilderAsync(ChatRequest chatRequest)
    {
        throw new NotImplementedException();
        
        await Task.CompletedTask;
    }

    public async Task<ChatResult> StartGameMasterConversationAsync(string username, string adventureName)
    {
        throw new NotImplementedException();
        
        await Task.CompletedTask;
    }

    public async Task<ChatResult?> ChatWithGameMasterAsync(ChatRequest chatRequest)
    {
        throw new NotImplementedException();
        
        await Task.CompletedTask;
    }
}