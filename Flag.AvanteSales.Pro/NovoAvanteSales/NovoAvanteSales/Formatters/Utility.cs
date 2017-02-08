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
using AvanteSales.Pro.Activities;

namespace AvanteSales.Pro.Formatters
{
    public class Utility
    {
        public static void setListViewHeightBasedOnChildren(ListView listView) {
        var listAdapter = listView.Adapter;
        if (listAdapter == null) {
            // pre-condition
            return;
        }

        int totalHeight = 0;
        int desiredWidth = Android.Views.View.MeasureSpec.MakeMeasureSpec(listView.Width, MeasureSpecMode.AtMost);
        for (int i = 0; i < listAdapter.Count; i++) {
            View listItem = listAdapter.GetView(i, null, listView);
            listItem.Measure(desiredWidth,0);
            totalHeight += listItem.MeasuredHeight;
        }

        ViewGroup.LayoutParams parametros = listView.LayoutParameters;
        parametros.Height = totalHeight + (listView.DividerHeight * (listAdapter.Count - 1));
        listView.LayoutParameters = parametros;
        listView.RequestLayout();
    }

    }
}