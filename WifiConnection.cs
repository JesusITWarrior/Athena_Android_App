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
    /// <summary>
    /// Handles WifiCredentaials acceptance
    /// </summary>
    [Activity(Label = "WifiConnection", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class WifiConnection : Activity
    {
        /// <summary>
        /// Run when Activity is started
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.WifiCredentials);
            //Sets up references to all necessary Views
            TextView title = FindViewById<TextView>(Resource.Id.wifiName);
            TextView user = FindViewById<TextView>(Resource.Id.usernameInput);
            TextView password = FindViewById<TextView>(Resource.Id.passwordInput);
            Button connect = FindViewById<Button>(Resource.Id.connectButton);

            //Creates Bundle that was passed in with the Intent
            Bundle bundle = Intent.Extras;
            string name = bundle.GetString("WifiTitle");

            title.Text = name;

            connect.Click += (o,e) =>
            {
                //TODO: Show "connecting"
                //Creates WifiStruct object to pass creds to Athena device
                BluetoothManager.WifiStruct cred = new BluetoothManager.WifiStruct();
                cred.SSID = title.Text;
                //If identity is empty, it passes null to device, otherwise it trims and sends the identity... will possibly remove
                cred.identity = (user.Text.Trim() == "") ? null : user.Text.Trim();
                cred.key = password.Text;
                //JSON payload created from WifiStruct "cred" object
                string auth = Newtonsoft.Json.JsonConvert.SerializeObject(cred);

                BluetoothManager.SendData(auth);
                bool gotGoodResponse = false;
                //Waits until it receives a confirmation instead of a Raw WifiList from the board
                while (!gotGoodResponse)
                {
                    //Attempts to convert response to a boolean
                    try
                    {
                        //Receives data from Athena device
                        string confirmation = System.Threading.Tasks.Task.Run(async () => await BluetoothManager.ReceiveData()).Result;
                        //Attempt to convert data to Boolean
                        bool isWorking = Convert.ToBoolean(confirmation);
                        //If we got here, that means it's a boolean
                        gotGoodResponse = true;
                        //If we received a successful connection: close up shop
                        if (isWorking)
                        {
                            BluetoothManager.socket.Close();
                            //Create Intent and set result to be true, passing it back to Onboarding Activity
                            Intent allGood = new Intent();
                            allGood.PutExtra("status", true);
                            SetResult(Result.Ok, allGood);
                            Finish();
                        }
                        //We got a failure, which means device couldn't connect
                        else
                        {
                            //Throw exception up here
                        }
                    }
                    //Response is not a boolean, so loop needs to be repeated
                    catch (Exception ex)
                    {
                        gotGoodResponse = false;
                    }
                }
            };
        }
    }
}