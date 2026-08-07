namespace AskMyResume.Api.Rag;

public sealed record EmbeddedChunk(TextChunk Chunk, ReadOnlyMemory<float> Vector);
