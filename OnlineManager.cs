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
            var destination = Path.Combine(Application.Context.GetExternalFilesDir(null).ToString(), "nani.athena");
            if (!File.Exists(destination))
                return false;
            string rawData = File.ReadAllText(destination);
            DataStruct ds = JsonConvert.DeserializeObject<DataStruct>(rawData);
            username = ds.username;
            password = ds.password;
            key = ds.key;
            //Handle any auth here
            //Login();
            return true;
        }

        /// <summary>
        /// Writes a local credential file with the current username and password instances
        /// </summary>
        public static void SaveLoginInfo()
        {
            var destination = Path.Combine(Application.Context.GetExternalFilesDir(null).ToString(), "nani.athena");
            DataStruct ds = new DataStruct();
            ds.username = username;
            ds.password = password;
            ds.key = key;
            string raw = JsonConvert.SerializeObject(ds);
            File.WriteAllText(destination, raw);
        }

        /// <summary>
        /// Used to serialize and pass username and password to device via Bluetooth
        /// </summary>
        public struct DataStruct
        {
            public string username { get; set; }
            public string password { get; set; }
            public Guid key { get; set; }
        }

        /// <summary>
        /// Gets the same UserData info, however in different formats and names for database storage
        /// </summary>
        public struct AuthDB
        {
            public string id { get; set; }
            public string password { get; set; }
            public Guid key { get; set; }
            public string picUUID { get; set; }
            public string pfp { get; set; }
        }
    }

    /// <summary>
    /// Handles all Database tasks
    /// </summary>
    class DatabaseManager
    {
        public enum RecordType
        {
            INV,
            PIC,
            STATUS
        }
        // The Azure Cosmos DB endpoint for running this sample.
        private static readonly string EndpointUri = "https://capstonetamu.documents.azure.com:443/";
        // The primary key for the Azure Cosmos account.
        private static readonly string PrimaryKey = "qOCK60HdQU4oEJx2xop6D4DI4tqXu7EotJxvoOuUFH6DKJlDf546ZHH8aHAEM6CEtm4r5rc0MrEm0BA2HO90jQ==";

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
        public static void GetAuthDBInfo()
        {
            try
            {
                //Fetches client if it doesn't exist yet
                if(client == null)
                    client = new CosmosClient(EndpointUri, PrimaryKey);
                //Fetches database if it doesn't exist yet
                if(database == null)
                    database = client.GetDatabase(databaseId);
                //Fetches auth container
                container = database.GetContainer(authContainerId);
                if (container != null)
                    isOnline = true;
            }
            catch
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
            catch
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
            DatabaseManager.GetAuthDBInfo();
            try
            {
                //Creates AuthDB object to log information to database
                UserData.AuthDB cred = new UserData.AuthDB();
                //Passed in user and password are set as id and password
                cred.id = user;
                cred.password = password;
                //New Guid is generated for the account
                cred.key = Guid.NewGuid();
                cred.picUUID = null;
                //pfp is assign to either a string, or null if no picture is chosen
                cred.pfp = picString;
                
                //Sends cred object to database
                ItemResponse<UserData.AuthDB> response;
                try
                {
                    response = await container.CreateItemAsync<UserData.AuthDB>(cred);
                }
                catch
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
        public static async Task WriteToDB(InventoryDB inventory)
        {
            try
            {
                //string itemsToBeSelected = "SELECT r.id FROM r";
                //string where = " WHERE r.accountID = \'" + UserData.key + "\' AND r.recordType = \'inventory\'";
                //Creates item logging object and populates it
                //string rawquery = itemsToBeSelected + where;
                //Attempts to replace item if it exists, or else it creates a new item
                ItemResponse<InventoryDB> response;
                try
                {
                    response = await container.ReplaceItemAsync<InventoryDB>(inventory, inventory.id);
                }
                catch
                {
                    response = await container.CreateItemAsync<InventoryDB>(inventory);
                }
            }
            catch
            {
                //Some error was hit
                return;
            }
        }

        public static async Task<InventoryDB> ReadInventoryItemsFromDB()
        {
            InventoryDB inventory = new InventoryDB();
            string itemsToBeSelected = "SELECT r.id, r.inventory FROM r ";
            string where = "WHERE r.accountID = \'" + UserData.key + "\' AND r.recordType = \'inventory\'";
            try
            {
                //Queries the database for the inventory list
                string rawQuery = itemsToBeSelected + where;
                //string rawQuery = "SELECT * FROM ReportedData r WHERE r.id = \"" + UserData.username + " Inventory\"";
                QueryDefinition query = new QueryDefinition(rawQuery);



                //Gets result of query
                using FeedIterator<InvGuidAndItems> queryResult = container.GetItemQueryIterator<InvGuidAndItems>(query);
                //Creating new itemDB object to populate with results
                InvGuidAndItems items = new InvGuidAndItems();

                //Populates itemDB object from results
                while (queryResult.HasMoreResults)
                {
                    FeedResponse<InvGuidAndItems> resultSet = await queryResult.ReadNextAsync();
                    foreach (InvGuidAndItems item in resultSet)
                    {
                        inventory.id = item.id;
                        inventory.inventory = item.inventory;
                    }
                }

                if(inventory.id == null)
                {
                    inventory.id = Guid.NewGuid().ToString();
                }
                //Returns itemDB for processing
                return inventory;
            }
            catch
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
        public static async Task<StatusDB> ReadCurrentStatusFromDB(bool fetchPictureToo)
        {
            try
            {
                string itemsToBeSelected = "SELECT TOP 1 r.updatedTime, r.DoorOpenStatus, r.Temperature FROM r ";
                string where = "WHERE r.accountID = \'" + UserData.key + "\' AND r.recordType = \'status\'";
                string order = " ORDER BY r.updatedTime DESC";
                //Queries database for the status values
                string rawQuery = itemsToBeSelected+where+order;
                //string rawQuery = "SELECT * FROM ReportedData r WHERE r.id = \"" + UserData.username + " Status\"";
                QueryDefinition query = new QueryDefinition(rawQuery);

                StatusDB overall = new StatusDB();
                //Gets result of query
                using (FeedIterator<Status> queryResult = container.GetItemQueryIterator<Status>(query)) {

                    //Sets up statusDB object to be populated

                    //Populates statusDB object
                    FeedResponse<Status> resultSet = await queryResult.ReadNextAsync();
                    foreach (Status item in resultSet)
                    {
                        overall.updatedTime = item.updatedTime;
                        overall.DoorOpenStatus = item.DoorOpenStatus;
                        overall.Temperature = item.Temperature;
                    }
                }
                if (fetchPictureToo)
                {
                    itemsToBeSelected = "SELECT r.Picture FROM r ";
                    where = "WHERE r.accountID = \'" + UserData.key + "\' AND r.recordType = \'picture\'";

                    rawQuery = itemsToBeSelected + where + order;
                    query = new QueryDefinition(rawQuery);

                    using (FeedIterator<PictureDB> queryResult = container.GetItemQueryIterator<PictureDB>(query))
                    {
                        while (queryResult.HasMoreResults)
                        {
                            FeedResponse<PictureDB> resultSet = await queryResult.ReadNextAsync();
                            foreach (PictureDB item in resultSet)
                            {
                                overall.Picture = item.Picture;
                            }
                        }
                    }
                }
                else
                {
                    overall.Picture = null;
                }

                //Returns statusDB object for processing
                return overall;
            }
            catch
            {
                //No longer connected to the internet!!!
                return null;
            }
        }

        public static async Task<List<StatusDB>> ReadStatusesFromDB(int entries = 1)
        {
            try
            {
                string itemsToBeSelected = "SELECT TOP "+entries+" r.updatedTime, r.DoorOpenStatus, r.Temperature FROM r ";
                string where = "WHERE r.accountID = \'" + UserData.key + "\' AND r.recordType = \'status\'";
                string order = " ORDER BY r.updatedTime DESC";
                //Queries database for the status values
                string rawQuery = itemsToBeSelected + where + order;
                //string rawQuery = "SELECT * FROM ReportedData r WHERE r.id = \"" + UserData.username + " Status\"";
                QueryDefinition query = new QueryDefinition(rawQuery);

                List<StatusDB> overall = new List<StatusDB>();
                //Gets result of query
                using (FeedIterator<Status> queryResult = container.GetItemQueryIterator<Status>(query))
                {
                    int i = 0;
                    //Sets up statusDB object to be populated

                    //Populates statusDB object
                    while (queryResult.HasMoreResults)
                    {
                        FeedResponse<Status> resultSet = await queryResult.ReadNextAsync();
                        foreach (Status item in resultSet)
                        {
                            overall[i].updatedTime = item.updatedTime;
                            overall[i].DoorOpenStatus = item.DoorOpenStatus;
                            overall[i].Temperature = item.Temperature;
                        }
                        overall[i].Picture = null;
                        i++;
                    }
                }

                //Returns statusDB object for processing
                return overall;
            }
            catch
            {
                //No longer connected to the internet!!!
                return null;
            }
        }
    }
}