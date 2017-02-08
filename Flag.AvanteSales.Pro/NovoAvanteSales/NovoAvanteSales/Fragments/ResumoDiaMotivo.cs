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
using AvanteSales.SystemFramework;
using SQLiteParameter = Mono.Data.Sqlite.SqliteParameter;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;
using SQLiteCommand = Mono.Data.Sqlite.SqliteCommand;

namespace AvanteSales.Pro.Fragments
{
    public class ResumoDiaMotivo : Android.Support.V4.App.Fragment
    {
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        LinearLayout HeaderListView;
        LayoutInflater thisLayoutInflater;
        static View thisView;
        ListView listMotivo;
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
            var view = inflater.Inflate(Resource.Layout.resumo_dia_motivo, container, false);
            FindViewsById(view);
            ActivityContext = Activity;
            thisLayoutInflater = inflater;
            thisView = view;
            return view;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            
            var view = thisLayoutInflater.Inflate(Resource.Layout.resumo_dia_motivo_header, null);
            HeaderListView.AddView(view);

            CarregaListMotivosNaoPositivacao();
        }

        private void FindViewsById(View view)
        {
            HeaderListView = view.FindViewById<LinearLayout>(Resource.Id.HeaderListView);
            listMotivo = view.FindViewById<ListView>(Resource.Id.listMotivo);
        }

        private void CarregaListMotivosNaoPositivacao()
        {
            SQLiteParameter paramDAT_HISTORICO_MOTIVO = null;
            SQLiteParameter paramCOD_DIA_VISITA = null;
            SQLiteParameter paramDAT_ULTIMA_DESCARGA = null;
            SQLiteDataReader reader = null;
            CSListViewItem lvi = null;
            StringBuilder sqlQuery = new StringBuilder();

            List<CSListViewItem> listNaoPositivado = new List<CSListViewItem>();

            try
            {
                listMotivo.Adapter = null;

                if (ResumoDiaPDV.totalNaoPositivados > 0)
                {
                    sqlQuery.Length = 0;

                    sqlQuery.Append("   SELECT T5.DSC_MOTIVO, COUNT(T5.DSC_MOTIVO) ");
                    sqlQuery.Append("     FROM HISTORICO_MOTIVO T3 ");
                    sqlQuery.Append("    INNER JOIN MOTIVO T5 ON T5.COD_MOTIVO = T3.COD_MOTIVO  AND T5.COD_TIPO_MOTIVO = T3.COD_TIPO_MOTIVO ");
                    sqlQuery.Append("    WHERE T3.COD_PDV IN (SELECT COD_PDV FROM CADASTRO_DIA_VISITA WHERE COD_DIA_VISITA = ?) ");
                    sqlQuery.Append("      AND date(T3.DAT_HISTORICO_MOTIVO) = ? AND datetime(T3.DAT_HISTORICO_MOTIVO) > datetime(?)");

                    sqlQuery.Append(" GROUP BY T5.DSC_MOTIVO ");

                    paramCOD_DIA_VISITA = new SQLiteParameter("@COD_DIA_VISITA", DayOfWeek);
                    paramDAT_HISTORICO_MOTIVO = new SQLiteParameter("@DAT_HISTORICO_MOTIVO", DateTime.Now.ToString("yyyy-MM-dd"));
                    paramDAT_ULTIMA_DESCARGA = new SQLiteParameter("@DAT_ULTIMA_DESCARGA", DataUltimaDescarga.ToString("yyyy-MM-dd HH:mm:ss"));
                    using (reader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), paramCOD_DIA_VISITA, paramDAT_HISTORICO_MOTIVO, paramDAT_ULTIMA_DESCARGA))
                    {
                        while (reader.Read())
                        {
                            lvi = new CSListViewItem();

                            string descricao = reader.GetString(0);
                            int quantidade = reader.GetInt32(1);

                            lvi.Text = descricao;
                            lvi.SubItems = new List<object>();
                            lvi.SubItems.Add(quantidade.ToString());
                            lvi.SubItems.Add((((quantidade / Convert.ToDecimal(ResumoDiaPDV.totalNaoPositivados)) * 100)).ToString(CSGlobal.DecimalStringFormat));

                            listNaoPositivado.Add(lvi);
                        }
                        reader.Close();
                        reader.Dispose();
                    }
                }

                lvi = new CSListViewItem();
                lvi.Text = "TOTAL";
                lvi.SubItems = new List<object>();
                lvi.SubItems.Add(ResumoDiaPDV.totalNaoPositivados.ToString());
                lvi.SubItems.Add("100.00");
                listNaoPositivado.Add(lvi);

                listMotivo.Adapter = new ListarResumoNaoPositivado(ActivityContext, Resource.Layout.resumo_dia_motivo_row, listNaoPositivado);

            }
            catch (Exception e)
            {
                MessageBox.AlertErro(ActivityContext, e.Message);
                throw e;
            }
        }

        private class ListarResumoNaoPositivado : ArrayAdapter<CSListViewItem>
        {
            Activity act;
            IList<CSListViewItem> produto;
            int resourceId;

            public ListarResumoNaoPositivado(Activity c, int textViewResourceId, IList<CSListViewItem> objects)
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
                    TextView tvMotivo = linha.FindViewById<TextView>(Resource.Id.tvMotivo);
                    TextView tvQtdVisitas = linha.FindViewById<TextView>(Resource.Id.tvQtdVisitas);
                    TextView tvPorcentagemVisitas = linha.FindViewById<TextView>(Resource.Id.tvPorcentagemVisitas);

                    tvMotivo.Text = item.Text;
                    tvQtdVisitas.Text = item.SubItems[0].ToString();
                    tvPorcentagemVisitas.Text = item.SubItems[1].ToString();

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