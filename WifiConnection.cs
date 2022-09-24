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
    [Activity(Label = "WifiConnection")]
    public class WifiConnection : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.WifiCredentials);
            TextView title = FindViewById<TextView>(Resource.Id.wifiName);
            TextView user = FindViewById<TextView>(Resource.Id.usernameInput);
            TextView password = FindViewById<TextView>(Resource.Id.passwordInput);
            Button connect = FindViewById<Button>(Resource.Id.connectButton);

            Bundle bundle = Intent.Extras;
            string name = bundle.GetString("WifiTitle");

            title.Text = name;

            connect.Click += (o,e) =>
            {
                //Show "connecting"
                BluetoothManager.WifiStruct cred = new BluetoothManager.WifiStruct();
                cred.SSID = title.Text;
                cred.identity = (user.Text.Trim() == "") ? null : user.Text.Trim();
                cred.key = password.Text;
                string auth = Newtonsoft.Json.JsonConvert.SerializeObject(cred);
                BluetoothManager.SendData(auth);
                bool gotGoodResponse = false;
                while (!gotGoodResponse)
                {
                    
                    try
                    {
                        string confirmation = System.Threading.Tasks.Task.Run(async () => await BluetoothManager.ReceiveData()).Result;
                        bool isWorking = Convert.ToBoolean(confirmation);
                        gotGoodResponse = true;
                        if (isWorking)
                        {
                            BluetoothManager.socket.Close();
                            Intent allGood = new Intent();
                            allGood.PutExtra("status", true);
                            SetResult(Result.Ok, allGood);
                            Finish();
                        }
                        else
                        {
                            //Throw exception up here
                        }
                    }
                    catch (Exception ex)
                    {
                        gotGoodResponse = false;
                    }
                }
            };
        }
    }
}