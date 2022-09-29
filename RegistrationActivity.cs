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
    [Activity(Label = "RegistrationActivity", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class RegistrationActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.registration);
            // Create your application here
            Button register = FindViewById<Button>(Resource.Id.RegisterButton);
            register.Click += async (o, e) =>
            {
                TextView user = FindViewById<TextView>(Resource.Id.usernameReg);
                TextView pass = FindViewById<TextView>(Resource.Id.passwordReg);
                bool success = await DatabaseManager.Register(user.Text, pass.Text);
                if (success)
                {
                    SetContentView(Resource.Layout.FirstPair);
                    Button accept = FindViewById<Button>(Resource.Id.accept);
                    Button decline = FindViewById<Button>(Resource.Id.decline);

                    accept.Click += (o, e) => {
                        StartActivity(typeof(OnboardingActivity));
                        Finish();
                    };

                    decline.Click += (o, e) =>
                    {
                        Finish();
                    };
                }
                else
                {
                    //Throw some sort of error.
                }
            };
        }
    }
}