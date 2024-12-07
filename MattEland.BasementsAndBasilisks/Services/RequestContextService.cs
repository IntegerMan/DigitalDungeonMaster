using System.Runtime.CompilerServices;
using MattEland.BasementsAndBasilisks.Blocks;
using MattEland.BasementsAndBasilisks.Models;

namespace MattEland.BasementsAndBasilisks.Services;

public class RequestContextService
{
    private readonly List<ChatBlockBase> _blocks = new();
    
    public void AddBlock(ChatBlockBase block)
    {
        _blocks.Add(block);
    }

    public IEnumerable<ChatBlockBase> Blocks => _blocks.AsReadOnly();
    public string? CurrentRuleset => CurrentAdventure?.Ruleset;
    public string? CurrentUser { get; set; }
    public string? CurrentAdventureId => CurrentAdventure?.RowKey;
    public AdventureInfo? CurrentAdventure { get; set; }

    public void BeginNewRequest(string message, bool clear)
    {
        if (clear)
        {
            ClearBlocks();
        }

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

    public void ClearBlocks()
    {
        _blocks.Clear();
    }

    public void Logout()
    {
        CurrentUser = null;
        CurrentAdventure = null;
    }
}