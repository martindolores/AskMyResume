namespace AskMyResume.Api.Rag;

// Bound from the "Gemini" section of appsettings.json. The API key is never
// stored in config — it's read separately from an environment variable
// (e.g. GEMINI_API_KEY) so it never ends up committed to the repo.
public sealed class GeminiEmbeddingOptions
{
    public const string SectionName = "Gemini";

    public required Uri Endpoint { get; init; }

    public required string EmbeddingModel { get; init; }
}
