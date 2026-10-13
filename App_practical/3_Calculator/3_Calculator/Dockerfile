# ============================================
# Этап 1: Сборка (build) — образ с .NET SDK
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1) Копируем ТОЛЬКО .csproj — это кэшируется и ускоряет повторные сборки
COPY ["3_Calculator.csproj", "./"]
RUN dotnet restore "3_Calculator.csproj"

# 2) Копируем весь остальной код (Program.cs, Controllers, Views, Data, ...)
COPY . .

# 3) Публикуем в Release
RUN dotnet publish "3_Calculator.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ============================================
# Этап 2: Runtime — облегчённый ASP.NET-образ
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Порт, который слушает Kestrel (совпадает с appsettings.json)
EXPOSE 5003

# Копируем опубликованные файлы из build-этапа
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "3_Calculator.dll"]


