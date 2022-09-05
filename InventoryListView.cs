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
    class InventoryListView
    {
        public string name;
        public int image;

        public InventoryListView(string name, int image)
        {
            this.name = name;
            this.image = image;
        }
    }
    class ListToListView : BaseAdapter<InventoryListView> {
        public List<InventoryListView> items;
        private Context context;

        public ListToListView(Context context, List<InventoryListView> items)
        {
            this.context = context;
            this.items = items;
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override InventoryListView this[int position]
        {
            get { return items[position]; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;

            if (row == null)
            {
                row = LayoutInflater.From(context).Inflate(Resource.Layout.list_view, null, false);
            }

            TextView txtName = row.FindViewById<TextView>(Resource.Id.itemName);
            txtName.Text = items[position].name;

            return row;
        }
    }

    class ListToListViewTest : BaseAdapter<string>
    {
        public List<string> mItems;
        private Context mContext;

        public ListToListViewTest(Context context, List<string> items)
        {
            mItems = items;
            mContext = context;
        }

        public override int Count
        {
            get { return mItems.Count; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override string this[int position]
        {
            get { return mItems[position]; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View row = convertView;

            if(row == null)
            {
                row = LayoutInflater.From(mContext).Inflate(Resource.Layout.list_view, null, false);
            }

            TextView txtName = row.FindViewById<TextView>(Resource.Id.itemName);
            txtName.Text = mItems[position];

            return row;
        }
    }

}