using pokemon_team_builder.Entities;

namespace pokemon_team_builder.Interfaces;

public interface ITeamBuilderService
{
    Task<TeamResponse?> GenerateTeamAsync(string version, string difficulty);
}