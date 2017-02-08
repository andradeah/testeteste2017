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
using AvanteSales.SystemFramework;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;

namespace AvanteSales.Pro.Fragments
{
    public class AcompanhamentoVendas : Android.Support.V4.App.Fragment
    {
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        Relatorio relatorio;
        static Spinner cmbGrupoComercializacao;
        static Spinner cmbGrupoProduto;
        static Spinner cmbFamiliaProduto;
        static CheckBox chkMostraValor;
        static CheckBox chkMeta;
        static RadioGroup rdgRadio;
        static RadioButton rdbAcumulado;
        static RadioButton rdbDia;
        static RadioButton rdbTendencia;
        static LinearLayout llHeader;
        static ListView lvwMetasVendas;
        static LinearLayout llBottom;
        static LinearLayout llBottomQTD;
        static TextView lblLabelA;
        static TextView lblLabelB;
        static TextView lblLabelC;
        static TextView lblPrimeiraLabel;
        static TextView lblSegundaLabel;
        static TextView lblTerceiraLabel;
        static TextView lblTotalItensQtd;
        static TextView lblQtdCaixa;
        private static bool CarregandoActivity;
        static ProgressDialog progress;
        LayoutInflater thisLayoutInflater;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.acompanhamento_vendas, container, false);
            FindViewsById(view);
            Eventos();
            thisLayoutInflater = inflater;
            ActivityContext = Activity;
            relatorio = (Relatorio)Activity;
            CarregandoActivity = true;
            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            Inicializacao();
        }

        private void Inicializacao()
        {
            progress = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
            progress.Show();

            new ThreadCarregaComboBoxGrupoComercializacao().Execute();
        }

        private void Eventos()
        {
            cmbGrupoProduto.ItemSelected += CmbGrupoProduto_ItemSelected;
            cmbGrupoComercializacao.ItemSelected += CmbGrupoComercializacao_ItemSelected;
            cmbFamiliaProduto.ItemSelected += CmbFamiliaProduto_ItemSelected;
            rdbAcumulado.Click += RdbAcumulado_Click;
            rdbDia.Click += RdbDia_Click;
            rdbTendencia.Click += RdbTendencia_Click;
            chkMeta.Click += ChkMeta_Click;
            chkMostraValor.Click += ChkMostraValor_Click;
        }

        private void ChkMostraValor_Click(object sender, EventArgs e)
        {
            if (!CarregandoActivity)
                CarregaMetasVendas();
        }

        private void ChkMeta_Click(object sender, EventArgs e)
        {
            if (!CarregandoActivity)
                CarregaMetasVendas();
        }

        private void RdbTendencia_Click(object sender, EventArgs e)
        {
            llHeader.RemoveAllViews();
            View view = thisLayoutInflater.Inflate(Resource.Layout.acompanhamento_vendas_header_tendencia, null);
            llHeader.AddView(view);

            lblLabelA.Text = "Total tendência";
            lblLabelB.Text = "Total objetivo";
            lblLabelC.Visibility = ViewStates.Visible;
            lblLabelC.Text = "%";

            if (!CarregandoActivity)
                CarregaMetasVendas();
        }

        private void RdbDia_Click(object sender, EventArgs e)
        {
            llHeader.RemoveAllViews();
            View view = thisLayoutInflater.Inflate(Resource.Layout.acompanhamento_vendas_header_dia, null);
            llHeader.AddView(view);

            lblLabelA.Text = "Total meta";
            lblLabelB.Text = "Total venda";
            lblLabelC.Visibility = ViewStates.Visible;
            lblLabelC.Text = "%";

            if (!CarregandoActivity)
                CarregaMetasVendas();
        }

        private void RdbAcumulado_Click(object sender, EventArgs e)
        {
            llHeader.RemoveAllViews();
            View view = thisLayoutInflater.Inflate(Resource.Layout.acompanhamento_vendas_header_acumulado, null);
            llHeader.AddView(view);

            lblLabelA.Text = "Total objetivo";
            lblLabelB.Text = "Total acumulado";
            lblLabelC.Visibility = ViewStates.Gone;

            if (!CarregandoActivity)
                CarregaMetasVendas();
        }

        private void CmbFamiliaProduto_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (!CarregandoActivity)
                CarregaMetasVendas();
        }

        private void CmbGrupoComercializacao_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (!CarregandoActivity)
                CarregaMetasVendas();
        }

        private void CarregaMetasVendas()
        {
            if (rdgRadio.CheckedRadioButtonId == -1)
                return;

            progress = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
            progress.Show();

            new ThreadCarregaMetasVendas().Execute();
        }

        private void CmbGrupoProduto_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (!CarregandoActivity)
            {
                progress = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
                progress.Show();

                new ThreadCarregarComboBoxFamiliaProduto().Execute();
            }
        }

        private void FindViewsById(View view)
        {
            lvwMetasVendas = view.FindViewById<ListView>(Resource.Id.lvwMetasVendas);
            cmbGrupoComercializacao = view.FindViewById<Spinner>(Resource.Id.cmbGrupoComercializacao);
            cmbGrupoProduto = view.FindViewById<Spinner>(Resource.Id.cmbGrupoProduto);
            cmbFamiliaProduto = view.FindViewById<Spinner>(Resource.Id.cmbFamiliaProduto);
            chkMostraValor = view.FindViewById<CheckBox>(Resource.Id.chkMostraValor);
            chkMeta = view.FindViewById<CheckBox>(Resource.Id.chkMeta);
            rdgRadio = view.FindViewById<RadioGroup>(Resource.Id.rdgRadio);
            rdbAcumulado = view.FindViewById<RadioButton>(Resource.Id.rdbAcumulado);
            rdbDia = view.FindViewById<RadioButton>(Resource.Id.rdbDia);
            rdbTendencia = view.FindViewById<RadioButton>(Resource.Id.rdbTendencia);
            llHeader = view.FindViewById<LinearLayout>(Resource.Id.llHeader);
            llBottom = view.FindViewById<LinearLayout>(Resource.Id.llBottom);
            llBottomQTD = view.FindViewById<LinearLayout>(Resource.Id.llBottomQTD);
            lblLabelA = view.FindViewById<TextView>(Resource.Id.lblLabelA);
            lblLabelB = view.FindViewById<TextView>(Resource.Id.lblLabelB);
            lblLabelC = view.FindViewById<TextView>(Resource.Id.lblLabelC);
            lblPrimeiraLabel = view.FindViewById<TextView>(Resource.Id.lblPrimeiraLabel);
            lblSegundaLabel = view.FindViewById<TextView>(Resource.Id.lblSegundaLabel);
            lblTerceiraLabel = view.FindViewById<TextView>(Resource.Id.lblTerceiraLabel);
            lblTotalItensQtd = view.FindViewById<TextView>(Resource.Id.lblTotalItensQtd);
            lblQtdCaixa = view.FindViewById<TextView>(Resource.Id.lblQtdCaixa);
        }

        private class ThreadCarregarComboBoxFamiliaProduto : AsyncTask
        {
            ArrayAdapter adapter;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                CarregarComboBoxFamiliaProduto();

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                // Limpa o combo de familias
                cmbFamiliaProduto.Adapter = null;

                cmbFamiliaProduto.Adapter = adapter;

                cmbFamiliaProduto.SetSelection(cmbFamiliaProduto.Adapter.Count - 1);

                CarregandoActivity = false;

                if (progress != null)
                    progress.Dismiss();
            }

            private void CarregarComboBoxFamiliaProduto()
            {
                adapter = new ArrayAdapter(ActivityContext, Android.Resource.Layout.SimpleSpinnerItem);
                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                // Busca o grupo selecionado
                CSGruposProduto.CSGrupoProduto grupo = (CSGruposProduto.CSGrupoProduto)((CSItemCombo)cmbGrupoProduto.SelectedItem).Valor;

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
            }
        }

        private class ThreadCarregaMetasVendas : AsyncTask
        {
            StringBuilder sqlQuery = null;
            string produto = "";
            string unidadeMedida = "";
            string unidadeMedidaMostra = "";
            int quantidadeUnidadeMedida = -1;
            int codigo_produto = 0;
            decimal objetivo = 0;
            decimal acumulado = 0;
            decimal tendencia = 0;
            decimal metadia = 0;
            decimal qtd_objetivo = 0;
            decimal qtd_tendencia = 0;
            decimal venda_dia = 0;
            decimal total_objetivo = 0;
            decimal total_acumulado = 0;
            decimal total_tendencia = 0;
            decimal total_metadia = 0;
            decimal total_venda_dia = 0;
            decimal percentual_meta_dia = 0;
            decimal percentual_tendencia_objetivo = 0;
            bool bateu_meta = false;
            int qtdProdutos = 0;
            int qtdCaixas = 0;
            List<CSListViewItem> listAcumulado;
            List<CSListViewItem> listDia;
            List<CSListViewItem> listTendencia;

            //1 = acumulado 2 = dia 3 = tendência
            int tipoMeta = 0;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                SQLiteParameter pDAT_PEDIDO1 = null;
                SQLiteParameter pDAT_PEDIDO2 = null;
                SQLiteParameter pCOD_GRUPO1 = null;
                SQLiteParameter pCOD_GRUPO2 = null;
                SQLiteParameter pCOD_GRUPO3 = null;
                SQLiteParameter pCOD_GRUPO4 = null;
                SQLiteParameter pCOD_GRUPO5 = null;
                SQLiteParameter pCOD_GRUPO6 = null;
                SQLiteParameter pCOD_FAMILIA_PRODUTO1 = null;
                SQLiteParameter pCOD_FAMILIA_PRODUTO2 = null;
                SQLiteParameter pCOD_FAMILIA_PRODUTO3 = null;
                SQLiteParameter pCOD_FAMILIA_PRODUTO4 = null;
                SQLiteParameter pCOD_FAMILIA_PRODUTO5 = null;
                SQLiteParameter pCOD_FAMILIA_PRODUTO6 = null;
                SQLiteParameter pCOD_GRUPO_COMERCIALIZACAO1 = null;
                SQLiteParameter pCOD_GRUPO_COMERCIALIZACAO2 = null;
                SQLiteParameter pCOD_GRUPO_COMERCIALIZACAO3 = null;
                SQLiteParameter pCOD_GRUPO_COMERCIALIZACAO4 = null;
                SQLiteParameter pCOD_GRUPO_COMERCIALIZACAO5 = null;
                SQLiteParameter pCOD_GRUPO_COMERCIALIZACAO6 = null;

                int _familia = -1;
                int _grupoProduto = -1;
                int _grupoComercializacao = -1;

                try
                {
                    sqlQuery = new StringBuilder();

                    sqlQuery.Append("  SELECT PRODUTO.COD_PRODUTO ");
                    sqlQuery.Append("       , PRODUTO.DSC_PRODUTO ");
                    sqlQuery.Append("       , PRODUTO.COD_UNIDADE_MEDIDA ");
                    sqlQuery.Append("       , PRODUTO.QTD_UNIDADE_EMBALAGEM ");
                    sqlQuery.Append("       , PRODUTO.QTD_OBJETIVO   AS QTD_OBJETIVO ");
                    sqlQuery.Append("       , PRODUTO.QTD_ACUMULADO  AS QTD_ACUMULADO ");
                    sqlQuery.Append("       , PRODUTO.QTD_TENDENCIA  AS QTD_TENDENCIA ");
                    sqlQuery.Append("       , PRODUTO.QTD_META_DIA   AS QTD_META_DIA ");
                    sqlQuery.Append("       , PRODUTO.VAL_OBJETIVO   AS VAL_OBJETIVO ");
                    sqlQuery.Append("       , PRODUTO.VAL_ACUMULADO  AS VAL_ACUMULADO ");
                    sqlQuery.Append("       , PRODUTO.VAL_TENDENCIA  AS VAL_TENDENCIA ");
                    sqlQuery.Append("       , PRODUTO.VAL_META_DIA   AS VAL_META_DIA ");
                    sqlQuery.Append("       , 0                      AS QTD_VENDA_DIA ");
                    sqlQuery.Append("       , 0                      AS VAL_VENDA_DIA ");
                    sqlQuery.Append("    FROM PRODUTO ");
                    sqlQuery.Append("   WHERE (PRODUTO.QTD_OBJETIVO  IS NOT NULL OR  ");
                    sqlQuery.Append("          PRODUTO.QTD_ACUMULADO IS NOT NULL OR ");
                    sqlQuery.Append("          PRODUTO.QTD_TENDENCIA IS NOT NULL OR ");
                    sqlQuery.Append("          PRODUTO.QTD_META_DIA  IS NOT NULL) ");
                    sqlQuery.Append("     AND ((PRODUTO.COD_GRUPO = ? AND ? <> -1) OR ? = -1) ");
                    sqlQuery.Append("     AND ((PRODUTO.COD_FAMILIA_PRODUTO = ? AND ? <> -1) OR ? = -1) ");
                    sqlQuery.Append("     AND ((PRODUTO.COD_GRUPO_COMERCIALIZACAO = ? AND ? <> -1) OR ? = -1) ");
                    sqlQuery.Append("     AND PRODUTO.COD_PRODUTO NOT IN (SELECT ITEM_PEDIDO.COD_PRODUTO ");
                    sqlQuery.Append("                                       FROM PEDIDO INNER JOIN OPERACAO ");
                    sqlQuery.Append("                                                      ON PEDIDO.COD_OPERACAO  = OPERACAO.COD_OPERACAO ");
                    sqlQuery.Append("                                                     AND OPERACAO.COD_OPERACAO_CFO IN (1, 21) ");
                    sqlQuery.Append("                                                   INNER JOIN ITEM_PEDIDO ");
                    sqlQuery.Append("                                                      ON PEDIDO.COD_PEDIDO = ITEM_PEDIDO.COD_PEDIDO ");
                    sqlQuery.AppendFormat("                                      WHERE DATE(PEDIDO.DAT_PEDIDO) = DATE('?') ", DateTime.Now.ToString("yyyy-MM-dd"));
                    sqlQuery.Append("                                        AND PEDIDO.IND_HISTORICO = 0) ");
                    sqlQuery.Append("   UNION ALL ");
                    sqlQuery.Append("  SELECT PRODUTO.COD_PRODUTO ");
                    sqlQuery.Append("       , PRODUTO.DSC_PRODUTO ");
                    sqlQuery.Append("       , PRODUTO.COD_UNIDADE_MEDIDA ");
                    sqlQuery.Append("       , PRODUTO.QTD_UNIDADE_EMBALAGEM ");
                    sqlQuery.Append("       , MIN(PRODUTO.QTD_OBJETIVO)   AS QTD_OBJETIVO ");
                    sqlQuery.Append("       , MIN(PRODUTO.QTD_ACUMULADO)  AS QTD_ACUMULADO ");
                    sqlQuery.Append("       , MIN(PRODUTO.QTD_TENDENCIA)  AS QTD_TENDENCIA ");
                    sqlQuery.Append("       , MIN(PRODUTO.QTD_META_DIA)   AS QTD_META_DIA ");
                    sqlQuery.Append("       , MIN(PRODUTO.VAL_OBJETIVO)   AS VAL_OBJETIVO ");
                    sqlQuery.Append("       , MIN(PRODUTO.VAL_ACUMULADO)  AS VAL_ACUMULADO ");
                    sqlQuery.Append("       , MIN(PRODUTO.VAL_TENDENCIA)  AS VAL_TENDENCIA ");
                    sqlQuery.Append("       , MIN(PRODUTO.VAL_META_DIA)   AS VAL_META_DIA ");
                    sqlQuery.Append("       , SUM(ITEM_PEDIDO.QTD_PEDIDA) AS QTD_VENDA_DIA ");
                    sqlQuery.Append("       , SUM(ITEM_PEDIDO.VLR_TOTAL)  AS VAL_VENDA_DIA ");
                    sqlQuery.Append("    FROM PEDIDO INNER JOIN OPERACAO ");
                    sqlQuery.Append("                   ON PEDIDO.COD_OPERACAO  = OPERACAO.COD_OPERACAO ");
                    sqlQuery.Append("                  AND OPERACAO.COD_OPERACAO_CFO IN (1, 21) ");
                    sqlQuery.Append("                INNER JOIN ITEM_PEDIDO ");
                    sqlQuery.Append("                   ON PEDIDO.COD_PEDIDO = ITEM_PEDIDO.COD_PEDIDO ");
                    sqlQuery.Append("                INNER JOIN PRODUTO ");
                    sqlQuery.Append("                   ON PRODUTO.COD_PRODUTO  = ITEM_PEDIDO.COD_PRODUTO ");
                    sqlQuery.AppendFormat("   WHERE DATE(PEDIDO.DAT_PEDIDO) = DATE('{0}') ", DateTime.Now.ToString("yyyy-MM-dd"));
                    sqlQuery.Append("     AND PEDIDO.IND_HISTORICO = 0 ");
                    sqlQuery.Append("     AND (PRODUTO.QTD_OBJETIVO  IS NOT NULL OR  ");
                    sqlQuery.Append("          PRODUTO.QTD_ACUMULADO IS NOT NULL OR ");
                    sqlQuery.Append("          PRODUTO.QTD_TENDENCIA IS NOT NULL OR ");
                    sqlQuery.Append("          PRODUTO.QTD_META_DIA  IS NOT NULL) ");
                    sqlQuery.Append("     AND ((PRODUTO.COD_GRUPO = ? AND ? <> -1) OR ? = -1) ");
                    sqlQuery.Append("     AND ((PRODUTO.COD_FAMILIA_PRODUTO = ? AND ? <> -1) OR ? = -1) ");
                    sqlQuery.Append("     AND ((PRODUTO.COD_GRUPO_COMERCIALIZACAO = ? AND ? <> -1) OR ? = -1) ");
                    sqlQuery.Append("GROUP BY PRODUTO.COD_PRODUTO ");
                    sqlQuery.Append("       , PRODUTO.DSC_PRODUTO ");
                    sqlQuery.Append("       , PRODUTO.COD_UNIDADE_MEDIDA ");
                    sqlQuery.Append("       , PRODUTO.QTD_UNIDADE_EMBALAGEM ");
                    sqlQuery.Append("ORDER BY PRODUTO.DSC_PRODUTO ");

                    if (cmbGrupoComercializacao.Adapter != null)
                    {
                        // Busca o grupo de comercializacao selecionado
                        CSGruposComercializacao.CSGrupoComercializacao grupoComercializacao =
                            (CSGruposComercializacao.CSGrupoComercializacao)((CSItemCombo)cmbGrupoComercializacao.SelectedItem).Valor;
                        _grupoComercializacao = grupoComercializacao.COD_GRUPO_COMERCIALIZACAO;
                    }

                    // Busca a familia selecionada
                    if (cmbFamiliaProduto.Adapter != null)
                    {
                        CSFamiliasProduto.CSFamiliaProduto familia = (CSFamiliasProduto.CSFamiliaProduto)((CSItemCombo)cmbFamiliaProduto.SelectedItem).Valor;
                        _familia = familia.COD_FAMILIA_PRODUTO;
                    }

                    // Busca o Grupo selecionado
                    if (cmbGrupoProduto.Adapter != null)
                    {
                        CSGruposProduto.CSGrupoProduto grupoProduto = (CSGruposProduto.CSGrupoProduto)((CSItemCombo)cmbGrupoProduto.SelectedItem).Valor;
                        _grupoProduto = grupoProduto.COD_GRUPO;
                    }

                    pDAT_PEDIDO1 = new SQLiteParameter("@DAT_PEDIDO1", DateTime.Now.ToString("yyyy-MM-dd"));
                    pDAT_PEDIDO2 = new SQLiteParameter("@DAT_PEDIDO2", DateTime.Now.ToString("yyyy-MM-dd"));
                    pCOD_GRUPO1 = new SQLiteParameter("@COD_GRUPO1", _grupoProduto);
                    pCOD_GRUPO2 = new SQLiteParameter("@COD_GRUPO2", _grupoProduto);
                    pCOD_GRUPO3 = new SQLiteParameter("@COD_GRUPO3", _grupoProduto);
                    pCOD_GRUPO4 = new SQLiteParameter("@COD_GRUPO4", _grupoProduto);
                    pCOD_GRUPO5 = new SQLiteParameter("@COD_GRUPO5", _grupoProduto);
                    pCOD_GRUPO6 = new SQLiteParameter("@COD_GRUPO6", _grupoProduto);
                    pCOD_FAMILIA_PRODUTO1 = new SQLiteParameter("@COD_FAMILIA_PRODUTO1", _familia);
                    pCOD_FAMILIA_PRODUTO2 = new SQLiteParameter("@COD_FAMILIA_PRODUTO2", _familia);
                    pCOD_FAMILIA_PRODUTO3 = new SQLiteParameter("@COD_FAMILIA_PRODUTO3", _familia);
                    pCOD_FAMILIA_PRODUTO4 = new SQLiteParameter("@COD_FAMILIA_PRODUTO4", _familia);
                    pCOD_FAMILIA_PRODUTO5 = new SQLiteParameter("@COD_FAMILIA_PRODUTO5", _familia);
                    pCOD_FAMILIA_PRODUTO6 = new SQLiteParameter("@COD_FAMILIA_PRODUTO6", _familia);
                    pCOD_GRUPO_COMERCIALIZACAO1 = new SQLiteParameter("@COD_GRUPO_COMERCIALIZACAO1", _grupoComercializacao);
                    pCOD_GRUPO_COMERCIALIZACAO2 = new SQLiteParameter("@COD_GRUPO_COMERCIALIZACAO2", _grupoComercializacao);
                    pCOD_GRUPO_COMERCIALIZACAO3 = new SQLiteParameter("@COD_GRUPO_COMERCIALIZACAO3", _grupoComercializacao);
                    pCOD_GRUPO_COMERCIALIZACAO4 = new SQLiteParameter("@COD_GRUPO_COMERCIALIZACAO4", _grupoComercializacao);
                    pCOD_GRUPO_COMERCIALIZACAO5 = new SQLiteParameter("@COD_GRUPO_COMERCIALIZACAO5", _grupoComercializacao);
                    pCOD_GRUPO_COMERCIALIZACAO6 = new SQLiteParameter("@COD_GRUPO_COMERCIALIZACAO6", _grupoComercializacao);

                    listAcumulado = new List<CSListViewItem>();
                    listDia = new List<CSListViewItem>();
                    listTendencia = new List<CSListViewItem>();

                    RadioButton rdb = ActivityContext.FindViewById<RadioButton>(rdgRadio.CheckedRadioButtonId);

                    using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), pCOD_GRUPO1, pCOD_GRUPO2, pCOD_GRUPO3, pCOD_FAMILIA_PRODUTO1, pCOD_FAMILIA_PRODUTO2, pCOD_FAMILIA_PRODUTO3, pCOD_GRUPO_COMERCIALIZACAO1, pCOD_GRUPO_COMERCIALIZACAO2, pCOD_GRUPO_COMERCIALIZACAO3, pCOD_GRUPO4, pCOD_GRUPO5, pCOD_GRUPO6, pCOD_FAMILIA_PRODUTO4, pCOD_FAMILIA_PRODUTO5, pCOD_FAMILIA_PRODUTO6, pCOD_GRUPO_COMERCIALIZACAO4, pCOD_GRUPO_COMERCIALIZACAO5, pCOD_GRUPO_COMERCIALIZACAO6))
                    {
                        while (sqlReader.Read())
                        {
                            qtdProdutos++;

                            CSListViewItem lviAcumulado = new CSListViewItem();
                            CSListViewItem lviDia = new CSListViewItem();
                            CSListViewItem lviTendencia = new CSListViewItem();

                            codigo_produto = sqlReader.GetValue(0) == System.DBNull.Value ? -1 : sqlReader.GetInt32(0);
                            produto = sqlReader.GetValue(1) == System.DBNull.Value ? "" : sqlReader.GetString(1);
                            unidadeMedida = sqlReader.GetValue(2) == System.DBNull.Value ? "" : sqlReader.GetString(2).ToUpper();
                            quantidadeUnidadeMedida = sqlReader.GetValue(3) == System.DBNull.Value ? -1 : sqlReader.GetInt32(3);

                            if (chkMostraValor.Checked)
                            {
                                qtd_objetivo = sqlReader.GetValue(4) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(4));
                                qtd_tendencia = sqlReader.GetValue(6) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(6));

                                objetivo = sqlReader.GetValue(8) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(8));
                                acumulado = sqlReader.GetValue(9) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(9));
                                tendencia = sqlReader.GetValue(10) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(10));
                                metadia = sqlReader.GetValue(11) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(11));
                                venda_dia = sqlReader.GetValue(13) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(13));

                                // Total geral 
                                total_objetivo += objetivo;
                                total_acumulado += acumulado;
                                total_tendencia += tendencia;
                                total_metadia += metadia;
                                total_venda_dia += venda_dia;
                            }
                            else
                            {
                                objetivo = sqlReader.GetValue(4) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(4));
                                acumulado = sqlReader.GetValue(5) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(5));
                                tendencia = sqlReader.GetValue(6) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(6));
                                metadia = sqlReader.GetValue(7) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(7));
                                venda_dia = sqlReader.GetValue(12) == System.DBNull.Value ? 0 : Convert.ToDecimal(sqlReader.GetValue(12));

                                qtd_objetivo = objetivo;
                                qtd_tendencia = tendencia;
                            }

                            bateu_meta = false;

                            if (qtd_tendencia >= qtd_objetivo && chkMeta.Checked == true)
                            {
                                bateu_meta = true;

                                // Total geral 
                                total_objetivo -= objetivo;
                                total_acumulado -= acumulado;
                                total_tendencia -= tendencia;
                                total_metadia -= metadia;
                                total_venda_dia -= venda_dia;
                            }

                            if (unidadeMedida == "CX")
                                unidadeMedidaMostra = unidadeMedida + "/" + quantidadeUnidadeMedida.ToString().Trim();
                            else
                                unidadeMedidaMostra = unidadeMedida;

                            switch (rdb.Text)
                            {
                                case "Acumulado":
                                    {
                                        lviAcumulado.Text = codigo_produto.ToString().Trim() + " - " + produto;
                                        lviAcumulado.SubItems = new List<object>();
                                        lviAcumulado.SubItems.Add(unidadeMedidaMostra);

                                        if (chkMostraValor.Checked)
                                        {
                                            lviAcumulado.SubItems.Add(objetivo.ToString(CSGlobal.DecimalStringFormat));
                                            lviAcumulado.SubItems.Add(acumulado.ToString(CSGlobal.DecimalStringFormat));
                                        }
                                        else
                                        {
                                            lviAcumulado.SubItems.Add(CSProdutos.CSProduto.ConverteUnidadesParaMedida(objetivo, unidadeMedida, quantidadeUnidadeMedida));
                                            lviAcumulado.SubItems.Add(CSProdutos.CSProduto.ConverteUnidadesParaMedida(acumulado, unidadeMedida, quantidadeUnidadeMedida));

                                            qtdCaixas += CSProdutos.CSProduto.ConverterParaQuantidadeCaixaVendida(objetivo, unidadeMedida, quantidadeUnidadeMedida);
                                        }

                                        if (!bateu_meta)
                                            listAcumulado.Add(lviAcumulado);
                                    }
                                    break;
                                case "Dia":
                                    {
                                        lviDia.Text = codigo_produto.ToString().Trim() + " - " + produto;
                                        lviDia.SubItems = new List<object>();
                                        lviDia.SubItems.Add(unidadeMedidaMostra);

                                        if (chkMostraValor.Checked)
                                        {
                                            lviDia.SubItems.Add(metadia.ToString(CSGlobal.DecimalStringFormat));
                                            lviDia.SubItems.Add(venda_dia.ToString(CSGlobal.DecimalStringFormat));
                                        }
                                        else
                                        {
                                            lviDia.SubItems.Add(CSProdutos.CSProduto.ConverteUnidadesParaMedida(metadia, unidadeMedida, quantidadeUnidadeMedida));
                                            lviDia.SubItems.Add(CSProdutos.CSProduto.ConverteUnidadesParaMedida(venda_dia, unidadeMedida, quantidadeUnidadeMedida));

                                            qtdCaixas += CSProdutos.CSProduto.ConverterParaQuantidadeCaixaVendida(metadia, unidadeMedida, quantidadeUnidadeMedida);
                                        }

                                        percentual_meta_dia = 0;
                                        if (metadia > 0)
                                            percentual_meta_dia = (venda_dia / metadia) * Convert.ToDecimal(100.0);

                                        lviDia.SubItems.Add(percentual_meta_dia.ToString(CSGlobal.DecimalStringFormat));

                                        if (!bateu_meta)
                                            listDia.Add(lviDia);
                                    }
                                    break;
                                case "Tendência":
                                    {
                                        lviTendencia.Text = codigo_produto.ToString().Trim() + " - " + produto;
                                        lviTendencia.SubItems = new List<object>();
                                        lviTendencia.SubItems.Add(unidadeMedidaMostra);

                                        percentual_tendencia_objetivo = 0;
                                        if (objetivo > 0)
                                            percentual_tendencia_objetivo = (tendencia / objetivo) * Convert.ToDecimal(100.0);

                                        if (chkMostraValor.Checked)
                                        {
                                            lviTendencia.SubItems.Add(tendencia.ToString(CSGlobal.DecimalStringFormat));
                                            lviTendencia.SubItems.Add(objetivo.ToString(CSGlobal.DecimalStringFormat));
                                        }
                                        else
                                        {
                                            lviTendencia.SubItems.Add(CSProdutos.CSProduto.ConverteUnidadesParaMedida(tendencia, unidadeMedida, quantidadeUnidadeMedida));
                                            lviTendencia.SubItems.Add(CSProdutos.CSProduto.ConverteUnidadesParaMedida(objetivo, unidadeMedida, quantidadeUnidadeMedida));

                                            qtdCaixas += CSProdutos.CSProduto.ConverterParaQuantidadeCaixaVendida(objetivo, unidadeMedida, quantidadeUnidadeMedida);
                                        }

                                        lviTendencia.SubItems.Add(percentual_tendencia_objetivo.ToString(CSGlobal.DecimalStringFormat));

                                        if (!bateu_meta)
                                            listTendencia.Add(lviTendencia);
                                    }
                                    break;
                            }
                        }

                        switch (rdb.Text)
                        {
                            case "Acumulado":
                                {
                                    tipoMeta = 1;
                                }
                                break;
                            case "Dia":
                                {
                                    tipoMeta = 2;
                                }
                                break;
                            case "Tendência":
                                {
                                    tipoMeta = 3;
                                }
                                break;
                        }
                    }
                }
                catch (Exception er)
                {
                    MessageBox.AlertErro(ActivityContext,er.Message);
                }

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                switch (tipoMeta)
                {
                    case 1:
                        {
                            if (chkMostraValor.Checked)
                            {
                                llBottom.Visibility = ViewStates.Visible;
                                llBottomQTD.Visibility = ViewStates.Gone;

                                lblPrimeiraLabel.Text = total_objetivo.ToString(CSGlobal.DecimalStringFormat);
                                lblSegundaLabel.Text = total_acumulado.ToString(CSGlobal.DecimalStringFormat);
                                lblTerceiraLabel.Visibility = ViewStates.Gone;
                            }
                            else
                            {
                                llBottom.Visibility = ViewStates.Gone;
                                llBottomQTD.Visibility = ViewStates.Visible;
                            }

                            lvwMetasVendas.Adapter = new ListaMetasVendasAcumulado(ActivityContext, Resource.Layout.acompanhamento_vendas_header_acumulado_row, listAcumulado);
                        }
                        break;
                    case 2:
                        {
                            if (chkMostraValor.Checked)
                            {
                                llBottom.Visibility = ViewStates.Visible;
                                llBottomQTD.Visibility = ViewStates.Gone;

                                lblPrimeiraLabel.Text = total_metadia.ToString(CSGlobal.DecimalStringFormat);
                                lblSegundaLabel.Text = total_venda_dia.ToString(CSGlobal.DecimalStringFormat);
                                lblTerceiraLabel.Visibility = ViewStates.Visible;
                                lblTerceiraLabel.Text = percentual_meta_dia.ToString(CSGlobal.DecimalStringFormat);
                            }
                            else
                            {
                                llBottom.Visibility = ViewStates.Gone;
                                llBottomQTD.Visibility = ViewStates.Visible;
                            }

                            lvwMetasVendas.Adapter = new ListaMetasVendasDia(ActivityContext, Resource.Layout.acompanhamento_vendas_header_dia_row, listDia);
                        }
                        break;
                    case 3:
                        {
                            if (chkMostraValor.Checked)
                            {
                                llBottom.Visibility = ViewStates.Visible;
                                llBottomQTD.Visibility = ViewStates.Gone;

                                lblPrimeiraLabel.Text = total_tendencia.ToString(CSGlobal.DecimalStringFormat);
                                lblSegundaLabel.Text = total_objetivo.ToString(CSGlobal.DecimalStringFormat);
                                lblTerceiraLabel.Visibility = ViewStates.Visible;
                                lblTerceiraLabel.Text = percentual_tendencia_objetivo.ToString(CSGlobal.DecimalStringFormat);
                            }
                            else
                            {
                                llBottom.Visibility = ViewStates.Gone;
                                llBottomQTD.Visibility = ViewStates.Visible;
                            }

                            lvwMetasVendas.Adapter = new ListaMetasVendasTendencia(ActivityContext, Resource.Layout.acompanhamento_vendas_header_tendencia_row, listTendencia);
                        }
                        break;
                }

                lblTotalItensQtd.Text = qtdProdutos.ToString();
                lblQtdCaixa.Text = qtdCaixas.ToString();

                if (progress != null)
                    progress.Dismiss();
            }
        }

        class ListaMetasVendasTendencia : ArrayAdapter<CSListViewItem>
        {
            Activity act;
            IList<CSListViewItem> tendencia;
            int resourceId;

            public ListaMetasVendasTendencia(Activity c, int textViewResourceId, IList<CSListViewItem> objects)
                : base(c, textViewResourceId, objects)
            {
                act = c;
                tendencia = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSListViewItem item = tendencia[position];

                LayoutInflater layout = (LayoutInflater)act.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(resourceId, null);

                try
                {
                    TextView tvCodigoNomeProduto = linha.FindViewById<TextView>(Resource.Id.tvCodigoNomeProduto);
                    TextView tvUnidadeMedida = linha.FindViewById<TextView>(Resource.Id.tvUnidadeMedida);
                    TextView tvTendencia = linha.FindViewById<TextView>(Resource.Id.tvTendencia);
                    TextView tvObjetivo = linha.FindViewById<TextView>(Resource.Id.tvObjetivo);
                    TextView tvPorcentagemTendencia = linha.FindViewById<TextView>(Resource.Id.tvPorcentagemTendencia);

                    tvCodigoNomeProduto.Text = item.Text;
                    tvUnidadeMedida.Text = item.SubItems[0].ToString();
                    tvTendencia.Text = item.SubItems[1].ToString();
                    tvObjetivo.Text = item.SubItems[2].ToString();
                    tvPorcentagemTendencia.Text = item.SubItems[3].ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.AlertErro(act, ex.Message);
                }

                return linha;
            }
        }

        class ListaMetasVendasDia : ArrayAdapter<CSListViewItem>
        {
            Activity act;
            IList<CSListViewItem> dia;
            int resourceId;

            public ListaMetasVendasDia(Activity c, int textViewResourceId, IList<CSListViewItem> objects)
                : base(c, textViewResourceId, objects)
            {
                act = c;
                dia = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSListViewItem item = dia[position];

                LayoutInflater layout = (LayoutInflater)act.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(resourceId, null);

                try
                {
                    TextView tvCodigoNomeProduto = linha.FindViewById<TextView>(Resource.Id.tvCodigoNomeProduto);
                    TextView tvUnidadeMedida = linha.FindViewById<TextView>(Resource.Id.tvUnidadeMedida);
                    TextView tvMetaDia = linha.FindViewById<TextView>(Resource.Id.tvMetaDia);
                    TextView tvVendaDia = linha.FindViewById<TextView>(Resource.Id.tvVendaDia);
                    TextView tvPorcentagemMetaVenda = linha.FindViewById<TextView>(Resource.Id.tvPorcentagemMetaVenda);

                    tvCodigoNomeProduto.Text = item.Text;
                    tvUnidadeMedida.Text = item.SubItems[0].ToString();
                    tvMetaDia.Text = item.SubItems[1].ToString();
                    tvVendaDia.Text = item.SubItems[2].ToString();
                    tvPorcentagemMetaVenda.Text = item.SubItems[3].ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.AlertErro(act, ex.Message);
                }

                return linha;
            }
        }

        class ListaMetasVendasAcumulado : ArrayAdapter<CSListViewItem>
        {
            Activity act;
            IList<CSListViewItem> acumulado;
            int resourceId;

            public ListaMetasVendasAcumulado(Activity c, int textViewResourceId, IList<CSListViewItem> objects)
                : base(c, textViewResourceId, objects)
            {
                act = c;
                acumulado = objects;
                resourceId = textViewResourceId;
            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                CSListViewItem item = acumulado[position];

                LayoutInflater layout = (LayoutInflater)act.GetSystemService(Context.LayoutInflaterService);
                View linha = layout.Inflate(resourceId, null);

                try
                {
                    TextView tvCodigoNomeProduto = linha.FindViewById<TextView>(Resource.Id.tvCodigoNomeProduto);
                    TextView tvUnidadeMedida = linha.FindViewById<TextView>(Resource.Id.tvUnidadeMedida);
                    TextView tvObjetivo = linha.FindViewById<TextView>(Resource.Id.tvObjetivo);
                    TextView tvAcumulado = linha.FindViewById<TextView>(Resource.Id.tvAcumulado);

                    tvCodigoNomeProduto.Text = item.Text;
                    tvUnidadeMedida.Text = item.SubItems[0].ToString();
                    tvObjetivo.Text = item.SubItems[1].ToString();
                    tvAcumulado.Text = item.SubItems[2].ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.AlertErro(act, ex.Message);
                }

                return linha;
            }
        }

        private class ThreadCarregaComboBoxGrupoComercializacao : AsyncTask
        {
            ArrayAdapter adapter;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                CarregaComboBoxGrupoComercializacao();

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                cmbGrupoComercializacao.Adapter = null;

                cmbGrupoComercializacao.Adapter = adapter;

                cmbGrupoComercializacao.SetSelection(cmbGrupoComercializacao.Adapter.Count - 1);

                new ThreadCarregaComboBoxGrupoProduto().Execute();
            }

            private void CarregaComboBoxGrupoComercializacao()
            {
                adapter = new ArrayAdapter(ActivityContext, Android.Resource.Layout.SimpleSpinnerItem);
                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                // Preenche o combo
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
            }
        }

        private class ThreadCarregaComboBoxGrupoProduto : AsyncTask
        {
            ArrayAdapter adapter;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                CarregaComboBoxGrupoProduto();

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                cmbGrupoProduto.Adapter = null;

                cmbGrupoProduto.Adapter = adapter;

                cmbGrupoProduto.SetSelection(cmbGrupoProduto.Adapter.Count - 1);

                new ThreadCarregarComboBoxFamiliaProduto().Execute();
            }

            private void CarregaComboBoxGrupoProduto()
            {
                adapter = new ArrayAdapter(ActivityContext, Android.Resource.Layout.SimpleSpinnerItem);
                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                // Preenche o combo de grupos
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
            }
        }
    }
}