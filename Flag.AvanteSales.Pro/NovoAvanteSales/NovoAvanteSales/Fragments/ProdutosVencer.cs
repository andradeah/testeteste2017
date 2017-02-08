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
    public class ProdutosVencer : Android.Support.V4.App.Fragment
    {
        static ProgressDialog progress;
        static ProgressDialog progressRadio;
        LayoutInflater thisLayoutInflater;
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        static ListView listProdutos;
        RadioGroup rdgRdbGroup;
        RadioButton rdbAVencer;
        RadioButton rdbVencidos;
        static List<CSProdutos.CSProdutoVencimento> Produtos;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.produtos_vencer, container, false);

            thisLayoutInflater = inflater;
            ActivityContext = ((Cliente)Activity);
            FindViewsById(view);
            Eventos();

            return view;
        }

        private void Eventos()
        {
            rdgRdbGroup.CheckedChange += RdgRdbGroup_CheckedChange;
        }

        private void RdgRdbGroup_CheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            progressRadio = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
            progressRadio.Show();

            new ThreadCarregarListaProdutosFiltro(rdbVencidos.Checked).Execute();
        }

        private void FindViewsById(View view)
        {
            listProdutos = view.FindViewById<ListView>(Resource.Id.listProdutos);
            rdgRdbGroup = view.FindViewById<RadioGroup>(Resource.Id.rdgRdbGroup);
            rdbAVencer = view.FindViewById<RadioButton>(Resource.Id.rdbAVencer);
            rdbVencidos = view.FindViewById<RadioButton>(Resource.Id.rdbVencidos);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            CarregarListaProdutos();
        }

        private void CarregarListaProdutos()
        {
            progress = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
            progress.Show();

            var linha = ((Cliente)Activity).LinhaSelecionada == null ? 0 : ((Cliente)Activity).LinhaSelecionada.COD_GRUPO_COMERCIALIZACAO_FILTRADO;
            var grupo = ((Cliente)Activity).GrupoSelecionado;
            
            new ThreadCarregarListaProdutos(linha,grupo).Execute();
        }

        private class ThreadCarregarListaProdutos : AsyncTask
        {
            int Linha;
            int Grupo;

            public ThreadCarregarListaProdutos(int linha,int grupo)
            {
                Linha = linha;
                Grupo = grupo;
            }

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                Produtos = CSProdutos.ItemsVencimento(Linha,Grupo);

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                listProdutos.Adapter = new ListProdutoAdapter(ActivityContext, Produtos.Where(p=> p.DAT_VENCIMENTO.Date > DateTime.Now.Date).ToList());

                progress.Dismiss();

                base.OnPostExecute(result);
            }
        }

        private class ThreadCarregarListaProdutosFiltro : AsyncTask
        {
            static List<CSProdutos.CSProdutoVencimento> ProdutosFiltrados;
            bool ProdutosVencidos;
            public ThreadCarregarListaProdutosFiltro(bool produtosVencidos)
            {
                ProdutosVencidos = produtosVencidos;
            }

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                if (ProdutosVencidos)
                    ProdutosFiltrados = Produtos.Where(p => p.DAT_VENCIMENTO.Date <= DateTime.Now.Date).ToList();
                else
                    ProdutosFiltrados = Produtos.Where(p => p.DAT_VENCIMENTO.Date > DateTime.Now.Date).ToList();

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                listProdutos.Adapter = new ListProdutoAdapter(ActivityContext, ProdutosFiltrados);

                progressRadio.Dismiss();

                base.OnPostExecute(result);
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
                      .Inflate(Resource.Layout.produtos_vencer_row, parent, false);

                TextView lblDescProduto = convertView.FindViewById<TextView>(Resource.Id.lblDescProduto);
                TextView lblCodigo = convertView.FindViewById<TextView>(Resource.Id.lblCodigo);
                TextView lblData = convertView.FindViewById<TextView>(Resource.Id.lblData);
                TextView lblQuantidade = convertView.FindViewById<TextView>(Resource.Id.lblQuantidade);
                TextView lblVencimento = convertView.FindViewById<TextView>(Resource.Id.lblVencimento);

                lblCodigo.Text = produtoAtual.DESCRICAO_APELIDO_PRODUTO;
                lblData.Text = produtoAtual.DAT_COLETA.ToString("dd/MM/yyyy");
                lblQuantidade.Text = produtoAtual.QTD_AVENCER.ToString();
                lblVencimento.Text = produtoAtual.DAT_VENCIMENTO.ToString("dd/MM/yyyy");

                return convertView;
            }
        }
    }
}