using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Azure.Cosmos;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    /// <summary>
    /// Main class instance for global credentials while application is running
    /// </summary>
    public static class UserData
    {
        //Username and password used for AuthLogin
        public static string username { get; set; }
        public static string password { get; set; }
        
        //Guid key used for database logging
        public static Guid key { get; set; }

        //Bitmap is used for profile picture. Is drawn during runtime in activity with Drawable functions
        public static Android.Graphics.Bitmap pfp { get; set; }

        /// <summary>
        /// Sets username and password instance to input for login screen
        /// </summary>
        /// <param name="user">The username</param>
        /// <param name="pass">The password</param>
        public static void SetCredentials(string user, string pass)
        {
            username = user;
            password = pass;
        }

        /// <summary>
        /// Reads the local credential file and sets it to username and password instance
        /// </summary>
        /// <returns></returns>
        public static bool ReadLoginInfo()
        {
            var destination = Path.Combine(Application.Context.GetExternalFilesDir(null).ToString(), "nani.fun");
            if (!File.Exists(destination))
                return false;
            string rawData = File.ReadAllText(destination);
            DataStruct ds = JsonConvert.DeserializeObject<DataStruct>(rawData);
            username = ds.username;
            password = ds.password;
            //Handle any auth here
            //Login();
            return true;
        }

        /// <summary>
        /// Writes a local credential file with the current username and password instances
        /// </summary>
        public static void SaveLoginInfo()
        {
            var destination = Path.Combine(Application.Context.GetExternalFilesDir(null).ToString(), "nani.fun");
            if (!File.Exists(destination))
                File.Create(destination);
            DataStruct ds = new DataStruct();
            ds.username = username;
            ds.password = password;
            string raw = JsonConvert.SerializeObject(ds);
            File.WriteAllText(destination, raw);
        }

        /// <summary>
        /// Used to serialize and pass username and password to device via Bluetooth
        /// </summary>
        public struct DataStruct
        {
            public string username;
            public string password;
        }

        /// <summary>
        /// Gets the same UserData info, however in different formats and names for database storage
        /// </summary>
        public struct AuthDB
        {
            public string id { get; set; }
            public string password { get; set; }
            public Guid key { get; set; }
            public string pfp { get; set; }
        }
    }

    /// <summary>
    /// Handles all Database tasks
    /// </summary>
    class DatabaseManager
    {
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = "https://iapyxretrofitapp.documents.azure.com:443/";
        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = "zGVFeZfK22QFtw7H8YMQ3ms6sy0UGpniSwzD2E0SmcCDwK45lSSYwGQideqvwY2PK9VS0qKkBL4myDG5Obqepg==";

        // The Cosmos client instance
        private static CosmosClient client;

        // The database instance
        private static Database database;

        // The container instance
        private static Container container;

        public static bool isOnline = true;

        // The name of the database
        private static string databaseId = "Global";

        //The Authentication container ID and normal logging container ID respectively
        private static string authContainerId = "AuthBase";
        private static string logContainerId = "ReportedData";

        /// <summary>
        /// Fetches client, database, and container values for AuthBase (Authentification)
        /// </summary>
        /// <returns></returns>
        public static async Task GetAuthDBInfo()
        {
            try
            {
                //Fetches client if it doesn't exist yet
                if(client != null)
                    client = new CosmosClient(EndpointUri, PrimaryKey);
                //Fetches database if it doesn't exist yet
                if(database != null)
                    database = client.GetDatabase(databaseId);
                //Fetches auth container
                container = database.GetContainer(authContainerId);
                if (container != null)
                    isOnline = true;
            }
            catch (Exception e)
            {
                isOnline = false;
            }
        }

        /// <summary>
        /// Fetches client, database, and container values for ReportedData (Logging)
        /// </summary>
        /// <returns></returns>
        public static async Task GetLogDBInfo()
        {
            //Fetches client if it doesn't exist yet
            if (client != null)
                client = new CosmosClient(EndpointUri, PrimaryKey);
            //Fetches database if it doesn't exist yet
            if (database != null)
                database = client.GetDatabase(databaseId);
            //Fetches log container
            container = await database.CreateContainerIfNotExistsAsync(logContainerId, partitionKeyPath: "/id", throughput: 400);

            if (container != null)
                isOnline = true;
        }

        /// <summary>
        /// Queries AuthBase with Username and Password. If it gets a return, it takes key and pfp, storing it in UserData instance
        /// </summary>
        /// <returns>
        /// true = successful login
        /// false = unsuccessful login
        /// </returns>
        public static async Task<bool> CheckLogin()
        {
            try
            {
                //Creates query with username and password for AuthBase
                string rawQuery = "SELECT * FROM AuthBase ab WHERE ab.id = \"" + UserData.username + "\" AND ab.password = \"" + UserData.password + "\"";
                QueryDefinition query = new QueryDefinition(rawQuery);
                using FeedIterator<UserData.AuthDB> queryResult = container.GetItemQueryIterator<UserData.AuthDB>(query);

                //Creates creds object to accept database items
                UserData.AuthDB creds = new UserData.AuthDB();

                //Results from query
                FeedResponse<UserData.AuthDB> resultSet = await queryResult.ReadNextAsync();


                foreach (UserData.AuthDB item in resultSet)
                {
                    //Assigns read item from resultSet as creds object
                    creds = item;

                    //Double checks passwords again just in case
                    if (UserData.password == creds.password)
                    {
                        //Assigns Guid key for logging
                        UserData.key = creds.key;

                        //Assigns pfp Bitmap if it exists, or else makes it default picture
                        if (creds.pfp != null)
                        {
                            byte[] picRaw = Convert.FromBase64String(creds.pfp);
                            UserData.pfp = Android.Graphics.BitmapFactory.DecodeByteArray(picRaw, 0, picRaw.Length);
                        }
                        else
                            UserData.pfp = null;

                        //Login successful
                        return true;
                    }
                }
                //Login unsuccessful
                return false;
            }
            catch (CosmosException ce)
            {
                //Some sort of error occured and login failed
                return false;
            }
        }

        /// <summary>
        /// Registers a new account with the username, password, and base64 string of the profile picture
        /// </summary>
        /// <param name="user">Username</param>
        /// <param name="password">Password</param>
        /// <param name="picString">Profile Picture represented as Base64 string</param>
        /// <returns></returns>
        public static async Task<bool> Register(string user, string password, string picString = null)
        {
            await DatabaseManager.GetAuthDBInfo();
            try
            {
                //Creates AuthDB object to log information to database
                UserData.AuthDB cred = new UserData.AuthDB();
                //Passed in user and password are set as id and password
                cred.id = user;
                cred.password = password;
                //New Guid is generated for the account
                cred.key = Guid.NewGuid();
                //pfp is assign to either a string, or null if no picture is chosen
                cred.pfp = picString;
                
                //Sends cred object to database
                ItemResponse<UserData.AuthDB> response;
                try
                {
                    response = await container.CreateItemAsync<UserData.AuthDB>(cred);
                }
                catch (CosmosException ce)
                {
                    //Most likely already exists or some other error was hit.
                    return false;
                }

                //Assigns new information in UserData instance
                UserData.username = user;
                UserData.password = password;
                UserData.key = cred.key;
                if(picString != null)
                {
                    byte[] bytes = Convert.FromBase64String(picString);
                    UserData.pfp = Android.Graphics.BitmapFactory.DecodeByteArray(bytes,0,bytes.Length);
                }
                else
                {
                    UserData.pfp = null;
                }
                //Successful Account Registration
                return true;
            }
            catch (CosmosException ce)
            {
                //Some sort of Failure, account registration unsuccessful
                return false;
            }
        }

        /// <summary>
        /// Writes Inventory List to Database
        /// </summary>
        /// <param name="inventory">The list of items to write to the database</param>
        /// <returns></returns>
        public static async Task WriteToDB(List<Item> inventory)
        {
            try
            {
                //Creates item logging object and populates it
                ItemDB items = new ItemDB();
                //items.id = UserData.username + " Inventory";
                items.id = UserData.key +" Inventory";
                items.updatedTime = System.DateTime.Now;
                items.currentInventory = inventory;

                //Attempts to replace item if it exists, or else it creates a new item
                ItemResponse<ItemDB> response;
                try
                {
                    response = await container.ReplaceItemAsync<ItemDB>(items, items.id);
                }
                catch (CosmosException ce)
                {
                    response = await container.CreateItemAsync<ItemDB>(items);
                }
            }
            catch (Exception ce)
            {
                //Some error was hit
                return;
            }
        }

        /// <summary>
        /// Reads Inventory List from Database
        /// </summary>
        /// <returns>
        /// ItemDB = object with database list attached
        /// </returns>
        public static async Task<ItemDB> ReadItemsFromDB()
        {
            try
            {
                //Queries the database for the inventory list
                string rawQuery = "SELECT * FROM ReportedData r WHERE r.id = \"" + UserData.key + " Inventory\"";
                //string rawQuery = "SELECT * FROM ReportedData r WHERE r.id = \"" + UserData.username + " Inventory\"";
                QueryDefinition query = new QueryDefinition(rawQuery);

                //Gets result of query
                using FeedIterator<ItemDB> queryResult = container.GetItemQueryIterator<ItemDB>(query);

                //Creating new itemDB object to populate with results
                ItemDB items = new ItemDB();

                //Populates itemDB object from results
                while (queryResult.HasMoreResults)
                {
                    FeedResponse<ItemDB> resultSet = await queryResult.ReadNextAsync();
                    foreach (ItemDB item in resultSet)
                    {
                        items = item;
                    }
                }

                //Returns itemDB for processing
                return items;
            }
            catch (Exception e)
            {
                //Some error has occurred
                return null;
            }
        }

        /// <summary>
        /// Reads Fridge Status from Database
        /// </summary>
        /// <returns>
        /// StatusDB = object with database status attached
        /// </returns>
        public static async Task<StatusDB> ReadStatusFromDB()
        {
            try
            {
                //Queries database for the status values
                string rawQuery = "SELECT * FROM ReportedData r WHERE r.id = \"" + UserData.key + " Status\"";
                //string rawQuery = "SELECT * FROM ReportedData r WHERE r.id = \"" + UserData.username + " Status\"";
                QueryDefinition query = new QueryDefinition(rawQuery);

                //Gets result of query
                using FeedIterator<StatusDB> queryResult = container.GetItemQueryIterator<StatusDB>(query);

                //Sets up statusDB object to be populated
                StatusDB status = new StatusDB();

                //Populates statusDB object
                while (queryResult.HasMoreResults)
                {
                    FeedResponse<StatusDB> resultSet = await queryResult.ReadNextAsync();
                    foreach (StatusDB item in resultSet)
                    {
                        status = item;
                    }
                }
                //Returns statusDB object for processing
                return status;
            }
            catch(Exception e)
            {
                //No longer connected to the internet!!!
                return null;
            }
        }
    }
}