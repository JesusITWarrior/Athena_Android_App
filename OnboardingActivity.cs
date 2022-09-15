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
using Android.Bluetooth;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    [Activity(Label = "OnboardingActivity")]
    public class OnboardingActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.onboardingprocess);
            // Create your application here
        }
    }
}