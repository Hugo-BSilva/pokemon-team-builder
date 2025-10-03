namespace pokemon_team_builder.Entities;

public class TeamResponse
{
    public string GameVersion { get; set; }
    public string Difficulty { get; set; }
    public List<TeamPokemon> Team { get; set; }
}