# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY TalentVerse.WebAPI/*.csproj TalentVerse.WebAPI/
RUN dotnet restore TalentVerse.WebAPI/TalentVerse.WebAPI.csproj

# Copy everything else and build
COPY . .
WORKDIR /src/TalentVerse.WebAPI
RUN dotnet build TalentVerse.WebAPI.csproj -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish TalentVerse.WebAPI.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 5249
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TalentVerse.WebAPI.dll"]
