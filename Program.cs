using Microsoft.OpenApi.Models;
using OpenAI;
using pokemon_team_builder.Interfaces;
using pokemon_team_builder.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Configurações de performance para produção
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxConcurrentConnections = 100;
    options.Limits.MaxConcurrentUpgradedConnections = 100;
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
});

// Ajusta nível de log (menos ruído em prod)
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Warning);

// Controllers + JSON mais leve
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// OpenAI Client
var openApiKey = builder.Configuration["OpenAi:ApiKey"];
if (string.IsNullOrEmpty(openApiKey))
    throw new InvalidOperationException("OpenAI API Key não configurada no appsettings.");

builder.Services.AddSingleton(new OpenAIClient(openApiKey));
builder.Services.AddScoped<ITeamBuilderService, TeamBuilderService>();

// Swagger opcional
var enableSwagger = builder.Configuration.GetValue<bool>("EnableSwagger", false);
if (enableSwagger)
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pokémon Team Builder API", Version = "v1" });
    });
}

// CORS dinâmico
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? ["*"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("DynamicCors", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Habilita Swagger se configurado
if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Render já faz HTTPS redirect
// app.UseHttpsRedirection();

app.UseCors("DynamicCors");
app.MapControllers();

// Aumenta pool mínimo para evitar cold-thread delays
ThreadPool.SetMinThreads(100, 100);

app.Run();