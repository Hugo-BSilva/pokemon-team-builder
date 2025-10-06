using System.Text.Json.Serialization;

namespace pokemon_team_builder.Entities;

public class Move
{
    [JsonPropertyName("Name")]
    public required string Name { get; set; }

    [JsonPropertyName("Type")]
    public required string Type { get; set; }

    [JsonPropertyName("Power")]
    public int? Power { get; set; }

    [JsonPropertyName("Method")]
    public string? Method { get; set; }
}