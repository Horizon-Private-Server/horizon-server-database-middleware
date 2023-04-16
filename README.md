# horizon-server-database-middleware
SQL database project for the Horizon server emulator

## Running in Docker
To run in docker, you need to:

1. Set the appropriate environment variables, or use an environment list file
2. Edit the `docker_config.json` file if you want to automatically import locations, app ids, and channels into the database
3. Expose TCP port 10000, or whatever port you put in the environment variables

| Environment Variable   | Description                                                                                                         |
|------------------------|---------------------------------------------------------------------------------------------------------------------|
| DB_USER                | The user to login to the database as                                                                                |
| DB_PASSWORD            | The user's password to login to the database                                                                        |
| DB_NAME                | The name of the database to connect to                                                                              |
| ASPNETCORE_ENVIRONMENT | The build ASP.NET core environment                                                                                  |
| MIDDLEWARE_SERVER      | The IP to bind to, generally looks like http://0.0.0.0:10000                                                        |
| MIDDLEWARE_SERVER_IP   | The external or local IP that will be hosting this. Can also be a docker internal IP. E.g. http://192.168.1.2:10000 |
| MIDDLEWARE_USER        | The name of the 'admin' middleware user. This will get stored as a medius account                                   |
| MIDDLEWARE_PASSWORD    | The password for the middleware admin user                                                                          |

For docker compose with the main server go [here](https://github.com/Horizon-Private-Server/horizon-server).

Multiple app groups not supported

## App location
```
http://localhost:10000/swagger/index.html
```
(change the port to be whatever you use)
