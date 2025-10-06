using Microsoft.OpenApi.Exceptions;
using OpenAI.Chat;
using pokemon_team_builder.Entities;
using pokemon_team_builder.Interfaces;
using System.Net;
using System.Text.Json;
using System.Threading;

namespace pokemon_team_builder.Services;

public class TeamBuilderService : ITeamBuilderService
{
    private readonly OpenAI.OpenAIClient _openAiClient;
    private readonly ChatClient _chatClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly string _schema;

    public TeamBuilderService(OpenAI.OpenAIClient openAiClient)
    {
        _openAiClient = openAiClient;
        _chatClient = _openAiClient.GetChatClient("gpt-4.1-mini"); // ✅ mantido em memória

        _schema = GetExpectedJsonSchema(); // ✅ carregado uma única vez

        _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
    }

    public async ValueTask<TeamResponse?> GenerateTeamAsync(string version, string difficulty, CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(version, difficulty);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are a Pokémon team expert. Always return ONLY a valid JSON object exactly in the expected schema."),
            new UserChatMessage(prompt)
        };

        var options = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                "pokemon_team_schema",
                System.BinaryData.FromString(_schema)
            )
        };

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(15)); // timeout de 15s

            var result = await _chatClient.CompleteChatAsync(messages, options, cts.Token);

            var completion = result.Value;
            if (completion?.Content is null || completion.Content.Count == 0)
                return null;

            var jsonText = string.Join("", completion.Content.Select(c => c.Text));
            return JsonSerializer.Deserialize<TeamResponse>(jsonText, _jsonSerializerOptions);
        }
        catch (OpenApiException)
        {
            // Retry simples com backoff exponencial
            await Task.Delay(1000, cancellationToken);
            return await GenerateTeamAsync(version, difficulty, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[OpenAI ERROR] {ex.Message}");
            return null;
        }
    }

    private string BuildPrompt(string version, string difficulty)
    {
        var jsonSchema = GetExpectedJsonSchema();

        return $"""
            Você é um especialista em estratégia Pokémon e um assistente de IA focado em planejamento de jornadas para jogos R.P.G. Sua tarefa é montar um time de 6 Pokémon para o jogo específico e nível de dificuldade fornecidos.

            **REGRAS OBRIGATÓRIAS:**
            1.  **Pokémons iniciais:** Na maioria dos jogos ao iniciar o game, temos 3 opções de pokémons iniciais para escolher, se você escolher um para o time, não poderá escolher os outros dois (ou suas posteriores evoluções). O time deve conter apenas 1 pokémon dos 3 iniciais, em red-blue por exemplo se escolher o Charmander como inicial, não poderá escolher bulbasaur nem squirtle, somente 1 incial é permitido.
            2.  **Time Size:** O time deve ter exatamente 6 Pokémon.
            3.  **Evolução:** É estritamente proibido incluir um Pokémon que só pode evoluir através de troca. São permitidas apenas evoluções por Level Up, Pedras ou Felicidade são permitidas.
            4.  **Disponibilidade:** Todos os 6 Pokémon devem ser **capturáveis** no jogo/versão especificado o quanto antes possível para que o jogador possa utilizar o pokémon o máximo possível.
            5.  **Estrutura de Saída:** O resultado final DEVE ser estritamente um objeto JSON válido. Retorne APENAS o JSON.

            **PARÂMETROS DA REQUISIÇÃO:**
            * **JOGO/VERSÃO:** {version}
            * **DIFICULDADE:** {difficulty}

            **DIRETRIZES DE DIFICULDADE:**
            * **Fácil (Easy):** Selecione os 6 Pokémon considerados de Tier S ou A (os mais poderosos, seja pelos stats ou pela variedade de golpes que pode aprender) para completar a história rapidamente, o time deve ter uma boa cobertura para toda a região, sendo assim o time terá pokémons de diversos tipos.
            * **Médio (Medium):** Selecione 6 Pokémon considerados de Tier B ou C (medianos em stats), que oferecem um desafio moderado, mas ainda são viáveis.
            * **Difícil (Hard):** Selecione 6 Pokémon considerados de Tier D, E ou F (os mais fracos, lentos ou que exigem muito investimento) para a jornada, sendo assim o time pode ser monotipo fogo/água/grama etc.

            **INFORMAÇÕES ADICIONAIS NECESSÁRIAS:**
            * Para CADA Pokémon, liste 4 movimentos (Moves). Priorize os movimentos mais fortes que podem ser aprendidos por **Level Up, TM ou HM** lembre-se de dar uma boa cobertura para o pokémon por exemplo Gardevoir com Calm Mind e Thunderbolt.
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