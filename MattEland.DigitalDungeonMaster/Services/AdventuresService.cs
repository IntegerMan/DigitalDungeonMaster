using Azure.Data.Tables;
using MattEland.DigitalDungeonMaster.Models;

namespace MattEland.DigitalDungeonMaster.Services;

public class AdventuresService
{
    private readonly StorageDataService _storageService;
    private readonly RequestContextService _context;

    public AdventuresService(StorageDataService storageService, RequestContextService context)
    {
        _storageService = storageService;
        _context = context;
    }
    
    public async Task CreateAdventureAsync(AdventureInfo adventure)
    {
        // Add an entry to table storage
        await _storageService.CreateTableEntryAsync("adventures", new TableEntity
        {
            PartitionKey = adventure.Owner,
            RowKey = adventure.RowKey,
            ["Name"] = adventure.Name,
            ["Description"] = adventure.Description,
            ["Container"] = adventure.Container,
            ["Ruleset"] = adventure.Ruleset
        });
        
        // Set our current adventure to this adventure
        _context.CurrentAdventure = adventure;
    }
}