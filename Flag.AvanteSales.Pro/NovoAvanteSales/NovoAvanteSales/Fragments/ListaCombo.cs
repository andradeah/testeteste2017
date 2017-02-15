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
using AvanteSales.Pro.Dialogs;
using AvanteSales.Pro.Formatters;
using Java.Lang;

namespace AvanteSales.Pro.Fragments
{
    public class ListaCombo : Android.Support.V4.App.Fragment
    {
        static ProgressDialog progress;
        LayoutInflater thisLayoutInflater;
        static ExpandableListView elvResultado;
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        static CSProdutos.CSProduto produtoCombo = null;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.lista_combo, container, false);
            thisLayoutInflater = inflater;
            FindViewById(view);
            Eventos();
            ActivityContext = ((Cliente)Activity);
            return view;
        }

        private void Eventos()
        {
            elvResultado.ItemLongClick += ElvResultado_ItemLongClick;
        }

        private void ElvResultado_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            produtoCombo = (CSProdutos.CSProduto)elvResultado.GetItemAtPosition(e.Position);

            progress = new ProgressDialogCustomizado(Activity, thisLayoutInflater).Customizar();
            progress.Show();

            new ThreadValidarCombo().Execute();
        }

        private class ThreadValidarCombo : AsyncTask
        {
            bool ComboValido;
            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                if (ValidaItemCombo(produtoCombo))
                {
                    ComboValido = true;
                }
                else
                    ComboValido = false;

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                if (ComboValido)
                {
                    ((Cliente)ActivityContext).AbrirDigitacaoCombo(produtoCombo.QTD_MAXIMA_COMBO_PEDIDO, produtoCombo.QuantidadeVendida(produtoCombo), produtoCombo.COD_PRODUTO);
                }

                progress.Dismiss();
            }

            private bool ValidaItemCombo(CSProdutos.CSProduto produto)
            {
                try
                {
                    CSGlobal.PedidoComCombo = false;

                    if (produto.IND_ITEM_COMBO)
                    {
                        int tblprc = CSPDVs.Current.COD_TABPRECO_PADRAO;
                        List<string> listaBloqueio = produto.ValidaTabelaPrecoCombo(produto.COD_PRODUTO);

                        if (!CSPDVs.Current.PermiteVendaCombo(produto.COD_PRODUTO))
                        {
                            MessageBox.Alert(ActivityContext, "Venda deste combo não permitido para este cliente.");
                            return false;
                        }
                        else if (listaBloqueio.Count > 0 &&
                                 !IsBroker())
                        {
                            string msg = string.Empty;

                            for (int i = 0; i < listaBloqueio.Count; i++)
                            {
                                msg += listaBloqueio[i];

                                if (i + 1 < listaBloqueio.Count)
                                    msg += ", ";
                            }

                            MessageBox.Alert(ActivityContext, "Os produtos a seguir não possuem suas respectivas tabelas de preço cadastradas: " + msg);
                            return false;
                        }
                        else
                            CSGlobal.PedidoComCombo = true;

                        if (IsBroker())
                        {
                            foreach (CSProdutos.CSProduto produtoAtual in CSProdutos.Items)
                            {
                                if (produtoAtual.COD_PRODUTO_CONJUNTO == produto.COD_PRODUTO)
                                {
                                    if (produtoAtual.PRECOS_PRODUTO == null || produtoAtual.PRECOS_PRODUTO.Count == 0)
                                    {
                                        MessageBox.Alert(ActivityContext, "Cliente ou Produto (" + produtoAtual.COD_PRODUTO + ") com informações incompletas no cadastro Nestlê!\nNão é possivel realizar esta venda.");
                                        CSGlobal.PedidoComCombo = false;
                                        return false;
                                    }

                                    if (!CSProdutos.GetProdutoPoliticaBroker(produtoAtual.COD_PRODUTO, CSPDVs.Current.COD_PDV, produtoAtual.GRUPO_COMERCIALIZACAO.COD_SETOR_BROKER))
                                    {
                                        MessageBox.Alert(ActivityContext, "Cliente ou Produto (" + produtoAtual.COD_PRODUTO + ") com informações incompletas no cadastro Nestlê!\nNão é possivel realizar esta venda.");
                                        CSGlobal.PedidoComCombo = false;
                                        return false;
                                    }
                                }
                            }
                        }
                    }

                    return true;
                }
                catch (System.Exception)
                {
                    return false;
                }
            }
        }

        private static bool IsBroker()
        {
            try
            {
                return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private void FindViewById(View view)
        {
            elvResultado = view.FindViewById<ExpandableListView>(Resource.Id.elvResultado);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            progress = new ProgressDialogCustomizado(Activity, thisLayoutInflater).Customizar();
            progress.Show();

            new ThreadCarregarCombos().Execute();
        }

        private class ProcuraProdutoExpandableListViewAdapter : BaseExpandableListAdapter
        {
            private Context context;
            private int groupResourceId;
            private int childResourceId;
            private IList<CSProdutos.CSProduto> grupoProdutos;
            private Dictionary<int, CSProdutos.CSProduto[]> dicItensProdutos;


            public ProcuraProdutoExpandableListViewAdapter(Context c, int grpResId, int childResId, IList<CSProdutos.CSProduto> grpPrds, Dictionary<int, CSProdutos.CSProduto[]> itensProdutos)
            {
                context = c;
                groupResourceId = grpResId;
                childResourceId = childResId;
                grupoProdutos = grpPrds;
                dicItensProdutos = itensProdutos;
            }

            public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
            {
                return dicItensProdutos[grupoProdutos[groupPosition].COD_PRODUTO];
            }

            public override long GetChildId(int groupPosition, int childPosition)
            {
                return dicItensProdutos[grupoProdutos[groupPosition].COD_PRODUTO][childPosition].COD_PRODUTO;
            }

            public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
            {
                try
                {
                    var combo = dicItensProdutos[grupoProdutos[groupPosition].COD_PRODUTO];
                    var produto = combo[childPosition];
                    LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                    View linha = layout.Inflate(childResourceId, null);

                    TextView tvProduto = linha.FindViewById<TextView>(Resource.Id.tvProduto);
                    TextView tvQuantidade = linha.FindViewById<TextView>(Resource.Id.tvQuantidade);

                    tvQuantidade.Visibility = ViewStates.Gone;
                    tvProduto.Text = produto.DESCRICAO_APELIDO_PRODUTO.Trim() + " - " + produto.DSC_PRODUTO.Trim();

                    return linha;
                }
                catch (System.Exception)
                {
                    return null;
                }
            }

            public override int GetChildrenCount(int groupPosition)
            {
                return dicItensProdutos[grupoProdutos[groupPosition].COD_PRODUTO].Count();
            }

            public override Java.Lang.Object GetGroup(int groupPosition)
            {
                return grupoProdutos[groupPosition];
            }

            public override long GetGroupId(int groupPosition)
            {
                return grupoProdutos[groupPosition].COD_PRODUTO;
            }

            public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
            {
                try
                {
                    var produto = grupoProdutos[groupPosition];
                    LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                    View linha = layout.Inflate(groupResourceId, null);

                    TextView tvCodigo = linha.FindViewById<TextView>(Resource.Id.tvCodigo);
                    TextView tvDescricao = linha.FindViewById<TextView>(Resource.Id.tvDescricao);
                    TextView tvQuantidade = linha.FindViewById<TextView>(Resource.Id.tvQuantidade);

                    if (tvQuantidade != null)
                        tvQuantidade.Visibility = ViewStates.Gone;

                    tvCodigo.Text = produto.DESCRICAO_APELIDO_PRODUTO.Trim();
                    tvDescricao.Text = produto.DSC_PRODUTO;

                    return linha;
                }
                catch (System.Exception)
                {
                    return null;
                }
            }

            public override int GroupCount
            {
                get { return grupoProdutos.Count; }
            }

            public override bool HasStableIds
            {
                get { return true; }
            }

            public override bool IsChildSelectable(int groupPosition, int childPosition)
            {
                return false;
            }

            public int GetCodProduto(int position)
            {
                return grupoProdutos[position].COD_PRODUTO;
            }
        }

        private class ThreadCarregarCombos : AsyncTask
        {
            Dictionary<int, CSProdutos.CSProduto[]> dic;
            List<CSProdutos.CSProduto> produtos;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                try
                {
                    produtos = new List<CSProdutos.CSProduto>();
                    foreach (CSProdutos.CSProduto produto in CSProdutos.Items)
                    {
                        if (produto.COD_TIPO_DISTRIBUICAO_POLITICA != CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA)
                            continue;

                        if (produto.IND_ITEM_COMBO == false)
                            continue;
                        else if (produto.IND_ITEM_COMBO)
                        {
                            if (produto.IND_STATUS_COMBO.ToUpper().Trim() == "I")
                                continue;

                            if ((produto.DAT_VALIDADE_INICIO_COMBO > CSEmpresa.Current.DATA_ENTREGA ||
                                 produto.DAT_VALIDADE_TERMINO_COMBO < CSEmpresa.Current.DATA_ENTREGA))
                                continue;
                        }

                        var itensCombo = CSProdutos.Items.Cast<CSProdutos.CSProduto>().Where(p => p.COD_PRODUTO_CONJUNTO == produto.COD_PRODUTO).OrderBy(p => int.Parse(p.DESCRICAO_APELIDO_PRODUTO)).ToList();

                        if (itensCombo.Count > 0)
                            produtos.Add(produto);
                    }

                    dic = new Dictionary<int, CSProdutos.CSProduto[]>();
                    foreach (var combo in produtos)
                    {
                        // Procura os dados do combo
                        var itensCombo = CSProdutos.Items.Cast<CSProdutos.CSProduto>().Where(p => p.COD_PRODUTO_CONJUNTO == combo.COD_PRODUTO).OrderBy(p => int.Parse(p.DESCRICAO_APELIDO_PRODUTO)).ToArray();
                        dic.Add(combo.COD_PRODUTO, itensCombo);
                    }

                }
                catch (System.Exception)
                {

                }

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                elvResultado.SetAdapter(new ProcuraProdutoExpandableListViewAdapter(ActivityContext, Resource.Layout.procura_produto_item_combo_group_row, Resource.Layout.procura_produto_item_combo_child_row, produtos, dic));

                progress.Dismiss();
            }
        }
    }
}