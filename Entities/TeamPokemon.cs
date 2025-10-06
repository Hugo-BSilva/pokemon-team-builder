using System.Text.Json.Serialization;

namespace pokemon_team_builder.Entities;

public class TeamPokemon
{
    [JsonPropertyName("PokemonName")]
    public required string PokemonName { get; set; }

    [JsonPropertyName("PokedexNumber")]
    public required int PokedexNumber { get; set; }

    [JsonPropertyName("ImageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("Moves")]
    public required List<Move> Moves { get; set; }

    [JsonPropertyName("Matchups")]
    public Matchups? Matchups { get; set; }
}