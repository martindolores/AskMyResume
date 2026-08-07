namespace AskMyResume.Api.Rag;

// Bound from the "Gemini" section of appsettings.json, same section and API
// key as GeminiEmbeddingOptions — the Gemini API free tier serves both chat
// and embeddings behind one OpenAI-compatible endpoint.
public sealed class GeminiChatOptions
{
    public const string SectionName = "Gemini";

    public required Uri Endpoint { get; init; }

    public required string ChatModel { get; init; }
}
