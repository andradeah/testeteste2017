using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;
using Android.Widget;
using AvanteSales.Pro.Activities;
using AvanteSales.Pro.Formatters;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;

namespace AvanteSales.Pro.Fragments
{
    public class ResumoPesoPedido : Android.Support.V4.App.Fragment
    {
        Android.Support.Design.Widget.TabLayout tblTab;
        Android.Support.V4.View.ViewPager vwptblTab;
        Android.Support.V4.App.FragmentActivity ActivityContext;
        Relatorio relatorio;
        LayoutInflater thisLayoutInflater;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.resumo_peso_pedido, container, false);
            FindViewsById(view);
            relatorio = (Relatorio)Activity;
            thisLayoutInflater = inflater;
            ActivityContext = Activity;
            return view;
        }

        private void FindViewsById(View view)
        {
            tblTab = view.FindViewById<Android.Support.Design.Widget.TabLayout>(Resource.Id.tblTab);
            vwptblTab = view.FindViewById<Android.Support.V4.View.ViewPager>(Resource.Id.vwpTab);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            Inicializacao();
        }

        private void Inicializacao()
        {
            vwptblTab.Adapter = new CustomAdapter(ChildFragmentManager, relatorio.ApplicationContext);
            tblTab.SetOnTabSelectedListener(new Listener(vwptblTab));
            tblTab.SetupWithViewPager(vwptblTab);
        }

        public class Listener : Android.Support.Design.Widget.TabLayout.IOnTabSelectedListener
        {
            ViewPager ViewPager;

            public Listener(ViewPager viewPager)
            {
                ViewPager = viewPager;
            }

            public IntPtr Handle
            {
                get
                {
                    return IntPtr.Zero;
                }
            }

            public void Dispose()
            {

            }

            public void OnTabReselected(TabLayout.Tab tab)
            {
                ViewPager.SetCurrentItem(tab.Position, true);
            }

            public void OnTabSelected(TabLayout.Tab tab)
            {
                ViewPager.SetCurrentItem(tab.Position, true);
            }

            public void OnTabUnselected(TabLayout.Tab tab)
            {
                ViewPager.SetCurrentItem(tab.Position, true);
            }
        }

        private class CustomAdapter : FragmentPagerAdapter
        {
            private Context applicationContext;
            private Android.Support.V4.App.FragmentManager supportFragmentManager;
            string[] fragments = { "Análise gráfica", "Informações" };

            public CustomAdapter(Android.Support.V4.App.FragmentManager fm, Context context) : base(fm)
            {
                applicationContext = context;
                supportFragmentManager = fm;
            }

            public override int Count
            {
                get
                {
                    return fragments.Length;
                }
            }

            public override Android.Support.V4.App.Fragment GetItem(int position)
            {
                switch (position)
                {
                    case 0:
                        return new ResumoPesoPedidoGrafico();
                    case 1:
                        return new ResumoPesoPedidoInformacoes();
                    default:
                        return null;
                }
            }

            public override Java.Lang.ICharSequence GetPageTitleFormatted(int position)
            {
                return CharSequence.ArrayFromStringArray(fragments)[position];
            }
        }

        //private class ThreadInformacoes : AsyncTask<int, int, decimal>
        //{
        //    protected override decimal RunInBackground(params int[] @params)
        //    {
        //        PreencherInformacoes();

        //        return 0;
        //    }

        //    protected override void OnPostExecute(decimal result)
        //    {
        //        progress.Dismiss();
        //        base.OnPostExecute(result);
        //    }

        //    private void PreencherInformacoes()
        //    {
        //        StringBuilder sql = new StringBuilder();

        //        //Carrega Quantidade de Itens vendidos, Valor total dos pedidos, Peso total dos pedidos e Volume em caixas

        //        sql.Length = 0;
        //        sql.Append("SELECT (SELECT COUNT(*) FROM [PEDIDO] WHERE DATE([PEDIDO].DAT_PEDIDO) = DATE('NOW')) AS 'NUM PEDIDOS', ");
        //        sql.Append("(SELECT COUNT(DISTINCT COD_PRODUTO) FROM [ITEM_PEDIDO] JOIN [PEDIDO] ON [ITEM_PEDIDO].COD_PEDIDO = [PEDIDO].COD_PEDIDO WHERE DATE([PEDIDO].DAT_PEDIDO) = DATE('NOW')) AS 'SKU', ");
        //        sql.Append("(SELECT SUM([VLR_TOTAL_PEDIDO]) FROM [PEDIDO] WHERE DATE(DAT_PEDIDO) = DATE('NOW')) AS 'VALOR TOTAL', ");
        //        sql.Append("SUM([ITEM_PEDIDO].[QTD_PEDIDA] / [PRODUTO].[QTD_UNIDADE_EMBALAGEM]) AS 'VOLUME' ");
        //        sql.Append("FROM [PEDIDO] ");
        //        sql.Append("JOIN [ITEM_PEDIDO] ON [PEDIDO].[COD_PEDIDO] = [ITEM_PEDIDO].[COD_PEDIDO] ");
        //        sql.Append("JOIN [PRODUTO] ON [PRODUTO].[COD_PRODUTO] = [ITEM_PEDIDO].[COD_PRODUTO] ");
        //        sql.Append("WHERE DATE([PEDIDO].[DAT_PEDIDO]) = DATE('NOW') ");

        //        using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString()))
        //        {
        //            while (sqlReader.Read())
        //            {
        //                NumeroPedidos = sqlReader.GetValue(0) == System.DBNull.Value ? "0" : sqlReader.GetInt32(0).ToString();
        //                NumeroProdutos = sqlReader.GetValue(1).ToString();
        //                ValorTotal = sqlReader.GetValue(2) == System.DBNull.Value ? "0" : Math.Round(Convert.ToDecimal(sqlReader.GetValue(2)), 2).ToString();
        //                Volume = sqlReader.GetValue(3) == System.DBNull.Value ? "0" : sqlReader.GetValue(3).ToString();

        //                break;
        //            }
        //        }

        //        //Carrega o Peso total dos pedidos feitos fora de rota

        //        sql.Length = 0;
        //        sql.Append("SELECT SUM(([PRODUTO].[VLR_PESO_PRODUTO] * [ITEM_PEDIDO].[QTD_PEDIDA]) / [PRODUTO].[QTD_UNIDADE_EMBALAGEM]) AS 'PESO TOTAL' ");
        //        sql.Append("FROM [PEDIDO] ");
        //        sql.Append("JOIN [ITEM_PEDIDO] ON [PEDIDO].[COD_PEDIDO] = [ITEM_PEDIDO].[COD_PEDIDO] ");
        //        sql.Append("JOIN [PRODUTO] ON [PRODUTO].[COD_PRODUTO] = [ITEM_PEDIDO].[COD_PRODUTO] ");
        //        sql.Append("WHERE DATE([PEDIDO].[DAT_PEDIDO]) = DATE('NOW') ");
        //        sql.Append("AND [PEDIDO].[COD_PDV] IN (SELECT DISTINCT T1.COD_PDV ");
        //        sql.Append("FROM PDV T1 ");
        //        sql.Append("JOIN CADASTRO_DIA_VISITA T2 ON T2.COD_PDV = T1.COD_PDV ");
        //        sql.Append("JOIN PEDIDO T3 ON T3.COD_PDV = T1.COD_PDV AND T3.IND_HISTORICO = 0 ");
        //        sql.Append("JOIN OPERACAO T6 ON T6.COD_OPERACAO = T3.COD_OPERACAO ");
        //        sql.Append("AND T6.COD_OPERACAO_CFO IN (1, 21) ");
        //        sql.Append("INNER JOIN PDV_GRUPO_COMERCIALIZACAO T4 ON T1.COD_PDV = T4.COD_PDV ");
        //        sql.Append("JOIN DAT_REFERENCIA_CICLO_VISITA T5 ON (T4.DSC_CICLO_VISITA NOT LIKE '%' || T5.COD_CICLO || '%' ");
        //        sql.Append("AND DATE('NOW') BETWEEN DATE(T5.DAT_INICIO_CICLO) AND DATE(T5.DAT_FINAL_CICLO) ");
        //        sql.Append("OR T2.COD_DIA_VISITA != ?) ");
        //        sql.Append("AND DATE(T3.DAT_PEDIDO) = DATE('NOW')) ");

        //        SQLiteParameter pCOD_VISITA = new SQLiteParameter("@COD_VISITA", DateTime.Now.DayOfWeek);

        //        using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString(), pCOD_VISITA))
        //        {
        //            while (sqlReader.Read())
        //            {
        //                PesoForaRota = sqlReader.GetValue(0) == System.DBNull.Value ? "0" : Math.Round(Convert.ToDecimal(sqlReader.GetValue(0)), 2).ToString();

        //                break;
        //            }
        //        }

        //        //Carrega o Peso total dos pedidos feitos dentro da rota

        //        sql.Replace("OR T2.COD_DIA_VISITA != ?) ", "OR T2.COD_DIA_VISITA == ?) ");

        //        using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sql.ToString(), pCOD_VISITA))
        //        {
        //            while (sqlReader.Read())
        //            {
        //                PesoNaRota = sqlReader.GetValue(0) == System.DBNull.Value ? "0" : Math.Round(Convert.ToDecimal(sqlReader.GetValue(0)), 2).ToString();

        //                break;
        //            }
        //        }
        //    }
        //}
    }
}