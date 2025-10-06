using System.Text.Json.Serialization;

namespace pokemon_team_builder.Entities;

public class TeamResponse
{
    [JsonPropertyName("gameVersion")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string GameVersion { get; set; } = string.Empty;

    [JsonPropertyName("difficulty")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Difficulty { get; set; } = string.Empty;

    [JsonPropertyName("team")]
    public List<TeamPokemon> Team { get; set; } = [];
}