using AskMyResume.Api.Rag;
using Microsoft.Extensions.AI;

namespace AskMyResume.Api.Tests;

public class ChatAnswererTests
{
    [Fact]
    public async Task AnswerAsync_ReturnsChatClientResponseText()
    {
        var chatClient = new FakeChatClient(_ => "The answer is 42.");
        var answerer = new ChatAnswerer(chatClient);

        var result = await answerer.AnswerAsync(
            "What is the answer?",
            [new TextChunk("a.txt", 0, "some context")]);

        Assert.Equal("The answer is 42.", result);
    }

    [Fact]
    public async Task AnswerAsync_SendsSystemPromptThenUserMessageWithContextAndQuestion()
    {
        List<ChatMessage>? captured = null;
        var chatClient = new FakeChatClient(messages =>
        {
            captured = messages.ToList();
            return "ignored";
        });
        var answerer = new ChatAnswerer(chatClient);

        await answerer.AnswerAsync(
            "Where did they go to school?",
            [new TextChunk("edu.txt", 0, "Studied CS at Foo University")]);

        Assert.NotNull(captured);
        Assert.Equal(2, captured!.Count);
        Assert.Equal(ChatRole.System, captured[0].Role);
        Assert.Equal(ChatRole.User, captured[1].Role);
        Assert.Contains("Studied CS at Foo University", captured[1].Text);
        Assert.Contains("Where did they go to school?", captured[1].Text);
    }

    [Fact]
    public async Task AnswerAsync_EmptyContext_StillSendsTheQuestion()
    {
        List<ChatMessage>? captured = null;
        var chatClient = new FakeChatClient(messages =>
        {
            captured = messages.ToList();
            return "ignored";
        });
        var answerer = new ChatAnswerer(chatClient);

        await answerer.AnswerAsync("Anything?", []);

        Assert.NotNull(captured);
        Assert.Contains("Anything?", captured![1].Text);
    }

    private sealed class FakeChatClient(Func<IEnumerable<ChatMessage>, string> respond) : IChatClient
    {
        public Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var text = respond(messages);
            return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, text)));
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public object? GetService(Type serviceType, object? serviceKey = null) => null;

        public void Dispose()
        {
        }
    }
}
