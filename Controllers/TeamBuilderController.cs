using Microsoft.AspNetCore.Mvc;
using pokemon_team_builder.Interfaces;

namespace pokemon_team_builder.Controllers;

[ApiController]
[Route("api/teambuilder")]
public class TeamBuilderController(ITeamBuilderService service) : ControllerBase
{
    private readonly ITeamBuilderService _service = service;

    [HttpGet("generate")]
    public async Task<IActionResult> GenerateTeam([FromQuery] string version, [FromQuery] string difficulty)
    {
        if (string.IsNullOrEmpty(version) || string.IsNullOrEmpty(difficulty))
        {
            return BadRequest(new { error = "Parâmetros 'version' e 'difficulty' são obrigatórios." });
        }

        try
        {
            // Chama o serviço que faz a comunicação com a IA
            var teamResponse = await _service.GenerateTeamAsync(version, difficulty, default);

            if (teamResponse == null || teamResponse.Team.Count == 0)
            {
                // Isso pode acontecer se a IA não conseguir gerar um time válido
                return StatusCode(500, new { error = "A IA não conseguiu gerar um time válido. Tente novamente." });
            }

            return Ok(teamResponse);
        }
        catch (HttpRequestException ex)
        {
            // Erro de comunicação com a API da IA
            return StatusCode(502, new { error = $"Erro ao se comunicar com a API de IA. {ex.Message}" });
        }
        catch (Exception ex)
        {
            // Outros erros (ex: chave não configurada, erro de deserialização)
            return StatusCode(500, new { error = $"Erro interno ao gerar o time: {ex.Message}" });
        }
    }
}