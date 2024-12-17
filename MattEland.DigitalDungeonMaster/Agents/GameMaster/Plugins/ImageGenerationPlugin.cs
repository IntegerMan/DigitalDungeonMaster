using MattEland.DigitalDungeonMaster.Services;
using Microsoft.SemanticKernel.TextToImage;

#pragma warning disable SKEXP0001

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[Description("A plugin that generates images based on text descriptions")]
public class ImageGenerationPlugin
{
    private readonly RequestContextService _context;
    private readonly ILogger<ImageGenerationPlugin> _logger;
    private readonly ITextToImageService _imageService;

    // TODO: When we're in the web or desktop, we won't need to download so an IOptions might be good here on download behavior
    public ImageGenerationPlugin(RequestContextService context, ILogger<ImageGenerationPlugin> logger, ITextToImageService imageService) 
    {
        _context = context;
        _logger = logger;
        _imageService = imageService;
    }
    
    [KernelFunction(nameof(GenerateImageAsync)), 
     Description("Generates an image based on a short description and shows it to the player")]
    public async Task<string> GenerateImageAsync(string description)
    {
        _logger.LogDebug("{Plugin}-{Method} called with {Description}", nameof(ImageGenerationPlugin), nameof(GenerateImageAsync), description);

        // Supported dimensions are 1792x1024, 1024x1024, 1024x1792 for DALL-E-3, only 1024x1024 works for DALL-E-2
        string imageUrl;
        try
        {
            imageUrl = await _imageService.GenerateImageAsync(description, 1024, 1024);
            _logger.LogDebug("Generated Image: {ImageUrl}", imageUrl);
        }
        catch (HttpOperationException ex)
        {
            if (ex.Message.Contains("content_policy"))
            {
                _logger.LogWarning(ex, "Image Prompt Flagged as Inappropriate: {Prompt}", description);
                
                return "That image prompt was flagged as being potentially inappropriate.";
            }
            
            _logger.LogError(ex, "Error Generating Image: {Message}", ex.Message);

            return "The system encountered an error generating an image.";
        }
        
        _logger.LogInformation("Image generated at {Url}", imageUrl);
        
        // TODO: Will need some way for this to get back to the kernel
        
        return $"Image generated at {imageUrl}";
    }
}