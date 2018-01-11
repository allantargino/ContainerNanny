dotnet restore
dotnet publish -c Release -o publish/
docker build . -t yourendpoint.azurecr.io/app/queueconsumer
docker login yourendpoint.azurecr.io -u youruser -p yourpassword
docker push yourendpoint.azurecr.io/app/queueconsumer