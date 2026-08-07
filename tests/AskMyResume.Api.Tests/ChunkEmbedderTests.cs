using AskMyResume.Api.Rag;
using Microsoft.Extensions.AI;

namespace AskMyResume.Api.Tests;

public class ChunkEmbedderTests
{
    [Fact]
    public async Task EmbedAsync_PairsEachChunkWithItsGeneratedVector()
    {
        var chunks = new[]
        {
            new TextChunk("a.txt", 0, "first chunk"),
            new TextChunk("a.txt", 1, "second chunk"),
        };
        var generator = new FakeEmbeddingGenerator(text => new float[] { text.Length, 0 });
        var embedder = new ChunkEmbedder(generator);

        var result = await embedder.EmbedAsync(chunks);

        Assert.Equal(2, result.Count);
        Assert.Equal(chunks[0], result[0].Chunk);
        Assert.Equal(new float[] { "first chunk".Length, 0 }, result[0].Vector.ToArray());
        Assert.Equal(chunks[1], result[1].Chunk);
        Assert.Equal(new float[] { "second chunk".Length, 0 }, result[1].Vector.ToArray());
    }

    [Fact]
    public async Task EmbedAsync_EmptyInput_ReturnsEmpty()
    {
        var embedder = new ChunkEmbedder(new FakeEmbeddingGenerator(_ => new float[] { 0 }));

        var result = await embedder.EmbedAsync([]);

        Assert.Empty(result);
    }

    private sealed class FakeEmbeddingGenerator(Func<string, float[]> embed)
        : IEmbeddingGenerator<string, Embedding<float>>
    {
        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
            IEnumerable<string> values,
            EmbeddingGenerationOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var embeddings = new GeneratedEmbeddings<Embedding<float>>(
                values.Select(v => new Embedding<float>(embed(v))));
            return Task.FromResult(embeddings);
        }

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }
}
