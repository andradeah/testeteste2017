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
using AvanteSales.Pro.Formatters;
using Java.Lang;
using AvanteSales.Pro.Activities;
using AvanteSales.Pro.Dialogs;

namespace AvanteSales.Pro.Fragments
{
    public class GrupoProduto : Android.Support.V4.App.Fragment
    {
        static GridView grdGrupoProduto;
        static ProgressDialog progress;
        LayoutInflater thisLayoutInflater;
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        static int LinhaSelecionada;
        static List<CSGruposProduto.CSGrupoProduto> GruposProduto;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.grupo_produto, container, false);
            thisLayoutInflater = inflater;
            ActivityContext = ((Cliente)Activity);
            FindViewsById(view);
            Eventos();

            Cliente cliente = (Cliente)Activity;
            LinhaSelecionada = cliente.LinhaSelecionada == null ? 0 : cliente.LinhaSelecionada.COD_GRUPO_COMERCIALIZACAO_FILTRADO;

            return view;
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();
        }

        private void Eventos()
        {
            grdGrupoProduto.ItemClick += GrdGrupoProduto_ItemClick;
            grdGrupoProduto.ItemLongClick += GrdGrupoProduto_ItemLongClick;
        }

        private void GrdGrupoProduto_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            DialogMarkup dialogMarkup = new DialogMarkup();

            Bundle arguments = new Bundle();
            arguments.PutInt("COD_GRUPO", GruposProduto[e.Position].COD_GRUPO_FILTRADO);
            arguments.PutInt("INDEX", e.Position);
            dialogMarkup.Arguments = arguments;
            dialogMarkup.Show(Activity.SupportFragmentManager, "DialogMarkup");
        }

        private void GrdGrupoProduto_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Cliente cliente = (Cliente)Activity;
            cliente.GrupoSelecionado = GruposProduto[e.Position].COD_GRUPO_FILTRADO;
            cliente.FamiliaSelecionada = -1;
            cliente.ProximoPasso(false);
        }

        private void FindViewsById(View view)
        {
            grdGrupoProduto = view.FindViewById<GridView>(Resource.Id.grdGrupoProduto);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            Inicializacao();
        }

        private void Inicializacao()
        {
            CarregarGrupoProduto();
        }

        private void CarregarGrupoProduto()
        {
            progress = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
            progress.Show();

            new ThreadGrupoProduto().Execute();
        }

        public static void AtualizarGrupoMarkup(decimal markup, int position)
        {
            var view = grdGrupoProduto.GetChildAt(position);
            TextView lblMarkup = view.FindViewById<TextView>(Resource.Id.lblMarkup);
            lblMarkup.Text = string.Format("Mk:{0}%", markup.ToString());
        }

        private class ThreadGrupoProduto : AsyncTask
        {
            public IListAdapter GrupoProdutoAdapter { get; private set; }

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                CSGruposProduto classeGrupoProduto = new CSGruposProduto();

                GruposProduto = classeGrupoProduto.GrupoProdutoFiltrado(LinhaSelecionada, CSPDVs.Current.COD_PDV, CSPDVs.Current.COD_CATEGORIA, CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA, CSEmpresa.Current.IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE);

                //if (!IsBroker())
                //{
                //    CSGruposProduto.CSGrupoProduto todos = new CSGruposProduto.CSGrupoProduto();
                //    todos.COD_GRUPO_FILTRADO = -1;
                //    todos.COD_GRUPO = -1;
                //    todos.DSC_GRUPO_FILTRADO = "Todos";
                //    todos.DSC_GRUPO = "Todos";

                //    GruposProduto.Add(todos);
                //}

                return true;
            }

            private bool IsBroker()
            {
                return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                grdGrupoProduto.Adapter = new GridViewGrupoProdutoAdapter(ActivityContext, GruposProduto);

                progress.Dismiss();
            }

            private class GridViewGrupoProdutoAdapter : BaseAdapter
            {
                List<CSGruposProduto.CSGrupoProduto> GruposProduto;
                Android.Support.V4.App.FragmentActivity Context;

                public GridViewGrupoProdutoAdapter(Android.Support.V4.App.FragmentActivity context, List<CSGruposProduto.CSGrupoProduto> gruposProdutos)
                {
                    GruposProduto = gruposProdutos;
                    Context = context;
                }

                public override int Count
                {
                    get
                    {
                        return GruposProduto.Count;
                    }
                }

                public override Java.Lang.Object GetItem(int position)
                {
                    return GruposProduto[position];
                }

                public override long GetItemId(int position)
                {
                    return GruposProduto[position].COD_GRUPO;
                }

                public override View GetView(int position, View convertView, ViewGroup parent)
                {
                    CSGruposProduto.CSGrupoProduto grupoAtual = GruposProduto[position];

                    if (convertView == null)
                        convertView = LayoutInflater.From(Context)
                          .Inflate(Resource.Layout.grupo_produto_grid_row, parent, false);

                    TextView lblGrupoProduto = convertView.FindViewById<TextView>(Resource.Id.lblGrupoProduto);
                    TextView lblGrupoComercializacao = convertView.FindViewById<TextView>(Resource.Id.lblGrupoComercializacao);
                    TextView lblQuantidadeVenda = convertView.FindViewById<TextView>(Resource.Id.lblQuantidadeVenda);
                    TextView lblMarkup = convertView.FindViewById<TextView>(Resource.Id.lblMarkup);

                    lblGrupoProduto.Text = grupoAtual.DSC_GRUPO_FILTRADO.ToUpper();
                    lblGrupoComercializacao.Text = grupoAtual.DSC_GRUPO_COMERCIALIZACAO;
                    lblMarkup.Text = string.Format("Mk:{0}%", grupoAtual.PCT_MARKUP.ToString());

                    if (grupoAtual.COD_GRUPO_FILTRADO == -1)
                    {
                        lblQuantidadeVenda.Visibility = ViewStates.Invisible;
                    }
                    else
                    {
                        lblQuantidadeVenda.Visibility = ViewStates.Visible;
                        lblQuantidadeVenda.Text = string.Format("{0}/{1}", grupoAtual.QTD_PRODUTO, grupoAtual.QTD_VENDA_DIA);
                    }

                    return convertView;
                }
            }
        }
    }
}