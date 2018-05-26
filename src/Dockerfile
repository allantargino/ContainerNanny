
# Build runtime image
FROM microsoft/dotnet:2.0-sdk
WORKDIR /publish
COPY . ./
ENTRYPOINT ["dotnet", "publish/Nanny.Main.Core.dll"]