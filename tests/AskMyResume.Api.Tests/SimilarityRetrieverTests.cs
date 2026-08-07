using AskMyResume.Api.Rag;

namespace AskMyResume.Api.Tests;

public class SimilarityRetrieverTests
{
    private static readonly TextChunk ChunkA = new("a.txt", 0, "A");
    private static readonly TextChunk ChunkB = new("b.txt", 0, "B");
    private static readonly TextChunk ChunkC = new("c.txt", 0, "C");

    private readonly SimilarityRetriever _retriever = new();

    [Fact]
    public void Retrieve_RanksByCosineSimilarityDescending()
    {
        var index = new[]
        {
            new EmbeddedChunk(ChunkA, new float[] { 1, 0 }),   // similarity 1.0 to query
            new EmbeddedChunk(ChunkB, new float[] { 0, 1 }),   // similarity 0.0 to query
            new EmbeddedChunk(ChunkC, new float[] { 0.9f, 0.1f }), // similarity ~0.99 to query
        };

        var results = _retriever.Retrieve(index, new float[] { 1, 0 }, topK: 3);

        Assert.Equal([ChunkA, ChunkC, ChunkB], results.Select(r => r.Chunk));
    }

    [Fact]
    public void Retrieve_ReturnsOnlyTopK()
    {
        var index = new[]
        {
            new EmbeddedChunk(ChunkA, new float[] { 1, 0 }),
            new EmbeddedChunk(ChunkB, new float[] { 0.5f, 0.5f }),
            new EmbeddedChunk(ChunkC, new float[] { 0, 1 }),
        };

        var results = _retriever.Retrieve(index, new float[] { 1, 0 }, topK: 1);

        Assert.Single(results);
        Assert.Equal(ChunkA, results[0].Chunk);
    }

    [Fact]
    public void Retrieve_TopKLargerThanIndex_ReturnsWholeIndex()
    {
        var index = new[] { new EmbeddedChunk(ChunkA, new float[] { 1, 0 }) };

        var results = _retriever.Retrieve(index, new float[] { 1, 0 }, topK: 5);

        Assert.Single(results);
    }

    [Fact]
    public void Retrieve_EmptyIndex_ReturnsEmpty()
    {
        var results = _retriever.Retrieve([], new float[] { 1, 0 }, topK: 3);

        Assert.Empty(results);
    }

    [Fact]
    public void Retrieve_TopKZeroOrNegative_ReturnsEmpty()
    {
        var index = new[] { new EmbeddedChunk(ChunkA, new float[] { 1, 0 }) };

        Assert.Empty(_retriever.Retrieve(index, new float[] { 1, 0 }, topK: 0));
        Assert.Empty(_retriever.Retrieve(index, new float[] { 1, 0 }, topK: -1));
    }
}
