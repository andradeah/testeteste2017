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
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;

namespace AvanteSales.Pro.Fragments
{
    public class ResumoPesoPedidoGrafico : Android.Support.V4.App.Fragment
    {
        Android.Support.V4.App.FragmentActivity ActivityContext;
        Relatorio relatorio;
        public static int height;
        public static int width;

        public static TextView lblPesoBruto;
        public static TextView lblPctOcupacao;
        private TextView lblVeiculo;
        public static TextView lblOcupacaoVeiculo;
        public static TextView lblPctOcupacaoVeiculo;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.resumo_peso_pedido_grafico, container, false);
            FindViewsById(view);
            relatorio = (Relatorio)Activity;
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
            DisplayMetrics displaymetrics = new DisplayMetrics();
            ActivityContext.WindowManager.DefaultDisplay.GetMetrics(displaymetrics);
            int screenWidth = displaymetrics.WidthPixels;
            int screenHeight = displaymetrics.HeightPixels;

            PreencherInformacoes();
        }

        private void FindViewsById(View view)
        {
            lblPesoBruto = view.FindViewById<TextView>(Resource.Id.lblPesoBruto);
            lblPctOcupacao = view.FindViewById<TextView>(Resource.Id.lblPctOcupacao);
            lblVeiculo = view.FindViewById<TextView>(Resource.Id.lblVeiculo);
            lblOcupacaoVeiculo = view.FindViewById<TextView>(Resource.Id.lblOcupacaoVeiculo);
            lblPctOcupacaoVeiculo = view.FindViewById<TextView>(Resource.Id.lblPctOcupacaoVeiculo);
        }

        private void PreencherInformacoes()
        {
            StringBuilder sql = new StringBuilder();

            //Carrega Quantidade de Itens vendidos, Valor total dos pedidos, Peso total dos pedidos e Volume em caixas

            sql.Length = 0;
            sql.Append("SELECT ");
            sql.Append("SUM(([PRODUTO].[VLR_PESO_PRODUTO] * [ITEM_PEDIDO].[QTD_PEDIDA]) / [PRODUTO].[QTD_UNIDADE_EMBALAGEM]) AS 'PESO BRUTO' ");
            sql.Append("FROM [PEDIDO] ");
            sql.Append("JOIN [ITEM_PEDIDO] ON [PEDIDO].[COD_PEDIDO] = [ITEM_PEDIDO].[COD_PEDIDO] ");
            sql.Append("JOIN [PRODUTO] ON [PRODUTO].[COD_PRODUTO] = [ITEM_PEDIDO].[COD_PRODUTO] ");
            sql.Append("WHERE DATE([PEDIDO].[DAT_PEDIDO]) = DATE('NOW') ");

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
            {
                while (sqlReader.Read())
                {
                    lblPesoBruto.Text = sqlReader.GetValue(0) == System.DBNull.Value ? "0" : Math.Round(Convert.ToDecimal(sqlReader.GetValue(0)), 2).ToString();

                    break;
                }
            }

            sql.Length = 0;
            sql.Append("SELECT DES_MODELO_VEICULO, ");
            sql.Append("VLR_CAPACIDADE_PESO_LIMITE, ");
            sql.Append("(VLR_CAPACIDADE_PESO_LIMITE - ?) AS DIFERENCA ");
            sql.Append(" FROM [MODELO_VEICULO] WHERE DIFERENCA > 0 ORDER BY DIFERENCA LIMIT 1 ");

            SQLiteParameter pPESO_BRUTO = new SQLiteParameter("@pPESO_BRUTO", lblPesoBruto.Text);

            using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString(), pPESO_BRUTO))
            {
                if (sqlReader.Read())
                {
                    lblVeiculo.Text = sqlReader.GetString(0);
                    lblOcupacaoVeiculo.Text = sqlReader.GetValue(1).ToString();
                }
                else
                {
                    lblVeiculo.Text = "Nenhum veículo apropriado";
                    lblOcupacaoVeiculo.Text = "-";
                }
            }

            lblPctOcupacao.Text = CSEmpresa.Current.PCT_TAXA_OCUPACAO_IDEAL_VEICULO_MODELO.ToString();
        }
    }
}