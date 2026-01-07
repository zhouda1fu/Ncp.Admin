#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/Ncp.Admin.Web/Ncp.Admin.Web.csproj"
WORKDIR "/src/src/Ncp.Admin.Web"
RUN dotnet build "Ncp.Admin.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Ncp.Admin.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ncp.Admin.Web.dll"]
