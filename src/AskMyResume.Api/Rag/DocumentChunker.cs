using System.Text;

namespace AskMyResume.Api.Rag;

// Splits documents on blank lines so each chunk stays semantically self-contained
// (one resume bullet, one section) for later embedding/retrieval. Paragraphs longer
// than MaxChunkLength are further split on sentence boundaries.
public sealed class DocumentChunker(int maxChunkLength = 800)
{
    public IReadOnlyList<TextChunk> Chunk(string sourceFile, string text)
    {
        var chunks = new List<TextChunk>();
        var index = 0;

        foreach (var paragraph in SplitIntoParagraphs(text))
        {
            foreach (var piece in SplitIfTooLong(paragraph))
            {
                chunks.Add(new TextChunk(sourceFile, index++, piece));
            }
        }

        return chunks;
    }

    private static IEnumerable<string> SplitIntoParagraphs(string text) =>
        text.Replace("\r\n", "\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => p.Length > 0);

    private IEnumerable<string> SplitIfTooLong(string paragraph)
    {
        if (paragraph.Length <= maxChunkLength)
        {
            yield return paragraph;
            yield break;
        }

        var sentences = paragraph.Split(". ", StringSplitOptions.RemoveEmptyEntries);
        var current = new StringBuilder();

        foreach (var sentence in sentences)
        {
            var candidate = current.Length == 0 ? sentence : $"{current}. {sentence}";
            if (candidate.Length > maxChunkLength && current.Length > 0)
            {
                yield return current.ToString();
                current.Clear();
                current.Append(sentence);
            }
            else
            {
                current.Clear();
                current.Append(candidate);
            }
        }

        if (current.Length > 0)
        {
            yield return current.ToString();
        }
    }
}
