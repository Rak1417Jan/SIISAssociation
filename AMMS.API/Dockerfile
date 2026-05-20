FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AMMS.API/AMMS.API.csproj AMMS.API/
COPY AMMS.Services/AMMS.Services.csproj AMMS.Services/
COPY AMMS.Repository/AMMS.Repository.csproj AMMS.Repository/
COPY AMMS.Model/AMMS.Model.csproj AMMS.Model/
COPY AMMS.Comman/AMMS.Comman.csproj AMMS.Comman/

RUN dotnet restore AMMS.API/AMMS.API.csproj

COPY . .
RUN dotnet publish AMMS.API/AMMS.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Cloud Run sets PORT (default 8080). .NET 8 also reads PORT / ASPNETCORE_HTTP_PORTS.
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "AMMS.API.dll"]
