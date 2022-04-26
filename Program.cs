using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;


namespace profiles
{
    class HttpServer
    {

        public static HttpListener? Listener;

        public static async Task HandleIncomingConnections(IConfigurationRoot config, EasyMango.EasyMango database)
        {
            while (true)
            {
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await Listener?.GetContextAsync()!;

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                // Print out some info about the request
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.Url?.ToString());
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);

                List<Token> Lexed = Lexer.Lex(req.Url?.AbsolutePath);
                if (req.HttpMethod == "GET" && Lexed.Count >= 2 && Lexed[0].Str == "profile")
                {
                    string TargetUser = Lexed[1].Str;
                    if (database.GetSingleDatabaseEntry("username", TargetUser, out BsonDocument TargetUserDocument))
                    {
                        Response.Success(resp, "Profile provided", TargetUserDocument.GetElement("name"), TargetUserDocument.GetElement("ProfilePicture"));
                    }
                    else
                    {
                        Response.Fail(resp, "User not found");
                    }
                }
                else
                {
                    Response.Fail(resp, "invalid body");
                }
                
                
                string jsonString = JsonConvert.SerializeObject(response);
                byte[] data = Encoding.UTF8.GetBytes(jsonString);

                resp.ContentType = "application/json";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                // Write out to the response stream (asynchronously), then close it
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }


        public static void Main(string[] args)
        {
            // Build the configuration for the env variables
            IConfigurationRoot config =
                new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true)
                    .AddEnvironmentVariables()
                    .Build();

            // Create a Http server and start listening for incoming connections
            string url = "http://*:" + config.GetValue<String>("Port") + "/";
            Listener = new HttpListener();
            Listener.Prefixes.Add(url);
            Listener.Start();
            Console.WriteLine("Listening for connections on {0}", url);

            string connectionString = config.GetValue<String>("connectionString");
            string databaseNAme = config.GetValue<String>("databaseName");
            string collectionName = config.GetValue<String>("collectionName");


            // Create a new EasyMango database
            EasyMango.EasyMango database = new EasyMango.EasyMango(connectionString, databaseNAme, collectionName);

            // Handle requests
            Task listenTask = HandleIncomingConnections(config, database);
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            Listener.Close();
        }
    }
}