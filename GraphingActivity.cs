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
            Button testButton = FindViewById<Button>(Resource.Id.testButton);
            testButton.Click += (o, e) =>
            {
                graphType = GraphType.LineChart;
                //graphView.LoadUrl(string.Format("javascript: drawChart({0},{1})", 1, 1));
                //graphView.LoadUrl("javascript:");
                databaseData = databaseData.Replace("\"", "\'");
                //graphView.EvaluateJavascript(string.Format("testFunction(\"{0}\")", databaseData), null);
                graphView.LoadUrl(string.Format("javascript: drawAthenaChart(\"{0}\",{1})", databaseData, (int)graphType));
            };
        }

        private async void GetFromDB()
        {
            List<GraphStatusDB> data = await DatabaseManager.ReadStatusesFromDB(DateTime.Now);
            databaseData = "";
            databaseData = JsonConvert.SerializeObject(data);
        }
    }
    public class GraphStatusDB
    {
        public string updatedTime { get; set; }
        public bool DoorOpenStatus { get; set; }
        public int Temperature { get; set; }
    }
}