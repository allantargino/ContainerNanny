FROM microsoft/dotnet:2.0-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.0-sdk AS build
WORKDIR /src
COPY Nanny.Main/Nanny.Main.csproj Nanny.Main/
COPY Nanny.ServiceBus/Nanny.ServiceBus.csproj Nanny.ServiceBus/
COPY Nanny.Common/Nanny.Common.csproj Nanny.Common/
COPY Nanny.StorageQueue/Nanny.StorageQueue.csproj Nanny.StorageQueue/
COPY Nanny.Kubernetes/Nanny.Kubernetes.csproj Nanny.Kubernetes/
RUN dotnet restore Nanny.Main/Nanny.Main.csproj
COPY . .
WORKDIR /src/Nanny.Main
RUN dotnet build Nanny.Main.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Nanny.Main.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Nanny.Main.dll"]