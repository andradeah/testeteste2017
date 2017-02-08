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
    public static class TabFormatter
    {
        public static View CreateTabView(this Context context, String title)
        {
            View view = LayoutInflater.From(context).Inflate(Resource.Layout.tabs_bg, null);
            TextView tv = (TextView)view.FindViewById(Resource.Id.tabsText);
            tv.Text = (title);
            return view;
        }
    }
}