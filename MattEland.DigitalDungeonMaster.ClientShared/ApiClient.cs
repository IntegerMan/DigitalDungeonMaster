using System.Net.Http.Headers;
using MattEland.DigitalDungeonMaster.Shared;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace MattEland.DigitalDungeonMaster.ClientShared;

public class ApiClient
{
    private readonly ILogger<ApiClient> _logger;
    private readonly HttpClient _client;

    public ApiClient(IHttpClientFactory client, ILogger<ApiClient> logger, IOptionsSnapshot<ApiClientOptions> options)
    {
        _logger = logger;
        _client = client.CreateClient();
        _client.BaseAddress = new(options.Value.BaseUrl);
        _client.DefaultRequestHeaders.Accept.Add(new("application/json"));
    }

    public bool IsAuthenticated => _client.DefaultRequestHeaders.Authorization is not null;
    public string Username { get; set; } = "Player";

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
                Username = username;
                
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

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Registered successfully as user {Username}", username);
                Username = username;
                success = true;
                
                StoreJwt(content);
            }
            else
            {
                _logger.LogWarning("Failed to register user {Username}", username);
                errorMessage = content;
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

    public async Task<IEnumerable<AdventureInfo>> LoadAdventuresAsync()
    {
        try
        {
            string json = await _client.GetStringAsync("adventures");
            
            return JsonConvert.DeserializeObject<IEnumerable<AdventureInfo>>(json) ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error occurred trying to load adventures");
            throw; // TODO: Better error handling needed
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timed out trying to load adventures");
            throw; // TODO: Better error handling needed
        }
    }

    public async Task<IEnumerable<Ruleset>> LoadRulesetsAsync()
    {
        try
        {
            string json = await _client.GetStringAsync("rulesets");
            
            return JsonConvert.DeserializeObject<IEnumerable<Ruleset>>(json) ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error occurred trying to load rulesets");
            throw; // TODO: Better error handling needed
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Timed out trying to load rulesets");
            throw; // TODO: Better error handling needed
        }
    }

    public async Task<ChatResult<NewGameSettingInfo>> StartWorldBuilderConversationAsync(AdventureInfo adventure)
    {
        string? errorMessage;
        try
        {
            _logger.LogDebug("Starting chat with world builder for adventure {Adventure}", adventure.RowKey);
            HttpResponseMessage response = await _client.PostAsync("adventures", content: CreateJsonContent(adventure));

            if (response.IsSuccessStatusCode)
            {
                return await ReadChatResult<NewGameSettingInfo>(response);
            }

            errorMessage = $"Failed to start chat with world builder. Server returned status code {response.StatusCode}";
            _logger.LogError("Failed to start chat with world builder for adventure {Adventure}", adventure.RowKey);
        }
        catch (HttpRequestException ex)
        {
            errorMessage = "Network error occurred trying to chat with the world builder";
            _logger.LogError(ex, "Network error occurred trying to chat with the world builder");
        }                
        catch (TaskCanceledException ex)
        {
            errorMessage = "Timed out trying to chat with the world builder";
            _logger.LogError(ex, "Timed out trying to chat with the world builder");
        }

        return new ChatResult<NewGameSettingInfo>
        {
            IsError = true,
            ErrorMessage = errorMessage,
            Id = Guid.Empty,
            Data = null,
            Replies = [
                new ChatMessage
                {
                    Author = "Error Handler",
                    Message = errorMessage
                }
            ]
        };
    }

    public async Task<ChatResult<NewGameSettingInfo>> ChatWithWorldBuilderAsync(ChatRequest<NewGameSettingInfo> chatRequest, AdventureInfo adventure)
    {
        string? errorMessage;
        try
        {
            _logger.LogDebug("Continuing chat {id} with world builder for adventure {Adventure}: {Message}", chatRequest.Id, adventure.RowKey, chatRequest.Message);
            HttpResponseMessage response = await _client.PostAsync($"adventures/{adventure.RowKey}/builder/{chatRequest.Id}", content: CreateJsonContent(chatRequest));

            if (response.IsSuccessStatusCode)
            {
                return await ReadChatResult<NewGameSettingInfo>(response);
            }

            errorMessage = $"Failed to chat with world builder. Server returned status code {response.StatusCode}";
            _logger.LogError("Failed to chat with world builder for adventure {Adventure}", adventure.RowKey);
        }
        catch (HttpRequestException ex)
        {
            errorMessage = "Network error occurred trying to chat with the world builder";
            _logger.LogError(ex, "Network error occurred trying to chat with the world builder");
        }                
        catch (TaskCanceledException ex)
        {
            errorMessage = "Timed out trying to chat with the world builder";
            _logger.LogError(ex, "Timed out trying to chat with the world builder");
        }

        return new ChatResult<NewGameSettingInfo>
        {
            IsError = true,
            ErrorMessage = errorMessage,
            Id = chatRequest.Id!.Value,
            Data = chatRequest.Data,
            Replies = [
                new ChatMessage
                {
                    Author = "Error Handler",
                    Message = errorMessage
                }
            ]
        };
    }

    public async Task<IChatResult> StartGameMasterConversationAsync(string adventureName)
    {
        string? errorMessage;
        try
        {
            _logger.LogDebug("Starting chat with game master for adventure {Adventure}", adventureName);
            HttpResponseMessage response = await _client.PostAsync($"adventures/{adventureName}", content: null);

            if (response.IsSuccessStatusCode)
            {
                return await ReadChatResult<object>(response);
            }
            
            errorMessage = $"Failed to start chat with game master. Server returned status code {response.StatusCode}";
            _logger.LogError("Failed to start chat with game master for adventure {Adventure}", adventureName);
        }
        catch (HttpRequestException ex)
        {
            errorMessage = "Network error occurred trying to chat with the game master";
            _logger.LogError(ex, "Network error occurred trying to chat with the game master");
        }                
        catch (TaskCanceledException ex)
        {
            errorMessage = "Timed out trying to chat with the game master";
            _logger.LogError(ex, "Timed out trying to chat with the game master");
        }

        return new ChatResult<object>
        {
            IsError = true,
            ErrorMessage = errorMessage,
            Id = Guid.Empty,
            Replies = [
                new ChatMessage
                {
                    Author = "Error Handler",
                    Message = errorMessage
                }
            ]
        };
    }

    private async Task<ChatResult<TData>> ReadChatResult<TData>(HttpResponseMessage response)
    {
        string json = await response.Content.ReadAsStringAsync();

        _logger.LogDebug("Chat Response: {Response} {Content}", response, json);
        ChatResult<TData> result = JsonConvert.DeserializeObject<ChatResult<TData>>(json)!;
        
        return result;
    }

    public async Task<IChatResult> ChatWithGameMasterAsync(IChatRequest chatRequest, string adventureName)
    {
        string? errorMessage;
        try
        {
            _logger.LogDebug("Sending to {Bot}: {Message} ({ConversationId})", chatRequest.RecipientName, chatRequest.Message, chatRequest.Id);
            HttpResponseMessage response = await _client.PostAsync($"adventures/{adventureName}/{chatRequest.Id}", CreateJsonContent(chatRequest));

            if (response.IsSuccessStatusCode)
            {
                return await ReadChatResult<object>(response);
            }
            
            errorMessage = $"Failed to chat with the game master. Server returned status code {response.StatusCode}";
            _logger.LogError("Failed to chat with the game master for adventure {Adventure}", adventureName);
        }
        catch (HttpRequestException ex)
        {
            errorMessage = "Network error occurred trying to chat with the game master";
            _logger.LogError(ex, "Network error occurred trying to chat with the game master");
        }                
        catch (TaskCanceledException ex)
        {
            errorMessage = "Timed out trying to chat with the game master";
            _logger.LogError(ex, "Timed out trying to chat with the game master");
        }

        return new ChatResult<object>
        {
            IsError = true,
            ErrorMessage = errorMessage,
            Id = chatRequest.Id.GetValueOrDefault(),
            Replies = [
                new ChatMessage
                {
                    Author = "Error Handler",
                    Message = errorMessage
                }
            ]
        };
    }
    
    private void StoreJwt(string content)
    {
        _logger.LogDebug("Storing JWT for subsequent requests");
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", content);
    }

    public void Logout()
    {
        _logger.LogDebug("Cleared stored JWT");
        _client.DefaultRequestHeaders.Authorization = null;
    }
}