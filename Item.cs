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
        public event EventHandler ItemChanged;
        public string itemName { get; set; }
        public int itemAmount { get; set; }

        public void eventInvocation()
        {
            ItemChanged?.Invoke(this, EventArgs.Empty);
        }
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
                if (inventory[position].itemAmount > 0)
                {
                    inventory[position].itemAmount--;
                    inventory[position].eventInvocation();
                }
                if (inventory[position].itemAmount == 0)
                {
                    //Warn user that there's nothing left, and ask if they want to clear it.
                    //if they say yes, clear item from list and visual list
                    //else leave it be.
                }
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