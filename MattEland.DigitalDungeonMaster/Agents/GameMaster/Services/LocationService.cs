using MattEland.DigitalDungeonMaster.Agents.GameMaster.Models;
using MattEland.DigitalDungeonMaster.Services;

namespace MattEland.DigitalDungeonMaster.Agents.GameMaster.Services;

public class LocationService(ILogger<LocationService> logger, IRecordStorageService recordStorage)
{
    public async Task<LocationInfo> GetCurrentLocationAsync(string username, string adventure)
    {
        LocationInfo? location = await recordStorage.FindByKeyAsync<LocationInfo>("PlayerLocations", username, adventure, row => new LocationInfo
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
        await recordStorage.UpsertAsync("PlayerLocations", new Dictionary<string, object?>()
        {
            {"PartitionKey", username },
            {"RowKey", adventure },
            {"Region", location.Region.ToLowerInvariant() },
            {"X", location.X },
            {"Y", location.Y }
        });
    }

    public async Task<LocationDetails?> GetDetailsOrDefaultAsync(string username, string adventure, LocationInfo location)
    {
        LocationDetails? details = await recordStorage.FindByKeyAsync<LocationDetails>("Locations", $"{username}_{adventure}", location.ToString(), row => new LocationDetails
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
        await recordStorage.UpsertAsync("Locations", new Dictionary<string, object?>
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