FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM caddy:2.7 as caddy
RUN /usr/bin/caddy version

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["WebApplication1.csproj", "WebApplication1.csproj"]
RUN dotnet restore "WebApplication1.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "WebApplication1.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WebApplication1.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=caddy /usr/bin/caddy /usr/bin/caddy
COPY ./Caddyfile ./Caddyfile
COPY ./ui/build ./build

ENV DOTNET_EnableDiagnostics=0
ENV ASPNETCORE_URLS=http://+:5000

ENTRYPOINT ["sh", "-c", "/usr/bin/caddy start --config ./Caddyfile && dotnet WebApplication1.dll"]
