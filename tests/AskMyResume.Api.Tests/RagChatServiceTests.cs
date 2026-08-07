using AskMyResume.Api.Rag;
using Microsoft.Extensions.AI;

namespace AskMyResume.Api.Tests;

public class RagChatServiceTests
{
    private static readonly TextChunk ChunkA = new("a.txt", 0, "Studied CS at Foo University");
    private static readonly TextChunk ChunkB = new("b.txt", 0, "Worked at Bar Inc as an engineer");

    [Fact]
    public async Task AnswerAsync_RetrievesTopKChunksAndPassesThemToTheAnswerer()
    {
        var index = new[]
        {
            new EmbeddedChunk(ChunkA, new float[] { 1, 0 }),
            new EmbeddedChunk(ChunkB, new float[] { 0, 1 }),
        };
        var embeddingGenerator = new FakeEmbeddingGenerator(texts => texts.Select(_ => new float[] { 1, 0 }));
        List<ChatMessage>? captured = null;
        var chatClient = new FakeChatClient(messages =>
        {
            captured = messages.ToList();
            return "The answer is 42.";
        });
        var service = new RagChatService(
            embeddingGenerator,
            new SimilarityRetriever(),
            new ChatAnswerer(chatClient),
            index,
            topK: 1);

        var answer = await service.AnswerAsync("Where did they go to school?");

        Assert.Equal("The answer is 42.", answer);
        Assert.NotNull(captured);
        Assert.Contains("Studied CS at Foo University", captured![1].Text);
        Assert.DoesNotContain("Worked at Bar Inc", captured[1].Text);
    }

    [Fact]
    public async Task AnswerAsync_EmbedsTheQuestionItself()
    {
        List<string>? embeddedTexts = null;
        var embeddingGenerator = new FakeEmbeddingGenerator(texts =>
        {
            embeddedTexts = texts.ToList();
            return texts.Select(_ => new float[] { 1, 0 });
        });
        var chatClient = new FakeChatClient(_ => "ignored");
        var service = new RagChatService(
            embeddingGenerator,
            new SimilarityRetriever(),
            new ChatAnswerer(chatClient),
            [],
            topK: 3);

        await service.AnswerAsync("Where did they go to school?");

        Assert.NotNull(embeddedTexts);
        Assert.Equal(["Where did they go to school?"], embeddedTexts);
    }

    [Fact]
    public async Task AnswerAsync_EmptyIndex_StillAnswersWithNoContext()
    {
        var embeddingGenerator = new FakeEmbeddingGenerator(texts => texts.Select(_ => new float[] { 1, 0 }));
        var chatClient = new FakeChatClient(_ => "I don't know.");
        var service = new RagChatService(
            embeddingGenerator,
            new SimilarityRetriever(),
            new ChatAnswerer(chatClient),
            [],
            topK: 3);

        var answer = await service.AnswerAsync("Anything?");

        Assert.Equal("I don't know.", answer);
    }

    private sealed class FakeEmbeddingGenerator(Func<IEnumerable<string>, IEnumerable<float[]>> embed)
        : IEmbeddingGenerator<string, Embedding<float>>
    {
        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var valueList = values.ToList();
            var embeddings = new GeneratedEmbeddings<Embedding<float>>(
                embed(valueList).Select(v => new Embedding<float>(v)));
            return Task.FromResult(embeddings);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }

    private sealed class FakeChatClient(Func<IEnumerable<ChatMessage>, string> respond) : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var text = respond(messages);
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, text)));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }
}
