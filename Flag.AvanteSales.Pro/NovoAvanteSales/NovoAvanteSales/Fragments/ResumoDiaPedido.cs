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
using AvanteSales.SystemFramework;
using AvanteSales.Pro.Formatters;

namespace AvanteSales.Pro.Fragments
{
    public class ResumoDiaPedido : Android.Support.V4.App.Fragment
    {
        Android.Support.V4.App.FragmentActivity ActivityContext;
        private Spinner cmbPedidos;
        private TextView lblCliente;
        private TextView lblOperacao;
        private TextView lblCondicao;
        private TextView lblAdf;
        private TextView lblValorDesconto;
        private TextView lblValorTotal;
        //private TextView lblPesoTotal;
        private Button btnListarProdutos;

        public static int pedido = -1;
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.resumo_dia_pedido, container, false);
            FindViewsById(view);
            Eventos();
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
            SQLiteParameter paramDAT_PEDIDO = null;
            CSItemCombo ic = null;
            int pedido = 0;

            StringBuilder sqlQuery = new StringBuilder();

            // Limpa tela do pedidos.
            LimpaTelaPedido();

            cmbPedidos.Adapter = null;

            var adapter = cmbPedidos.SetDefaultAdapter();

            paramDAT_PEDIDO = new SQLiteParameter("@DAT_PEDIDO", DateTime.Now.Date);

            sqlQuery.Length = 0;
            sqlQuery.Append(" SELECT PEDIDO.COD_PEDIDO ");
            sqlQuery.Append("      , PEDIDO.VLR_TOTAL_PEDIDO  ");
            sqlQuery.Append("      , OPERACAO.DSC_OPERACAO  ");
            sqlQuery.Append("   FROM PEDIDO ");
            sqlQuery.Append("   JOIN OPERACAO ");
            sqlQuery.Append("       ON PEDIDO.[COD_OPERACAO] = OPERACAO.[COD_OPERACAO]");
            sqlQuery.Append("  WHERE DATE(PEDIDO.DAT_PEDIDO) = DATE(?) ");
            sqlQuery.Append("    AND PEDIDO.IND_HISTORICO = 0 ");

            using (SQLiteDataReader reader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString(), paramDAT_PEDIDO))
            {
                while (reader.Read())
                {
                    pedido = reader.GetValue(0) == System.DBNull.Value ? -1 : reader.GetInt32(0);

                    ic = new CSItemCombo();

                    ic.Texto = string.Format("Pedido {0} - {1}", pedido.ToString().Trim(), reader.GetValue(2).ToString());
                    ic.Valor = pedido;
                    adapter.Add(ic);
                }

                reader.Close();
                reader.Dispose();
            }
        }

        private void Eventos()
        {
            cmbPedidos.ItemSelected += CmbPedidos_ItemSelected;
            btnListarProdutos.Click += BtnListarProdutos_Click;
        }

        private void BtnListarProdutos_Click(object sender, EventArgs e)
        {
            if (ExistePedidoSelecionado())
            {
                ((Activities.Relatorio)Activity).AbrirListaProdutos((int)Controles.ActivitiesNames.ResumoPedido, string.Empty, string.Empty);
            }
            else
            {
                MessageBox.ShowShortMessageCenter(Activity, "Selecione um pedido");
            }
        }

        private bool ExistePedidoSelecionado()
        {
            return cmbPedidos.SelectedItem != null;
        }

        private void CmbPedidos_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            // Limpa tela 
            LimpaTelaPedido();

            // Caso tenha selecionado algum pedido
            if (cmbPedidos.Adapter != null)
            {
                MostraPedidoSelecionado();
            }
        }

        private void LimpaTelaPedido()
        {
            lblCliente.Text = string.Empty;
            lblOperacao.Text = string.Empty;
            lblCondicao.Text = string.Empty;
            lblAdf.Text = string.Empty;
            lblValorDesconto.Text = string.Empty;
            lblValorTotal.Text = string.Empty;
        }

        private void FindViewsById(View view)
        {
            cmbPedidos = view.FindViewById<Spinner>(Resource.Id.cmbPedidos);
            lblCliente = view.FindViewById<TextView>(Resource.Id.lblCliente);
            lblOperacao = view.FindViewById<TextView>(Resource.Id.lblOperacao);
            lblCondicao = view.FindViewById<TextView>(Resource.Id.lblCondicao);
            lblAdf = view.FindViewById<TextView>(Resource.Id.lblAdf);
            lblValorDesconto = view.FindViewById<TextView>(Resource.Id.lblValorDesconto);
            lblValorTotal = view.FindViewById<TextView>(Resource.Id.tvValorTotal);
            btnListarProdutos = view.FindViewById<Button>(Resource.Id.btnListarProdutos);
        }

        private void MostraPedidoSelecionado()
        {
            string produto = string.Empty;
            string unidadeMedida = string.Empty;
            string unidadeMedidaMostra = string.Empty;
            decimal valorDesconto = 0;
            decimal valorpedido = 0;
            StringBuilder sqlQuery = new StringBuilder();

            SQLiteParameter paramCOD_PEDIDO = null;

            pedido = Convert.ToInt32(((CSItemCombo)cmbPedidos.SelectedItem).Valor);
            paramCOD_PEDIDO = new SQLiteParameter("@COD_PEDIDO", pedido);

            try
            {
                if (pedido != -1)
                {
                    sqlQuery.Length = 0;
                    sqlQuery.Append(" SELECT OPERACAO.DSC_OPERACAO ");
                    sqlQuery.Append("      , CONDICAO_PAGAMENTO.DSC_CONDICAO_PAGAMENTO ");
                    sqlQuery.Append("      , PDV.DSC_RAZAO_SOCIAL ");
                    sqlQuery.Append("      , ITEM_PEDIDO.PRC_ADICIONAL_FINANCEIRO ");
                    sqlQuery.Append("      , ITEM_PEDIDO.VLR_DESCONTO ");
                    sqlQuery.Append("      , PEDIDO.VLR_TOTAL_PEDIDO");
                    sqlQuery.Append("      , (ITEM_PEDIDO.VLR_TOTAL - ITEM_PEDIDO.VLR_UNITARIO_INDENIZACAO)");
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
                            if (lblCliente.Text == string.Empty)
                            {
                                lblCliente.Text = reader.GetValue(2) == System.DBNull.Value ? "" : reader.GetString(2);
                                lblOperacao.Text = reader.GetValue(0) == System.DBNull.Value ? "" : reader.GetString(0);
                                lblCondicao.Text = reader.GetValue(1) == System.DBNull.Value ? "" : reader.GetString(1);
                                lblAdf.Text = ((reader.GetValue(3) == System.DBNull.Value ? 0 : reader.GetDecimal(3))).ToString(CSGlobal.DecimalStringFormat);
                            }

                            valorDesconto += reader.GetValue(4) == System.DBNull.Value ? 0 : Convert.ToDecimal(reader.GetValue(4));

                            valorpedido += reader.GetValue(6) == System.DBNull.Value ? 0 : Convert.ToDecimal(reader.GetValue(6));
                        }

                        lblValorTotal.Text = valorpedido.ToString(CSGlobal.DecimalStringFormat);
                        lblValorDesconto.Text = valorDesconto.ToString(CSGlobal.DecimalStringFormat);

                        reader.Close();
                        reader.Dispose();
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.AlertErro(ActivityContext, e.Message);
            }
        }
    }
}