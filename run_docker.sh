#!/bin/bash

docker container kill horizon-middleware
sleep 1

set -e

echo "Building middleware container ..."
docker build . -t horizonprivateserver/horizon-server-database-middleware

echo "Starting middleware container ..."
docker run \
  -d \
  --rm \
  -e HORIZON_DB_USER=${HORIZON_DB_USER} \
  -e HORIZON_MSSQL_SA_PASSWORD=${HORIZON_MSSQL_SA_PASSWORD} \
  -e HORIZON_DB_NAME=${HORIZON_DB_NAME} \
  -e HORIZON_DB_SERVER=${HORIZON_DB_SERVER} \
  -e ASPNETCORE_ENVIRONMENT=${HORIZON_ASPNETCORE_ENVIRONMENT} \
  -e HORIZON_MIDDLEWARE_SERVER=${HORIZON_MIDDLEWARE_SERVER} \
  -e HORIZON_MIDDLEWARE_USER=${HORIZON_MIDDLEWARE_USER} \
  -e HORIZON_MIDDLEWARE_PASSWORD=${HORIZON_MIDDLEWARE_PASSWORD} \
  -v ${PWD}/appsettings.json:/appsettings.json \
  -p 10000:10000 \
  -p 10001:10001 \
  --name horizon-middleware \
  horizonprivateserver/horizon-server-database-middleware
