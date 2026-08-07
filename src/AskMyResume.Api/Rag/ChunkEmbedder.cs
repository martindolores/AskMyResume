using Microsoft.Extensions.AI;

namespace AskMyResume.Api.Rag;

// Thin wrapper over Semantic Kernel's embedding generator abstraction so the
// retrieval/scoring logic (SimilarityRetriever, CosineSimilarity) stays
// unit-testable without a network call, while this glue can be swapped for a
// fake generator in tests.
public sealed class ChunkEmbedder(IEmbeddingGenerator<string, Embedding<float>> generator)
{
    public async Task<IReadOnlyList<EmbeddedChunk>> EmbedAsync(
        IReadOnlyList<TextChunk> chunks,
        CancellationToken cancellationToken = default)
    {
        if (chunks.Count == 0)
        {
            return [];
        }

        var embeddings = await generator.GenerateAsync(
            chunks.Select(c => c.Text),
            options: null,
            cancellationToken);

        return chunks
            .Zip(embeddings, (chunk, embedding) => new EmbeddedChunk(chunk, embedding.Vector))
            .ToList();
    }
}
