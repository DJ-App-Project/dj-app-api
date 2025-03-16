FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

WORKDIR /app

COPY dj-api.sln .
COPY *.csproj ./
RUN dotnet restore

COPY . ./

RUN dotnet publish dj-api.sln -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "dj-api.dll"]