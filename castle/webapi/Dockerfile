FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Castle.WebApi/Castle.WebApi.csproj", "Castle.WebApi/"]
COPY ["Castle.Protocol/Castle.Protocol.csproj", "Castle.Protocol/"]
RUN dotnet restore "Castle.WebApi/Castle.WebApi.csproj"
COPY . .
WORKDIR "/src/Castle.WebApi"
RUN dotnet build "Castle.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Castle.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Castle.WebApi.dll"]