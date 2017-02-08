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
using AvanteSales.Pro.Controles;
using AvanteSales.Pro.Dialogs;

namespace AvanteSales.Pro.Fragments
{
    public class UltimasVisitas : Android.Support.V4.App.Fragment
    {
        private RadioGroup rgpRadioGroup;
        private TextView lblOperacao;
        private TextView lblCondicao;
        private TextView lblAdf;
        private TextView lblValorDesconto;
        private TextView lblTotalPedi;
        private TextView lblStatusPedido;
        private TextView lblValorAbatimento;
        private Button btnListarPedido;
        private TextView lblTotalPeso;
        private TableLayout tblUltimasVisitas;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.ultimas_visitas, container, false);

            FindViewsById(view);
            Eventos();

            return view;
        }

        private void Eventos()
        {
            btnListarPedido.Click += BtnListarPedido_Click;
            rgpRadioGroup.CheckedChange += RgpRadioGroup_CheckedChange;
        }

        private void BtnListarPedido_Click(object sender, EventArgs e)
        {
            if (ExistePedidoSelecionado())
            {
                if (Activity.Class.SimpleName == "Cliente")
                    ((Cliente)Activity).AbrirListaProdutos((int)ActivitiesNames.UltimasVisitas, string.Empty, string.Empty);
                else
                    ((RelatorioPdv)Activity).AbrirListaProdutos((int)ActivitiesNames.UltimasVisitas, string.Empty, string.Empty);
            }
            else
            {
                MessageBox.ShowShortMessageCenter(Activity, "Selecione um pedido");
            }
        }

        private CSUltimasVisitasPDV.CSUltimaVisitaPDV UltimaVisitaSelecionada(int codPedido)
        {
            return CSPDVs.Current.ULTIMAS_VISITAS.Items.Cast<CSUltimasVisitasPDV.CSUltimaVisitaPDV>()
                .Where(p => p.COD_PEDIDO == codPedido).FirstOrDefault();
        }

        private bool ExistePedidoSelecionado()
        {
            return rgpRadioGroup.CheckedRadioButtonId != -1;
        }

        private void RgpRadioGroup_CheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            var pedido = UltimaVisitaSelecionada(e.CheckedId);
            ExibirPedido(pedido);
            tblUltimasVisitas.Visibility = ViewStates.Visible;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            Inicializacao();
        }

        private void Inicializacao()
        {
            LimpaTela();

            foreach (CSUltimasVisitasPDV.CSUltimaVisitaPDV ultimasVisitas in CSPDVs.Current.ULTIMAS_VISITAS.Items)
            {
                string cod_pedido = ultimasVisitas.COD_PEDIDO.ToString();
                string data = ultimasVisitas.DAT_PEDIDO.ToString("dd/MM/yyyy");
                string valorTotal = ultimasVisitas.VLR_TOTAL_PEDIDO.ToString(CSGlobal.DecimalStringFormat);

                RadioButton rdb = new RadioButton(Activity);
                var lp = new LinearLayout.LayoutParams(WindowManagerLayoutParams.MatchParent, Resource.Dimension.widgets_height);

                rdb.LayoutParameters = lp;
                rdb.SetPadding(75, 0, 0, 0);
                rdb.Text = "Pedido  " + cod_pedido + "  " + data + "  " + valorTotal;
                rdb.Id = Convert.ToInt32(cod_pedido);
                rgpRadioGroup.AddView(rdb);
            }
        }

        private void LimpaTela()
        {
            lblOperacao.Text = string.Empty;
            lblCondicao.Text = string.Empty;
            lblAdf.Text = string.Empty;
            lblValorDesconto.Text = string.Empty;
            lblValorAbatimento.Text = string.Empty;
            lblTotalPedi.Text = string.Empty;
            lblTotalPeso.Text = string.Empty;
        }

        private void FindViewsById(View view)
        {
            rgpRadioGroup = view.FindViewById<RadioGroup>(Resource.Id.rgpRadioGroup);
            lblOperacao = view.FindViewById<TextView>(Resource.Id.lblOperacao);
            lblCondicao = view.FindViewById<TextView>(Resource.Id.lblCondicao);
            lblAdf = view.FindViewById<TextView>(Resource.Id.lblAdf);
            lblValorDesconto = view.FindViewById<TextView>(Resource.Id.lblValorDesconto);
            lblTotalPedi = view.FindViewById<TextView>(Resource.Id.lblTotalPedi);
            lblStatusPedido = view.FindViewById<TextView>(Resource.Id.lblStatusPedido);
            btnListarPedido = view.FindViewById<Button>(Resource.Id.btnListarPedido);
            lblTotalPeso = view.FindViewById<TextView>(Resource.Id.lblTotalPeso);
            lblValorAbatimento = view.FindViewById<TextView>(Resource.Id.lblValorAbatimento);
            tblUltimasVisitas = view.FindViewById<TableLayout>(Resource.Id.tblUltimasVisitas);
        }

        private void ExibirPedido(CSUltimasVisitasPDV.CSUltimaVisitaPDV pedido)
        {
            try
            {
                // Seta qual é o pedido atual
                CSPDVs.Current.ULTIMAS_VISITAS.Current = pedido;

                var operacao = CSOperacoes.Items.Cast<CSOperacoes.CSOperacao>()
                    .Where(o => o.COD_OPERACAO == CSPDVs.Current.ULTIMAS_VISITAS.Current.COD_OPERACAO).FirstOrDefault();

                if (operacao != null)
                {
                    // Preenche o textBox de operação
                    lblOperacao.Text = operacao.DSC_OPERACAO;
                }

                else
                {
                    lblOperacao.Text = "-";
                }

                var condicao = CSCondicoesPagamento.Items.Cast<CSCondicoesPagamento.CSCondicaoPagamento>()
                    .Where(c => c.COD_CONDICAO_PAGAMENTO == CSPDVs.Current.ULTIMAS_VISITAS.Current.COD_CONDICAO_PAGAMENTO).FirstOrDefault();

                if (condicao != null)
                {
                    // Preenche o textBox de condições de pagamento
                    lblCondicao.Text = condicao.DSC_CONDICAO_PAGAMENTO;
                }

                else
                {
                    lblCondicao.Text = "-";
                }

                if (CSPDVs.Current.ULTIMAS_VISITAS.Current.ITEMS_PEDIDOS.Items.Count > 0)
                {
                    // Busca dos valores
                    var itemPedido = CSPDVs.Current.ULTIMAS_VISITAS.Current.ITEMS_PEDIDOS.Items[0];
                    lblAdf.Text = itemPedido.PRC_ADICIONAL_FINANCEIRO.ToString(CSGlobal.DecimalStringFormat);

                    decimal abatimento = pedido.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(pp => pp.VLR_INDENIZACAO_UNIDADE > 0).Sum(p => p.VLR_INDENIZACAO_UNIDADE);
                    decimal desconto = pedido.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(pp => pp.VLR_DESCONTO > 0).Sum(p => p.VLR_DESCONTO);

                    lblValorDesconto.Text = (desconto - abatimento).ToString(CSGlobal.DecimalStringFormat);
                    lblValorAbatimento.Text = abatimento.ToString(CSGlobal.DecimalStringFormat);
                    lblTotalPedi.Text = pedido.VLR_TOTAL_PEDIDO.ToString(CSGlobal.DecimalStringFormat);
                    lblTotalPeso.Text = CSPDVs.Current.ULTIMAS_VISITAS.Current.ITEMS_PEDIDOS.Items.Cast<CSItemsPedido.CSItemPedido>()
                        .Sum(pt => pt.PRODUTO.VLR_PESO_PRODUTO).ToString(CSGlobal.DecimalStringFormat);

                    switch (pedido.STA_PEDIDO_FLEXX)
                    {
                        case 1:
                            lblStatusPedido.Text = "Faturado";
                            break;
                        case 2:
                            lblStatusPedido.Text = "Entregue";
                            break;
                        case 3:
                            lblStatusPedido.Text = "Embarcado";
                            break;
                        default:
                            lblStatusPedido.Text = string.Empty;
                            break;
                    }

                    btnListarPedido.Visibility = ViewStates.Visible;
                    btnListarPedido.Text = string.Format("Lista de produtos ({0})", CSPDVs.Current.ULTIMAS_VISITAS.Current.ITEMS_PEDIDOS.Count);
                }
                else
                {
                    btnListarPedido.Visibility = ViewStates.Invisible;
                    MessageBox.ShowShortMessageCenter(Activity, "Não foi possível carregar os itens do pedido escolhido.");
                }

            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(Activity, ex.Message);
            }
        }
    }
}