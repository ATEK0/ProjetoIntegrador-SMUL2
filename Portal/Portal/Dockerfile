FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Portal.csproj", "./"]
RUN dotnet restore "Portal.csproj"

COPY . .
RUN dotnet build "Portal.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Portal.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "Portal.dll"]
