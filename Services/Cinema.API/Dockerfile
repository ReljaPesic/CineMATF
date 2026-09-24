# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Services/Cinema.API/Cinema.API.csproj", "Services/Cinema.API/"]
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked \
    dotnet restore "Services/Cinema.API/Cinema.API.csproj"
COPY . .
WORKDIR "/src/Services/Cinema.API"
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked \
    dotnet build "Cinema.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked \
    dotnet publish "Cinema.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cinema.API.dll"]