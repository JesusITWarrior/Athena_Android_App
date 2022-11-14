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
using System.Drawing;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    /// <summary>
    /// Basic item object with name, amount, and event used locally
    /// </summary>
    public class Item
    {
        public delegate void Updater(Item i);
        public event Updater ItemChanged;
        public string itemName { get; set; }
        public int itemAmount { get; set; }

        /// <summary>
        /// Anytime a class needs to invoke the event attached, they call this function
        /// </summary>
        public void eventInvocation()
        {
            ItemChanged?.Invoke(this);
        }
    }

    public struct InvGuidAndItems
    {
        public string id { get; set; }
        public List<Item> inventory { get; set; }
    }

    /// <summary>
    /// Item object used for logging to the database. Holds id, time updated, and list of inventory
    /// </summary>
    public class InventoryDB
    {
        public string id { get; set; }
        public readonly string accountID = UserData.key.ToString();
        public readonly string recordType = "inventory";
        public DateTime updatedTime { get; set; }
        public List<Item> inventory { get; set; }
    }

    /// <summary>
    /// Item object used for logging to file. Holds time updated and list of inventory
    /// </summary>
    public struct ItemFile
    {
        public DateTime updatedTime { get; set; }
        public List<Item> currentInventory{ get; set; }
    }
    /// <summary>
    /// Basic Status object with name and value, used locally
    /// </summary>
    public struct Status
    {
        public DateTime updatedTime { get; set; }
        public bool DoorOpenStatus { get; set; }
        //public int[] Temperature { get; set; }
        public int Temperature { get; set; }
    }

    public struct PictureDB
    {
        public string Picture { get; set; }
    }

    /// <summary>
    /// Status object used for logging to database. Holds id, time updated, and List of status values
    /// </summary>
    public class StatusDB
    {
        public DateTime updatedTime { get; set; }
        public bool DoorOpenStatus { get; set; }
        //public int[] Temperature { get; set; }
        public int Temperature { get; set; }
        public string Picture { get; set; }
    }

    /// <summary>
    /// Status object used for logging to file. Holds time updated and list of status values.
    /// </summary>
    public struct StatusFile
    {
        public DateTime updatedTime { get; set; }
        public List<Status> loggedStatus { get; set; }
    }

    /// <summary>
    /// Used to Update ListView for Inventory
    /// </summary>
    class ItemListViewAdapter : BaseAdapter<Item> {
        public List<Item> inventory;
        private Context ctx;

        public ItemListViewAdapter(Context ctx, List<Item> list)
        {
            inventory = list;
            this.ctx = ctx;

        }

        public override int Count
        {
            get { return inventory.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override Item this[int position]
        {
            get { return inventory[position]; }
        }

        /// <summary>
        /// Populates ListView with the Inventory List
        /// </summary>
        /// <param name="position">List index</param>
        /// <param name="convertView">The ListView used</param>
        /// <param name="parent"></param>
        /// <returns>
        /// View = Updated ListView 
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            //Creates a space for the View
            if(convertView == null)
            {
                convertView = LayoutInflater.From(ctx).Inflate(Resource.Layout.list_view, null, false);
            }

            //Populates all List items
            TextView txtName = convertView.FindViewById<TextView>(Resource.Id.itemName);
            Button sub = convertView.FindViewById<Button>(Resource.Id.subtractItemButton);
            sub.Click += (o, e) =>
            {
                //Subtracts amount from Item itemAmount and invokes update change
                inventory[position].itemAmount--;
                inventory[position].eventInvocation();
            };
            Button add = convertView.FindViewById<Button>(Resource.Id.addItemButton);
            add.Click += (o, e) =>
            {
                //Adds amount from Item itemAmount and invokes update change
                inventory[position].itemAmount++;
                inventory[position].eventInvocation();
            };

            TextView txtAmount = convertView.FindViewById<TextView>(Resource.Id.itemAmount);

            txtName.Text = inventory[position].itemName;
            txtAmount.Text = inventory[position].itemAmount.ToString();

            //Returns convertView for Updating by the Activity
            return convertView;
        }
    }

}