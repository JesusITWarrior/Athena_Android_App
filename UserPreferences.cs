using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IAPYX_INNOVATIONS_RETROFIT_FRIDGE_APP
{
    internal static class UserPreferences
    {
        private const string configFileName = "UserSettings.athena";
        public static int temperature;
        public static bool isF;
        //public static int 

        static UserPreferences()
        {
            var destination = Path.Combine(Application.Context.GetExternalFilesDir(null).ToString(), configFileName);
            if (File.Exists(destination))
            {
                UserPrefStruct prefs = JsonConvert.DeserializeObject<UserPrefStruct>(File.ReadAllText(destination));
                temperature = prefs.temp;
                isF = prefs.isF;
            }
            else
            {
                File.Create(destination);
                temperature = 0;
                isF = true;
                WriteToFile();
            }
        }

        public static void WriteToFile()
        {
            UserPrefStruct ups = new UserPrefStruct();
            ups.temp = temperature;
            ups.isF = isF;
            string payload = JsonConvert.SerializeObject(ups);
            File.WriteAllText(Path.Combine(Application.Context.GetExternalFilesDir(null).ToString(), configFileName),payload);
        }

        private struct UserPrefStruct
        {
            public int temp { get; set; }
            public bool isF { get; set; }
        }
    }
}