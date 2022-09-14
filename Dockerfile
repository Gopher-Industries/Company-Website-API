## https://hub.docker.com/_/microsoft-dotnet
#FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
#WORKDIR /app
#EXPOSE 443
#
## copy csproj and restore as distinct layers
#COPY ProjectX.WebAPI/*.csproj .
#RUN dotnet restore -r linux-musl-x64
#
## copy everything else and build app
#COPY ProjectX.WebAPI/. .
#RUN dotnet publish -c Release -o /app -r linux-musl-x64 --self-contained false
#
## final stage/image
#FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine-amd64
#WORKDIR /ProjectX.WebAPI
#COPY --from=build /ProjectX.WebAPI .
#ENTRYPOINT ["dotnet", "ProjectX.WebAPI.dll"]
#
## See: https://github.com/dotnet/announcements/issues/20
## Uncomment to enable globalization APIs (or delete)
## ENV \
##     DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
##     LC_ALL=en_US.UTF-8 \
##     LANG=en_US.UTF-8
## RUN apk add --no-cache icu-libs

# This docker file uses multi-stage build strategy
# to ensure optimal image build times and sizes
# End result container image requires .NET runtime,
# however creating it requires .NET SDK.
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine AS base
WORKDIR /src

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /src
COPY . .
RUN dotnet restore dotnet-cloud-run-hello-world.csproj
RUN dotnet build "./dotnet-cloud-run-hello-world.csproj" -c Debug -o /out

FROM build AS publish
RUN dotnet publish dotnet-cloud-run-hello-world.csproj -c Debug -o /out

# Building final image used in running container
FROM base AS final
RUN apk update \
    && apk add unzip procps
WORKDIR /src
COPY --from=publish /out .
CMD ASPNETCORE_URLS=http://*:$PORT dotnet dotnet-cloud-run-hello-world.dll
