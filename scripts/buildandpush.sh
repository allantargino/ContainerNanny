#!/bin/bash

if [ $# -lt 2 ]
  then
    echo "provide azure register username and password"
    exit -1
fi

echo 'Building Dotnet project'
dotnet restore ./Queue.sln && dotnet publish ./Queue.sln -c Release -o ./obj/Docker/publish

if [ $? -lt 0 ]
then
    echo 'Error building the project'
    exit -1
fi


echo 'Creating docker image'
docker-compose build

if [ $? -lt 0 ]
then
    echo 'Error creating image'
    exit -1
fi

echo 'docker login'
docker login visouza.azurecr.io -u $1 -p $2

if [ $? -lt 0 ]
then
    echo 'Error Docker Login'
    exit -1
fi

echo 'docker push image to azure visouza.azurecr.io/app/queueconsumer'
docker push visouza.azurecr.io/app/queueconsumer

if [ $? -lt 0 ]
then
    echo 'Error Docker Push'
    exit -1
fi