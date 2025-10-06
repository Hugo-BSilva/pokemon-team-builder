using pokemon_team_builder.Entities;

namespace pokemon_team_builder.Interfaces;

public interface ITeamBuilderService
{
    ValueTask<TeamResponse?> GenerateTeamAsync(string version, string difficulty, CancellationToken cancellationToken);
}