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
using AvanteSales.Pro.Controles;
using AvanteSales.Pro.Dialogs;
using AvanteSales.SystemFramework;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
using AvanteSales.Pro.Activities;

namespace AvanteSales.Pro.Fragments
{
    public class ListaProdutosPedido : Android.Support.V4.App.Fragment
    {
        private const int frmProdutos = 0;
        View thisView;
        private static ActivitiesNames UltimaTela;
        private static int PosicaoLista = 0;
        private static ProgressDialog progressDialog;
        public static bool EdicaoSelecionada = false;
        private static Android.Support.V4.App.FragmentActivity CurrentActivity;
        private static bool m_IsDirty = false;
        private static bool IsLoading = false;
        private static bool blIndenizacaoItem = true;
        private static int? indexProdutoSelecionado = null;
        private decimal txtAdf;
        private static decimal txtDescontoIndenizacao;
        private LinearLayout HeaderListView;
        public static bool ListarApelido = false;
        public static List<CSItemsPedido.CSItemPedido> ItemsAdapter;
        string DescontoIndenizacao;
        string Adf;
        static ListView listProdutos;

        #region [ Propriedades ]

        private static bool IsDirty
        {
            get
            {
                return m_IsDirty;
            }
            set
            {
                if (IsLoading)
                    return;
                if (value == true)
                {
                    if (CSPDVs.Current.PEDIDOS_PDV.Current.STATE != ObjectState.NOVO)
                        CSPDVs.Current.PEDIDOS_PDV.Current.STATE = ObjectState.ALTERADO;
                }
                m_IsDirty = value;
            }
        }

        #endregion

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.lista_produtos_pedido, container, false);
            ItemsAdapter = new List<CSItemsPedido.CSItemPedido>();
            FindViewsById(view);
            Eventos();
            thisView = view;
            CurrentActivity = Activity;
            UltimaTela = (ActivitiesNames)Arguments.GetInt("ultimaActivity");
            DescontoIndenizacao = Arguments.GetString("txtDescontoIndenizacao", "0");
            Adf = Arguments.GetString("txtAdf", "0");

