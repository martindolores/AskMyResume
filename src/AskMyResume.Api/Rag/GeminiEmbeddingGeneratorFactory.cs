using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AskMyResume.Api.Rag;

// Builds the embedding generator that talks to the Gemini API's free tier via
// its OpenAI-compatible endpoint (see README cost guardrails — GitHub Models,
// the original choice, was retired 2026-07-30; do not point this at paid
// Azure OpenAI). Network/config glue, verified by running rather than unit
// tested, per CLAUDE.md's TDD scope.
public static class GeminiEmbeddingGeneratorFactory
{
    public static IEmbeddingGenerator<string, Embedding<float>> Create(GeminiEmbeddingOptions options, string apiKey)
    {
        var client = new OpenAIClient(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = options.Endpoint });

        return client.GetEmbeddingClient(options.EmbeddingModel).AsIEmbeddingGenerator();
    }
}
