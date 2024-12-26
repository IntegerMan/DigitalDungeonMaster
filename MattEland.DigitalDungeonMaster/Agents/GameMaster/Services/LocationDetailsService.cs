using MattEland.DigitalDungeonMaster.Agents.GameMaster.Models;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;

public class LocationDetailsService(IRecordStorageService recordStorage)
{
    private const string TableName = "locations";

    public async Task<LocationDetails?> GetDetailsOrDefaultAsync(string username, string adventure, LocationInfo location)
    {
        LocationDetails? details = await recordStorage.FindByKeyAsync<LocationDetails>(TableName, 
            $"{username}_{adventure}", 
            location.ToString(),
            row => new LocationDetails
        {
            Location = location,
            Name = (string)row["Name"]!,
            Description = row["Description"] as string,
            GameHistory = row["GameHistory"] as string,
            StorytellerNotes = row["PrivateStorytellerNotes"] as string
        });

        return details;
    }

    public async Task UpdateLocationDetailsAsync(string username, string adventure, LocationDetails locationDetails)
    {
        await recordStorage.UpsertAsync(TableName, new Dictionary<string, object?>
        {
            {"PartitionKey", $"{username}_{adventure}" },
            {"RowKey", locationDetails.Location.ToString() },
            {"Name", locationDetails.Name },
            {"Description", locationDetails.Description },
            {"GameHistory", locationDetails.GameHistory },
            {"PrivateStorytellerNotes", locationDetails.StorytellerNotes }
        });
    }
}