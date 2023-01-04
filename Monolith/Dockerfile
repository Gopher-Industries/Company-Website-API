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
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /app
EXPOSE 443

# copy csproj and restore as distinct layers
COPY ProjectX.WebAPI/*.csproj .

# We're making sure the project is ready to be built for the alpine linux box
RUN dotnet restore -r linux-x64

# copy everything else and build app
COPY ProjectX.WebAPI/. .
RUN dotnet publish -c Release -o /app/publish -r alpine-x64 --self-contained true /p:PublishTrimmed=true /p:PublishSingleFile=true

# final stage/image. This base image is a linux machine with only dotnet runtime dependencies installed. Very small image size as a result.
FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=build /app/publish .

# set entry point to the application
#ENTRYPOINT ["./ProjectX.WebAPI", "ProjectX.WebAPI.dll"]
#ENTRYPOINT ["./ProjectX.WebAPI"]