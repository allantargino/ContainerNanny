#FROM microsoft/dotnet:2.0-runtime
#ARG source
#WORKDIR /app
#COPY ${source:-obj/Docker/publish} .
#ENTRYPOINT ["dotnet", "Queue.Checker.CORE.dll"]
#

FROM microsoft/aspnetcore-build:2.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/aspnetcore:2.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "aspnetapp.dll"]