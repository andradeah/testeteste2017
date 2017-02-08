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

namespace AvanteSales.Pro.Dialogs
{
    [Activity(Label = "DialogInformacaoProduto",Theme = "@style/AvanteSalesTheme.Dialogs", ScreenOrientation = ScreenOrientation.Portrait)]
    public class DialogInformacaoProduto : Activity
    {
        TextView tvCaloricas;
        TextView tvNutricional;
        TextView tvOutros;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.dialog_informacao_produto);

            FindViewsById();

            CarregarInformacoes();
        }

        private void CarregarInformacoes()
        {
            tvCaloricas.Text = CSProdutos.Current.DSC_INFO_CALORICA;
            tvNutricional.Text = CSProdutos.Current.DSC_INFO_NUTRICIONAL;
            tvOutros.Text = CSProdutos.Current.DSC_INFO_OUTRAS;
        }

        private void FindViewsById()
        {
            tvCaloricas = FindViewById<TextView>(Resource.Id.tvCaloricas);
            tvNutricional = FindViewById<TextView>(Resource.Id.tvNutricional);
            tvOutros = FindViewById<TextView>(Resource.Id.tvOutros);
        }
    }
}