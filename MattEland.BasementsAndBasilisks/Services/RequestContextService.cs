using System.Runtime.CompilerServices;
using MattEland.BasementsAndBasilisks.Blocks;

namespace MattEland.BasementsAndBasilisks.Services;

public class RequestContextService
{
    private readonly List<ChatBlockBase> _blocks = new();
    
    public void AddBlock(ChatBlockBase block)
    {
        _blocks.Add(block);
    }

    public IEnumerable<ChatBlockBase> Blocks => _blocks.AsReadOnly();
    public string CurrentRuleset { get; set; } = "dnd5e"; // TODO: This should be set after setting adventure info
    public string CurrentUser { get; set; } = "meland"; // TODO: Should come from login
    public string CurrentAdventureName { get; set; } = "unknownlands"; // TODO: Should be set after setting adventure info

    public void BeginNewRequest(string message)
    {
        _blocks.Clear();
        _blocks.Add(new MessageBlock
        {
            Message = message,
            IsUserMessage = true,
        });
    }

    public void LogPluginCall(string? metadata = null, [CallerMemberName] string caller = "")
    {
        AddBlock(new DiagnosticBlock
        {
            Header = $"{caller} Plugin Called",
            Metadata = metadata
        });
    }
}