FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["3_Calculator.csproj", "./"]
RUN dotnet restore "3_Calculator.csproj"
COPY . .
RUN dotnet publish "3_Calculator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5003
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "3_Calculator.dll"]


