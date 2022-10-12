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
        public enum WindowSize { COMPACT, MEDIUM, EXPANDED}
        private const string configFileName = "UserSettings.athena";
        public static WindowSize widthWindowSize;
        public static WindowSize heightWindowSize;
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

        public static void SetViewSize(Context ctx)
        {
            float height = (ctx.Resources.DisplayMetrics.HeightPixels / ctx.Resources.DisplayMetrics.Density);
            float width = (ctx.Resources.DisplayMetrics.WidthPixels / ctx.Resources.DisplayMetrics.Density);

            if (width < 600f)
            {
                widthWindowSize = WindowSize.COMPACT;
            }
            else if (width < 840f)
            {
                widthWindowSize = WindowSize.MEDIUM;
            }
            else
            {
                widthWindowSize = WindowSize.EXPANDED;
            }
            

            if (height < 480f)
            {
                heightWindowSize = WindowSize.COMPACT;
            }
            else if (height < 900f)
            {
                heightWindowSize = WindowSize.MEDIUM;
            }
            else
            {
                heightWindowSize = WindowSize.EXPANDED;
            }
        }

        private struct UserPrefStruct
        {
            public int temp { get; set; }
            public bool isF { get; set; }
        }
    }
}