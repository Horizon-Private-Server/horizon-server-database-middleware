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

namespace Horizon.Database
{
    public class DatabaseChecker
    {

        private DbContext context = null;
        private string folderPath = null;

        public DatabaseChecker()
        {
            Console.WriteLine($"Checking if server is already initialized ...");

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
            var sql = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'";
            var result = context.Database.ExecuteSqlRaw(sql);

            return result > 0;
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

            }

        }

        public void CreateAdminUser() {
            Console.WriteLine("Creating admin middleware user ...");
            string middlewareAdminUser = Environment.GetEnvironmentVariable("HORIZON_MIDDLEWARE_USER");
            string middlewareAdminPassword = ComputeSHA256(Environment.GetEnvironmentVariable("HORIZON_MIDDLEWARE_PASSWORD"));

            string createAdmin = $@"
                INSERT INTO accounts.account (account_name, account_password, create_dt, is_active, app_id, reset_password_on_next_login)
                VALUES ('{middlewareAdminUser}', '{middlewareAdminPassword}', getdate(), 1, 0, 0);
            ";
            ExecuteSqlCommand(createAdmin);


            // Add database role 
            int roleId = (int)QueryDatabase("SELECT role_id FROM keys.roles where role_name = 'database'");
            int accountId = (int)QueryDatabase($"SELECT account_id FROM accounts.account where account_name = '{middlewareAdminUser}'");
            ExecuteSqlCommand($"INSERT INTO accounts.user_role VALUES({accountId}, {roleId}, GETDATE(), GETDATE(), null)");

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

        private var QueryDatabase(string sql)
        {
            var result = context.Database.ExecuteSqlRaw(sql);
            return result;
        }

        public void ExecuteSqlCommand(string command) {
            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    string db_name = Environment.GetEnvironmentVariable("HORIZON_DB_NAME");
                    string use_db_command = $"use [{db_name}]";

                    Console.WriteLine($"Executing command: {use_db_command}");
                    context.Database.ExecuteSqlRaw(use_db_command);

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