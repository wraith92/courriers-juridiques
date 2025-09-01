using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// HttpClient (usine à HttpClient)
builder.Services.AddHttpClient();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowVueDev");

// Endpoint racine
app.MapGet("/", () => Results.Ok("✅ LegalApi running"));

// Endpoint qui génère un courrier via Ollama
app.MapPost("/generate-letter", async (LetterRequest req, IHttpClientFactory httpClientFactory) =>
{
    var client = httpClientFactory.CreateClient();

    // Prompt envoyé à l’IA
    var prompt = $@"
    Rédige un courrier juridique de type '{req.Type}'.
    Nom de l’expéditeur : {req.Nom}
    Destinataire : {req.Destinataire}
    Contexte : {req.Contexte}
    Montant : {req.Montant}
    Fournis un texte structuré, clair et professionnel.";

    var body = new
    {
        model = "llama3",  // <-- modèle installé dans Ollama
        prompt = prompt,
        stream = false
    };

    var response = await client.PostAsync(
        "http://localhost:11434/api/generate",
        new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
    );

    if (!response.IsSuccessStatusCode)
    {
        return Results.Problem("❌ Erreur lors de l'appel à Ollama.");
    }

    var responseJson = await response.Content.ReadAsStringAsync();
    dynamic? json = JsonConvert.DeserializeObject(responseJson);
    string content = json?.response ?? "Pas de réponse IA";

    return Results.Ok(new LetterResponse($"Courrier - {req.Type}", content));
})
.WithName("GenerateLetter");

app.Run();

// Records pour simplifier les DTO
public record LetterRequest(string Type, string Nom, string Destinataire, string Contexte, string Montant);
public record LetterResponse(string Title, string Content);
