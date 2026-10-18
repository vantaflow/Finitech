FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/ApiHost/Finitech.ApiHost/Finitech.ApiHost.csproj"
WORKDIR "/src/src/ApiHost/Finitech.ApiHost"
RUN dotnet build "Finitech.ApiHost.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Finitech.ApiHost.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Finitech.ApiHost.dll"]
