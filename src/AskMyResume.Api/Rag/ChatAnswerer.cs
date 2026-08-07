using Microsoft.Extensions.AI;

namespace AskMyResume.Api.Rag;

// Thin wrapper over Semantic Kernel's chat client abstraction so prompt
// construction stays unit-testable without a network call, while this glue
// can be swapped for a fake client in tests (mirrors ChunkEmbedder).
public sealed class ChatAnswerer(IChatClient chatClient)
{
    private const string SystemPrompt =
        "You are an assistant that answers questions about the resume and portfolio owner, " +
        "using only the provided context. If the answer isn't in the context, say you don't know.";

    public async Task<string> AnswerAsync(
        string question,
        IReadOnlyList<TextChunk> context,
        CancellationToken cancellationToken = default)
    {
        var response = await chatClient.GetResponseAsync(BuildMessages(question, context), cancellationToken: cancellationToken);
        return response.Text;
    }

    private static List<ChatMessage> BuildMessages(string question, IReadOnlyList<TextChunk> context)
    {
        var contextText = context.Count == 0
            ? "(no context retrieved)"
            : string.Join("\n\n", context.Select(c => c.Text));

        return
        [
            new ChatMessage(ChatRole.System, SystemPrompt),
            new ChatMessage(ChatRole.User, $"Context:\n{contextText}\n\nQuestion: {question}"),
        ];
    }
}
