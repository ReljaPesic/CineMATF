# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
# QuestPDF (ticket PDF generation) needs at least one real font on the system to render text.
RUN apt-get update \
    && apt-get install -y --no-install-recommends fontconfig fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Services/Reservation.API/Reservation.API.csproj", "Services/Reservation.API/"]
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked \
    dotnet restore "Services/Reservation.API/Reservation.API.csproj"
COPY . .
WORKDIR "/src/Services/Reservation.API"
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked \
    dotnet build "Reservation.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN --mount=type=cache,target=/root/.nuget/packages,sharing=locked \
    dotnet publish "Reservation.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Reservation.API.dll"]
