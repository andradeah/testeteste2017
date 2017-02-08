using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "Sobre", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AvanteSalesTheme")]
    public class Sobre : Activity
    {
        private TextView labelVersion;
        private TextView labelVersionWS;
        private TextView labelFlag;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.about);
            labelVersion = FindViewById<TextView>(Resource.Id.labelVersion);
            labelVersionWS = FindViewById<TextView>(Resource.Id.labelVersionWS);
            labelFlag = FindViewById<TextView>(Resource.Id.labelFlag);

            labelVersion.Text = "Versão " + PackageManager.GetPackageInfo(PackageName, 0).VersionName;
            labelFlag.Text = string.Format(labelFlag.Text, DateTime.Now.Year.ToString());

            if (CSEmpresa.Current != null)
            {
                if (CSEmpresa.ColunaExiste("EMPRESA", "VERSAO_WEBSERVICE"))
                {
                    labelVersionWS.Text = "WS " + CSEmpresa.Current.VERSAO_WEBSERVICE;
                    labelVersionWS.Visibility = ViewStates.Visible;
                }
            }
        }
    }
}