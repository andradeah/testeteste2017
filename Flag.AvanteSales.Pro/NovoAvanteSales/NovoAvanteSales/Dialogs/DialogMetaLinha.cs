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
using AvanteSales.Pro.Activities;
using AvanteSales.Pro.Formatters;
using Java.Lang;

namespace AvanteSales.Pro.Dialogs
{
    public class DialogMetaLinha : Android.Support.V4.App.DialogFragment
    {
        static ProgressDialog progress;
        LayoutInflater thisLayoutInflater;
        static ListView listMetaLinha;
        static Android.Support.V4.App.FragmentActivity CurrentActivity;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dialog_meta_linha, container, false);
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            thisLayoutInflater = inflater;
            FindViewsById(view);
            CurrentActivity = Activity;

            return view;
        }

        private void FindViewsById(View view)
        {
            listMetaLinha = view.FindViewById<ListView>(Resource.Id.listMetaLinha);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            ListaCliente.MetaVendaClick = false;

            progress = new ProgressDialogCustomizado(Activity, thisLayoutInflater).Customizar();
            progress.Show();

            new ThreadCarregarMetasVendas().Execute();
        }

        private class ThreadCarregarMetasVendas : AsyncTask
        {
            List<CSGruposComercializacao.CSGrupoComercializacaoMetaVenda> adapter;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                adapter = new CSGruposComercializacao.CSGrupoComercializacaoMetaVenda().RetornarMetaVendaGrupoComercializacao();

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                listMetaLinha.Adapter = new ListMetasVendaAdapter(CurrentActivity, adapter);

                progress.Dismiss();
            }
        }

        private class ListMetasVendaAdapter : BaseAdapter
        {
            List<CSGruposComercializacao.CSGrupoComercializacaoMetaVenda> Linhas;
            Android.Support.V4.App.FragmentActivity Context;

            public ListMetasVendaAdapter(Android.Support.V4.App.FragmentActivity context, List<CSGruposComercializacao.CSGrupoComercializacaoMetaVenda> linhas)
            {
                Linhas = linhas;
                Context = context;
            }

            public override int Count
            {
                get
                {
                    return Linhas.Count;
                }
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return Linhas[position];
            }

            public override long GetItemId(int position)
            {
                return Linhas[position].COD_GRUPO_COMERCIALIZACAO;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSGruposComercializacao.CSGrupoComercializacaoMetaVenda linhaAtual = Linhas[position];

                if (convertView == null)
                    convertView = LayoutInflater.From(Context)
                      .Inflate(Resource.Layout.dialog_meta_linha_row, parent, false);

                TextView lblDescricaoLinha = convertView.FindViewById<TextView>(Resource.Id.lblDescricaoLinha);
                TextView lblMeta = convertView.FindViewById<TextView>(Resource.Id.lblMeta);
                TextView lblVendido = convertView.FindViewById<TextView>(Resource.Id.lblVendido);

                lblDescricaoLinha.Text = linhaAtual.DES_GRUPO_COMERCIALIZACAO;
                lblMeta.Text = linhaAtual.VLR_OBJETIVO.ToString(".00");
                lblVendido.Text = linhaAtual.VLR_VENDIDO == 0 ? "0,00" : linhaAtual.VLR_VENDIDO.ToString(".00");
                
                return convertView;
            }
        }
    }
}