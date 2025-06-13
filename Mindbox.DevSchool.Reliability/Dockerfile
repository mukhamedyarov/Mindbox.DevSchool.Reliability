FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Mindbox.DevSchool.Reliability/Mindbox.DevSchool.Reliability.csproj", "Mindbox.DevSchool.Reliability/"]
RUN dotnet restore "Mindbox.DevSchool.Reliability/Mindbox.DevSchool.Reliability.csproj"
COPY . .
WORKDIR "/src/Mindbox.DevSchool.Reliability"
RUN dotnet build "./Mindbox.DevSchool.Reliability.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Mindbox.DevSchool.Reliability.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:5013
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Mindbox.DevSchool.Reliability.dll"]
