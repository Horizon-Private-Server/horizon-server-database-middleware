echo "Building middleware container ..."
docker build . -t horizon-middleware

echo "Starting middleware container ..."
docker run \
  -d \
  --rm \
  -e DB_USER=${HORIZON_DB_USER} \
  -e DB_PASSWORD=${HORIZON_MSSQL_SA_PASSWORD} \
  -e DB_NAME=${HORIZON_DB_NAME} \
	-e DB_SERVER=${HORIZON_DB_SERVER} \
  -e ASPNETCORE_ENVIRONMENT=${HORIZON_ASPNETCORE_ENVIRONMENT} \
	-e MIDDLEWARE_SERVER=${HORIZON_MIDDLEWARE_SERVER} \
	-e MIDDLEWARE_SERVER_IP=${HORIZON_MIDDLEWARE_SERVER_IP} \
	-e MIDDLEWARE_USER=${HORIZON_MIDDLEWARE_USER} \
	-e MIDDLEWARE_PASSWORD=${HORIZON_MIDDLEWARE_PASSWORD} \
  -p 10000:10000 \
  -p 10001:10001 \
  --name horizon-middleware \
  horizon-middleware
