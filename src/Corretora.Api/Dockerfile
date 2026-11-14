# A API da corretora num contêiner. O contexto do build é a raiz do repositório.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json ./
COPY src/Corretora.Api/Corretora.Api.csproj src/Corretora.Api/
RUN dotnet restore src/Corretora.Api/Corretora.Api.csproj
COPY src/Corretora.Api/ src/Corretora.Api/
RUN dotnet publish src/Corretora.Api/Corretora.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Corretora.Api.dll"]
