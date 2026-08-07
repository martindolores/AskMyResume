FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY AskMyResume.slnx .
COPY src/AskMyResume.Api/AskMyResume.Api.csproj src/AskMyResume.Api/
COPY tests/AskMyResume.Api.Tests/AskMyResume.Api.Tests.csproj tests/AskMyResume.Api.Tests/
RUN dotnet restore src/AskMyResume.Api/AskMyResume.Api.csproj

COPY src/AskMyResume.Api/ src/AskMyResume.Api/
RUN dotnet publish src/AskMyResume.Api/AskMyResume.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

EXPOSE 8080

ENTRYPOINT ["dotnet", "AskMyResume.Api.dll"]
