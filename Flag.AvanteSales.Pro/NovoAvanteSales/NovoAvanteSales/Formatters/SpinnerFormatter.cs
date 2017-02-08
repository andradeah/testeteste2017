using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace AvanteSales.Pro.Formatters
{
    public static class SpinnerFormatter
    {
        public static ArrayAdapter SetDefaultAdapter(this Spinner spn)
        {
            var adapter = new ArrayAdapter(spn.Context, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            spn.Adapter = adapter;
            return adapter;
        }

        public static void Add(this Spinner spn, Java.Lang.Object obj)
        {
            ((ArrayAdapter)spn.Adapter).Add(obj);
        }

        public static void Clear(this Spinner spn)
        {
            spn.Adapter = null;
        }
    }
}