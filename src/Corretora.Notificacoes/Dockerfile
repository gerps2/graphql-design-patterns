# O serviço de notificações num contêiner. O contexto do build é a raiz do repositório.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY global.json ./
COPY src/Corretora.Notificacoes/Corretora.Notificacoes.csproj src/Corretora.Notificacoes/
RUN dotnet restore src/Corretora.Notificacoes/Corretora.Notificacoes.csproj
COPY src/Corretora.Notificacoes/ src/Corretora.Notificacoes/
RUN dotnet publish src/Corretora.Notificacoes/Corretora.Notificacoes.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Corretora.Notificacoes.dll"]
