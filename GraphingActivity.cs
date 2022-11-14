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
using AndroidX.AppCompat.App;
using Android.Webkit;
using Newtonsoft.Json;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    [Activity(Label = "GraphingActivity")]
    public class GraphingActivity : AppCompatActivity
    {
        public enum GraphType
        {
            ColumnChart,
            BarChart,
            LineChart,
            Table
        }
        public enum SortType
        {
            Default,
            Entries,
            Date
        }

        GraphType graphType = GraphType.ColumnChart;
        SortType sortType = SortType.Default;
        DateTime? oldDate = null;
        int entries = 1;
        WebView graphView;
        string databaseData;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.usage_graph);
            if (!DatabaseManager.isOnline)
            {
                //Don't load any graphs and warn user!
                return;
            }
            graphView = FindViewById<WebView>(Resource.Id.graph);
            WebSettings settings = graphView.Settings;
            settings.BuiltInZoomControls = true;
            settings.JavaScriptEnabled = true;
            graphView.SetWebViewClient(new WebViewClient());
            graphView.LoadUrl("file:///android_asset/chart.html");
            //graphView.AddJavascriptInterface();

            GetFromDB();
            graphView.LoadUrl(string.Format("javascript: drawAthenaChart(\"{0}\",{1})", databaseData, (int)graphType));
            Button submitButton = FindViewById<Button>(Resource.Id.confirmButton);
            Spinner graphChoice = FindViewById<Spinner>(Resource.Id.graphType);
            Spinner sortChoice = FindViewById<Spinner>(Resource.Id.sortType);

            ArrayAdapter graphAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.graphTypes, Android.Resource.Layout.SimpleSpinnerItem);
            graphAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            graphChoice.Adapter = graphAdapter;

            ArrayAdapter sortAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.sortTypes, Android.Resource.Layout.SimpleSpinnerItem);
            sortAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            sortChoice.Adapter = sortAdapter;

            submitButton.Click += (o, e) =>
            {
                switch (sortType) {
                    case SortType.Default:
                        GetFromDB();
                        break;
                    case SortType.Date:
                        GetFromDB(sortType, oldDate);
                        break;
                    case SortType.Entries:
                        GetFromDB(entries);
                        break;
                }
                graphView.LoadUrl(string.Format("javascript: drawAthenaChart(\"{0}\",{1})", databaseData, (int)graphType));
            };
        }

        private void SortTypeChanged(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            if (spinner.Id == Resource.Id.sortType)
            {
                sortType = (SortType)e.Position;
                DatePicker date1 = FindViewById<DatePicker>(Resource.Id.oldDate);
                DatePicker date2 = FindViewById<DatePicker>(Resource.Id.newDate);
                TextView entries = FindViewById<TextView>(Resource.Id.entriesInput);
                switch (sortType)
                {
                    case SortType.Default:
                        date1.Visibility = ViewStates.Gone;
                        date2.Visibility = ViewStates.Gone;
                        entries.Visibility = ViewStates.Gone;
                        break;
                    case SortType.Date:
                        date1.Visibility = ViewStates.Visible;
                        date2.Visibility = ViewStates.Visible;
                        entries.Visibility = ViewStates.Gone;
                        break;
                    case SortType.Entries:
                        date1.Visibility = ViewStates.Gone;
                        date2.Visibility = ViewStates.Gone;
                        entries.Visibility = ViewStates.Visible;
                        break;
                }
            }

        }

        private void GraphTypeChanged(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            Spinner spinner = (Spinner)sender;
            if(spinner.Id == Resource.Id.graphType)
                graphType = (GraphType)e.Position;
        }

        private async void GetFromDB(SortType sort = SortType.Default, DateTime? oldDate = null)
        {
            switch (sort) {
                case SortType.Default:
                    {
                        List<GraphStatusDB> data = await DatabaseManager.ReadStatusesFromDB(DateTime.Now);
                        databaseData = "";
                        databaseData = JsonConvert.SerializeObject(data);
                        databaseData = databaseData.Replace("\"", "\'");
                        break;
                    }
                case SortType.Date:
                    {
                        List<GraphStatusDB> data = await DatabaseManager.ReadStatusesFromDB(DateTime.Now, oldDate);
                        databaseData = "";
                        databaseData = JsonConvert.SerializeObject(data);
                        databaseData = databaseData.Replace("\"", "\'");
                        break;
                    }
            }
        }

        private async void GetFromDB(int entries, SortType sort = SortType.Entries)
        {
            List<GraphStatusDB> data = await DatabaseManager.ReadStatusesFromDB(entries);
            databaseData = "";
            databaseData = JsonConvert.SerializeObject(data);
            databaseData = databaseData.Replace("\"", "\'");
        }
    }
    public class GraphStatusDB
    {
        public string updatedTime { get; set; }
        public bool DoorOpenStatus { get; set; }
        public int Temperature { get; set; }
    }
}