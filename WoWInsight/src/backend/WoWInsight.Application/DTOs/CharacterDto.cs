using System;

namespace WoWInsight.Application.DTOs;

public class CharacterDto
{
    public string CharacterKey { get; set; } = string.Empty; // {region}:{realmSlug}:{nameLower}
    public string Name { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Class { get; set; } = string.Empty;
    public string Faction { get; set; } = string.Empty;
}
