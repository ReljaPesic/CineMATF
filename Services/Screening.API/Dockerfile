FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Services/Screening.API/Screening.API.csproj", "Services/Screening.API/"]
RUN dotnet restore "Services/Screening.API/Screening.API.csproj"
COPY . .
WORKDIR "/src/Services/Screening.API"
RUN dotnet build "Screening.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Screening.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Screening.API.dll"]
