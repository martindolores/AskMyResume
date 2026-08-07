var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/chat", () => Results.Ok());

app.Run();
