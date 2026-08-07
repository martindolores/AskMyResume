using System.Runtime.CompilerServices;
using AskMyResume.Api.Rag;

namespace AskMyResume.Api.Tests;

// Exercises CorpusLoader + DocumentChunker against the real resume corpus committed
// under src/AskMyResume.Api/Content/Resume, so a malformed content file fails CI.
public class ResumeCorpusTests
{
    private static string ContentRoot([CallerFilePath] string testFilePath = "") =>
        Path.Combine(Path.GetDirectoryName(testFilePath)!, "..", "..", "src", "AskMyResume.Api", "Content", "Resume");

    [Fact]
    public void ResumeCorpus_LoadsExpectedFiles()
    {
        var loader = new CorpusLoader(ContentRoot());

        var documents = loader.LoadDocuments();

        Assert.Equal(
            ["education.txt", "experience.txt", "skills.txt", "summary.txt"],
            documents.Select(d => d.FileName).OrderBy(f => f, StringComparer.Ordinal));
        Assert.All(documents, d => Assert.False(string.IsNullOrWhiteSpace(d.Text)));
    }

    [Fact]
    public void ResumeCorpus_ChunksIntoMultipleNonEmptyChunksPerFile()
    {
        var loader = new CorpusLoader(ContentRoot());
        var chunker = new DocumentChunker();

        var allChunks = loader.LoadDocuments()
            .SelectMany(doc => chunker.Chunk(doc.FileName, doc.Text))
            .ToList();

        Assert.NotEmpty(allChunks);
        Assert.All(allChunks, c => Assert.False(string.IsNullOrWhiteSpace(c.Text)));

        var experienceChunks = allChunks.Where(c => c.SourceFile == "experience.txt").ToList();
        Assert.True(experienceChunks.Count > 5, "expected the experience section to chunk into multiple bullet-sized pieces");
    }
}
