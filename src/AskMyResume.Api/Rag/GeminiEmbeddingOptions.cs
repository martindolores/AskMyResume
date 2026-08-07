namespace AskMyResume.Api.Rag;

// Bound from the "Gemini" section of appsettings.json. The API key is never
// stored here — it's read separately via the "GEMINI_API_KEY" config key
// (user-secrets locally, environment variable in prod; see Program.cs) so it
// never ends up committed to the repo.
public sealed class GeminiEmbeddingOptions
{
    public const string SectionName = "Gemini";

    public required Uri Endpoint { get; init; }

    public required string EmbeddingModel { get; init; }
}
