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
    public class ProdutoIndenizacao : Android.Support.V4.App.Fragment
    {
        TextView lblCodigoProduto;
        TextView lblProduto;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.produto_indenizacao, container, false);

            FindViewsById(view);

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            lblCodigoProduto.Text = CSProdutos.Current.DESCRICAO_APELIDO_PRODUTO;
            lblProduto.Text = CSProdutos.Current.DSC_PRODUTO;
        }

        private void FindViewsById(View view)
        {
            lblCodigoProduto = view.FindViewById<TextView>(Resource.Id.lblCodigoProduto);
            lblProduto = view.FindViewById<TextView>(Resource.Id.lblProduto);
        }
    }
}