FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY MinecraftSkins.sln .
COPY MinecraftSkins.Domain/*.csproj MinecraftSkins.Domain/
COPY MinecraftSkins.Application/*.csproj MinecraftSkins.Application/
COPY MinecraftSkins.Infrastructure/*.csproj MinecraftSkins.Infrastructure/
COPY MinecraftSkins.WebAPI/*.csproj MinecraftSkins.WebAPI/
COPY MinecraftSkins.Tests/*.csproj MinecraftSkins.Tests/

RUN dotnet restore

COPY . .

WORKDIR /src/MinecraftSkins.WebAPI
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "MinecraftSkins.WebAPI.dll"]