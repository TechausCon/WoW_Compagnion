using System;

namespace WoWInsight.Application.DTOs;

public class RunDto
{
    public string Dungeon { get; set; } = string.Empty;
    public int Level { get; set; }
    public double Score { get; set; }
    public bool Timed { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
}

public class MythicPlusSummaryDto
{
    public double TotalScore { get; set; }
    public List<RunDto> BestRuns { get; set; } = new();
    public List<RunDto> RecentRuns { get; set; } = new();
    public DateTimeOffset UpdatedAt { get; set; }
}
