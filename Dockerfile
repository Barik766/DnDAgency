FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DnDAgency/DnDAgency.Api.csproj", "DnDAgency/"]
COPY ["DnDAgency.Application/DnDAgency.Application.csproj", "DnDAgency.Application/"]
COPY ["DnDAgency.Domain/DnDAgency.Domain.csproj", "DnDAgency.Domain/"]
COPY ["DnDAgency.Infrastructure/DnDAgency.Infrastructure.csproj", "DnDAgency.Infrastructure/"]
RUN dotnet restore "DnDAgency/DnDAgency.Api.csproj"
COPY . .
WORKDIR "/src/DnDAgency"
RUN dotnet build "DnDAgency.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DnDAgency.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DnDAgency.Api.dll"]