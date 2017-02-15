using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Webkit;
using Android.Widget;
using System.Runtime.InteropServices;
using Java.Lang;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "SplashScreen", MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/SplashTheme", WindowSoftInputMode = SoftInput.StateAlwaysHidden)]
    public class MainActivity : AppCompatActivity, IRunnable
    {
        WebView wbvWebView;

        [DllImport("libsqlite.so")]
        internal static extern int sqlite3_shutdown();

        [DllImport("libsqlite.so")]
        internal static extern int sqlite3_initialize();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            sqlite3_shutdown();

            Mono.Data.Sqlite.SqliteConnection.SetConfig(Mono.Data.Sqlite.SQLiteConfig.Serialized);

            sqlite3_initialize();

            MobileCenter.Start("b0a3593d-f58a-4b47-bfc8-854e314d1351",
                    typeof(Analytics), typeof(Crashes));

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.splash_screen);

            wbvWebView = FindViewById<WebView>(Resource.Id.wbvWebView);
            wbvWebView.LoadUrl("file:///android_asset/avante_sales_pro_gif.gif");

            Handler handler = new Handler();
            handler.PostDelayed(this, 2500);
        }

        public void Run()
        {
            Intent i = new Intent();
            i.SetClass(this, typeof(Login));
            this.StartActivity(i);

            Finish();
        }
    }
}