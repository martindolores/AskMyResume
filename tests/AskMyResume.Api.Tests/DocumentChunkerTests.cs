using AskMyResume.Api.Rag;

namespace AskMyResume.Api.Tests;

public class DocumentChunkerTests
{
    [Fact]
    public void Chunk_SplitsOnBlankLines()
    {
        var chunker = new DocumentChunker();
        var text = "First paragraph.\n\nSecond paragraph.\n\nThird paragraph.";

        var chunks = chunker.Chunk("resume.txt", text);

        Assert.Equal(3, chunks.Count);
        Assert.Equal("First paragraph.", chunks[0].Text);
        Assert.Equal("Second paragraph.", chunks[1].Text);
        Assert.Equal("Third paragraph.", chunks[2].Text);
    }

    [Fact]
    public void Chunk_AssignsSequentialIndexesPerSourceFile()
    {
        var chunker = new DocumentChunker();
        var text = "One.\n\nTwo.\n\nThree.";

        var chunks = chunker.Chunk("skills.txt", text);

        Assert.Equal([0, 1, 2], chunks.Select(c => c.Index));
        Assert.All(chunks, c => Assert.Equal("skills.txt", c.SourceFile));
    }

    [Fact]
    public void Chunk_TrimsWhitespaceAndDropsEmptyParagraphs()
    {
        var chunker = new DocumentChunker();
        var text = "  Padded paragraph.  \n\n\n\nAnother one.\n\n   \n\n";

        var chunks = chunker.Chunk("resume.txt", text);

        Assert.Equal(2, chunks.Count);
        Assert.Equal("Padded paragraph.", chunks[0].Text);
        Assert.Equal("Another one.", chunks[1].Text);
    }

    [Fact]
    public void Chunk_SplitsOversizedParagraphOnSentenceBoundaries()
    {
        var chunker = new DocumentChunker(maxChunkLength: 30);
        var text = "This is sentence one. This is sentence two. This is sentence three.";

        var chunks = chunker.Chunk("resume.txt", text);

        Assert.True(chunks.Count > 1, "expected the oversized paragraph to be split into multiple chunks");
        Assert.Contains("sentence one", chunks[0].Text);
        Assert.Contains("sentence three", chunks[^1].Text);
    }

    [Fact]
    public void Chunk_KeepsParagraphUnderLimitAsSingleChunk()
    {
        var chunker = new DocumentChunker(maxChunkLength: 800);
        var text = "A short paragraph that easily fits inside the default chunk size limit.";

        var chunks = chunker.Chunk("resume.txt", text);

        Assert.Single(chunks);
        Assert.Equal(text, chunks[0].Text);
    }

    [Fact]
    public void Chunk_ReturnsEmptyForBlankInput()
    {
        var chunker = new DocumentChunker();

        var chunks = chunker.Chunk("resume.txt", "   \n\n   ");

        Assert.Empty(chunks);
    }
}
