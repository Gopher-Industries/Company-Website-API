#
# =============================
# =PROJECT X DOCKER BUILD FILE=
# =============================
#
# The purpose of this file is instructions to build the containerized
# image for this project. 
#
# You can build this docker image yourself running the command:
# docker build -t project-x .
#
# Make sure that you have docker desktop installed & running.
#

# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

# copy csproj and restore as distinct layers
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ProjectX.WebAPI/*.csproj .
RUN dotnet restore
COPY ProjectX.WebAPI/. .
RUN dotnet build "ProjectX.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProjectX.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjectX.WebAPI.dll"]