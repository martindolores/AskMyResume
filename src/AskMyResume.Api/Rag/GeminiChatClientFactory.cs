using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;

namespace AskMyResume.Api.Rag;

// Builds the chat client that talks to the Gemini API's free tier via its
// OpenAI-compatible endpoint — same connector shape as
// GeminiEmbeddingGeneratorFactory (see README cost guardrails; do not point
// this at paid Azure OpenAI). Network/config glue, verified by running rather
// than unit tested, per CLAUDE.md's TDD scope.
public static class GeminiChatClientFactory
{
    public static IChatClient Create(GeminiChatOptions options, string apiKey)
    {
        var client = new OpenAIClient(
            new ApiKeyCredential(apiKey),
            new OpenAIClientOptions { Endpoint = options.Endpoint });

        return client.GetChatClient(options.ChatModel).AsIChatClient();
    }
}
