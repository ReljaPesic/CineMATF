FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Services/Cinema.API/*.csproj ./Cinema.API/
RUN dotnet restore Cinema.API/Cinema.API.csproj

COPY Services/Cinema.API/ ./Cinema.API/
RUN dotnet build Cinema.API/Cinema.API.csproj -c Release -o /app/build

RUN dotnet publish Cinema.API/Cinema.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8000
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 8000

ENTRYPOINT ["dotnet", "Cinema.API.dll"]