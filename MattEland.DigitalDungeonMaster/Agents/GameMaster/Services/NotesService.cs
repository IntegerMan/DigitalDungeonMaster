using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;

public class NotesService(IRecordStorageService recordStorage)
{
    private const string TableName = "storytellernotes";

    public async Task AddPrivateNoteAsync(string username, string adventure, string note)
    {
        await recordStorage.UpsertAsync(TableName, new Dictionary<string, object?>
        {
            { "PartitionKey", $"{username}_{adventure}" },
            { "RowKey", Guid.NewGuid().ToString() },
            { "Note", note }
        });
    }

    public async Task<IEnumerable<string>> GetPrivateNotesAsync(string username, string adventure)
    {
        return await recordStorage.GetPartitionedDataAsync(TableName,
            $"{username}_{adventure}", row => (string)row["Note"]!);
    }
}