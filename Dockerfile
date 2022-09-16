# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /app
EXPOSE 443

# copy csproj and restore as distinct layers
COPY ProjectX.WebAPI/*.csproj .

# We're making sure the project is ready to be built for the alpine linux box
RUN dotnet restore -r linux-musl-x64

# copy everything else and build app
COPY ProjectX.WebAPI/. .
RUN dotnet publish -c Release -o /app -r linux-musl-x64 --self-contained false

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine-amd64
WORKDIR /app
COPY --from=build /app .

# set entry point to the application
ENTRYPOINT ["dotnet", "ProjectX.WebAPI.dll"]