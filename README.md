# horizon-server-database-middleware
SQL database project for the Horizon server emulator

## Running in Docker
To run in docker, you need to:

1. Set the appropriate environment variables, or use an environment list file
2. Edit the `appsettings.json` file if you want to automatically import locations, app ids, and channels into the database (RECOMMENDED). This needs to be tailored to your application
3. Expose TCP port 10000, or whatever port you put in the environment variables

| Environment Variable   | Description                                                                                                         |
|------------------------|---------------------------------------------------------------------------------------------------------------------|
| HORIZON_DB_USER                | The user to login to the database as                                                                                |
| HORIZON_MSSQL_SA_PASSWORD            | The user's password to login to the database                                                                        |
| HORIZON_DB_NAME                | The name of the database. Put this as medius_database       |
| HORIZON_DB_SERVER                | The database URL in SQL server format (e.g. 192.168.1.1,1433) to                                                                              |
| HORIZON_ASPNETCORE_ENVIRONMENT | The build ASP.NET core. Put this as Prod                                                                                   |
| HORIZON_MIDDLEWARE_SERVER      | The IP to bind to, generally looks like http://0.0.0.0:10000                                                        |
| HORIZON_MIDDLEWARE_USER        | The name of the 'admin' middleware user. This will get stored as a medius account                                   |
| HORIZON_MIDDLEWARE_PASSWORD    | The password for the middleware admin user                                                                          |


## App location
```
http://localhost:10000/swagger/index.html
```
(change the port to be whatever you use)
