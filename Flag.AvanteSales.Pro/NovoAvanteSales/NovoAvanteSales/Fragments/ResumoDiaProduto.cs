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
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;
using AvanteSales.SystemFramework;
using AvanteSales.Pro.Formatters;
using AvanteSales.Pro.Dialogs;

namespace AvanteSales.Pro.Fragments
{
    public class ResumoDiaProduto : Android.Support.V4.App.Fragment
    {
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        LayoutInflater thisLayoutInflater;
        private Spinner cmbGrupoComercializacao;
        private Spinner cmbGrupoProduto;
        private Spinner cmbFamilia;
        private Spinner cmbOperacao;
        private TextView lblTotalItensQtd;
        private TextView lblValorFinalQtd;
        private TextView lblQtdCaixa;
        private RadioGroup rdgRadio;
        private RadioButton rdbValores;
        private RadioButton rdbQuantidades;
        private LinearLayout llHeader;
        private static int opcaoOrdenacao;
        private ListView listResumo;
        private View thisView;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.resumo_dia_produto, container, false);
            thisView = view;
            FindViewsById(view);
            Eventos();
            thisLayoutInflater = inflater;
            ActivityContext = Activity;
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);
            Inicializacao();
        }

        private void Inicializacao()
        {
            opcaoOrdenacao = 1;

            CarregarComboOperacao();
            CarregaComboBoxGrupoComercializacao();
            CarregaComboBoxGrupoProdutos();
            CarregarComboBoxFamilia();
        }

        private void Eventos()
        {
            cmbGrupoComercializacao.ItemSelected += CmbGrupoComercializacao_ItemSelected;
            cmbGrupoProduto.ItemSelected += CmbGrupoProduto_ItemSelected;
            cmbFamilia.ItemSelected += CmbFamilia_ItemSelected;
            cmbOperacao.ItemSelected += CmbOperacao_ItemSelected;
            rdbQuantidades.Click += RdbQuantidades_Click;
            rdbValores.Click += RdbValores_Click;
            llHeader.Click += LlHeader_Click;
        }

        private void LlHeader_Click(object sender, EventArgs e)
        {
            opcaoOrdenacao++;

            if (opcaoOrdenacao > 2)
                opcaoOrdenacao = 1;

            CarregaListProdutos();
        }

        private void RdbValores_Click(object sender, EventArgs e)
        {
            llHeader.RemoveAllViews();
            listResumo.Adapter = null;
            View headerView = thisLayoutInflater.Inflate(Resource.Layout.resumo_dia_produto_valores_header, null);
            llHeader.AddView(headerView);

            TextView tvDesc = headerView.FindViewById<TextView>(Resource.Id.tvDesc);

            if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2)
                tvDesc.Visibility = ViewStates.Gone;

            CarregaListProdutos();
        }

        private void RdbQuantidades_Click(object sender, EventArgs e)
        {
            llHeader.RemoveAllViews();
            listResumo.Adapter = null;
            View view = thisLayoutInflater.Inflate(Resource.Layout.resumo_dia_produto_quantidades_header, null);
            llHeader.AddView(view);
            CarregaListProdutos();
        }

        private void CmbOperacao_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            CarregaListProdutos();
        }

        private void CmbFamilia_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            listResumo.Adapter = null;

            if (cmbGrupoComercializacao.Adapter != null && cmbGrupoProduto.Adapter != null && cmbFamilia.Adapter != null)
            {
                CarregaListProdutos();
            }
        }

        private void CmbGrupoProduto_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            CarregarComboBoxFamilia();
        }

        private void CmbGrupoComercializacao_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            listResumo.Adapter = null;

            if (cmbGrupoComercializacao.Adapter != null && cmbGrupoProduto.Adapter != null && cmbFamilia.Adapter != null)
            {
                CarregaListProdutos();
            }
        }

        private void FindViewsById(View view)
        {
            cmbGrupoComercializacao = view.FindViewById<Spinner>(Resource.Id.cmbGrupoComercializacao);
            cmbGrupoProduto = view.FindViewById<Spinner>(Resource.Id.cmbGrupoProduto);
            cmbFamilia = view.FindViewById<Spinner>(Resource.Id.cmbFamilia);
            cmbOperacao = view.FindViewById<Spinner>(Resource.Id.cmbOperacao);
            lblTotalItensQtd = view.FindViewById<TextView>(Resource.Id.lblTotalItensQtd);
            lblValorFinalQtd = view.FindViewById<TextView>(Resource.Id.lblValorFinalQtd);
            rdbValores = view.FindViewById<RadioButton>(Resource.Id.rdbValores);
            rdbQuantidades = view.FindViewById<RadioButton>(Resource.Id.rdbQuantidades);
            llHeader = view.FindViewById<LinearLayout>(Resource.Id.llHeader);
            rdgRadio = view.FindViewById<RadioGroup>(Resource.Id.rdgRadio);
            lblQtdCaixa = view.FindViewById<TextView>(Resource.Id.lblQtdCaixa);
            listResumo = view.FindViewById<ListView>(Resource.Id.listResumo);
        }

        private void CarregarComboOperacao()
        {
            cmbOperacao.Adapter = null;

            var adapter = cmbOperacao.SetDefaultAdapter();

            CSItemCombo ic = new CSItemCombo();

            ic.Texto = "Venda";
            ic.Valor = 1;

            ((ArrayAdapter)cmbOperacao.Adapter).Add(ic);

            ic = new CSItemCombo();

            ic.Texto = "Bonificação";
            ic.Valor = 2;

            ((ArrayAdapter)cmbOperacao.Adapter).Add(ic);

            ic = new CSItemCombo();

            ic.Texto = "Troca";
            ic.Valor = 3;

            ((ArrayAdapter)cmbOperacao.Adapter).Add(ic);

            ic = new CSItemCombo();

            ic.Texto = "Outras S.R";
            ic.Valor = 4;

            ((ArrayAdapter)cmbOperacao.Adapter).Add(ic);
        }

        private void CarregaComboBoxGrupoComercializacao()
        {
            // [ Limpa o combo ]
            cmbGrupoComercializacao.Adapter = null;

            var adapter = cmbGrupoComercializacao.SetDefaultAdapter();

            // [ Preenche o combo ]
            foreach (CSGruposComercializacao.CSGrupoComercializacao grp in CSGruposComercializacao.Items)
            {
                CSItemCombo ic = new CSItemCombo();
                ic.Texto = grp.DES_GRUPO_COMERCIALIZACAO;
                ic.Valor = grp;
                adapter.Add(ic);
            }

            // Adiciona um opção para selecionar todos os grupos
            CSGruposComercializacao.CSGrupoComercializacao grptodos = new CSGruposComercializacao.CSGrupoComercializacao();
            grptodos.COD_GRUPO_COMERCIALIZACAO = -1;
            grptodos.DES_GRUPO_COMERCIALIZACAO = "==== TODOS ====";
            grptodos.COD_SETOR_BROKER = "";

            CSItemCombo ictodos = new CSItemCombo();
            ictodos.Texto = grptodos.DES_GRUPO_COMERCIALIZACAO;
            ictodos.Valor = grptodos;
            adapter.Add(ictodos);

            // Coloca como default o último grupo.
            if (cmbGrupoComercializacao.Adapter != null)
                cmbGrupoComercializacao.SetSelection(cmbGrupoComercializacao.Adapter.Count - 1);
        }

        private void CarregaComboBoxGrupoProdutos()
        {
            cmbGrupoProduto.Adapter = null;

            var adapter = cmbGrupoProduto.SetDefaultAdapter();

            // [ Preenche o combo de grupos do produtos ]
            foreach (CSGruposProduto.CSGrupoProduto grp in CSGruposProduto.Items)
            {
                CSItemCombo ic = new CSItemCombo();

                ic.Texto = grp.DSC_GRUPO;
                ic.Valor = grp;

                adapter.Add(ic);
            }

            // Adiciona um opção para selecionar todos os grupos
            CSGruposProduto.CSGrupoProduto grptodos = new CSGruposProduto.CSGrupoProduto();

            grptodos.COD_GRUPO = -1;
            grptodos.DSC_GRUPO = "==== TODOS ====";

            CSItemCombo ictodos = new CSItemCombo();

            ictodos.Texto = grptodos.DSC_GRUPO;
            ictodos.Valor = grptodos;

            adapter.Add(ictodos);

            // Coloca como default o último grupo.
            if (cmbGrupoProduto.Adapter != null)
                cmbGrupoProduto.SetSelection(cmbGrupoProduto.Adapter.Count - 1);
        }

        private void CarregarComboBoxFamilia()
        {
            // Busca o grupo selecionado
            CSGruposProduto.CSGrupoProduto grupo = (CSGruposProduto.CSGrupoProduto)((CSItemCombo)cmbGrupoProduto.SelectedItem).Valor;

            // Limpa o combo de familias
            cmbFamilia.Adapter = null;

            // Preenche o combo de familias a partir do grupo selecionado
            var adapter = cmbFamilia.SetDefaultAdapter();

            foreach (CSFamiliasProduto.CSFamiliaProduto fam in CSFamiliasProduto.Items)
            {
                if (fam.GRUPO.COD_GRUPO == grupo.COD_GRUPO)
                {
                    CSItemCombo ic = new CSItemCombo();

                    ic.Texto = fam.DSC_FAMILIA_PRODUTO;
                    ic.Valor = fam;
                    adapter.Add(ic);
                }
            }

            // Adiciona um opção para selecionar todos as produtos da familia
            CSFamiliasProduto.CSFamiliaProduto famtodos = new CSFamiliasProduto.CSFamiliaProduto();
            famtodos.GRUPO = grupo;
            famtodos.COD_FAMILIA_PRODUTO = -1;

            famtodos.DSC_FAMILIA_PRODUTO = "==== TODOS ====";


            CSItemCombo ictodos = new CSItemCombo();
            ictodos.Texto = famtodos.DSC_FAMILIA_PRODUTO;
            ictodos.Valor = famtodos;
            adapter.Add(ictodos);

            // Coloca como default a opção todos os produtos da familia.
            if (cmbFamilia.Adapter.Count > 0)
                cmbFamilia.SetSelection(cmbFamilia.Adapter.Count - 1);
        }

        private void CarregaListProdutos()
        {
            List<CSListViewItem> listaItens = new List<CSListViewItem>();

            SQLiteParameter pCOD_GRUPO = null;
            SQLiteParameter pCOD_GRUPO_COMERCIALIZACAO = null;
            SQLiteParameter pCOD_FAMILIA_PRODUTO = null;
            SQLiteParameter pDAT_PEDIDO = null;
            SQLiteParameter pCOD_PRODUTO = null;
            StringBuilder sqlQuery = new StringBuilder();
            CSGruposProduto.CSGrupoProduto grupo =
                (CSGruposProduto.CSGrupoProduto)((CSItemCombo)cmbGrupoProduto.SelectedItem).Valor;

            CSGruposComercializacao.CSGrupoComercializacao grupoComercializacao =
                (CSGruposComercializacao.CSGrupoComercializacao)((CSItemCombo)cmbGrupoComercializacao.SelectedItem).Valor;

            CSFamiliasProduto.CSFamiliaProduto familia = (CSFamiliasProduto.CSFamiliaProduto)((CSItemCombo)cmbFamilia.SelectedItem).Valor;

            //CSOperacoes.CSOperacao operacao = (CSOperacoes.CSOperacao)((CSItemCombo)cmbOperacao.SelectedItem).Valor;

            string sqlProduto;

            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT T3.DESCRICAO_APELIDO_PRODUTO, T3.DSC_PRODUTO ");
            sqlQuery.Append("      ,T3.COD_UNIDADE_MEDIDA,T3.QTD_UNIDADE_EMBALAGEM ");
            sqlQuery.Append("      ,SUM(T2.QTD_PEDIDA) - SUM(T2.QTD_INDENIZACAO) AS QTD_PEDIDA ");
            sqlQuery.Append("      ,SUM(T2.VLR_TOTAL) - SUM(T2.VLR_UNITARIO_INDENIZACAO) AS VLR_TOTAL ");
            sqlQuery.Append("      ,SUM(T2.VLR_DESCONTO) AS VLR_DESCONTO ");
            sqlQuery.Append("      ,COUNT(T1.COD_PEDIDO) ");
            sqlQuery.Append("      ,T2.COD_PRODUTO ");
            sqlQuery.Append("  FROM PEDIDO T1 ");
            sqlQuery.Append("  JOIN ITEM_PEDIDO T2 ON T1.COD_PEDIDO = T2.COD_PEDIDO ");
            sqlQuery.Append("   AND T1.COD_EMPREGADO = T2.COD_EMPREGADO ");
            sqlQuery.Append("  JOIN PRODUTO T3 ON T2.COD_PRODUTO = T3.COD_PRODUTO ");
            sqlQuery.Append("  JOIN OPERACAO T4 ON T4.COD_OPERACAO = T1.COD_OPERACAO ");

            //1 = Venda com tipo de CFO = 1,2 OU 21
            //2 = Bonificação com tipo de CFO = 3
            //3 = Troca com tipo de CFO = 6
            //4 = Outras sem receita diferente de 1,2,21,3 e 6

            switch (((CSItemCombo)cmbOperacao.SelectedItem).Valor.ToString())
            {
                case "1":
                    {
                        sqlQuery.Append("   AND T4.COD_OPERACAO_CFO IN (1,2, 21) ");
                    }
                    break;

                case "2":
                    {
                        sqlQuery.Append("   AND T4.COD_OPERACAO_CFO = 3 ");
                    }
                    break;

                case "3":
                    {
                        sqlQuery.Append("   AND T4.COD_OPERACAO_CFO = 6 ");
                    }
                    break;

                case "4":
                    {
                        sqlQuery.Append("   AND T4.COD_OPERACAO_CFO NOT IN (1,2,3,6,21) ");
                    }
                    break;
            }

            sqlQuery.Append(" WHERE T1.IND_HISTORICO = 0 AND DATE(T1.DAT_PEDIDO) = DATE(?) ");

            sqlQuery.Append("   AND ? IN (-1, T3.COD_GRUPO) ");
            sqlQuery.Append("   AND ? IN (-1, T3.COD_GRUPO_COMERCIALIZACAO) ");
            sqlQuery.Append("   AND ? IN (-1, T3.COD_FAMILIA_PRODUTO) ");
            // [ Indenização ]
            sqlQuery.Append("   AND T2.QTD_PEDIDA > 0 ");
            sqlQuery.Append("   AND (T2.QTD_PEDIDA - T2.QTD_INDENIZACAO) > 0 ");
            sqlQuery.Append(" GROUP BY T2.COD_PRODUTO,T3.DESCRICAO_APELIDO_PRODUTO,T3.DSC_PRODUTO,T3.COD_UNIDADE_MEDIDA,T3.QTD_UNIDADE_EMBALAGEM ");

            if (opcaoOrdenacao == 1)
                sqlQuery.Append(" ORDER BY CAST(DESCRICAO_APELIDO_PRODUTO AS INT) ");
            else
                sqlQuery.Append(" ORDER BY T3.DSC_PRODUTO ");

            sqlProduto = sqlQuery.ToString();

            // [ Monta query para selecionar os pdvs que compraram o produto ]
            sqlQuery.Length = 0;
            sqlQuery.Append("SELECT COUNT(*) ");
            sqlQuery.Append("  FROM PDV ");
            sqlQuery.Append(" WHERE COD_PDV IN (SELECT T1.COD_PDV ");
            sqlQuery.Append("                     FROM PEDIDO T1 ");
            sqlQuery.Append("                     JOIN ITEM_PEDIDO T2 ON T1.COD_PEDIDO = T2.COD_PEDIDO ");
            sqlQuery.Append("                      AND T1.COD_EMPREGADO = T2.COD_EMPREGADO ");
            sqlQuery.Append("                     JOIN OPERACAO T3 ON T3.COD_OPERACAO = T1.COD_OPERACAO ");

            //1 = Venda com tipo de CFO = 1,2 OU 21
            //2 = Bonificação com tipo de CFO = 3
            //3 = Troca com tipo de CFO = 6
            //4 = Outras sem receita diferente de 1,2,21,3 e 6

            switch (((CSItemCombo)cmbOperacao.SelectedItem).Valor.ToString())
            {
                case "1":
                    {
                        sqlQuery.Append("   AND T3.COD_OPERACAO_CFO IN (1,2, 21) ");
                    }
                    break;

                case "2":
                    {
                        sqlQuery.Append("   AND T3.COD_OPERACAO_CFO = 3 ");
                    }
                    break;

                case "3":
                    {
                        sqlQuery.Append("   AND T3.COD_OPERACAO_CFO = 6 ");
                    }
                    break;

                case "4":
                    {
                        sqlQuery.Append("   AND T3.COD_OPERACAO_CFO NOT IN (1,2,3,6,21) ");
                    }
                    break;
            }

            sqlQuery.Append("                    WHERE T1.IND_HISTORICO = 0 ");
            sqlQuery.Append("                      AND DATE(T1.DAT_PEDIDO) = DATE(?) ");

            sqlQuery.Append("                      AND T2.COD_PRODUTO = ?) ");

            //sqlPDV = sqlQuery.ToString();

            pDAT_PEDIDO = new SQLiteParameter("@DAT_PEDIDO", DateTime.Now.Date);

            pCOD_GRUPO = new SQLiteParameter("@COD_GRUPO", grupo.COD_GRUPO);
            pCOD_GRUPO_COMERCIALIZACAO = new SQLiteParameter("@COD_GRUPO_COMERCIALIZACAO", grupoComercializacao.COD_GRUPO_COMERCIALIZACAO);
            pCOD_FAMILIA_PRODUTO = new SQLiteParameter("@COD_FAMILIA_PRODUTO", familia.COD_FAMILIA_PRODUTO);

            int totalItens = 0;
            decimal valorFinal = 0;
            int qtdCaixa = 0;
            // Busca todos os contatos do PDV
            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlProduto.ToString(), pDAT_PEDIDO, pCOD_GRUPO, pCOD_GRUPO_COMERCIALIZACAO, pCOD_FAMILIA_PRODUTO))
            {
                while (sqlReader.Read())
                {
                    totalItens++;

                    CSListViewItem lvi = new CSListViewItem();
                    lvi.SubItems = new List<object>();
                    lvi.Text = sqlReader.GetString(0).Trim() + " - " + sqlReader.GetString(1).Trim();
                    lvi.SubItems.Add(CSProdutos.CSProduto.ConverteUnidadesParaMedida(Convert.ToDecimal(sqlReader.GetValue(4)), sqlReader.GetString(2), sqlReader.GetInt32(3)));

                    // [ Valor total ]
                    decimal valorTotal = Convert.ToDecimal(sqlReader.GetValue(5));
                    valorFinal += valorTotal;
                    lvi.SubItems.Add(valorTotal.ToString(CSGlobal.DecimalStringFormat));

                    // [ Valor médio do item ]
                    lvi.SubItems.Add(((Convert.ToDecimal(sqlReader.GetValue(5)) * CSProdutos.CSProduto.GetUnidadesPorCaixa(sqlReader.GetString(2), sqlReader.GetInt32(3))) / Convert.ToDecimal(sqlReader.GetValue(4))).ToString(CSGlobal.DecimalStringFormat));

                    // [ Oculta coluna de desconto para brokers ]
                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2)
                    {
                        // [ Desconto ]
                        lvi.SubItems.Add(Convert.ToDecimal(sqlReader.GetValue(6)).ToString(CSGlobal.DecimalStringFormat));
                    }

                    // [ Quantidade de pedidos quem contém o item ]
                    lvi.SubItems.Add(sqlReader.GetInt32(7).ToString());

                    pDAT_PEDIDO = new SQLiteParameter("@DAT_PEDIDO", DateTime.Now.Date);
                    pCOD_PRODUTO = new SQLiteParameter("@COD_PRODUTO", sqlReader.GetInt32(8));

                    int numPDVs = Convert.ToInt32(CSDataAccess.Instance.ExecuteScalar(sqlQuery.ToString(), pDAT_PEDIDO, pCOD_PRODUTO));
                    lvi.SubItems.Add(numPDVs.ToString());

                    listaItens.Add(lvi);

                    qtdCaixa += CSProdutos.CSProduto.ConverterParaQuantidadeCaixaVendida(Convert.ToDecimal(sqlReader.GetValue(4)), sqlReader.GetString(2), sqlReader.GetInt32(3));
                }
                sqlReader.Close();
                sqlReader.Dispose();

                RadioButton rdb = thisView.FindViewById<RadioButton>(rdgRadio.CheckedRadioButtonId);

                if (rdb != null)
                {
                    if (rdb.Text == "Quantidades")
                        listResumo.Adapter = new ListarResumoProdutosQuantidades(ActivityContext, Resource.Layout.resumo_dia_produto_quantidades_row, listaItens);

                    else
                        listResumo.Adapter = new ListarResumoProdutosValores(ActivityContext, Resource.Layout.resumo_dia_produto_valores_row, listaItens);
                }
            }

            lblTotalItensQtd.Text = totalItens.ToString();
            lblValorFinalQtd.Text = valorFinal.ToString(CSGlobal.DecimalStringFormat);
            lblQtdCaixa.Text = qtdCaixa.ToString();
        }

        private class ListarResumoProdutosValores : ArrayAdapter<CSListViewItem>
        {
            Activity act;
            IList<CSListViewItem> produto;
            int resourceId;

            public ListarResumoProdutosValores(Activity c, int textViewResourceId, IList<CSListViewItem> objects)
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
                View linha = layout.Inflate(resourceId, null);

                try
                {
                    TextView tvProduto = linha.FindViewById<TextView>(Resource.Id.tvProduto);
                    TextView tvQtd = linha.FindViewById<TextView>(Resource.Id.tvQtd);
                    TextView tvValorTotal = linha.FindViewById<TextView>(Resource.Id.tvValorTotal);
                    TextView tvValorMedio = linha.FindViewById<TextView>(Resource.Id.tvValorMedio);
                    TextView tvValorDesconto = linha.FindViewById<TextView>(Resource.Id.tvValorDesconto);

                    tvProduto.Text = item.Text;
                    tvQtd.Text = item.SubItems[0].ToString();
                    tvValorTotal.Text = item.SubItems[1].ToString();
                    tvValorMedio.Text = item.SubItems[2].ToString();

                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2)
                    {
                        tvValorDesconto.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        tvValorDesconto.Text = item.SubItems[3].ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.AlertErro(ActivityContext, ex.Message);
                }

                return linha;
            }

        }

        private class ListarResumoProdutosQuantidades : ArrayAdapter<CSListViewItem>
        {
            Activity act;
            IList<CSListViewItem> produto;
            int resourceId;

            public ListarResumoProdutosQuantidades(Activity c, int textViewResourceId, IList<CSListViewItem> objects)
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
                View linha = layout.Inflate(resourceId, null);

                try
                {
                    TextView tvProduto = linha.FindViewById<TextView>(Resource.Id.tvProduto);
                    TextView tvQtd = linha.FindViewById<TextView>(Resource.Id.tvQtd);
                    TextView tvQtdPed = linha.FindViewById<TextView>(Resource.Id.tvQtdPed);
                    TextView tvQtdCli = linha.FindViewById<TextView>(Resource.Id.tvQtdCli);

                    tvProduto.Text = item.Text;
                    tvQtd.Text = item.SubItems[0].ToString();

                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2)
                    {
                        tvQtdPed.Text = item.SubItems[4].ToString();
                        tvQtdCli.Text = item.SubItems[5].ToString();
                    }

                    else
                    {
                        tvQtdPed.Text = item.SubItems[3].ToString();
                        tvQtdCli.Text = item.SubItems[4].ToString();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.AlertErro(ActivityContext, ex.Message);
                }

                return linha;
            }

        }
    }
}