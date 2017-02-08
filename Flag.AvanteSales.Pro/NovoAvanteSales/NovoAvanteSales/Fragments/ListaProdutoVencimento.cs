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

namespace AvanteSales.Pro.Fragments
{
    public class ListaProdutoVencimento : Android.Support.V4.App.Fragment
    {
        static ProgressDialog progress;
        LayoutInflater thisLayoutInflater;
        static ListView listProdutos;
        static Android.Support.V4.App.FragmentActivity ActivityContext;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.lista_produto_vencimento, container, false);
            thisLayoutInflater = inflater;
            FindViewById(view);
            ActivityContext = ((Cliente)Activity);
            return view;
        }

        private void FindViewById(View view)
        {
            listProdutos = view.FindViewById<ListView>(Resource.Id.listProdutos);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            var linha = ((Cliente)Activity).LinhaSelecionada == null ? 0 : ((Cliente)Activity).LinhaSelecionada.COD_GRUPO_COMERCIALIZACAO_FILTRADO;
            var grupo = ((Cliente)Activity).GrupoSelecionado;

            progress = new ProgressDialogCustomizado(Activity, thisLayoutInflater).Customizar();
            progress.Show();

            new ThreadCarregarProdutos(linha,grupo).Execute();
        }

        private class ThreadCarregarProdutos : AsyncTask
        {
            int Linha;
            int Grupo;
            List<CSProdutos.CSProdutoVencimento> Produtos;

            public ThreadCarregarProdutos(int linha, int grupo)
            {
                Linha = linha;
                Grupo = grupo;
            }

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                Produtos = CSProdutos.ItemsVencimentoColetaAtual(Linha, Grupo);

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                listProdutos.Adapter = new ListProdutoAdapter(ActivityContext, Produtos);

                progress.Dismiss();
            }
        }

        private class ListProdutoAdapter : BaseAdapter
        {
            List<CSProdutos.CSProdutoVencimento> Produtos;
            Android.Support.V4.App.FragmentActivity Context;

            public ListProdutoAdapter(Android.Support.V4.App.FragmentActivity context, List<CSProdutos.CSProdutoVencimento> produtos)
            {
                Produtos = produtos;
                Context = context;
            }

            public override int Count
            {
                get
                {
                    return Produtos.Count;
                }
            }

            public override Java.Lang.Object GetItem(int position)
            {
                return Produtos[position];
            }

            public override long GetItemId(int position)
            {
                return Produtos[position].COD_PRODUTO;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSProdutos.CSProdutoVencimento produtoAtual = Produtos[position];

                if (convertView == null)
                    convertView = LayoutInflater.From(Context)
                      .Inflate(Resource.Layout.lista_produto_vencimento_row, parent, false);

                TextView lblDescProduto = convertView.FindViewById<TextView>(Resource.Id.lblDescProduto);
                TextView lblCodigo = convertView.FindViewById<TextView>(Resource.Id.lblCodigo);
                TextView lblDataColeta = convertView.FindViewById<TextView>(Resource.Id.lblDataColeta);
                TextView lblQuantidade = convertView.FindViewById<TextView>(Resource.Id.lblQuantidade);
                TextView lblDataVencimento = convertView.FindViewById<TextView>(Resource.Id.lblDataVencimento);

                lblDescProduto.Text = produtoAtual.DSC_PRODUTO;
                lblCodigo.Text = produtoAtual.DESCRICAO_APELIDO_PRODUTO;
                lblDataColeta.Text = produtoAtual.DAT_COLETA.ToString("dd/MM/yyyy");
                lblQuantidade.Text = produtoAtual.QTD_AVENCER.ToString();
                lblDataVencimento.Text = produtoAtual.DAT_VENCIMENTO.ToString("dd/MM/yyyy");

                return convertView;
            }
        }
    }
}