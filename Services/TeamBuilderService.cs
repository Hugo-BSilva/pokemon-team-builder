using OpenAI.Chat;
using pokemon_team_builder.Entities;
using pokemon_team_builder.Interfaces;
using System.Text.Json;

namespace pokemon_team_builder.Services;

public class TeamBuilderService : ITeamBuilderService
{
    private readonly OpenAI.OpenAIClient _openAiClient;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true // aceita vírgula no fim do json
    };

    public TeamBuilderService(OpenAI.OpenAIClient openAiClient)
    {
        _openAiClient = openAiClient;

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true // Permite que "pokemonName" no JSON seja "PokemonName" na C#
        };
    }

    public async Task<TeamResponse?> GenerateTeamAsync(string version, string difficulty)
    {
        var prompt = BuildPrompt(version, difficulty);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "You are a Pokémon team expert. Always return ONLY a valid JSON object exactly in the expected schema, without markdown, commentary or extra text."
            ),
            new UserChatMessage(prompt)
        };

        var chatClient = _openAiClient.GetChatClient("gpt-4o-mini");

        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                "pokemon_team_schema",
                System.BinaryData.FromString(GetExpectedJsonSchema())
            )
        };

        var result = await chatClient.CompleteChatAsync(messages, options);

        var completion = result.Value;

        if (completion == null || completion.Content == null || completion.Content.Count == 0)
            throw new Exception("A IA retornou uma resposta vazia (Content).");

        var jsonText = string.Join("", completion.Content.Select(c => c.Text ?? string.Empty));


        if (string.IsNullOrWhiteSpace(jsonText))
            throw new Exception("A IA retornou um conteúdo JSON vazio.");

        return JsonSerializer.Deserialize<TeamResponse>(jsonText, _jsonSerializerOptions);
    }

    private string BuildPrompt(string version, string difficulty)
    {
        var jsonSchema = GetExpectedJsonSchema();

        return $"""
            Você é um especialista em estratégia Pokémon e um assistente de IA focado em planejamento de jornadas para jogos R.P.G. Sua tarefa é montar um time de 6 Pokémon para o jogo específico e nível de dificuldade fornecidos.

            **REGRAS OBRIGATÓRIAS:**
            1.  **Time Size:** O time deve ter exatamente 6 Pokémon.
            2.  **Evolução:** É estritamente proibido incluir Pokémon ou evoluções que só possam ser obtidas através de **troca com outro jogador**. Evolução por Level Up, Pedras ou Felicidade são permitidas.
            3.  **Disponibilidade:** Todos os 6 Pokémon devem ser **capturáveis** no jogo/versão especificado.
            4.  **Estrutura de Saída:** O resultado final DEVE ser estritamente um objeto JSON válido. Retorne APENAS o JSON.

            **PARÂMETROS DA REQUISIÇÃO:**
            * **JOGO/VERSÃO:** {version}
            * **DIFICULDADE:** {difficulty}

            **DIRETRIZES DE DIFICULDADE:**
            * **Fácil (Easy):** Selecione os 6 Pokémon considerados de Tier S ou A (os mais poderosos ou fáceis de treinar) para completar a história rapidamente.
            * **Médio (Medium):** Selecione 6 Pokémon considerados de Tier B ou C (medianos), que oferecem um desafio moderado, mas ainda são viáveis.
            * **Difícil (Hard):** Selecione 6 Pokémon considerados de Tier D, E ou F (os mais fracos, lentos ou que exigem muito investimento) para a jornada.

            **INFORMAÇÕES ADICIONAIS NECESSÁRIAS:**
            * Para CADA Pokémon, liste 4 movimentos (Moves). Priorize os movimentos mais fortes que podem ser aprendidos por **Level Up, TM ou HM**.
            * Para CADA Pokémon, analise o time completo dos 8 **Líderes de Ginásio** e da **Elite Four** desse jogo ({version}). Liste os Pokémon dos líderes/Elite 4 contra os quais o Pokémon sugerido é Super Efetivo (`strongAgainstLeaders`) e liste os Pokémon dos líderes/Elite 4 que causam dano Super Efetivo ao Pokémon sugerido (`weakAgainstLeaders`).

            **REQUISITO DE SAÍDA - DEVE SER OBRIGATORIAMENTE ESTE JSON (SEM MARKKDOWN):**
            {jsonSchema}
            """;
    }

    private string GetExpectedJsonSchema() => """
        {
          "type": "object",
          "properties": {
            "gameVersion": { "type": "string" },
            "difficulty": { "type": "string" },
            "team": {
              "type": "array",
              "items": {
                "type": "object",
                "properties": {
                  "pokemonName": { "type": "string" },
                  "pokedexNumber": { "type": "integer" },
                  "imageUrl": { "type": "string" },
                  "moves": {
                    "type": "array",
                    "items": {
                      "type": "object",
                      "properties": {
                        "name": { "type": "string" },
                        "type": { "type": "string" },
                        "power": { "type": "integer" },
                        "method": { "type": "string" }
                      },
                      "required": ["name", "type", "power", "method"]
                    }
                  },
                  "matchups": {
                    "type": "object",
                    "properties": {
                      "weakAgainstLeaders": {
                        "type": "array",
                        "items": { "type": "string" }
                      },
                      "strongAgainstLeaders": {
                        "type": "array",
                        "items": { "type": "string" }
                      }
                    },
                    "required": ["weakAgainstLeaders", "strongAgainstLeaders"]
                  }
                },
                "required": ["pokemonName", "pokedexNumber", "imageUrl", "moves", "matchups"]
              }
            }
          },
          "required": ["gameVersion", "difficulty", "team"]
        }
        """;
}