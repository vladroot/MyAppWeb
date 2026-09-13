FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
# RUN apt-get update && apt-get install -y --no-install-recommends \
#     libkrb5-3 \
#     && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080

RUN mkdir -p /app/data && chmod 755 /app/data

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["MyAppWeb.csproj", "./"]
RUN dotnet restore "MyAppWeb.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "MyAppWeb.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "MyAppWeb.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MyAppWeb.dll"]
