FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Копируем файл решения
COPY ["../../exchangesms.sln", "./"]

# Копируем файлы проектов
COPY ["../../Domain/exchangesms/exchangesms.csproj", "../../Domain/exchangesms/"]
COPY ["../../Infrastructure/repos/Data/Data.csproj", "../../Infrastructure/repos/Data/"]
COPY ["Web.csproj", "."]

# Восстанавливаем зависимости (NuGet packages)
RUN dotnet restore "exchangesms.sln"

# Копируем все остальные файлы
COPY ../../. .

# Собираем Web проект
WORKDIR "/src/Presentation/Web"
RUN dotnet build "Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Web.dll"]