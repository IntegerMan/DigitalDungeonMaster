using System.Net;
using MattEland.BasementsAndBasilisks.Blocks;
using MattEland.BasementsAndBasilisks.Services;
using Microsoft.SemanticKernel.TextToImage;

#pragma warning disable SKEXP0001

namespace MattEland.BasementsAndBasilisks.Plugins;

[Description("A plugin that generates images based on text descriptions")]
[BasiliskPlugin(PluginName = "ImageGenerationPlugin")]
public class ImageGenerationPlugin : BasiliskPlugin
{
    public ImageGenerationPlugin(RequestContextService context) : base(context)
    {
        
    }
    
    [KernelFunction(nameof(GenerateImageAsync)), 
     Description("Generates an image based on a short description and shows it to the player")]
    public async Task<string> GenerateImageAsync(string description)
    {
        Context.LogPluginCall(description);

        ITextToImageService imageGen = Kernel!.GetRequiredService<ITextToImageService>();

        // Supported dimensions are 1792x1024, 1024x1024, 1024x1792 for DALL-E-3, only 1024x1024 works for DALL-E-2
        string imageUrl;
        try
        {
            imageUrl = await imageGen.GenerateImageAsync(description, 1024, 1024, kernel: Kernel);
            // Context.AddBlock(new DiagnosticBlock {Header = "Generated Image", Metadata = imageUrl});
        }
        catch (HttpOperationException ex)
        {
            if (ex.Message.Contains("content_policy"))
            {
                return "That image prompt was flagged as being potentially inappropriate.";
            }
            
            Context.AddBlock(new DiagnosticBlock {Header = "Error Generating Image: " + ex.GetType().Name, Metadata = ex.Message});
            return "Error generating image: " + ex.Message;
        }
        
        // Open a stream from the URL
        string localFile = Path.ChangeExtension(Path.Combine(Environment.CurrentDirectory, Path.GetRandomFileName()), ".png");
        using (WebClient client = new())
        {
            await client.DownloadFileTaskAsync(new Uri(imageUrl), localFile);
        }
        
        Context.AddBlock(new ImageBlock(localFile, description));
        
        return $"Image generated and saved to disk at {localFile}";
    }
}