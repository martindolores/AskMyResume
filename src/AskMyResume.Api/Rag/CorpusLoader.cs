namespace AskMyResume.Api.Rag;

public sealed record CorpusDocument(string FileName, string Text);

// Reads the plain-text resume/portfolio corpus from disk. In-memory only —
// no vector DB, per the README's cost guardrails and open decisions.
public sealed class CorpusLoader(string contentRoot)
{
    public IReadOnlyList<CorpusDocument> LoadDocuments() =>
        Directory.EnumerateFiles(contentRoot, "*.txt", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.Ordinal)
            .Select(path => new CorpusDocument(Path.GetFileName(path), File.ReadAllText(path)))
            .ToList();
}
