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
using System.Collections;

namespace AvanteSales.Pro.Formatters
{
    public static class AdapterFormatter
    {
        public static ArrayAdapter<String> ChangeToAdapter(this Context context, ArrayList getEmpresas)
        {
            return new ArrayAdapter<String>(context, Android.Resource.Layout.SimpleSpinnerItem, getEmpresas.Cast<String>().ToArray());
        }
    }
}