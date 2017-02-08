using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Java.Lang;
using Android.Content.PM;
using System.Runtime.InteropServices;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "Avante Sales", Theme = "@style/AvanteSalesTheme")]
    public class Entrada : Activity, IRunnable
    {
        private TextView labelVersion;
        private TextView labelFlag;

        [DllImport("libsqlite.so")]
        internal static extern int sqlite3_shutdown();

        [DllImport("libsqlite.so")]
        internal static extern int sqlite3_initialize();

        protected override void OnCreate(Bundle bundle)
        {
            sqlite3_shutdown();

            Mono.Data.Sqlite.SqliteConnection.SetConfig(Mono.Data.Sqlite.SQLiteConfig.Serialized);

            sqlite3_initialize();

            base.OnCreate(bundle);
            CSGlobal.Context = this.Application.ApplicationContext;
            SetContentView(Resource.Layout.about);

            labelVersion = FindViewById<TextView>(Resource.Id.labelVersion);
            labelFlag = FindViewById<TextView>(Resource.Id.labelFlag);

            labelVersion.Text = "Versão " + PackageManager.GetPackageInfo(PackageName, 0).VersionName;
            labelFlag.Text = string.Format(labelFlag.Text, DateTime.Now.Year.ToString());

            Handler handler = new Handler();
            handler.PostDelayed(this, 250);
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

