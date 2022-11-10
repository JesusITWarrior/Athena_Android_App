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

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    [Activity(Label = "GraphingActivity")]
    public class GraphingActivity : AppCompatActivity
    {
        WebView graphView;

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

            graphView.EvaluateJavascript(string.Format(""));
        }
    }
}