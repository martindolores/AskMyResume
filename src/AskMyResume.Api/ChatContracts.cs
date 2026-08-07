namespace AskMyResume.Api;

public sealed record ChatRequest(string Question);

public sealed record ChatAnswerResponse(string Answer);
