FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Catalog.Api.csproj","./"]
RUN dotnet restore "Catalog.Api.csproj"
COPY . .
RUN dotnet publish "Catalog.Api.csproj" -c Release -o /app/publish

FROM base as final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Catalog.Api.dll"]




