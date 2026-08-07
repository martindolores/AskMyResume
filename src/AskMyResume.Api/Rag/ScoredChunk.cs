namespace AskMyResume.Api.Rag;

public sealed record ScoredChunk(TextChunk Chunk, double Score);
