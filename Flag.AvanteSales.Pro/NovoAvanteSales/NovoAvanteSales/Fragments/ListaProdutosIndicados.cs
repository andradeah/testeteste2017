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
using AvanteSales.BusinessRules;
using AvanteSales.Pro.Activities;
using AvanteSales.Pro.Dialogs;
using AvanteSales.Pro.Formatters;

namespace AvanteSales.Pro.Fragments
{
    public class ListaProdutosIndicados : Android.Support.V4.App.Fragment
    {
        static List<CSProdutos.CSProduto> ProdutosIndicados;
        static bool ListarApelido;
        private const int dialogProduto = 1;
        private const int frmProdutoPedido = 2;
        private static int PositionItemVendido;
        //private CSItemsPedido.CSItemPedido backupItem;
        public static decimal decimalPedido;
        private static int currentPosition;
        static TextView txtProdutosVendidos;
        static TextView txtTotal;
        static TextView txtPesquisa;
        static List<CSProdutos.CSProduto> CurrentAdapter;
        static CheckBox chkFiltroProdutos;
        //private static bool ExisteProdutoTop;
        static ProgressDialog progressDialog;
        static ListView listProdutos;
        static Android.Support.V4.App.FragmentActivity CurrentActivity;
        LayoutInflater thisLayoutInflater;
        static bool CarregandoDados;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.lista_produtos_indicados, container, false);
            PositionItemVendido = 0;
            //ExisteProdutoTop = false;
            CurrentActivity = Activity;
            thisLayoutInflater = inflater;
            FindViewsById(view);
            Eventos();
            ProdutosIndicados = null;
            ((Cliente)Activity).RotinaProdutosIndicados = false;
            CarregandoDados = true;
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            if (CSPDVs.Current.PEDIDOS_PDV.Current != null &&
                CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO != null)
                decimalPedido = CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO.PRC_ADICIONAL_FINANCEIRO;
            else
                decimalPedido = 0;

            ListarApelido = false;

