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
    public class MensagemPedido : Android.Support.V4.App.Fragment
    {
        CheckBox chkFob;
        EditText txtMensagem;
        EditText txtRecado;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.mensagem_pedido, container, false);

            FindViewsById(view);

            return view;
        }

        public override void OnDestroyView()
        {
            if (CSPDVs.Current.PEDIDOS_PDV.Current != null)
            {
                CSPDVs.Current.PEDIDOS_PDV.Current.MENSAGEM_PEDIDO = txtMensagem.Text.Replace("\r", "").Replace("\n", "");
                CSPDVs.Current.PEDIDOS_PDV.Current.RECADO_PEDIDO = txtRecado.Text.Replace("\r", "").Replace("\n", "");
                CSPDVs.Current.PEDIDOS_PDV.Current.IND_FOB = chkFob.Checked;
            }

            base.OnDestroyView();
        }

        private void FindViewsById(View view)
        {
            chkFob = view.FindViewById<CheckBox>(Resource.Id.chkFob);
            txtMensagem = view.FindViewById<EditText>(Resource.Id.txtMensagem);
            txtRecado = view.FindViewById<EditText>(Resource.Id.txtRecado);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            if (CSPDVs.Current.PEDIDOS_PDV.Current != null)
            {
                txtMensagem.Text = CSPDVs.Current.PEDIDOS_PDV.Current.MENSAGEM_PEDIDO;
                txtRecado.Text = CSPDVs.Current.PEDIDOS_PDV.Current.RECADO_PEDIDO;
                chkFob.Checked = CSPDVs.Current.PEDIDOS_PDV.Current.IND_FOB;
            }
        }
    }
}