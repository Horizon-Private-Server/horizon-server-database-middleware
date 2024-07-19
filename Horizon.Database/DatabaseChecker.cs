using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Horizon.Database.Entities;
using Horizon.Database.Helpers;
using Horizon.Database.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Filters;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Horizon.Database
{
    public class DatabaseChecker
    {

        private DbContext context = null;
        private string folderPath = null;

        private string middlewareAdminUser = null;
        private string middlewareAdminPassword = null;
        private string horizonDatabase = null;

        public DatabaseChecker()
        {
            Console.WriteLine($"Checking if server is already initialized ...");

            middlewareAdminUser = Environment.GetEnvironmentVariable("HORIZON_MIDDLEWARE_USER");
            middlewareAdminPassword = ComputeSHA256(Environment.GetEnvironmentVariable("HORIZON_MIDDLEWARE_PASSWORD"));
            horizonDatabase = Environment.GetEnvironmentVariable("HORIZON_DB_NAME");

            // Set folder path
            var st = new StackTrace(true);
            var frame = st.GetFrame(0);
            string filepath = frame.GetFileName();
            folderPath = Path.GetDirectoryName(filepath);

            var builder = new ConfigurationBuilder()
            .SetBasePath(folderPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

            IConfiguration Configuration = builder.Build();
            var connectionStringPlaceHolder = Configuration.GetConnectionString("DbConnection");
            string serverName = Environment.GetEnvironmentVariable("HORIZON_DB_SERVER");
            string dbName = "master";
            string dbUserName = Environment.GetEnvironmentVariable("HORIZON_DB_USER");
            string dbPassword = Environment.GetEnvironmentVariable("HORIZON_MSSQL_SA_PASSWORD");

            var connectionString = connectionStringPlaceHolder.Replace("{_SERVER}", serverName).Replace("{_DBNAME}", dbName).Replace("{_USERNAME}", dbUserName).Replace("{_PASSWORD}", dbPassword);

            // Configure DbContextOptionsBuilder
            var optionsBuilder = new DbContextOptionsBuilder();

            // Use SQL Server provider with connection string
            optionsBuilder.UseSqlServer(connectionString);

            // Create DbContextOptions from optionsBuilder
            var options = optionsBuilder.Options;

            // Create a DbContext instance with the dynamically configured options
            context = new DbContext(options);
        }

        private bool IsServerAlive()
        {
            Console.WriteLine("Checking if we can login to database with master table ...");
            try
            {
                context.Database.OpenConnection();
                Console.WriteLine("Initial master connection successful.");
                context.Database.CloseConnection();
                return true;
            }
            catch
            {        
                Console.WriteLine("Unable to connect to master table.");
                return false;
            }
        }

        private bool IsServerInitialized()
        {
            var databaseName = context.Database.GetDbConnection().Database;
            string sql = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{horizonDatabase}'";
            return QueryDatabaseInt(sql) > 0;
        }


        public void WaitForDatabase(int pollingIntervalSeconds = 10)
        {

            while(!IsServerAlive()) {
                Console.WriteLine($"Waiting {pollingIntervalSeconds} seconds before polling again ...");
                Thread.Sleep(pollingIntervalSeconds * 1000);
            }

            if (!IsServerInitialized()) {
                // Initialize database
                Console.WriteLine($"Need to initialize database!");
                
                // Run CREATE DATABASE script
                Console.WriteLine($"Running CREATE_DATABASE.sql ...");
                ExecuteSqlScript(Path.Combine(folderPath, "scripts", "CREATE_DATABASE.sql"));

                // Run CREATE TABLE script
                Console.WriteLine($"Running CREATE_TABLES.sql ...");
                ExecuteSqlScript(Path.Combine(folderPath, "scripts", "CREATE_TABLES.sql"));

                // Create admin middleware user
                CreateAdminUser();

                // Set all app settings into the database
                SetAppSettings();
            } else {
                Console.WriteLine($"Database already initialized!");
            }

        }

        public void CreateAdminUser() {
            Console.WriteLine("Creating admin middleware user ...");

            string createAdmin = $@"
                INSERT INTO accounts.account (account_name, account_password, create_dt, is_active, app_id, reset_password_on_next_login)
                VALUES ('{middlewareAdminUser}', '{middlewareAdminPassword}', getdate(), 1, 0, 0);
            ";
            ExecuteSqlCommand(createAdmin);

            // Add database role 
            int roleId = QueryDatabaseInt($"SELECT role_id FROM {horizonDatabase}.KEYS.roles where role_name = 'database'");
            int accountId = QueryDatabaseInt($"SELECT account_id FROM {horizonDatabase}.accounts.account where account_name = '{middlewareAdminUser}'");
            ExecuteSqlCommand($"INSERT INTO {horizonDatabase}.accounts.user_role VALUES({accountId}, {roleId}, GETDATE(), GETDATE(), null)");
        }

        public void SetAppSettings() {
            // Insert app group ids

            string filePath = Path.Combine(folderPath, "appsettings.json");
            
            // Read the JSON file content
            string jsonContent = File.ReadAllText(filePath);

            AppGroupSettings appGroupSettings = JsonConvert.DeserializeObject<AppGroupSettings>(jsonContent);

            // Process AppGroups
            foreach (var appGroup in appGroupSettings.AppGroups)
            {
                // Dim app groups
                ExecuteSqlCommand($"INSERT INTO keys.dim_app_groups (group_name) VALUES('{appGroup.Name}')");
            }

            // Eula
            ExecuteSqlCommand($"INSERT INTO keys.dim_eula (eula_title, eula_body, create_dt, modified_dt, from_dt) VALUES('{appGroupSettings.Eula.Title}', '{appGroupSettings.Eula.Body}', getdate(), getdate(), getdate())");

            // Process Apps
            foreach (var app in appGroupSettings.Apps)
            {
                int groupId = QueryDatabaseInt($"SELECT group_id FROM {horizonDatabase}.keys.dim_app_groups where group_name = '{app.GroupName}'");

                // Dim apps
                ExecuteSqlCommand($"INSERT INTO keys.dim_app_ids VALUES({app.Id}, '{app.Name}', {groupId})");

                // Announcements
                foreach (var announcement in app.Announcements) {
                    ExecuteSqlCommand($"INSERT INTO keys.dim_announcements (announcement_title, announcement_body, create_dt, modified_dt, from_dt, app_id) VALUES('{announcement.Title}', '{announcement.Body}', getdate(), getdate(), getdate(), {app.Id})");
                }

                // Server Settings
                ExecuteSqlCommand($"INSERT INTO keys.server_settings (app_id, name, value) VALUES({app.Id}, 'EnableEncryption', '{app.ServerSettings.EnableEncryption}')");
                ExecuteSqlCommand($"INSERT INTO keys.server_settings (app_id, name, value) VALUES({app.Id}, 'CreateAccountOnNotFound', '{app.ServerSettings.CreateAccountOnNotFound}')");
                ExecuteSqlCommand($"INSERT INTO keys.server_settings (app_id, name, value) VALUES({app.Id}, 'ClientLongTimeoutSeconds', '{app.ServerSettings.ClientLongTimeoutSeconds}')");
                ExecuteSqlCommand($"INSERT INTO keys.server_settings (app_id, name, value) VALUES({app.Id}, 'ClientTimeoutSeconds', '{app.ServerSettings.ClientTimeoutSeconds}')");
                ExecuteSqlCommand($"INSERT INTO keys.server_settings (app_id, name, value) VALUES({app.Id}, 'DmeTimeoutSeconds', '{app.ServerSettings.DmeTimeoutSeconds}')");
                ExecuteSqlCommand($"INSERT INTO keys.server_settings (app_id, name, value) VALUES({app.Id}, 'KeepAliveGracePeriodSeconds', '{app.ServerSettings.KeepAliveGracePeriodSeconds}')");
                ExecuteSqlCommand($"INSERT INTO keys.server_settings (app_id, name, value) VALUES({app.Id}, 'GameTimeoutSeconds', '{app.ServerSettings.GameTimeoutSeconds}')");
                ExecuteSqlCommand($"INSERT INTO keys.server_settings (app_id, name, value) VALUES({app.Id}, 'TextFilterAccountName', '{app.ServerSettings.TextFilterAccountName}')");
            }

            // Process Locations
            foreach (var location in appGroupSettings.Locations)
            {
                ExecuteSqlCommand($"INSERT INTO world.locations VALUES({location.Id},{location.AppId},'{location.Name}')");
            }

            // Process Channels
            foreach (var channel in appGroupSettings.Channels)
            {
                ExecuteSqlCommand($"INSERT INTO world.channels VALUES({channel.Id},{channel.AppId},'{channel.Name}',{channel.MaxPlayers},{channel.GenericField1},{channel.GenericField2},{channel.GenericField3},{channel.GenericField4},{channel.GenericFieldFilter})");           
            }

        }

        public void ExecuteSqlScript(string filePath)
        {
            string sqlScript = File.ReadAllText(filePath);
            string[] sqlCommands = sqlScript.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var command in sqlCommands)
            {
                if (command.Contains("CREATE DATABASE", StringComparison.OrdinalIgnoreCase) || command.Contains("ALTER DATABASE", StringComparison.OrdinalIgnoreCase))
                {
                    // Execute CREATE DATABASE outside of the transaction
                    Console.WriteLine($"Executing CREATE/ALTER database: {command}");
                    context.Database.ExecuteSqlRaw(command);
                }
            }

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    foreach (var command in sqlCommands)
                    {
                        if (!(command.Contains("CREATE DATABASE", StringComparison.OrdinalIgnoreCase) || command.Contains("ALTER DATABASE", StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrWhiteSpace(command))
                        {
                            Console.WriteLine($"Executing command: {command}");
                            context.Database.ExecuteSqlRaw(command);
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
            
            Console.WriteLine("SQL script executed successfully.");
        }

        private int QueryDatabaseInt(string sql)
        {               
            Console.WriteLine($"Querying database with: {sql}");
            var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;
            context.Database.OpenConnection();
            var result = (int)command.ExecuteScalar();
            context.Database.CloseConnection();
            return (int)result;
        }

        public void ExecuteSqlCommand(string command) {
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    UseHorizonDb();

                    Console.WriteLine($"Executing command: {command}");
                    context.Database.ExecuteSqlRaw(command);
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void UseHorizonDb() {
            string use_db_command = $"use [{horizonDatabase}]";

            Console.WriteLine($"Executing command: {use_db_command}");
            context.Database.ExecuteSqlRaw(use_db_command);
        }

        public static string ComputeSHA256(string input)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));

                return builder.ToString();
            }
        }



    }
}