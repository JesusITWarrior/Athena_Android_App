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
using AndroidX.Core.Graphics.Drawable;
using Android.Graphics;
using Google.Android.Material.TextField;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    [Activity(Label = "GraphingActivity")]
    public class GraphingActivity : AppCompatActivity, DatePickerDialog.IOnDateSetListener
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
            Date,
            Entries
        }

        GraphType graphType = GraphType.ColumnChart;
        SortType sortType = SortType.Default;
        DateTime newDate = DateTime.Now;
        DateTime? oldDate = null;
        int entries = 1;
        WebView graphView;
        string databaseData;

        Button submitButton, startDateButton, endDateButton;
        Button placeholder;
        LinearLayout startDate, endDate;
        Spinner graphChoice, sortChoice;
        TextView entriesText;
        TextInputLayout entriesLayout;

        int year, month, date;

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
            ImageButton pfp = FindViewById<ImageButton>(Resource.Id.pfp);
            if (UserData.pfp != null)
            {
                RoundedBitmapDrawable rbmpd = RoundedBitmapDrawableFactory.Create(Resources, UserData.pfp);
                rbmpd.Circular = true;
                pfp.SetImageDrawable(rbmpd);
            }
            else
            {
                Bitmap bmp = BitmapFactory.DecodeResource(Resources, Resource.Drawable.blank_profile_picture);
                RoundedBitmapDrawable rbmpd = RoundedBitmapDrawableFactory.Create(Resources, bmp);
                rbmpd.Circular = true;
                pfp.SetImageDrawable(rbmpd);
            }

            graphView = FindViewById<WebView>(Resource.Id.graph);
            WebSettings settings = graphView.Settings;
            settings.BuiltInZoomControls = true;
            settings.JavaScriptEnabled = true;
            graphView.SetWebViewClient(new WebViewClient());
            graphView.LoadUrl("file:///android_asset/chart.html");
            //graphView.AddJavascriptInterface();

            GetFromDB(DateTime.Now);
            graphView.LoadUrl(string.Format("javascript: drawAthenaChart(\"{0}\",{1})", databaseData, (int)graphType));

            //Find all components needed
            submitButton = FindViewById<Button>(Resource.Id.confirmButton);
            graphChoice = FindViewById<Spinner>(Resource.Id.graphType);
            graphChoice.ItemSelected += GraphTypeChanged;
            sortChoice = FindViewById<Spinner>(Resource.Id.sortType);
            sortChoice.ItemSelected += SortTypeChanged;

            startDateButton = FindViewById<Button>(Resource.Id.startDateButton);
            startDateButton.Click += ShowDate;
            endDateButton = FindViewById<Button>(Resource.Id.endDateButton);
            endDateButton.Click += ShowDate;

            startDate = FindViewById<LinearLayout>(Resource.Id.startDate);
            endDate = FindViewById<LinearLayout>(Resource.Id.endDate);

            entriesText = FindViewById<TextView>(Resource.Id.entriesText);
            entriesLayout = FindViewById<TextInputLayout>(Resource.Id.entriesLayout);

            ArrayAdapter graphAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.graphTypes, Android.Resource.Layout.SimpleSpinnerItem);
            graphAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            graphChoice.Adapter = graphAdapter;

            ArrayAdapter sortAdapter = ArrayAdapter.CreateFromResource(this, Resource.Array.sortTypes, Android.Resource.Layout.SimpleSpinnerItem);
            sortAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            sortChoice.Adapter = sortAdapter;

            //Button DatePicker1 = FindViewById<Button>(Resource.Id.);
            //Button DatePicker2 = FindViewById<Button>(Resource.Id.);

            submitButton.Click += (o, e) =>
            {
                switch (sortType) {
                    case SortType.Default:
                        GetFromDB(DateTime.Now);
                        break;
                    case SortType.Date:
                        GetFromDB((newDate != null) ? newDate : DateTime.Now, sortType, oldDate);
                        break;
                    case SortType.Entries:
                        entries = int.Parse(entriesText.Text);
                        GetFromDB(entries);
                        break;
                }
                graphView.LoadUrl(string.Format("javascript: drawAthenaChart(\"{0}\",{1})", databaseData, (int)graphType));
            };
        }

        private void ShowDate(object sender, EventArgs e)
        {
            placeholder = (Button)sender;
            string raw = DateTime.Now.ToString("MM-dd-yyyy");
            month = int.Parse(raw.Substring(0,2))-1;
            date = int.Parse(raw.Substring(3,2));
            year = int.Parse(raw.Substring(6));
            ShowDialog(1);
        }

        protected override Dialog OnCreateDialog(int id)
        {
            if (id == 1)
            {
                return new DatePickerDialog(this, this, year, month, date);
            }
            return null;
        }

        public void OnDateSet(DatePicker view, int year, int month, int dayOfMonth)
        {
            month += 1;
            if (month == 13)
                month = 1;

            string monthString = (month < 10) ? "0" + month.ToString() : month.ToString();
            string dayString = (dayOfMonth < 10) ? "0" + dayOfMonth.ToString() : dayOfMonth.ToString();
            placeholder.Text = monthString + "-" + dayString + "-" + year.ToString();
            if(placeholder.Id == Resource.Id.startDateButton)
            {
                oldDate = DateTime.Parse(placeholder.Text);
            }else if(placeholder.Id == Resource.Id.endDateButton)
            {
                newDate = DateTime.Parse(placeholder.Text);
            }
            placeholder = null;
        }

        private void SortTypeChanged(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            sortType = (SortType)e.Position;
            switch (sortType)
            {
                case SortType.Default:
                    startDate.Visibility = ViewStates.Gone;
                    endDate.Visibility = ViewStates.Gone;
                    entriesLayout.Visibility = ViewStates.Gone;
                    break;
                case SortType.Date:
                    startDate.Visibility = ViewStates.Visible;
                    endDate.Visibility = ViewStates.Visible;
                    entriesLayout.Visibility = ViewStates.Gone;
                    break;
                case SortType.Entries:
                    startDate.Visibility = ViewStates.Gone;
                    endDate.Visibility = ViewStates.Gone;
                    entriesLayout.Visibility = ViewStates.Visible;
                    break;
            }

        }

        private void GraphTypeChanged(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            graphType = (GraphType)e.Position;
        }

        private async void GetFromDB(DateTime newDate, SortType sort = SortType.Default, DateTime? oldDate = null)
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
                        List<GraphStatusDB> data = await DatabaseManager.ReadStatusesFromDB(newDate, oldDate);
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