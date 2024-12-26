using System.Diagnostics;
using MattEland.DigitalDungeonMaster.Services;
using Microsoft.SemanticKernel.TextToImage;

#pragma warning disable SKEXP0001

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Plugins;

[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "This is invoked by Semantic Kernel as a plugin")]
[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Instantiated via Reflection")]
[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global", Justification = "Instantiated via Dependency Injection")]
[Description("A plugin that generates images based on text descriptions")]
public class ImageGenerationPlugin(
    RequestContextService context,
    ILogger<ImageGenerationPlugin> logger,
    ITextToImageService imageService)
    : PluginBase(logger)
{
    private readonly RequestContextService _context = context;

    // TODO: When we're in the web or desktop, we won't need to download so an IOptions might be good here on download behavior

    [KernelFunction(nameof(GenerateImageAsync)), 
     Description("Generates an image based on a short description and shows it to the player")]
    public async Task<string> GenerateImageAsync(string description)
    {
        using Activity? activity = LogActivity($"Description: {description}");

        // Supported dimensions are 1792x1024, 1024x1024, 1024x1792 for DALL-E-3, only 1024x1024 works for DALL-E-2
        string imageUrl;
        try
        {
            imageUrl = await imageService.GenerateImageAsync(description, 1024, 1024);
            Logger.LogDebug("Generated Image: {ImageUrl}", imageUrl);
        }
        catch (HttpOperationException ex)
        {
            if (ex.Message.Contains("content_policy"))
            {
                Logger.LogWarning(ex, "Image Prompt Flagged as Inappropriate: {Prompt}", description);
                
                return "That image prompt was flagged as being potentially inappropriate.";
            }
            
            Logger.LogError(ex, "Error Generating Image: {Message}", ex.Message);

            return "The system encountered an error generating an image.";
        }
        
        Logger.LogInformation("Image generated at {Url}", imageUrl);
        activity?.AddTag("Url", imageUrl);
        
        // TODO: Will need some way for this to get back to the kernel
        
        return $"Image generated at {imageUrl}";
    }
}