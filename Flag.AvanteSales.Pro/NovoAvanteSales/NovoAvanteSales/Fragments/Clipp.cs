using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace AvanteSales.Pro.Fragments
{
    public class Clipp : Android.Support.V4.App.Fragment
    {
        private TextView tvClipp;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.clipp, container, false);
            FindViewsById(view);
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            Inicializacao();
        }

        private void Inicializacao()
        {
            if (string.IsNullOrEmpty(tvClipp.Text))
                tvClipp.Text = "Sem dados cadastrados.";
            else
                tvClipp.Text = CSPDVs.Current.DSC_CLIPPING_INFORMATIVO;
        }

        private void FindViewsById(View view)
        {
            tvClipp = view.FindViewById<TextView>(Resource.Id.tvClipp);
        }
    }
}