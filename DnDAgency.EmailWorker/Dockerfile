FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем все проекты (Worker зависит от Application)
COPY ["DnDAgency.EmailWorker/DnDAgency.EmailWorker.csproj", "DnDAgency.EmailWorker/"]
COPY ["DnDAgency.Application/DnDAgency.Application.csproj", "DnDAgency.Application/"]
COPY ["DnDAgency.Domain/DnDAgency.Domain.csproj", "DnDAgency.Domain/"]

RUN dotnet restore "DnDAgency.EmailWorker/DnDAgency.EmailWorker.csproj"

COPY . .
WORKDIR "/src/DnDAgency.EmailWorker"
RUN dotnet build "DnDAgency.EmailWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DnDAgency.EmailWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DnDAgency.EmailWorker.dll"]