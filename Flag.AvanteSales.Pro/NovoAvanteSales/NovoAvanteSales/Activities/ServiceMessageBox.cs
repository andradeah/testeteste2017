using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "ServiceMessageBox", Theme = "@style/AvanteSales.Theme.Dialogs")]
    public class ServiceMessageBox : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            AppCompatActivity activity = this;

            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetMessage("O seu expediente foi encerrado.");
            builder.SetCancelable(false);
            builder.SetPositiveButton("OK", (_sender, _e) =>
             {
                 Intent i = new Intent();
                 i.SetClass(this, typeof(Main));
                 i.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                 StartActivity(i);
                 activity.Finish();
             });

            Android.Support.V7.App.AlertDialog alert = builder.Create();
            alert.Window.SetType(WindowManagerTypes.SystemAlert);
            alert.Show();
        }
    }
}