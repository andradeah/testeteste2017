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
using AvanteSales.Pro.Dialogs;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;

namespace AvanteSales.Pro.Fragments
{
    public class ResumoDiaPDV : Android.Support.V4.App.Fragment
    {
        private TextView lblQtdePrevistas;
        private TextView lblPorcPrevistas;
        private TextView lblQtdeRealizadas;
        private TextView lblPorcRealizadas;
        private TextView lblQtdePositivados;
        private TextView lblPorcPositivados;
        private TextView lblQtdeNaoPositivados;
        private TextView lblPorcNaoPositivados;
        private TextView lblQtdePositivadosForaRota;
        private TextView lblPorcPositivadosForaRota;
        private TextView lblQtdeCoberturaDia;
        private TextView lblPorcCoberturaDia;

        private int totalPrevistas;
        private int totalRealizadas;
        private int totalPositivados;
        public static int totalNaoPositivados;
        private int totalPositivadosForaRota;
        Android.Support.V4.App.FragmentActivity ActivityContext;
        private int DayOfWeek
        {
            get
            {
                int day = (int)DateTime.Now.DayOfWeek;

                // [ Teste para domingo, compatibilidade com o flexx ]
                return (day == 0) ? 7 : day;
            }
        }

        public static DateTime DataUltimaDescarga
        {
            get
            {
                object objData = null;

                //CSDataAccess.Instance.AbreConexao();

                try
                {
                    // Busca a data da ultima descarga
                    objData = CSDataAccess.Instance.ExecuteScalar("SELECT DATA_ULTIMA_SINCRONIZACAO FROM INFORMACOES_SINCRONIZACAO WHERE COD_EMPREGADO = " + CSConfiguracao.GetConfig("vendedor" + CSGlobal.COD_REVENDA));
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                if (objData == null)
                    objData = DateTime.Now;

                return Convert.ToDateTime(objData);
            }
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.resumo_dia_pdv, container, false);
            FindViewsById(view);
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
            StringBuilder sqlQuery = null;
            SQLiteParameter paramCOD_DIA_VISITA = null;
            SQLiteParameter paramDAT_CICLO = null;
            SQLiteParameter paramDAT_PEDIDO = null;
            //SQLiteParameter paramDAT_HISTORICO_MOTIVO = null;

            sqlQuery = new StringBuilder();

            object result = null;

            decimal porcentagemRealizadas = 0;
            decimal percentagemPositivados = 0;
            decimal percentagemNaoPositivados = 0;

            try
            {
                sqlQuery.Append("SELECT COUNT(*) ");
                sqlQuery.Append("  FROM PDV ");
                sqlQuery.Append(" WHERE COD_PDV IN (SELECT T1.COD_PDV ");
                sqlQuery.Append("                     FROM PDV T1 ");
                sqlQuery.Append("                    INNER JOIN PDV_GRUPO_COMERCIALIZACAO T2 ON T1.COD_PDV = T2.COD_PDV ");
                sqlQuery.Append("                    INNER JOIN CATEGORIA T3 ON T1.COD_CATEGORIA = T3.COD_CATEGORIA ");
                sqlQuery.Append("                    INNER JOIN CONDICAO_PAGAMENTO T4 ON T2.COD_CONDICAO_PAGAMENTO = T4.COD_CONDICAO_PAGAMENTO ");
                sqlQuery.Append("                    INNER JOIN CADASTRO_DIA_VISITA T5 ON T2.COD_PDV = T5.COD_PDV ");
                sqlQuery.Append("                      AND T2.COD_GRUPO_COMERCIALIZACAO = T5.COD_GRUPO_COMERCIALIZACAO ");
                sqlQuery.Append("   AND T5.COD_DIA_VISITA = ? ");
                sqlQuery.Append(" INNER JOIN DAT_REFERENCIA_CICLO_VISITA T6 ON T2.DSC_CICLO_VISITA LIKE '%' || T6.COD_CICLO || '%' ");
                sqlQuery.Append("   AND DATE(?) BETWEEN DATE(T6.DAT_INICIO_CICLO) AND DATE(T6.DAT_FINAL_CICLO))");

                paramDAT_CICLO = new SQLiteParameter("@DAT_CICLO", DateTime.Now.Date);
                paramCOD_DIA_VISITA = new SQLiteParameter("@COD_DIA_VISITA", DayOfWeek);
                result = CSDataAccess.Instance.ExecuteScalar(sqlQuery.ToString(), paramCOD_DIA_VISITA, paramDAT_CICLO);

                if (result != null)
                    totalPrevistas = int.Parse(result.ToString());
                else
                    totalPrevistas = 0;

                if (totalPrevistas > 0)
                {
                    // [ ---------------- ]
                    // [ PDVs positivados ]
                    // [ ---------------- ]
                    sqlQuery.Length = 0;
                    sqlQuery.Append("SELECT COUNT(*) ");
                    sqlQuery.Append("  FROM PDV ");
                    sqlQuery.Append(" WHERE COD_PDV IN (SELECT DISTINCT T1.COD_PDV ");
                    sqlQuery.Append("                     FROM CADASTRO_DIA_VISITA T1 ");
                    sqlQuery.Append("                     JOIN PEDIDO T2 ON T2.COD_PDV = T1.COD_PDV AND T2.IND_HISTORICO = 0 ");
                    sqlQuery.Append("                    INNER JOIN PDV_GRUPO_COMERCIALIZACAO T3 ON T1.COD_PDV = T3.COD_PDV ");
                    sqlQuery.Append("                    INNER JOIN DAT_REFERENCIA_CICLO_VISITA T4 ON T3.DSC_CICLO_VISITA LIKE '%' || T4.COD_CICLO || '%' ");
                    sqlQuery.Append("                      AND DATE(?) BETWEEN DATE(T4.DAT_INICIO_CICLO) AND DATE(T4.DAT_FINAL_CICLO) ");
                    sqlQuery.Append("                    INNER JOIN OPERACAO T5 ON T5.COD_OPERACAO = T2.COD_OPERACAO ");
                    sqlQuery.Append("                      AND T5.COD_OPERACAO_CFO IN (1, 21) ");
                    sqlQuery.Append("                    WHERE T1.COD_DIA_VISITA = ? ");
                    sqlQuery.Append("                      AND DATE(T2.DAT_PEDIDO) = DATE(?)) ");


                    paramDAT_CICLO = new SQLiteParameter("@DAT_CICLO", DateTime.Now.Date);
                    paramCOD_DIA_VISITA = new SQLiteParameter("@COD_DIA_VISITA", DayOfWeek);
                    paramDAT_PEDIDO = new SQLiteParameter("@DAT_PEDIDO", DateTime.Now.Date);

                    result = CSDataAccess.Instance.ExecuteScalar(sqlQuery.ToString(), paramDAT_CICLO, paramCOD_DIA_VISITA, paramDAT_PEDIDO);

                    if (result != null)
                        totalPositivados = int.Parse(result.ToString());
                    else
                    {
                        totalPositivados = 0;
                        percentagemPositivados = 0;
                    }

                    // Percentagem positivados
                    percentagemPositivados = ((totalPositivados * 100) / Convert.ToDecimal(totalPrevistas));


                    // [ -------------------- ]
                    // [ PDVs não positivados ]
                    // [ -------------------- ]
                    sqlQuery.Length = 0;
                    sqlQuery.Append("SELECT COUNT(*) ");
                    sqlQuery.Append("  FROM PDV ");
                    sqlQuery.Append(" WHERE COD_PDV IN (SELECT DISTINCT T1.COD_PDV ");
                    sqlQuery.Append("                     FROM CADASTRO_DIA_VISITA T1 ");
                    sqlQuery.Append("                     JOIN HISTORICO_MOTIVO T2 ON T2.COD_PDV = T1.COD_PDV ");
                    sqlQuery.Append("                    INNER JOIN PDV_GRUPO_COMERCIALIZACAO T3 ON T1.COD_PDV = T3.COD_PDV ");
                    sqlQuery.Append("                    INNER JOIN DAT_REFERENCIA_CICLO_VISITA T4 ON T3.DSC_CICLO_VISITA LIKE '%' || T4.COD_CICLO || '%' ");
                    sqlQuery.Append("                      AND datetime(?) BETWEEN DATETIME(T4.DAT_INICIO_CICLO) AND DATETIME(T4.DAT_FINAL_CICLO) ");
                    sqlQuery.Append("                    WHERE T1.COD_DIA_VISITA = ? ");
                    sqlQuery.Append("                      AND julianday('now') - julianday(T2.DAT_HISTORICO_MOTIVO) < 1 ");
                    sqlQuery.Append("                      AND T2.COD_TIPO_MOTIVO = 2)");// AND datetime(T2.DAT_HISTORICO_MOTIVO) > datetime(?))");

                    paramDAT_CICLO = new SQLiteParameter("@DAT_CICLO", DateTime.Now.Date);
                    paramCOD_DIA_VISITA = new SQLiteParameter("@COD_DIA_VISITA", DayOfWeek);
                    //paramDAT_HISTORICO_MOTIVO = new SQLiteParameter("@DAT_HISTORICO_MOTIVO", DataUltimaDescarga.ToString("yyyy-MM-dd HH:mm:ss"));

                    result = CSDataAccess.Instance.ExecuteScalar(sqlQuery.ToString(), paramDAT_CICLO, paramCOD_DIA_VISITA);//, paramDAT_HISTORICO_MOTIVO);

                    if (result != null)
                        totalNaoPositivados = int.Parse(result.ToString());
                    else
                    {
                        totalNaoPositivados = 0;
                        percentagemNaoPositivados = 0;
                    }

                    percentagemNaoPositivados = (decimal)((totalNaoPositivados * 100) / (decimal)totalPrevistas);


                    // [ --------------------------- ]
                    // [ Total de visitas realizadas ]
                    // [ --------------------------- ]
                    totalRealizadas = totalPositivados + totalNaoPositivados;
                    porcentagemRealizadas = (decimal)((totalRealizadas * 100) / (decimal)totalPrevistas);

                }

                // [ ----------------------------- ]
                // [ PDVs positivados fora da rota ]
                // [ ----------------------------- ]
                paramDAT_CICLO = new SQLiteParameter("@DAT_CICLO", DateTime.Now.Date);
                paramCOD_DIA_VISITA = new SQLiteParameter("@COD_DIA_VISITA", DayOfWeek);
                paramDAT_PEDIDO = new SQLiteParameter("@DAT_PEDIDO", DateTime.Now.Date);

                result = CSPDVs.RetornaPdvsPositivadosForaRota(paramCOD_DIA_VISITA, paramDAT_CICLO, paramDAT_PEDIDO);

                if (result != null)
                    totalPositivadosForaRota = int.Parse(result.ToString());
                else
                    totalPositivadosForaRota = 0;


                // Quantidade de Visitas Previstas
                lblQtdePrevistas.Text = totalPrevistas.ToString();
                lblPorcPrevistas.Text = "-";

                // Exibe o valor
                lblQtdePositivados.Text = totalPositivados.ToString();
                lblPorcPositivados.Text = percentagemPositivados.ToString(CSGlobal.DecimalStringFormat) + "%";

                lblQtdeNaoPositivados.Text = totalNaoPositivados.ToString();
                lblPorcNaoPositivados.Text = percentagemNaoPositivados.ToString(CSGlobal.DecimalStringFormat) + "%";

                lblQtdePositivadosForaRota.Text = totalPositivadosForaRota.ToString();
                lblPorcPositivadosForaRota.Text = "-";

                lblQtdeRealizadas.Text = totalRealizadas.ToString();
                lblPorcRealizadas.Text = porcentagemRealizadas.ToString(CSGlobal.DecimalStringFormat) + "%";

                lblQtdeCoberturaDia.Text = ((int)(totalPositivados + totalPositivadosForaRota)).ToString();
                lblPorcCoberturaDia.Text = "-";

            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(ActivityContext, ex.Message);
            }
        }

