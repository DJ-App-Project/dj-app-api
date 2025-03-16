FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["dj-api.sln", "./"]
COPY ["dj-api/dj-api.csproj", "dj-api/"]
COPY ["DjTests/DjTests.csproj", "DjTests/"]

RUN dotnet restore "dj-api/dj-api.csproj"

COPY . .

RUN dotnet publish "dj-api/dj-api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ARG ASPNETCORE_URLS
ARG ConnectionStrings__DbConnection
ARG JWTSecrets__issuer
ARG JWTSecrets__audience
ARG JWTSecrets__secretKey
ARG JWTSecrets__expires

ENV ASPNETCORE_URLS=${ASPNETCORE_URLS}
ENV ConnectionStrings__DbConnection=${ConnectionStrings__DbConnection}
ENV JWTSecrets__issuer=${JWTSecrets__issuer}
ENV JWTSecrets__audience=${JWTSecrets__audience}
ENV JWTSecrets__secretKey=${JWTSecrets__secretKey}
ENV JWTSecrets__expires=${JWTSecrets__expires}

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5152

ENTRYPOINT ["dotnet", "dj-api.dll"] 