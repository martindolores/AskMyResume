using AskMyResume.Api.Rag;

namespace AskMyResume.Api.Tests;

public class CosineSimilarityTests
{
    [Fact]
    public void IdenticalVectors_ReturnsOne()
    {
        var a = new float[] { 1, 2, 3 };
        var b = new float[] { 1, 2, 3 };

        var score = CosineSimilarity.Compute(a, b);

        Assert.Equal(1.0, score, precision: 10);
    }

    [Fact]
    public void OrthogonalVectors_ReturnsZero()
    {
        var a = new float[] { 1, 0 };
        var b = new float[] { 0, 1 };

        var score = CosineSimilarity.Compute(a, b);

        Assert.Equal(0.0, score, precision: 10);
    }

    [Fact]
    public void OppositeVectors_ReturnsNegativeOne()
    {
        var a = new float[] { 1, 2, 3 };
        var b = new float[] { -1, -2, -3 };

        var score = CosineSimilarity.Compute(a, b);

        Assert.Equal(-1.0, score, precision: 10);
    }

    [Fact]
    public void ScaledVectors_ReturnSameSimilarityAsUnscaled()
    {
        var a = new float[] { 1, 2, 3 };
        var b = new float[] { 2, 4, 6 };

        var score = CosineSimilarity.Compute(a, b);

        Assert.Equal(1.0, score, precision: 10);
    }

    [Fact]
    public void ZeroVector_ReturnsZeroRatherThanNaN()
    {
        var a = new float[] { 0, 0, 0 };
        var b = new float[] { 1, 2, 3 };

        var score = CosineSimilarity.Compute(a, b);

        Assert.Equal(0.0, score, precision: 10);
    }

    [Fact]
    public void MismatchedLengths_ThrowsArgumentException()
    {
        var a = new float[] { 1, 2, 3 };
        var b = new float[] { 1, 2 };

        Assert.Throws<ArgumentException>(() => CosineSimilarity.Compute(a, b));
    }
}
