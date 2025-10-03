namespace pokemon_team_builder.Entities;

public class TeamPokemon
{
    public string PokemonName { get; set; }
    public int PokedexNumber { get; set; }
    public string ImageUrl { get; set; }
    public List<Move> Moves { get; set; }
    public Matchups Matchups { get; set; }
}