namespace AskMyResume.Api.Rag;

public sealed record TextChunk(string SourceFile, int Index, string Text);
