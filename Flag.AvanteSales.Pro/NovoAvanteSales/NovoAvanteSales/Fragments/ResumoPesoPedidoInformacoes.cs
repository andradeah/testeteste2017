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
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;

namespace AvanteSales.Pro.Fragments
{
    public class ResumoPesoPedidoInformacoes : Android.Support.V4.App.Fragment
    {
        Relatorio relatorio;
        private static TextView lblNumeroProdutos;
        private static TextView lblVolume;
        private static TextView lblValorTotal;
        private static TextView lblPesoForaRota;
        private static TextView lblPesoNaRota;
        private static TextView lblNumeroPedidos;
        static ProgressDialog progress;
        Android.Support.V4.App.FragmentActivity ActivityContext;
        LayoutInflater thisLayoutInflater;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.resumo_peso_pedido_informacoes, container, false);
            thisLayoutInflater = inflater;
            ActivityContext = Activity;
            FindViewsById(view);
            relatorio = (Relatorio)Activity;
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

            new ThreadInformacoes().Execute();
        }

        private class ThreadInformacoes : AsyncTask<int, int, decimal>
        {
            public string NumeroPedidos;
            public string NumeroProdutos;
            public string ValorTotal;
            public string Volume;
            public string PesoForaRota;
            public string PesoNaRota;

            protected override decimal RunInBackground(params int[] @params)
            {
                PreencherInformacoes();

                return 0;
            }

            protected override void OnPostExecute(decimal result)
            {
                lblNumeroProdutos.Text = NumeroProdutos;
                lblVolume.Text = Volume;
                lblValorTotal.Text = ValorTotal;
                lblPesoForaRota.Text = PesoForaRota;
                lblPesoNaRota.Text = PesoNaRota;
                lblNumeroPedidos.Text = NumeroPedidos;

                progress.Dismiss();
                base.OnPostExecute(result);
            }

            private void PreencherInformacoes()
            {
                StringBuilder sql = new StringBuilder();

                //Carrega Quantidade de Itens vendidos, Valor total dos pedidos, Peso total dos pedidos e Volume em caixas

                sql.Length = 0;
                sql.Append("SELECT (SELECT COUNT(*) FROM [PEDIDO] WHERE DATE([PEDIDO].DAT_PEDIDO) = DATE('NOW')) AS 'NUM PEDIDOS', ");
                sql.Append("(SELECT COUNT(DISTINCT COD_PRODUTO) FROM [ITEM_PEDIDO] JOIN [PEDIDO] ON [ITEM_PEDIDO].COD_PEDIDO = [PEDIDO].COD_PEDIDO WHERE DATE([PEDIDO].DAT_PEDIDO) = DATE('NOW')) AS 'SKU', ");
                sql.Append("(SELECT SUM([VLR_TOTAL_PEDIDO]) FROM [PEDIDO] WHERE DATE(DAT_PEDIDO) = DATE('NOW')) AS 'VALOR TOTAL', ");
                sql.Append("SUM([ITEM_PEDIDO].[QTD_PEDIDA] / [PRODUTO].[QTD_UNIDADE_EMBALAGEM]) AS 'VOLUME' ");
                sql.Append("FROM [PEDIDO] ");
                sql.Append("JOIN [ITEM_PEDIDO] ON [PEDIDO].[COD_PEDIDO] = [ITEM_PEDIDO].[COD_PEDIDO] ");
                sql.Append("JOIN [PRODUTO] ON [PRODUTO].[COD_PRODUTO] = [ITEM_PEDIDO].[COD_PRODUTO] ");
                sql.Append("WHERE DATE([PEDIDO].[DAT_PEDIDO]) = DATE('NOW') ");

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
                {
                    while (sqlReader.Read())
                    {
                        NumeroPedidos = sqlReader.GetValue(0) == System.DBNull.Value ? "0" : sqlReader.GetInt32(0).ToString();
                        NumeroProdutos = sqlReader.GetValue(1).ToString();
                        ValorTotal = sqlReader.GetValue(2) == System.DBNull.Value ? "0" : Math.Round(Convert.ToDecimal(sqlReader.GetValue(2)), 2).ToString();
                        Volume = sqlReader.GetValue(3) == System.DBNull.Value ? "0" : sqlReader.GetValue(3).ToString();

                        break;
                    }
                }

                //Carrega o Peso total dos pedidos feitos fora de rota

                sql.Length = 0;
                sql.Append("SELECT SUM(([PRODUTO].[VLR_PESO_PRODUTO] * [ITEM_PEDIDO].[QTD_PEDIDA]) / [PRODUTO].[QTD_UNIDADE_EMBALAGEM]) AS 'PESO TOTAL' ");
                sql.Append("FROM [PEDIDO] ");
                sql.Append("JOIN [ITEM_PEDIDO] ON [PEDIDO].[COD_PEDIDO] = [ITEM_PEDIDO].[COD_PEDIDO] ");
                sql.Append("JOIN [PRODUTO] ON [PRODUTO].[COD_PRODUTO] = [ITEM_PEDIDO].[COD_PRODUTO] ");
                sql.Append("WHERE DATE([PEDIDO].[DAT_PEDIDO]) = DATE('NOW') ");
                sql.Append("AND [PEDIDO].[COD_PDV] IN (SELECT DISTINCT T1.COD_PDV ");
                sql.Append("FROM PDV T1 ");
                sql.Append("JOIN CADASTRO_DIA_VISITA T2 ON T2.COD_PDV = T1.COD_PDV ");
                sql.Append("JOIN PEDIDO T3 ON T3.COD_PDV = T1.COD_PDV AND T3.IND_HISTORICO = 0 ");
                sql.Append("JOIN OPERACAO T6 ON T6.COD_OPERACAO = T3.COD_OPERACAO ");
                sql.Append("AND T6.COD_OPERACAO_CFO IN (1, 21) ");
                sql.Append("INNER JOIN PDV_GRUPO_COMERCIALIZACAO T4 ON T1.COD_PDV = T4.COD_PDV ");
                sql.Append("JOIN DAT_REFERENCIA_CICLO_VISITA T5 ON (T4.DSC_CICLO_VISITA NOT LIKE '%' || T5.COD_CICLO || '%' ");
                sql.Append("AND DATE('NOW') BETWEEN DATE(T5.DAT_INICIO_CICLO) AND DATE(T5.DAT_FINAL_CICLO) ");
                sql.Append("OR T2.COD_DIA_VISITA != ?) ");
                sql.Append("AND DATE(T3.DAT_PEDIDO) = DATE('NOW')) ");

                SQLiteParameter pCOD_VISITA = new SQLiteParameter("@COD_VISITA", DateTime.Now.DayOfWeek);

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString(), pCOD_VISITA))
                {
                    while (sqlReader.Read())
                    {
                        PesoForaRota = sqlReader.GetValue(0) == System.DBNull.Value ? "0" : Math.Round(Convert.ToDecimal(sqlReader.GetValue(0)), 2).ToString();

                        break;
                    }
                }

                //Carrega o Peso total dos pedidos feitos dentro da rota

                sql.Replace("OR T2.COD_DIA_VISITA != ?) ", "OR T2.COD_DIA_VISITA == ?) ");

                using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString(), pCOD_VISITA))
                {
                    while (sqlReader.Read())
                    {
                        PesoNaRota = sqlReader.GetValue(0) == System.DBNull.Value ? "0" : Math.Round(Convert.ToDecimal(sqlReader.GetValue(0)), 2).ToString();

                        break;
                    }
                }
            }
        }

        private void FindViewsById(View view)
        {
            lblNumeroProdutos = view.FindViewById<TextView>(Resource.Id.lblNumeroProdutos);
            lblVolume = view.FindViewById<TextView>(Resource.Id.lblVolume);
            lblValorTotal = view.FindViewById<TextView>(Resource.Id.lblValorTotal);
            lblPesoForaRota = view.FindViewById<TextView>(Resource.Id.lblPesoForaRota);
            lblPesoNaRota = view.FindViewById<TextView>(Resource.Id.lblPesoNaRota);
            lblNumeroPedidos = view.FindViewById<TextView>(Resource.Id.lblNumeroPedidos);
        }
    }
}