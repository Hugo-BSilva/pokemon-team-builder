using Microsoft.OpenApi.Models;
using OpenAI;
using pokemon_team_builder.Interfaces;
using pokemon_team_builder.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();

var openApiKey = builder.Configuration["OpenAi:ApiKey"];
if (string.IsNullOrEmpty(openApiKey))
    throw new InvalidOperationException("OpenAI API Key não configurada no appsettings.");

builder.Services.AddSingleton(new OpenAIClient(openApiKey));
builder.Services.AddScoped<ITeamBuilderService, TeamBuilderService>();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pokémon Team Builder API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowLocalhost",
        builder =>
        {
            builder.WithOrigins("http://localhost:5173", "https://pokemon-extension.vercel.app/")
                    .AllowAnyHeader()
                    .AllowAnyMethod();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pokémon Team Builder API v1");

    });
    app.UseHttpsRedirection();
}

app.MapControllers();
app.Run();