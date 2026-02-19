namespace WoWInsight.Infrastructure.Configuration;

public class BlizzardSettings
{
    public const string SectionName = "Blizzard";

    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
    public string RegionDefault { get; set; } = "eu";
}
