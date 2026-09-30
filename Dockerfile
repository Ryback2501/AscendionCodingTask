# Stage 1: build the API with the .NET SDK image.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy only the project files first, so Docker can reuse the package download step.
COPY global.json Directory.Build.props ./
COPY src/BestStories.Api/BestStories.Api.csproj src/BestStories.Api/
RUN dotnet restore src/BestStories.Api/BestStories.Api.csproj

# Copy the rest of the code and publish the app.
COPY src/ src/
RUN dotnet publish src/BestStories.Api/BestStories.Api.csproj -c Release --no-restore -o /app

# Stage 2: run the API with the smaller ASP.NET runtime image.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Run as a normal user, not as root. The API listens on port 8080.
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "BestStories.Api.dll"]
