using System;
using System.Collections.Generic;

namespace WoWInsight.Mobile.DTOs;

public class CharacterDto
{
    public string CharacterKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Class { get; set; } = string.Empty;
    public string Faction { get; set; } = string.Empty;
}

public class MythicPlusSummaryDto
{
    public double TotalScore { get; set; }
    public List<RunDto> BestRuns { get; set; } = new();
    public List<RunDto> RecentRuns { get; set; } = new();
    public DateTimeOffset UpdatedAt { get; set; }
}

public class RunDto
{
    public string Dungeon { get; set; } = string.Empty;
    public int Level { get; set; }
    public double Score { get; set; }
    public bool Timed { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
}
