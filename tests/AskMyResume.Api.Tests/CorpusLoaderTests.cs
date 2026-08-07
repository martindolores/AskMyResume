using AskMyResume.Api.Rag;

namespace AskMyResume.Api.Tests;

public class CorpusLoaderTests : IDisposable
{
    private readonly string _tempDir = Directory.CreateTempSubdirectory("askmyresume-corpus-tests-").FullName;

    [Fact]
    public void LoadDocuments_ReadsAllTxtFilesRecursively()
    {
        File.WriteAllText(Path.Combine(_tempDir, "summary.txt"), "summary text");
        var subDir = Directory.CreateDirectory(Path.Combine(_tempDir, "nested"));
        File.WriteAllText(Path.Combine(subDir.FullName, "skills.txt"), "skills text");
        File.WriteAllText(Path.Combine(_tempDir, "notes.md"), "should be ignored");

        var loader = new CorpusLoader(_tempDir);
        var documents = loader.LoadDocuments();

        Assert.Equal(2, documents.Count);
        Assert.Contains(documents, d => d.FileName == "summary.txt" && d.Text == "summary text");
        Assert.Contains(documents, d => d.FileName == "skills.txt" && d.Text == "skills text");
    }

    [Fact]
    public void LoadDocuments_ReturnsEmptyForDirectoryWithNoTextFiles()
    {
        var loader = new CorpusLoader(_tempDir);

        var documents = loader.LoadDocuments();

        Assert.Empty(documents);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);
}
