FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Coiny.Api/Coiny.Api.csproj", "src/Coiny.Api/"]
COPY ["src/Coiny.Application/Coiny.Application.csproj", "src/Coiny.Application/"]
COPY ["src/Coiny.Domain/Coiny.Domain.csproj", "src/Coiny.Domain/"]
COPY ["src/Coiny.Infrastructure/Coiny.Infrastructure.csproj", "src/Coiny.Infrastructure/"]
COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
RUN dotnet restore "src/Coiny.Api/Coiny.Api.csproj"
COPY . .
WORKDIR "/src/src/Coiny.Api"
RUN dotnet build "Coiny.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Coiny.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Coiny.Api.dll"]
