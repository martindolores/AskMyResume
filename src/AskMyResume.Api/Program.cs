using AskMyResume.Api;
using AskMyResume.Api.Rag;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Symmetric across environments: reads from user-secrets in Development (see
// `dotnet user-secrets set GEMINI_API_KEY ...`) and from a real environment
// variable in production (Container Apps, sourced from Key Vault) — same
// config key, same code path, never stored in a file inside the repo.
var apiKey = builder.Configuration["GEMINI_API_KEY"]
    ?? throw new InvalidOperationException("GEMINI_API_KEY is not set (user-secrets locally, env var in prod).");

var embeddingOptions = builder.Configuration.GetSection(GeminiEmbeddingOptions.SectionName).Get<GeminiEmbeddingOptions>()
    ?? throw new InvalidOperationException($"Missing \"{GeminiEmbeddingOptions.SectionName}\" configuration section.");
var chatOptions = builder.Configuration.GetSection(GeminiChatOptions.SectionName).Get<GeminiChatOptions>()
    ?? throw new InvalidOperationException($"Missing \"{GeminiChatOptions.SectionName}\" configuration section.");

var embeddingGenerator = GeminiEmbeddingGeneratorFactory.Create(embeddingOptions, apiKey);
var chatClient = GeminiChatClientFactory.Create(chatOptions, apiKey);

var contentRoot = Path.Combine(app.Environment.ContentRootPath, "Content", "Resume");
var corpusLoader = new CorpusLoader(contentRoot);
var chunker = new DocumentChunker();
var chunks = corpusLoader.LoadDocuments()
    .SelectMany(doc => chunker.Chunk(doc.FileName, doc.Text))
    .ToList();

var chunkEmbedder = new ChunkEmbedder(embeddingGenerator);
var index = await chunkEmbedder.EmbedAsync(chunks);

var chatService = new RagChatService(
    embeddingGenerator,
    new SimilarityRetriever(),
    new ChatAnswerer(chatClient),
    index);

app.MapPost("/chat", async (ChatRequest request, CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Question))
    {
        return Results.BadRequest(new { error = "Question is required." });
    }

    var answer = await chatService.AnswerAsync(request.Question, cancellationToken);
    return Results.Ok(new ChatAnswerResponse(answer));
});

app.Run();
