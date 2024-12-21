namespace MattEland.DigitalDungeonMaster.WebAPI.Settings;

public record RegistrationSettings
{
    public bool AllowRegistration { get; init; } = true;
}