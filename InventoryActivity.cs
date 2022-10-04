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
using Newtonsoft.Json;
using System.IO;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    /// <summary>
    /// Handles all Inventory Activity
    /// </summary>
    [Activity(Label = "InventoryActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class InventoryActivity : Activity
    {
        
        string filename = "inv-list.txt";
        List<Item> inventory;
        private ListView lv;
        
        /// <summary>
        /// Run when Activity is first started. Will make initial setup.
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            
            SetContentView(Resource.Layout.inventory);
            
            //Sets up references to all components seen
            TextView title = FindViewById<TextView>(Resource.Id.ListTitle);
            Button addButton = FindViewById<Button>(Resource.Id.addToList);
            
            //Subscribes addButton click event to AddNewItem function
            addButton.Click += AddNewItem;

            lv = FindViewById<ListView>(Resource.Id.inventory);
            inventory = new List<Item>();

            //Initializes most up-to-date list
            InitList();
            //As long as there is an inventory, it will update inventory. Otherwise, it will hide it.
            if (inventory.Count != 0)
            {
                ItemListViewAdapter adapter = new ItemListViewAdapter(this, inventory);
                lv.Adapter = adapter;
            }
        }

        /// <summary>
        /// Initializes the most up-to-date inventory list, whether that is in database or local file
        /// </summary>
        public async void InitList()
        {
            //variables to be compared for most recent
            DateTime fileTime = DateTime.MinValue, dbTime = DateTime.MinValue;
            //ItemFile struct for reading
            ItemFile itemFile = new ItemFile();
            //ItemDB class for reading
            ItemDB databaseList = new ItemDB();
            var destination = Path.Combine(Application.Context.GetExternalFilesDir(null).ToString(), filename);
            //If the file exists, then read it.
            if (File.Exists(destination))
            {
                //Read the JSON from file and convert it to object
                string rawJson = File.ReadAllText(destination);
                if (rawJson != "")
                {
                    itemFile = JsonConvert.DeserializeObject<ItemFile>(rawJson);
                    //inventory = itemFile.currentInventory;
                    fileTime = itemFile.updatedTime;
                }
            }
            //If file doesn't exist, create one right now.
            else
            {
                File.Create(destination);
            }

            try
            {
                //Try to fetch list from database
                if (!DatabaseManager.isOnline)
                    await DatabaseManager.GetLogDBInfo();
                databaseList = await DatabaseManager.ReadItemsFromDB();
                //TODO: Show loading
                dbTime = databaseList.updatedTime;
            }
            catch (Exception e)
            {
                //Failed to get anything from database, probably offline
                DatabaseManager.isOnline = false;
            }
            //If we are online, we proceed with check as normal, updating list to be whatever is most recent up-to-date version is
            if (DatabaseManager.isOnline)
                inventory = (fileTime >= dbTime) ? itemFile.currentInventory : databaseList.currentInventory;
            //Otherwise, we only use local file
            else
                inventory = itemFile.currentInventory;
            
            if (inventory == null)
                inventory = new List<Item>();
            
            //Adds all necessary events to each item and then makes sure the view is synchronized with the inventory file/database
            AddItemEvents();
            UpdateView();
        }

        /// <summary>
        /// Writes the Inventory to the file
        /// </summary>
        public void WriteListToFile()
        {
            //Gets path, and creates file if it doesn't exist
            var destination = Path.Combine(Application.Context.GetExternalFilesDir(null).ToString(), filename);
            if (!File.Exists(destination))
                File.Create(destination);

            //Creates ItemFile object for writing
            ItemFile itemFile = new ItemFile();
            itemFile.updatedTime = System.DateTime.Now;
            itemFile.currentInventory = inventory;

            //Converts object to JSON string and writes/overwrites it to the file.
            string rawJson = JsonConvert.SerializeObject(itemFile);
            File.WriteAllText(destination, rawJson);
        }

        /// <summary>
        /// Updates the view of the inventory list
        /// </summary>
        /// <param name="i"></param>
        public void UpdateView(Item i=null)
        {
            if (i != null)
            {
                //Removes item from list entirely if it reaches 0
                if (i.itemAmount == 0)
                {
                    inventory.Remove(i);
                }
            }

            //Writes all changes to local file and database
            WriteListToFile();
            DatabaseManager.WriteToDB(inventory);

            //Adapts ListView to entire list
            ItemListViewAdapter adapter = new ItemListViewAdapter(this, inventory);
            lv.Adapter = adapter;
        }

        /// <summary>
        /// Subscribes all item events in inventory to UpdateView
        /// </summary>
        public void AddItemEvents()
        {
            foreach (Item i in inventory)
            {
                i.ItemChanged += UpdateView;
            }
        }

        /// <summary>
        /// Adds new item to list and sorts it alphabetically
        /// </summary>
        /// <param name="o">Button that ran this</param>
        /// <param name="e">Empty</param>
        public void AddNewItem(object o, EventArgs e)
        {
            //Fetches name of item from the input field
            string itemToAdd = FindViewById<TextView>(Resource.Id.itemInput).Text;
            bool foundIt = false;
            //Checks to see really quickly if that item already exists
            for (int i = 0; i < inventory.Count; i++)
            {
                //Found item with same name, adding one to it
                if (inventory[i].itemName == itemToAdd)
                {
                    inventory[i].itemAmount++;
                    foundIt = true;
                    break;
                }
            }
            //If item is brand new item, add it to the list
            if (!foundIt)
            {
                //Creates Item object and subscribes event to UpdateView
                Item newItem = new Item();
                newItem.itemName = itemToAdd;
                newItem.itemAmount = 1;
                newItem.ItemChanged += UpdateView;

                //Adds and sorts item alphabetically to list
                inventory.Add(newItem);
                inventory.Sort((x,y) => string.Compare(x.itemName, y.itemName));
            }

            UpdateView();
            /*WriteListToFile();
            DatabaseManager.WriteToDB(inventory);
            ItemListViewAdapter adapter = new ItemListViewAdapter(this, inventory);
            lv.Adapter = adapter;*/
        }
    }
}