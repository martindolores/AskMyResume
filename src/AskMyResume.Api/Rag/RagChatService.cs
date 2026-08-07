using Microsoft.Extensions.AI;

namespace AskMyResume.Api.Rag;

// Orchestrates a single /chat turn: embed the question, retrieve the closest
// chunks from the pre-built in-memory index, and hand them to ChatAnswerer.
// The index is built once at startup (see Program.cs) — no per-request re-embedding
// of the corpus.
//
// topK defaults to 8 rather than something tighter: at this corpus's size (~30
// chunks across 4 files) a few chunks per document are near-duplicate headings
// (e.g. "Martin Dolores — Education" scores higher than the actual education
// paragraph for education-related queries), so a small topK can crowd out the
// chunk that actually answers the question. Cost is a non-issue at this scale.
public sealed class RagChatService(
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
    SimilarityRetriever retriever,
    ChatAnswerer answerer,
    IReadOnlyList<EmbeddedChunk> index,
    int topK = 8)
{
    public async Task<string> AnswerAsync(string question, CancellationToken cancellationToken = default)
    {
        var queryEmbedding = await embeddingGenerator.GenerateAsync([question], cancellationToken: cancellationToken);
        var queryVector = queryEmbedding[0].Vector;

        var context = retriever.Retrieve(index, queryVector, topK)
            .Select(scored => scored.Chunk)
            .ToList();

        return await answerer.AnswerAsync(question, context, cancellationToken);
    }
}
