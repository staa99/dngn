FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["dngn-api-backend/dngn-api-backend.csproj", "dngn-api-backend/"]
RUN dotnet restore "dngn-api-backend/dngn-api-backend.csproj"
COPY . .
WORKDIR "/src/dngn-api-backend"
RUN dotnet build "dngn-api-backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "dngn-api-backend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "dngn-api-backend.dll"]
