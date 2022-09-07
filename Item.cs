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

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    public class Item
    {
        public delegate void Updater(Item i);
        public event Updater ItemChanged;
        public string itemName { get; set; }
        public int itemAmount { get; set; }

        public void eventInvocation()
        {
            ItemChanged?.Invoke(this);
        }
    }

    public class ItemDB
    {
        public string id { get; set; }
        public List<Item> currentInventory { get; set; }
    }

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

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;
            if(row == null)
            {
                row = LayoutInflater.From(ctx).Inflate(Resource.Layout.list_view, null, false);
            }

            TextView txtName = row.FindViewById<TextView>(Resource.Id.itemName);
            Button sub = row.FindViewById<Button>(Resource.Id.subtractItemButton);
            sub.Click += (o, e) =>
            {
                //get item here from list
                inventory[position].itemAmount--;
                inventory[position].eventInvocation();
            };
            Button add = row.FindViewById<Button>(Resource.Id.addItemButton);
            add.Click += (o, e) =>
            {
                //Should add items to this object
                inventory[position].itemAmount++;
                inventory[position].eventInvocation();
            };

            TextView txtAmount = row.FindViewById<TextView>(Resource.Id.itemAmount);

            txtName.Text = inventory[position].itemName;
            txtAmount.Text = inventory[position].itemAmount.ToString();

            return row;
        }
    }

}