FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AMMS.Web/AMMS.Web.csproj AMMS.Web/
RUN dotnet restore AMMS.Web/AMMS.Web.csproj

COPY AMMS.Web/ AMMS.Web/
RUN dotnet publish AMMS.Web/AMMS.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "AMMS.Web.dll"]
