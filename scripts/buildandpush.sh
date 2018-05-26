#!/bin/bash

# Exit on any error
set -e

if [ $# -lt 2 ]
  then
    echo "provide azure register username and password"
    exit -1
fi

echo 'Building Dotnet project'
dotnet restore ./Queue.sln && dotnet publish ./Queue.sln -c Release -o ./obj/Docker/publish

echo 'Creating docker image'
docker-compose build

echo 'docker login'
docker login visouza.azurecr.io -u $1 -p $2

echo 'docker push image to azure visouza.azurecr.io/app/queueconsumer'
docker push visouza.azurecr.io/app/queueconsumer
