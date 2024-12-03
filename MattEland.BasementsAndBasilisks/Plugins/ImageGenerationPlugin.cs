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
    
    [KernelFunction, Description("Generates an image based on a short description and shows it to the player")]
    public async Task GenerateImageAsync(string description)
    {
        Context.LogPluginCall(description);

        ITextToImageService imageGen = Kernel!.GetRequiredService<ITextToImageService>();

        // Supported dimensions are 1792x1024, 1024x1024, 1024x1792 
        string result = await imageGen.GenerateImageAsync(description, 1792, 1024, kernel: Kernel);
        
        Context.AddBlock(new DiagnosticBlock {Header = "Image Generation", Metadata = result});
    }
}