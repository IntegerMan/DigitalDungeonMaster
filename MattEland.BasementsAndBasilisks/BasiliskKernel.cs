namespace MattEland.BasementsAndBasilisks;

using Microsoft.SemanticKernel;

public class BasiliskKernel
{
    private readonly Kernel _kernel;

    public BasiliskKernel(string openAiDeploymentName, string openAiEndpoint, string openAiApiKey)
    {
        IKernelBuilder builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(openAiDeploymentName, 
            openAiEndpoint, 
            openAiApiKey);

        _kernel = builder.Build();
    }
}