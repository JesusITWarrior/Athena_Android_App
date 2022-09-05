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
    class Item
    {
        public string itemName { get; set; }
        public int itemAmount { get; set; }
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
            Button add = row.FindViewById<Button>(Resource.Id.addItemButton);
            TextView txtAmount = row.FindViewById<TextView>(Resource.Id.itemAmount);

            txtName.Text = inventory[position].itemName;
            txtAmount.Text = inventory[position].itemAmount.ToString();

            return row;
        }
    }

}