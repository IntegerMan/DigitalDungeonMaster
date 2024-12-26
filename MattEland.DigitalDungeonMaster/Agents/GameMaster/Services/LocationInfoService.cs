using MattEland.DigitalDungeonMaster.Agents.GameMaster.Models;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;

public class LocationInfoService(ILogger<LocationInfoService> logger, IRecordStorageService recordStorage)
{
    private const string TableName = "playerlocations";

    public async Task<LocationInfo> GetCurrentLocationAsync(string username, string adventure)
    {
        LocationInfo? location = await recordStorage.FindByKeyAsync<LocationInfo>(TableName, username, adventure, row => new LocationInfo
        {
            Region = (string)row["Region"]!,
            X = Convert.ToInt32(row["X"]),
            Y = Convert.ToInt32(row["Y"])
        });

        if (location == null)
        {
            logger.LogWarning("No location found for {Username} in {AdventureName}. Defaulting to origin.", username, adventure);
            location = new LocationInfo
            {
                Region = "default",
                X = 0,
                Y = 0
            };
            
            // Store it for next time
            await SetLocationAsync(username, adventure, location);
        }

        return location;
    }

    public async Task SetLocationAsync(string username, string adventure, LocationInfo location)
    {
        await recordStorage.UpsertAsync(TableName, new Dictionary<string, object?>()
        {
            {"PartitionKey", username },
            {"RowKey", adventure },
            {"Region", location.Region.ToLowerInvariant() },
            {"X", location.X },
            {"Y", location.Y }
        });
    }
}