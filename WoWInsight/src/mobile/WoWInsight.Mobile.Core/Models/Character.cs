using System;
using SQLite;

namespace WoWInsight.Mobile.Models;

[Table("Characters")]
public class Character
{
    [PrimaryKey]
    public string CharacterKey { get; set; } = string.Empty; // {region}:{realm}:{name}

    public string Name { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Class { get; set; } = string.Empty;
    public string Faction { get; set; } = string.Empty;

    // Store MythicPlusSummary as JSON blob or separate table?
    // Since it's 1:1 and simple, separate table linked by key is better, or just one big object.
    // I'll use separate table for summary.
}

[Table("MythicPlusSummaries")]
public class MythicPlusSummary
{
    [PrimaryKey]
    public string CharacterKey { get; set; } = string.Empty; // Foreign Key essentially

    public double TotalScore { get; set; }
    public string BestRunsJson { get; set; } = string.Empty; // Serialized List<RunDto>
    public string RecentRunsJson { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAt { get; set; }
}

public class WeeklyChecklist
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string CharacterKey { get; set; } = string.Empty;
    public bool IsWeeklyChestDone { get; set; }
    public bool IsRaidDone { get; set; }
    public bool IsWorldBossDone { get; set; }
    // Reset weekly based on region?
    // Logic for reset can be in ViewModel.
    public DateTimeOffset LastUpdated { get; set; }
}