        private void FindViewsById(View view)
        {
            lblQtdePrevistas = view.FindViewById<TextView>(Resource.Id.lblQtdePrevistas);
            lblPorcPrevistas = view.FindViewById<TextView>(Resource.Id.lblPorcPrevistas);
            lblQtdeRealizadas = view.FindViewById<TextView>(Resource.Id.lblQtdeRealizadas);
            lblPorcRealizadas = view.FindViewById<TextView>(Resource.Id.lblPorcRealizadas);
            lblQtdePositivados = view.FindViewById<TextView>(Resource.Id.lblQtdePositivados);
            lblPorcPositivados = view.FindViewById<TextView>(Resource.Id.lblPorcPositivados);
            lblQtdeNaoPositivados = view.FindViewById<TextView>(Resource.Id.lblQtdeNaoPositivados);
            lblPorcNaoPositivados = view.FindViewById<TextView>(Resource.Id.lblPorcNaoPositivados);
            lblQtdePositivadosForaRota = view.FindViewById<TextView>(Resource.Id.lblQtdePositivadosForaRota);
            lblPorcPositivadosForaRota = view.FindViewById<TextView>(Resource.Id.lblPorcPositivadosForaRota);
            lblQtdeCoberturaDia = view.FindViewById<TextView>(Resource.Id.lblQtdeCoberturaDia);
            lblPorcCoberturaDia = view.FindViewById<TextView>(Resource.Id.lblPorcCoberturaDia);
        }
    }
}