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
                    StreamReader reader = new StreamReader(req.InputStream);
                    string bodyString = await reader.ReadToEndAsync();
                    dynamic body = JsonConvert.DeserializeObject(bodyString)!;
                    
                    string AskingToken = ((string) body.token).Trim() ?? "";
                    string AskingUserid = WebToken.GetIdFromToken(AskingToken);
                    
                    string TargetUser = Lexed[1].Str;
                    // check if user asking have to permission de to so (for now to have the permission you just have to exist)
                    if (database.GetSingleDatabaseEntry("_id",new BsonObjectId(AskingUserid), out BsonDocument AskingUserDocument))
                    {
                        // check if target user exist in DB
                        if (database.GetSingleDatabaseEntry("username", TargetUser, out BsonDocument TargetUserDocument))
                        {
                            //build the data of the response request (a json file stringified)
                            string Avatar = TargetUserDocument.GetElement("avatar").Value.AsString;
                            string bio = TargetUserDocument.GetElement("bio").Value.AsString;
                            string color = TargetUserDocument.GetElement("color").Value.AsString;
                            string banner = TargetUserDocument.GetElement("banner").Value.AsString;
                            string id = TargetUserDocument.GetElement("_id").Value.ToString();
                            
                            Response.Success(resp, "Profile provided",Response.BuildData(TargetUser,Avatar,bio,banner,color,id));
                        }
                        else
                        {
                            Response.Fail(resp, "User not found");
                        }
                    }
                    else
                    {
                        Response.Fail(resp, "Unauthorized");
                    }
                }
                else
                {
                    Response.Fail(resp, "invalid body");
                }
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