            if (((Cliente)Activity).LinhaSelecionada != null)
            {
                progressDialog = new ProgressDialogCustomizado(Activity, thisLayoutInflater).Customizar();
                progressDialog.Show();

                new ThreadCarregarProdutosIndicados().Execute();
            }
            else
                CarregandoDados = false;
        }

        private void FindViewsById(View view)
        {
            txtProdutosVendidos = view.FindViewById<TextView>(Resource.Id.txtProdutosVendidos);
            txtTotal = view.FindViewById<TextView>(Resource.Id.txtTotal);
            txtPesquisa = view.FindViewById<TextView>(Resource.Id.txtPesquisa);
            chkFiltroProdutos = view.FindViewById<CheckBox>(Resource.Id.chkFiltroProdutos);
            listProdutos = view.FindViewById<ListView>(Resource.Id.listProdutos);
        }

        private void Eventos()
        {
            try
            {
                txtPesquisa.TextChanged += TxtPesquisa_TextChanged;
                listProdutos.ItemLongClick += ListProdutos_ItemLongClick;
                listProdutos.ItemClick += ListProdutos_ItemClick;
                chkFiltroProdutos.CheckedChange += ChkFiltroProdutos_CheckedChange;
            }
            catch (Exception)
            {

            }
        }

        private void ChkFiltroProdutos_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                CarregarAdapterAtualComFiltros();
            }
            catch (Exception)
            {

            }
        }

        private void ListProdutos_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            try
            {
                SelecionarProduto(e.Position);
            }
            catch (Exception)
            {

            }
        }

        private void AbrirProduto(bool produtoVendido)
        {
            try
            {
                AbrirDialogProduto(produtoVendido);
            }
            catch (Exception)
            {
            }
        }

        private void ListProdutos_ItemLongClick(object sender, AdapterView.ItemLongClickEventArgs e)
        {
            try
            {
                var produtoJaVendido = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.STATE != ObjectState.DELETADO && p.PRODUTO.COD_PRODUTO == ((CSProdutos.CSProduto)listProdutos.Adapter.GetItem(e.Position)).COD_PRODUTO).FirstOrDefault();

                if (produtoJaVendido != null)
                    MessageBox.Alert(CurrentActivity, "Deseja excluir este item do pedido?", "Excluir",
                        (_sender, _e) =>
                        {
                            produtoJaVendido.STATE = ObjectState.DELETADO;
                            produtoJaVendido.AtualizaImagem();
                            MostrarResumo();
                            SetScrollPosition();
                        }, "Cancelar", (_sender, _e) => { }, true);
            }
            catch (Exception)
            {

            }
        }

        private void TxtPesquisa_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (!CarregandoDados)
                    TodosProdutosIndicadosFiltradoPesquisa();
            }
            catch (Exception)
            {

            }
        }

        public static void CarregarAdapterAtualComFiltros()
        {
            try
            {
                if (chkFiltroProdutos.Checked)
                {
                    CurrentAdapter = RetornaAdapterComRegraDeProdutosNaoVendidos(CurrentAdapter);
                    SetScrollPosition();
                }
                else
                {
                    TodosProdutosIndicadosFiltradoPesquisa();
                }
            }
            catch (Exception)
            {
            }
        }

        private static List<CSProdutos.CSProduto> RetornaAdapterComRegraDeProdutosNaoVendidos(List<CSProdutos.CSProduto> produtosParaFiltrar)
        {
            try
            {
                var produtosPedido = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.STATE != ObjectState.DELETADO).Select(it => it.PRODUTO.COD_PRODUTO);
                var produtosNaoVendidos = produtosParaFiltrar.Where(p => !produtosPedido.Contains(p.COD_PRODUTO)).ToList();

                return produtosNaoVendidos;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void TodosProdutosIndicadosFiltradoPesquisa()
        {
            try
            {
                if (string.IsNullOrEmpty(txtPesquisa.Text))
                {

                    if (listProdutos.Adapter != null)
                        listProdutos.Adapter.Dispose();

                    if (chkFiltroProdutos.Checked)
                    {
                        var produtosNaoVendidos = RetornaAdapterComRegraDeProdutosNaoVendidos(ProdutosIndicados);
                        listProdutos.Adapter = new ProdutosIndicadosLitemItemAdapter(CurrentActivity, Resource.Layout.lista_produtos_pedido_indicado_row, produtosNaoVendidos);
                        CurrentAdapter = produtosNaoVendidos;
                    }
                    else
                    {
                        listProdutos.Adapter = new ProdutosIndicadosLitemItemAdapter(CurrentActivity, Resource.Layout.lista_produtos_pedido_indicado_row, ProdutosIndicados);
                        CurrentAdapter = ProdutosIndicados;
                    }
                }
                else
                {
                    int codigo;
                    List<CSProdutos.CSProduto> produtosFiltrados = new List<CSProdutos.CSProduto>();

                    if (int.TryParse(txtPesquisa.Text, out codigo))
                    {
                        produtosFiltrados = ProdutosIndicados.Where(p => p.DESCRICAO_APELIDO_PRODUTO.StartsWith(txtPesquisa.Text)).ToList();
                    }
                    else
                    {
                        produtosFiltrados = ProdutosIndicados.Where(p => (ListarApelido ? p.DSC_APELIDO_PRODUTO : p.DSC_PRODUTO).ToLower().Contains(txtPesquisa.Text.ToLower())).ToList();
                    }

                    if (chkFiltroProdutos.Checked)
                    {
                        produtosFiltrados = RetornaAdapterComRegraDeProdutosNaoVendidos(produtosFiltrados);
                    }

                    listProdutos.Adapter.Dispose();
                    listProdutos.Adapter = new ProdutosIndicadosLitemItemAdapter(CurrentActivity, Resource.Layout.lista_produtos_pedido_indicado_row, produtosFiltrados);
                    CurrentAdapter = produtosFiltrados;
                }
            }
            catch (Exception)
            {
            }
        }

        private static void MostrarResumo()
        {
            try
            {
                txtTotal.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(i => i.STATE != ObjectState.DELETADO && i.PRODUTO.IND_PROD_ESPECIFICO_CATEGORIA).Sum(ip => ip.VLR_TOTAL_ITEM).ToString(".00");
                txtProdutosVendidos.Text = string.Format("{0}/{1}", CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(i => i.STATE != ObjectState.DELETADO && i.PRODUTO.IND_PROD_ESPECIFICO_CATEGORIA).Count(), ProdutosIndicados.Count);
            }
            catch (Exception)
            {
            }
        }

        private void SaveScrollPosition()
        {
            try
            {
                currentPosition = listProdutos.FirstVisiblePosition;
            }
            catch (Exception)
            {
            }
        }

        private static void SetScrollPosition()
        {
            try
            {
                listProdutos.Adapter = new ProdutosIndicadosLitemItemAdapter(CurrentActivity, Resource.Layout.lista_produtos_pedido_indicado_row, CurrentAdapter);
                listProdutos.SetSelection(currentPosition);
            }
            catch (Exception)
            {
            }
        }

        private void SelecionarProduto(int position)
        {
            try
            {
                CSProdutos.Current = (CSProdutos.CSProduto)listProdutos.Adapter.GetItem(position);

                lvwProdutos_ItemActivate(position);
            }
            catch (Exception)
            {
            }
        }

        private void lvwProdutos_ItemActivate(int position)
        {
            try
            {
                var produtoJaVendido = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.STATE != ObjectState.DELETADO && p.PRODUTO.COD_PRODUTO == CSProdutos.Current.COD_PRODUTO).FirstOrDefault();

                if (produtoJaVendido != null)
                {
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current = produtoJaVendido;

                    // Guarda o valor do adcional financeiro
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_ADICIONAL_FINANCEIRO = CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO.PRC_ADICIONAL_FINANCEIRO;

                    // Guarda qual o produto atual do item do pedido
                    if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO != null)
                        CSProdutos.Current = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO;

                    //blAtualizaPreco = true;
                    bool blIndenizacaoItem = true;
                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2 && CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Count > 0 && blIndenizacaoItem == false)
                        CSPDVs.Current.PEDIDOS_PDV.Current.DesfazRateioIndenizacao();
                }
                else
                {
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current = new CSItemsPedido.CSItemPedido();

                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_ADICIONAL_FINANCEIRO = decimalPedido;

                    if (CSProdutos.Current.PRECOS_PRODUTO == null || CSProdutos.Current.PRECOS_PRODUTO.Count == 0)
                    {
                        if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 1 /*&& CSGlobal.CalculaPrecoNestle*/)
                            MessageBox.ShowShortMessageCenter(CurrentActivity, "Preço do produto não cadastrado.\r\nNão é possivel realizar esta venda.");
                        else
                            MessageBox.ShowShortMessageCenter(CurrentActivity, "Cliente ou Produto com informações incompletas no cadastro Nestlê!\nNão é possivel realizar esta venda.");

                        return;
                    }

                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO = CSProdutos.Current;

                    if (!CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.LOCK_QTD && CSGlobal.PedidoComCombo)
                        return;
                }

                PositionItemVendido = position;

                AbrirProduto(produtoJaVendido == null ? false : true);
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(Activity, ex.Message);
            }
        }

        private void AbrirDialogProduto(bool produtoVendido)
        {
            try
            {
                if (!CSGlobal.PedidoComCombo)
                {
                    ((Cliente)Activity).AbrirDialogProduto(ProdutoFoiVendidoNasUltimasVisitas(), !CSGlobal.PedidoComCombo, true, produtoVendido);

                    if (IsBunge())
                    {
                        if (!ValidarDadosBunge(CSProdutos.Current))
                            return;
                    }

                    SaveScrollPosition();
                }
                else
                    MessageBox.Alert(CurrentActivity, "Não é possível adicionar produtos em um pedido combo.");
            }
            catch (Exception)
            {
            }
        }

        private bool IsBunge()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 3;
        }

        private bool ValidarDadosBunge(CSProdutos.CSProduto produto)
        {
            bool retorno;

            try
            {
                CSPoliticaBunge pricingBunge = new CSPoliticaBunge(produto.COD_PRODUTO, CSEmpresa.Current.COD_NOTEBOOK1, CSPDVs.Current.COD_PDV, DateTime.Now, produto, 1, 0, 0m, null);
                pricingBunge.DadosIniciais();
                retorno = true;
            }
            catch (Exception ex)
            {
                retorno = false;

                MessageBox.AlertErro(CurrentActivity, ex.Message);
            }

            return retorno;
        }

        private bool ProdutoFoiVendidoNasUltimasVisitas()
        {
            try
            {
                foreach (CSUltimasVisitasPDV.CSUltimaVisitaPDV pedido in CSPDVs.Current.ULTIMAS_VISITAS.Items)
                {
                    CSPDVs.Current.ULTIMAS_VISITAS.Current = pedido;
                    var itempedido = CSPDVs.Current.ULTIMAS_VISITAS.Current.ITEMS_PEDIDOS.Items.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.PRODUTO.COD_PRODUTO == CSProdutos.Current.COD_PRODUTO).FirstOrDefault();
                    if (itempedido != null)
                        return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private class ThreadCarregarProdutosIndicados : AsyncTask
        {
            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                ListarProdutosIndicados();

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                listProdutos.Adapter = new ProdutosIndicadosLitemItemAdapter(CurrentActivity, Resource.Layout.lista_produtos_pedido_indicado_row, ProdutosIndicados);
                CurrentAdapter = ProdutosIndicados;

                MostrarResumo();

                CarregarAdapterAtualComFiltros();
                listProdutos.SetSelection(currentPosition);
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current = null;

                CarregandoDados = false;
                progressDialog.Dismiss();

                base.OnPostExecute(result);
            }

            private void ListarProdutosIndicados()
            {
                try
                {
                    if (ProdutosIndicados == null)
                    {
                        var listaProdutosIndicados = new List<CSProdutos.CSProduto>();

                        foreach (var prod in CSProdutos.Items.Cast<CSProdutos.CSProduto>().Distinct().Where(p => p.IND_PROD_TOP_CATEGORIA && ((p.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO == 0 ? p.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO_FILTRADO == ((Cliente)CurrentActivity).LinhaSelecionada.COD_GRUPO_COMERCIALIZACAO_FILTRADO : p.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO == ((Cliente)CurrentActivity).LinhaSelecionada.COD_GRUPO_COMERCIALIZACAO_FILTRADO) || ((Cliente)CurrentActivity).LinhaSelecionada.COD_GRUPO_COMERCIALIZACAO_FILTRADO == -1)).ToList())
                        {
                            if (!listaProdutosIndicados.Select(p => p.COD_PRODUTO).Contains(prod.COD_PRODUTO))
                                listaProdutosIndicados.Add(prod);
                        }

                        foreach (var prod in CSProdutos.Items.Cast<CSProdutos.CSProduto>().Distinct().Where(p => p.IND_PROD_ESPECIFICO_CATEGORIA && ((p.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO == 0 ? p.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO_FILTRADO == ((Cliente)CurrentActivity).LinhaSelecionada.COD_GRUPO_COMERCIALIZACAO_FILTRADO : p.GRUPO_COMERCIALIZACAO.COD_GRUPO_COMERCIALIZACAO == ((Cliente)CurrentActivity).LinhaSelecionada.COD_GRUPO_COMERCIALIZACAO_FILTRADO) || ((Cliente)CurrentActivity).LinhaSelecionada.COD_GRUPO_COMERCIALIZACAO_FILTRADO == -1)).ToList())
                        {
                            if (!listaProdutosIndicados.Select(p => p.COD_PRODUTO).Contains(prod.COD_PRODUTO))
                                listaProdutosIndicados.Add(prod);
                        }

                        ProdutosIndicados = listaProdutosIndicados;
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        class ProdutosIndicadosLitemItemAdapter : ArrayAdapter<CSProdutos.CSProduto>
        {
            Context context;
            IList<CSProdutos.CSProduto> produtos;
            int resourceId;

            public ProdutosIndicadosLitemItemAdapter(Context c, int resource, IList<CSProdutos.CSProduto> objects)
                : base(c, resource, objects)
            {
                context = c;
                produtos = objects;
                resourceId = resource;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                try
                {
                    CSProdutos.CSProduto produto = produtos[position];

                    LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);

                    if (convertView == null)
                        convertView = layout.Inflate(resourceId, null);

                    if (produto != null)
                    {
                        TextView tvDescProduto = convertView.FindViewById<TextView>(Resource.Id.tvDescProduto);
                        TextView tvQtd = convertView.FindViewById<TextView>(Resource.Id.tvQtd);
                        TextView tvPrecoUnitario = convertView.FindViewById<TextView>(Resource.Id.tvPrecoUnitario);
                        TextView tvPctDesc = convertView.FindViewById<TextView>(Resource.Id.tvPctDesc);
                        TextView tvValorTotal = convertView.FindViewById<TextView>(Resource.Id.tvValorTotal);
                        TextView tvUnidadeMedida = convertView.FindViewById<TextView>(Resource.Id.tvUnidadeMedida);
                        ImageView imgProdEspecifico = convertView.FindViewById<ImageView>(Resource.Id.imgProdEspecifico);

                        if (produto.IND_PROD_TOP_CATEGORIA)
                        {
                            imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_verde_top);
                            //ExisteProdutoTop = true;
                        }
                        else
                            imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_azul);

                        tvDescProduto.Text = produto.DESCRICAO_APELIDO_PRODUTO + " - " + (ListarApelido ? produto.DSC_APELIDO_PRODUTO : produto.DSC_PRODUTO);
                        tvUnidadeMedida.Text = produto.DSC_UNIDADE_MEDIDA;

                        if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS == null ||
                            CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Count == 0 ||
                            !CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.STATE != ObjectState.DELETADO).Select(it => it.PRODUTO.COD_PRODUTO).Contains(produto.COD_PRODUTO))
                        {
                            tvQtd.Text = "0";
                            tvPrecoUnitario.Text = string.Empty;
                            tvPctDesc.Text = string.Empty;
                            tvValorTotal.Text = string.Empty;
                        }
                        else
                        {
                            var itemPedido = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(i => i.PRODUTO.COD_PRODUTO == produto.COD_PRODUTO).FirstOrDefault();

                            if (produto.COD_UNIDADE_MEDIDA != "KG" && produto.COD_UNIDADE_MEDIDA != "LT")
                            {
                                tvQtd.Text = string.Format("{0}/{1}", itemPedido.QTD_PEDIDA_INTEIRA.ToString(), itemPedido.QTD_PEDIDA_UNIDADE.ToString("###000"));
                            }
                            else
                            {
                                tvQtd.Text = itemPedido.QTD_PEDIDA_INTEIRA.ToString();
                            }

                            tvPctDesc.Text = itemPedido.PRC_DESCONTO.ToString(CSGlobal.DecimalStringFormat) + "%";

                            tvPrecoUnitario.Text = itemPedido.VLR_ITEM_UNIDADE.ToString(CSGlobal.DecimalStringFormat);

                            tvValorTotal.Text = itemPedido.VLR_TOTAL_ITEM.ToString(CSGlobal.DecimalStringFormat);
                        }
                    }
                    return convertView;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }
}