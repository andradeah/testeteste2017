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
using AvanteSales.Pro.Fragments;

namespace AvanteSales.Pro.Dialogs
{
    public class DialogMarkup : Android.Support.V4.App.DialogFragment
    {
        int CodigoGrupo;
        Button btnOK;
        Button btnCancelar;
        EditText txtMarkup;
        int IndexGrupo;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dialog_markup, container, false);
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            CodigoGrupo = Arguments.GetInt("COD_GRUPO", 0);
            IndexGrupo = Arguments.GetInt("INDEX", 0);
            FindViewsById(view);
            Eventos();
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            txtMarkup.FindFocus();
        }

        private void Eventos()
        {
            btnOK.Click += BtnOK_Click;
            btnCancelar.Click += BtnCancelar_Click;
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            Dismiss();
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            CSGruposProduto.Markup(CSPDVs.Current.COD_PDV, CodigoGrupo, txtMarkup.Text);
            GrupoProduto.AtualizarGrupoMarkup(CSGlobal.StrToDecimal(txtMarkup.Text), IndexGrupo);
            MessageBox.ShowShortMessageCenter(Activity, "Markup alterado com sucesso.");
            Dismiss();
        }

        private void FindViewsById(View view)
        {
            btnOK = view.FindViewById<Button>(Resource.Id.btnOK);
            btnCancelar = view.FindViewById<Button>(Resource.Id.btnCancelar);
            txtMarkup = view.FindViewById<EditText>(Resource.Id.txtMarkup);
        }
    }
}