            return view;
        }

        private void CarregaDadosTelaPedido()
        {
            try
            {
                if (!CSGlobal.PedidoSugerido)
                {
                    LinearLayout llFooter = thisView.FindViewById<LinearLayout>(Resource.Id.llFooter);
                    llFooter.Visibility = ViewStates.Gone;
                    txtDescontoIndenizacao = CSGlobal.StrToDecimal(DescontoIndenizacao); ;
                    txtAdf = CSGlobal.StrToDecimal(Adf);
                }
                listProdutos.ItemClick += ListProdutos_ItemClick;
            }
            catch (Exception ex)
            {
                MessageBox.ShowShortMessageCenter(CurrentActivity, ex.Message);
            }
            CarregaListViewItemPedido();
        }

        private void ListProdutos_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            indexProdutoSelecionado = e.Position;

            var produtoSelecionado = ItemsAdapter[indexProdutoSelecionado.Value];

            if (!produtoSelecionado.IND_UTILIZA_QTD_SUGERIDA ||
                (!CSGlobal.PedidoSugerido && !produtoSelecionado.IND_UTILIZA_QTD_SUGERIDA))
            {
                var nomProduto = e.View.FindViewById<TextView>(Resource.Id.tvDescProduto).Text;

                switch (UltimaTela)
                {
                    case ActivitiesNames.SimulacaoPreco:
                        {
                            MessageBox.Alert(CurrentActivity, "Deseja excluir o produto?", "Excluir", ListView_ItemClick_Excluir_Yes, "Cancelar", null, true);
                        }
                        break;
                    default:
                        {
                            if (CSGlobal.PedidoComCombo)
                            {
                                MessageBox.Alert(CurrentActivity, "Selecione a opção desejada", "Excluir", ListView_ItemClick_Excluir, "Voltar", null, true);
                            }
                            else
                                MessageBox.Alert(CurrentActivity, "Selecione a opção desejada", "Editar", ListView_ItemClick_Editar, "Excluir", ListView_ItemClick_Excluir, true);
                        }
                        break;
                }
            }
            else
                MessageBox.AlertErro(CurrentActivity, "Não é permitido edição/exclusão de item do layout.");
        }

        protected void ListView_ItemClick_Editar(object sender, DialogClickEventArgs e)
        {
            PosicaoLista = listProdutos.FirstVisiblePosition;

            // Guarda qual o item de pedido atual
            CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current = ItemsAdapter[indexProdutoSelecionado.Value];

            // Guarda o valor do adcional financeiro
            CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_ADICIONAL_FINANCEIRO = txtAdf;

            // Guarda qual o produto atual do item do pedido
            if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO != null)
                CSProdutos.Current = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO;

            if (CSGlobal.PedidoSugerido)
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO.EDITOU_DADOS = true;

            //blAtualizaPreco = true;

            if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2 && CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Count > 0 && blIndenizacaoItem == false)
                CSPDVs.Current.PEDIDOS_PDV.Current.DesfazRateioIndenizacao();

            EdicaoSelecionada = true;

            ((Cliente)Activity).NavegarEdicaoProduto();
        }

        protected void ListView_ItemClick_Excluir(object sender, DialogClickEventArgs e)
        {
            if (indexProdutoSelecionado.HasValue)
                if (CSGlobal.PedidoComCombo)
                    MessageBox.Alert(CurrentActivity, "Deseja excluir todos os produtos do Combo?", "Excluir", ListView_ItemClick_Excluir_Yes, "Cancelar", null, true);
                else
                    MessageBox.Alert(CurrentActivity, "Deseja excluir o produto?", "Excluir", ListView_ItemClick_Excluir_Yes, "Cancelar", null, true);
        }

        private static void RefreshDadosTela()
        {
            CarregaListViewItemPedido();
        }

        protected void ListView_ItemClick_Excluir_Yes(object sender, DialogClickEventArgs e)
        {
            switch (UltimaTela)
            {
                case ActivitiesNames.SimulacaoPreco:
                    {
                        CSItemsPedido.CSItemPedido produto = ItemsAdapter[ListaProdutosPedido.indexProdutoSelecionado.Value];
                        produto.ATUALIZAR_SALDO_DESCONTO = false;
                        produto.STATE = ObjectState.DELETADO;

                        RefreshDadosTela();
                    }
                    break;

                default:
                    {
                        CSGlobal.PedidoComCombo = false;

                        PosicaoLista = listProdutos.FirstVisiblePosition;

                        progressDialog = new ProgressDialog(CurrentActivity);
                        progressDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
                        progressDialog.SetTitle("Processando...");
                        progressDialog.SetCancelable(false);
                        progressDialog.SetMessage("Excluindo produto...");
                        progressDialog.Show();

                        new ThreadExcluirProduto().Execute();
                    }
                    break;
            }

        }

        private static void ExcluirItensCombo(int produto)
        {
            for (int i = 0; i < listProdutos.Adapter.Count; i++)
            {
                CSItemsPedido.CSItemPedido pedido = ItemsAdapter[i];
                if (pedido.COD_ITEM_COMBO == produto)
                {
                    pedido.STATE = ObjectState.DELETADO;
                    pedido.AtualizaImagem();
                }
            }
            CSGlobal.PedidoComCombo = false;
        }

        private class ThreadExcluirProduto : AsyncTask<int, int, decimal>
        {
            protected override decimal RunInBackground(params int[] @params)
            {
                CSItemsPedido.CSItemPedido produto = ItemsAdapter[ListaProdutosPedido.indexProdutoSelecionado.Value];
                produto.STATE = ObjectState.DELETADO;

                if (produto.LOCK_QTD)
                {
                    ListaProdutosPedido.ExcluirItensCombo(produto.COD_ITEM_COMBO);
                }
                else
                {
                    produto.AtualizaImagem();
                    //ListView.RemoveViewAt(indexProdutoSelecionado.Value);
                }

                if (!ListaProdutosPedido.blIndenizacaoItem)
                    CSPDVs.Current.PEDIDOS_PDV.Current.CalculaRateioIndenizacao(ListaProdutosPedido.txtDescontoIndenizacao);

                ListaProdutosPedido.RefreshDadosTela();

                // Marca que foi excluido
                ListaProdutosPedido.IsDirty = true;

                //Limpa index do produto selecionado
                ListaProdutosPedido.indexProdutoSelecionado = null;
                CSGlobal.PedidoComCombo = false;
                CurrentActivity.SetResult(Result.Ok);
                return 0;
            }

            protected override void OnPostExecute(decimal result)
            {
                if (progressDialog != null)
                {
                    ((Cliente)CurrentActivity).AtualizarValorParcial();
                    progressDialog.Dismiss();
                    progressDialog.Dispose();
                    listProdutos.SetSelection(PosicaoLista);
                }
                base.OnPostExecute(result);
            }
        }

        class ProdutosPedidoLitemItemAdapter : ArrayAdapter<CSItemsPedido.CSItemPedido>
        {
            Context context;
            IList<CSItemsPedido.CSItemPedido> itensPedidos;
            int resourceId;

            public ProdutosPedidoLitemItemAdapter(Context c, int resource, IList<CSItemsPedido.CSItemPedido> objects)
                : base(c, resource, objects)
            {
                context = c;
                itensPedidos = objects;
                resourceId = resource;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSItemsPedido.CSItemPedido itemPedido = itensPedidos[position];

                LayoutInflater layout = (LayoutInflater)context.GetSystemService(Context.LayoutInflaterService);
                //View linha = layout.Inflate(resourceId, null);
                if (convertView == null)
                    convertView = layout.Inflate(resourceId, null);

                if (itemPedido != null)
                {
                    TextView tvDescProduto = convertView.FindViewById<TextView>(Resource.Id.tvDescProduto);
                    TextView tvQtd = convertView.FindViewById<TextView>(Resource.Id.tvQtd);
                    TextView tvPrecoUnitario = convertView.FindViewById<TextView>(Resource.Id.tvPrecoUnitario);
                    TextView tvPctDesc = convertView.FindViewById<TextView>(Resource.Id.tvPctDesc);
                    TextView tvValorTotal = convertView.FindViewById<TextView>(Resource.Id.tvValorTotal);
                    TextView tvUnidadeMedida = convertView.FindViewById<TextView>(Resource.Id.tvUnidadeMedida);
                    ImageView imgProdEspecifico = convertView.FindViewById<ImageView>(Resource.Id.imgProdEspecifico);

                    tvDescProduto.Text = itemPedido.PRODUTO.DESCRICAO_APELIDO_PRODUTO + " - " + (ListarApelido ? itemPedido.PRODUTO.DSC_APELIDO_PRODUTO : itemPedido.PRODUTO.DSC_PRODUTO);

                    if (itemPedido.PRODUTO.COD_UNIDADE_MEDIDA != "KG" && itemPedido.PRODUTO.COD_UNIDADE_MEDIDA != "LT")
                    {
                        tvQtd.Text = string.Format("{0}/{1}", itemPedido.QTD_PEDIDA_INTEIRA.ToString(), itemPedido.QTD_PEDIDA_UNIDADE.ToString("###000"));
                    }
                    else
                    {
                        tvQtd.Text = itemPedido.QTD_PEDIDA_INTEIRA.ToString();
                    }

                    tvPctDesc.Text = itemPedido.PRC_DESCONTO.ToString(CSGlobal.DecimalStringFormat) + "%";

                    decimal markup = Math.Round(itemPedido.VLR_ITEM_UNIDADE * (itemPedido.PRODUTO.GRUPO.PCT_MARKUP / 100), 2, MidpointRounding.AwayFromZero);

                    markup = itemPedido.VLR_ITEM_UNIDADE + markup;

                    tvPrecoUnitario.Text = string.Format("{0} / {1}", itemPedido.VLR_ITEM_UNIDADE.ToString(CSGlobal.DecimalStringFormat), markup.ToString());

                    tvValorTotal.Text = itemPedido.VLR_TOTAL_ITEM.ToString(CSGlobal.DecimalStringFormat);

                    tvUnidadeMedida.Text = itemPedido.PRODUTO.DSC_UNIDADE_MEDIDA;

                    bool especificoCategoria = false;

                    if (UltimaTela != ActivitiesNames.UltimasVisitas &&
                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.IND_UTILIZA_QTD_SUGERIDA && p.STATE != ObjectState.DELETADO).Count() > 0)
                    {
                        if (CSEmpresa.Current.IND_CLUSTER_PRODUTOS_PELO_TIPO_FREEZER)
                            especificoCategoria = CSProdutos.CSProduto.CSCategoria.GetEspecificoCategoria(CSPDVs.Current.COD_DENVER, itemPedido.PRODUTO.COD_PRODUTO);
                        else
                            especificoCategoria = CSProdutos.CSProduto.CSCategoria.GetEspecificoCategoria(CSPDVs.Current.COD_CATEGORIA, itemPedido.PRODUTO.COD_PRODUTO);

                        if (especificoCategoria)
                            imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_azul);
                        else
                            imgProdEspecifico.SetImageResource(Resource.Drawable.circulo_cinza);
                    }
                    else
                        imgProdEspecifico.Visibility = ViewStates.Gone;
                }
                return convertView;
            }
        }

        private static void CarregaListViewItemPedido()
        {
            var itensPedidos = CSProdutos.OrdenarListaProdutosPedido(CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.STATE != ObjectState.DELETADO).ToList());

            CurrentActivity.RunOnUiThread(() =>
            {
                ItemsAdapter = itensPedidos.ToList();
                listProdutos.Adapter = new ProdutosPedidoLitemItemAdapter(CurrentActivity, Resource.Layout.lista_produtos_pedido_row, ItemsAdapter);
                listProdutos.SetSelection(PosicaoLista);
            });
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            switch (UltimaTela)
            {
                case ActivitiesNames.Pedido:
                case ActivitiesNames.Produtos:
                case ActivitiesNames.SimulacaoPreco:
                    {
                        LinearLayout llFooter = view.FindViewById<LinearLayout>(Resource.Id.llFooter);
                        llFooter.Visibility = ViewStates.Gone;

                        if (!CSGlobal.PedidoSugerido)
                            CarregaDadosTelaPedido();
                    }
                    break;
                case ActivitiesNames.UltimasVisitas:
                    {
                        LinearLayout llFooter = view.FindViewById<LinearLayout>(Resource.Id.llFooter);
                        llFooter.Visibility = ViewStates.Gone;
                        CarregaListViewUltimasVisitas();
                    }
                    break;
                case ActivitiesNames.ResumoPedido:
                    {
                        LinearLayout llFooterpagina = view.FindViewById<LinearLayout>(Resource.Id.llFooter);
                        llFooterpagina.Visibility = ViewStates.Gone;
                        CarregaListViewResumoPedido();
                    }
                    break;
                case ActivitiesNames.Nenhum:
                default:
                    MessageBox.ShowShortMessageCenter(CurrentActivity, "Falha ao tentar descobrir a tela anterior. Por favor entre em contato com o suporte.");
                    break;
            }

            base.OnViewCreated(view, savedInstanceState);
        }

        private void CarregaListViewResumoPedido()
        {
            int quantidadeUnidadeMedida = -1;
            decimal qtdItem = 0;
            decimal qtdItemIndenizacao = 0;
            decimal valorUnitario = 0;
            decimal valorTotal = 0;
            decimal valorUnitarioIndenizacao = 0;
            string produto = string.Empty;
            string unidadeMedida = string.Empty;
            string unidadeMedidaMostra = string.Empty;
            decimal valorDesconto = 0;
            decimal peso = 0;

            List<CSListViewItem> listItemPedido = new List<CSListViewItem>();

            StringBuilder sqlQuery = new StringBuilder();

            SQLiteParameter paramCOD_PEDIDO = null;

            paramCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", ResumoDiaPedido.pedido);

            try
            {
                //if (Resumo_Tab_Pedido.pedido != -1)
                //{
                sqlQuery.Length = 0;
                sqlQuery.Append(" SELECT PEDIDO.COD_PEDIDO ");
                sqlQuery.Append("      , OPERACAO.DSC_OPERACAO ");
                sqlQuery.Append("      , CONDICAO_PAGAMENTO.DSC_CONDICAO_PAGAMENTO ");
                sqlQuery.Append("      , PDV.DSC_RAZAO_SOCIAL ");
                sqlQuery.Append("      , PRODUTO.DSC_PRODUTO ");
                sqlQuery.Append("      , PRODUTO.COD_UNIDADE_MEDIDA ");
                sqlQuery.Append("      , PRODUTO.QTD_UNIDADE_EMBALAGEM ");
                sqlQuery.Append("      , ITEM_PEDIDO.PRC_ADICIONAL_FINANCEIRO ");
                sqlQuery.Append("      , ITEM_PEDIDO.VLR_DESCONTO ");
                sqlQuery.Append("      , ITEM_PEDIDO.QTD_PEDIDA ");
                sqlQuery.Append("      , ITEM_PEDIDO.VLR_UNITARIO ");
                sqlQuery.Append("      , ITEM_PEDIDO.VLR_TOTAL ");
                sqlQuery.Append("      , ITEM_PEDIDO.QTD_INDENIZACAO ");
                sqlQuery.Append("      , ITEM_PEDIDO.VLR_UNITARIO_INDENIZACAO ");
                sqlQuery.Append("      , PEDIDO.VLR_TOTAL_PEDIDO");
                sqlQuery.Append("      , PRODUTO.VLR_PESO_PRODUTO");
                sqlQuery.Append("      , PRODUTO.DESCRICAO_APELIDO_PRODUTO");
                sqlQuery.Append("      , PRODUTO.DSC_APELIDO_PRODUTO");
                sqlQuery.Append("      , PEDIDO.COD_PDV");
                sqlQuery.Append("      , PRODUTO.COD_GRUPO");
                sqlQuery.Append("   FROM PEDIDO INNER JOIN ITEM_PEDIDO ");
                sqlQuery.Append("                  ON PEDIDO.COD_EMPREGADO = ITEM_PEDIDO.COD_EMPREGADO AND ");
                sqlQuery.Append("                     PEDIDO.COD_PEDIDO    = ITEM_PEDIDO.COD_PEDIDO ");
                sqlQuery.Append("               INNER JOIN PDV ");
                sqlQuery.Append("                  ON PEDIDO.COD_PDV = PDV.COD_PDV ");
                sqlQuery.Append("               INNER JOIN OPERACAO ");
                sqlQuery.Append("                  ON PEDIDO.COD_OPERACAO = OPERACAO.COD_OPERACAO ");
                sqlQuery.Append("               INNER JOIN CONDICAO_PAGAMENTO  ");
                sqlQuery.Append("                  ON PEDIDO.COD_CONDICAO_PAGAMENTO = CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO ");
                sqlQuery.Append("               INNER JOIN PRODUTO ");
                sqlQuery.Append("                  ON ITEM_PEDIDO.COD_PRODUTO = PRODUTO.COD_PRODUTO ");
                sqlQuery.Append("  WHERE PEDIDO.COD_PEDIDO = ? ");

                using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), paramCOD_PEDIDO))
                {
                    while (reader.Read())
                    {
                        CSListViewItem lviitempedido = new CSListViewItem();

                        produto = ListarApelido ? (reader.GetValue(17) == System.DBNull.Value ? "" : reader.GetString(17)) : (reader.GetValue(4) == System.DBNull.Value ? "" : reader.GetString(4));
                        unidadeMedida = reader.GetValue(5) == System.DBNull.Value ? "" : reader.GetString(5);
                        quantidadeUnidadeMedida = reader.GetValue(6) == System.DBNull.Value ? 0 : reader.GetInt32(6);

                        if (unidadeMedida == "CX")
                            unidadeMedidaMostra = unidadeMedida + "/" + quantidadeUnidadeMedida.ToString().Trim();
                        else
                            unidadeMedidaMostra = unidadeMedida;

                        qtdItem = reader.GetValue(9) == System.DBNull.Value ? 0 : reader.GetDecimal(9);
                        valorUnitario = reader.GetValue(10) == System.DBNull.Value ? 0 : reader.GetDecimal(10);
                        valorTotal = reader.GetValue(11) == System.DBNull.Value ? 0 : reader.GetDecimal(11);

                        qtdItemIndenizacao = reader.GetValue(12) == System.DBNull.Value ? 0 : reader.GetDecimal(12);
                        valorUnitarioIndenizacao = reader.GetValue(13) == System.DBNull.Value ? 0 : reader.GetDecimal(13);

                        valorDesconto = reader.GetValue(8) == System.DBNull.Value ? 0 : reader.GetDecimal(8);
                        peso = reader.GetValue(15) == System.DBNull.Value ? 0 : reader.GetDecimal(15);

                        int pdv = reader.GetValue(18) == DBNull.Value ? 0 : reader.GetInt32(18);
                        int grupo = reader.GetValue(19) == DBNull.Value ? 0 : reader.GetInt32(19);

                        decimal markup = Math.Round(valorUnitario * (CSGruposProduto.CSGrupoProduto.RetornarMakup(grupo, pdv) / 100), 2, MidpointRounding.AwayFromZero);
                        markup = valorUnitario + markup;

                        lviitempedido.Text = reader.GetValue(16) + " - " + (reader.GetValue(4) == System.DBNull.Value ? "" : reader.GetString(4));
                        lviitempedido.SubItems = new List<object>();
                        lviitempedido.SubItems.Add(unidadeMedidaMostra.Trim());
                        lviitempedido.SubItems.Add(CSProdutos.CSProduto.ConverteUnidadesParaMedida(qtdItem, unidadeMedida, quantidadeUnidadeMedida));
                        lviitempedido.SubItems.Add(valorTotal.ToString(CSGlobal.DecimalStringFormat));
                        lviitempedido.SubItems.Add(valorDesconto.ToString(CSGlobal.DecimalStringFormat));
                        lviitempedido.SubItems.Add(valorUnitario.ToString(CSGlobal.DecimalStringFormat));
                        lviitempedido.SubItems.Add(markup.ToString(CSGlobal.DecimalStringFormat));

                        listItemPedido.Add(lviitempedido);
                    }

                    listProdutos.Adapter = new ListarProdutosResumoPedido(CurrentActivity, Resource.Layout.lista_produtos_pedido_row, listItemPedido);
                }
                //}
            }

            catch (Exception e)
            {
                MessageBox.ShowShortMessageCenter(CurrentActivity, e.Message);
            }
        }

        class ListarProdutosResumoPedido : ArrayAdapter<CSListViewItem>
        {
            Activity act;
            IList<CSListViewItem> produto;
            int resourceId;

            public ListarProdutosResumoPedido(Activity c, int textViewResourceId, IList<CSListViewItem> objects)
                : base(c, textViewResourceId, objects)
            {
                act = c;
                produto = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSListViewItem item = produto[position];

                LayoutInflater layout = (LayoutInflater)act.GetSystemService(Context.LayoutInflaterService);
                //View linha = layout.Inflate(resourceId, null);

                if (convertView == null)
                    convertView = layout.Inflate(resourceId, null);

                try
                {
                    TextView tvDescProduto = convertView.FindViewById<TextView>(Resource.Id.tvDescProduto);
                    TextView tvQtd = convertView.FindViewById<TextView>(Resource.Id.tvQtd);
                    TextView tvPrecoUnitario = convertView.FindViewById<TextView>(Resource.Id.tvPrecoUnitario);
                    TextView tvPctDesc = convertView.FindViewById<TextView>(Resource.Id.tvPctDesc);
                    TextView tvValorTotal = convertView.FindViewById<TextView>(Resource.Id.tvValorTotal);
                    TextView tvUnidadeMedida = convertView.FindViewById<TextView>(Resource.Id.tvUnidadeMedida);
                    ImageView imgProdEspecifico = convertView.FindViewById<ImageView>(Resource.Id.imgProdEspecifico);

                    imgProdEspecifico.Visibility = ViewStates.Gone;

                    tvDescProduto.Text = item.Text;
                    tvQtd.Text = item.SubItems[1].ToString();
                    tvPrecoUnitario.Text = string.Format("{0} / {1}", item.SubItems[4].ToString(), item.SubItems[5].ToString());
                    tvPctDesc.Text = item.SubItems[3].ToString();
                    tvValorTotal.Text = item.SubItems[2].ToString();
                    tvUnidadeMedida.Text = item.SubItems[0].ToString();

                }
                catch (Exception ex)
                {
                    MessageBox.AlertErro(act, ex.Message);
                }

                return convertView;
            }
        }

        private void CarregaListViewUltimasVisitas()
        {
            var itensUltimasVisitas = CSProdutos.OrdenarListaProdutosPedido(CSPDVs.Current.ULTIMAS_VISITAS.Current.ITEMS_PEDIDOS.Items.Cast<CSItemsPedido.CSItemPedido>().ToList());

            listProdutos.Adapter = new ProdutosPedidoLitemItemAdapter(CurrentActivity, Resource.Layout.lista_produtos_pedido_row, itensUltimasVisitas);
        }

        private void Eventos()
        {

        }

        private void FindViewsById(View view)
        {
            listProdutos = view.FindViewById<ListView>(Resource.Id.listProdutos);
        }
    }
}