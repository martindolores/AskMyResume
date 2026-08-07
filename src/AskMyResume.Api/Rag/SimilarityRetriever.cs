namespace AskMyResume.Api.Rag;

// Ranks the in-memory embedding index by cosine similarity to a query vector.
// No vector DB — a linear scan over a handful of resume/portfolio chunks is
// plenty at this corpus size (see README's open decisions).
public sealed class SimilarityRetriever
{
    public IReadOnlyList<ScoredChunk> Retrieve(
        IReadOnlyList<EmbeddedChunk> index,
        ReadOnlyMemory<float> queryVector,
        int topK)
    {
        if (topK <= 0)
        {
            return [];
        }

        return index
            .Select(e => new ScoredChunk(e.Chunk, CosineSimilarity.Compute(e.Vector.Span, queryVector.Span)))
            .OrderByDescending(s => s.Score)
            .Take(topK)
            .ToList();
    }
}
