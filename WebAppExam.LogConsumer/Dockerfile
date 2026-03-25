FROM mcr.microsoft.com/dotnet/runtime-deps:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the projects needed for build to avoid publishing duplicate content
COPY WebAppExam.LogConsumer/ WebAppExam.LogConsumer/
COPY WebAppExam/ WebAppExam/

WORKDIR /src/WebAppExam.LogConsumer

# Remove appsettings from the referenced project to avoid duplicate publish output file collisions
RUN rm -f /src/WebAppExam/appsettings.json || true

RUN dotnet restore "WebAppExam.LogConsumer.csproj"
# Publish a self-contained linux-x64 executable to avoid relying on the container image's shared frameworks
RUN dotnet publish "WebAppExam.LogConsumer.csproj" -c Release -r linux-x64 --self-contained true -o /app/publish /p:PublishTrimmed=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
# The publish produces a self-contained executable named `WebAppExam.LogConsumer`
ENTRYPOINT ["./WebAppExam.LogConsumer"]
