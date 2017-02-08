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
using Java.Lang;

namespace AvanteSales.Pro.Fragments
{
    public class ProdutoVenda : Android.Support.V4.App.Fragment, TextView.IOnEditorActionListener
    {
        private decimal quantidadeGiroProduto = 0;
        private bool IsLoading = false;
        private decimal quantidadeVendidoAnterior = 0;
        private static bool m_IsDirty = false;
        private static bool ZerarDesconto = true;
        private static bool VendaBungePermitida = true;
        private bool aceitaPontuacaoQtdeInteiro = false;
        static ProgressDialog progress;
        LayoutInflater thisLayoutInflater;
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        private bool m_Recalc_Desconto = false;
        private bool IgnorarEventoVlrUnitario = false;
        private bool IgnorarEvento = false;
        private decimal valorTabelaBunge = 0m;
        private decimal quantidadeMediaVendaProduto = 0;
        private static decimal saldoDesconto = 0;

        private static bool IsDirty
        {
            get
            {
                return m_IsDirty;
            }
            set
            {
                m_IsDirty = value;
            }
        }

        internal bool Recalc_Desconto
        {
            get
            {
                return m_Recalc_Desconto;
            }
            set
            {
                m_Recalc_Desconto = value;
            }
        }


        #region [ Controles ]
        //static TextView lblQ1;
        //static TextView lblQ2;
        //static TextView lblQ3;
        //static TextView lblMediaQ;
        //static TextView lblEstoqueAnterior;
        //static TextView lblEstoqueVenda;
        //static TextView lblEstoqueAtual;
        //static TextView lblEstoqueGiro;
        static TextView lblDescIncond;
        public static EditText txtQtdeInteiro;
        public static EditText txtQtdeUnidade;
        public static EditText txtDescIncond;
        static TextView lblValorTotalItem;
        //static EditText txtQtdeInteiroIndenizacao;
        //static EditText txtQtdeUnidadeIndenizacao;
        static TextView txtValorFinalItem;
        static Spinner cboTabelaPreco;
        static EditText txtValorUnitarioSemADF;
        public static TextView lblValorTabela;
        static TextView lblCodigoProduto;
        static TextView lblProduto;
        static TextView lblUM;
        static TextView lblOrganizVendas;
        public static TextView lblValorDescontoUnitario;
        public static TextView lblValorAdicionalFinanceiro;
        static TextView lblValorFinalItem;
        //static EditText txtValorFinalItemIndenizacao;
        static TextView lblPctLucratividade;
        static TextView lblSaldoEstoque;
        static TextView lblPz;
        static TextView lblTrib;
        static TextView lblValorUnitario;
        //static TextView lblInfoQ1;
        //static TextView lblInfoQ2;
        //static TextView lblInfoQ3;
        //static TextView lblInfoMedia;
        static TextView lblIndenizacao;
        //static TextView lblEstoque;
        //static TextView lblInfo1;
        //static TextView lblInfo2;
        //static TextView lblInfo3;
        //static TextView lblInfo4;
        static TextView lblValorUnitarioSemADF;
        static Button btnCalcular;
        static TextView lblOrganizVendasTit;
        static TextView lblDescPctLucratividade;
        //static TextView lblQtdeIndenizacaoInt;
        //static TextView lblQtdeIndenizacaoUnit;
        //static TextView lblValorUnitarioIndenizacao;
        //static Button btnGiro;
        //static Button btnMedia;
        static TextView lblDesc;
        static TextView lblCalcular;
        static TextView lblOrganizVendasBunge;
        static TextView lblOrganizVendasTitBunge;
        //static ImageView imvAbatimento;
        #endregion

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public bool OnEditorAction(TextView v, Android.Views.InputMethods.ImeAction actionId, KeyEvent e)
        {
            switch (v.Id)
            {
                case Resource.Id.txtQtdeInteiro:
                case Resource.Id.txtQtdeUnidade:
                    BtnCalcular_Click(null, null);
                    break;
                default:
                    break;
            }
            return false;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            //var view = inflater.Inflate(Resource.Layout.produto_venda, container, false);
            var view = inflater.Inflate(Resource.Layout.produto_venda_novo, container, false);
            ActivityContext = ((Cliente)Activity);
            FindViewsById(view);
            
            Eventos();
            thisLayoutInflater = inflater;

            ConfiguraTela();

            return view;
        }

        private void ConfiguraTela()
        {
            try
            {
                // Se nao for broker nao mostra os botoes de detalhes dos calculos
                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 1)
                {
                    lblValorFinalItem.Visibility = ViewStates.Gone;
                    txtValorFinalItem.Visibility = ViewStates.Visible;
                    //txtValorFinalItemIndenizacao.Visibility = ViewStates.Visible;
                    lblValorUnitarioSemADF.Visibility = ViewStates.Visible;
                    txtValorUnitarioSemADF.Visibility = ViewStates.Visible;
                    btnCalcular.Visibility = ViewStates.Gone;
                    lblOrganizVendasTit.Visibility = ViewStates.Gone;
                    lblOrganizVendas.Visibility = ViewStates.Gone;
                    lblDesc.Text = "R$ Desc.";

                }
                else
                {
                    lblValorFinalItem.Visibility = ViewStates.Visible;
                    txtValorFinalItem.Visibility = ViewStates.Gone;
                    //txtValorFinalItemIndenizacao.Visibility = ViewStates.Gone;
                    txtValorUnitarioSemADF.Visibility = ViewStates.Gone;
                    btnCalcular.Visibility = ViewStates.Visible;
                    lblOrganizVendasTit.Visibility = ViewStates.Visible;
                    lblOrganizVendas.Visibility = ViewStates.Visible;

                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                    {
                        lblValorUnitarioSemADF.Visibility = ViewStates.Invisible;
                        txtValorUnitarioSemADF.Visibility = ViewStates.Gone;
                        lblDesc.Text = "R$ Desc. Total";
                    }
                    else
                    {
                        lblOrganizVendas.Visibility = ViewStates.Gone;
                        lblOrganizVendasTit.Visibility = ViewStates.Gone;
                        lblOrganizVendasBunge.Visibility = ViewStates.Visible;
                        lblOrganizVendasTitBunge.Visibility = ViewStates.Visible;
                        lblValorUnitarioSemADF.Visibility = ViewStates.Visible;
                        txtValorUnitarioSemADF.Visibility = ViewStates.Visible;
                        lblCalcular.Visibility = ViewStates.Visible;
                        txtDescIncond.Visibility = ViewStates.Gone;
                        lblDescIncond.Visibility = ViewStates.Gone;
                        lblIndenizacao.Visibility = ViewStates.Gone;
                        //lblQtdeIndenizacaoInt.Visibility = ViewStates.Gone;
                        //lblQtdeIndenizacaoUnit.Visibility = ViewStates.Gone;
                        //lblValorUnitarioIndenizacao.Visibility = ViewStates.Gone;
                        //txtQtdeInteiroIndenizacao.Visibility = ViewStates.Gone;
                        //txtQtdeUnidadeIndenizacao.Visibility = ViewStates.Gone;
                        //txtValorFinalItemIndenizacao.Visibility = ViewStates.Gone;
                        lblIndenizacao.Visibility = ViewStates.Gone;
                    }
                }

                //Verifica tipo de calculo da lucratividade
                // 0 - Nao Calcular
                // 1 - Venda por Custo
                // 2 - Curto por Venda
                //Verifica se a empresa trabalha com % de lucratividade 
                if (CSEmpresa.Current.TIPO_CALCULO_LUCRATIVIDADE == 0 ||
                    CSEmpresa.Current.IND_VISUALIZA_LUCRATIVIDADE.Trim() == "N")
                {
                    lblDescPctLucratividade.Visibility = ViewStates.Gone;
                    lblPctLucratividade.Visibility = ViewStates.Gone;
                }

                // [ A guia de indenização sera indisponivel para: ]
                // [ Operação diferente de vendas;                 ]            
                // [ Quando Broker;                                ]
                // [ Quando o produto é um Combo;                  ]
                if ((CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO != 1 && CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO != 21) ||
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.LOCK_QTD == true ||
                    IsBroker() ||
                    CSGlobal.PedidoTroca)
                {
                    lblIndenizacao.Visibility = ViewStates.Gone;
                    //lblQtdeIndenizacaoInt.Visibility = ViewStates.Gone;
                    //lblQtdeIndenizacaoUnit.Visibility = ViewStates.Gone;
                    //lblValorUnitarioIndenizacao.Visibility = ViewStates.Gone;
                    //txtQtdeInteiroIndenizacao.Visibility = ViewStates.Gone;
                    //txtQtdeUnidadeIndenizacao.Visibility = ViewStates.Gone;
                    //txtValorFinalItemIndenizacao.Visibility = ViewStates.Gone;

                    if (IsBroker())
                    {
                        lblIndenizacao.Text = "Estoque";
                    }

                }
                else if (!IsBunge())
                {
                    lblIndenizacao.Visibility = ViewStates.Visible;
                    //lblQtdeIndenizacaoInt.Visibility = ViewStates.Visible;
                    //lblQtdeIndenizacaoUnit.Visibility = ViewStates.Visible;
                    //lblValorUnitarioIndenizacao.Visibility = ViewStates.Visible;
                    //txtQtdeInteiroIndenizacao.Visibility = ViewStates.Visible;
                    //txtQtdeUnidadeIndenizacao.Visibility = ViewStates.Visible;
                    //txtValorFinalItemIndenizacao.Visibility = ViewStates.Visible;
                    lblIndenizacao.Visibility = ViewStates.Visible;

                    lblIndenizacao.Text = "Abatimento";
                }
            }
            catch (System.Exception ex)
            {

            }
        }

        private void Eventos()
        {
            cboTabelaPreco.ItemSelected += CboTabelaPreco_ItemSelected;
            txtQtdeInteiro.TextChanged += TxtQtdeInteiro_TextChanged;
            txtQtdeUnidade.TextChanged += TxtQtdeUnidade_TextChanged;
            txtDescIncond.TextChanged += TxtDescIncond_TextChanged;
            txtValorUnitarioSemADF.TextChanged += TxtValorUnitarioSemADF_TextChanged;
            //txtValorFinalItemIndenizacao.TextChanged += TxtValorFinalItemIndenizacao_TextChanged;

            //if (txtQtdeInteiroIndenizacao.Visibility == ViewStates.Visible)
            //    txtQtdeInteiroIndenizacao.TextChanged += TxtQtdeInteiroIndenizacao_TextChanged;

            //if (txtQtdeUnidadeIndenizacao.Visibility == ViewStates.Visible)
            //    txtQtdeUnidadeIndenizacao.TextChanged += TxtQtdeUnidadeIndenizacao_TextChanged;

            btnCalcular.Click += BtnCalcular_Click;

            //if (!CSGlobal.PedidoSugerido)
            //{
            //    btnMedia.Click += BtnMedia_Click;
            //    btnGiro.Click += BtnGiro_Click;
            //}

            txtQtdeInteiro.SetOnEditorActionListener(this);
            txtQtdeUnidade.SetOnEditorActionListener(this);
        }

        private void BtnGiro_Click(object sender, EventArgs e)
        {
            try
            {
                if (quantidadeGiroProduto > 0)
                {
                    if (CSProdutos.Current.COD_UNIDADE_MEDIDA == "UN")
                    {
                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_TOTAL = System.Math.Round(quantidadeGiroProduto, MidpointRounding.AwayFromZero); ;

                        txtQtdeInteiro.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_INTEIRA.ToString();
                    }
                    else
                    {
                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_TOTAL = quantidadeGiroProduto;

                        txtQtdeInteiro.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_INTEIRA.ToString();
                        txtQtdeUnidade.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_UNIDADE.ToString();
                    }
                }
            }
            catch (System.Exception ex)
            {

            }
        }

        private void BtnMedia_Click(object sender, EventArgs e)
        {
            try
            {
                if (quantidadeMediaVendaProduto > 0)
                {
                    if (CSProdutos.Current.COD_UNIDADE_MEDIDA == "UN")
                    {
                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_TOTAL = System.Math.Round(quantidadeMediaVendaProduto, MidpointRounding.AwayFromZero); ;

                        txtQtdeInteiro.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_INTEIRA.ToString();
                    }
                    else
                    {
                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_TOTAL = quantidadeMediaVendaProduto;

                        txtQtdeInteiro.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_INTEIRA.ToString();
                        txtQtdeUnidade.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_UNIDADE.ToString();
                    }
                }
            }
            catch (System.Exception ex)
            {

            }
        }

        private void TxtQtdeUnidadeIndenizacao_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (CSProdutos.Current.COD_UNIDADE_MEDIDA == "UN")
                {
                    MessageBox.ShowShortMessageCenter(Activity, "Este produto só pode ser indenizado inteiro.");

                    //if (txtQtdeUnidadeIndenizacao.Text.Length == 1)
                    //    txtQtdeUnidadeIndenizacao.Text = string.Empty;

                    return;
                }

                if (IgnorarEvento)
                    return;

                // Marca que foi alterado
                IsDirty = true;

                // Marca que o objeto foi alterado e deve ser salvo durante o flush
                if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.INALTERADO || CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.SALVO)
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE = ObjectState.ALTERADO;

                // nao deixa que entre em um loop de eventos ao se modificar...
                IgnorarEvento = true;

                // [ Se não for broker e bunge... ]
                if ((CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2 &&
                    CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 3) ||
                    CSGlobal.PedidoSugerido)
                {
                    //if (!IsLoading)
                    //{
                    //    // calcula e mostra o valor total      
                    //    txtValorFinalItemIndenizacao.Text = CalculaValorTotalItemIndenizacao().ToString(CSGlobal.DecimalStringFormat);
                    //}
                }

                IgnorarEvento = false;
            }
            catch (System.Exception ex)
            {

            }
        }

        private static decimal CalculaValorTotalItemIndenizacao()
        {
            decimal valorTotalItem = 0;
            decimal valorTotalUnitario = 0;
            decimal valorFinalItem = 0;

            try
            {
                //if (DesconsiderarDesconto)
                //{
                valorFinalItem = CSGlobal.StrToDecimal(txtValorFinalItem.Text);
                //}
                //else
                //{
                //    valorFinalItem = CSGlobal.StrToDecimal(lblValorTabela.Text);
                //}

                valorTotalUnitario = decimal.Round(valorFinalItem / CSProdutos.Current.QTD_UNIDADE_EMBALAGEM, 4);

                //// Calculo dos produtos com a caixa fechada
                //valorTotalItem = valorFinalItem * CSGlobal.StrToDecimal(txtQtdeInteiroIndenizacao.Text);

                //// Calculo dos produtos com a caixa aberta
                //valorTotalItem += (valorTotalUnitario * CSGlobal.StrToDecimal(txtQtdeUnidadeIndenizacao.Text));

            }
            catch (System.Exception ex)
            {
                MessageBox.AlertErro(ActivityContext, ex.Message);
            }

            return valorTotalItem;
        }

        private void TxtQtdeInteiroIndenizacao_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (IgnorarEvento)
                    return;

                // Marca que foi alterado
                IsDirty = true;

                // Marca que o objeto foi alterado e deve ser salvo durante o flush
                if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.INALTERADO || CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.SALVO)
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE = ObjectState.ALTERADO;

                // nao deixa que entre em um loop de eventos ao se modificar...
                IgnorarEvento = true;

                // [ Se não for broker e bunge... ]
                //if ((CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2 &&
                //     CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 3) ||
                //    CSGlobal.PedidoSugerido)
                //{
                //    //if (!IsLoading)
                //    //{
                //    // calcula e mostra o valor total      
                //    txtValorFinalItemIndenizacao.Text = CalculaValorTotalItemIndenizacao().ToString(CSGlobal.DecimalStringFormat);
                //    //}
                //}

                IgnorarEvento = false;
            }
            catch (System.Exception ex)
            {

            }
        }

        private void TxtValorFinalItemIndenizacao_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (!ValidaFormatacaoNumerica())
                    return;

                //if (!string.IsNullOrEmpty(txtValorFinalItemIndenizacao.Text))
                //    if (txtValorFinalItemIndenizacao.Text.Contains("."))
                //    {
                //        txtValorFinalItemIndenizacao.Text = txtValorFinalItemIndenizacao.Text.Replace(".", ",");
                //        txtValorFinalItemIndenizacao.SetSelection(txtValorFinalItemIndenizacao.Text.Length);
                //    }

                //if (txtValorFinalItemIndenizacao.Text.Contains(','))
                //{
                //    int posicao = txtValorFinalItemIndenizacao.Text.IndexOf(',');

                //    if (txtValorFinalItemIndenizacao.Text.Substring(posicao + 1, txtValorFinalItemIndenizacao.Text.Length - posicao - 1).Length > 2)
                //    {
                //        txtValorFinalItemIndenizacao.Text = txtValorFinalItemIndenizacao.Text.Remove(txtValorFinalItemIndenizacao.Text.Length - 1);
                //        txtValorFinalItemIndenizacao.SetSelection(txtValorFinalItemIndenizacao.Text.Length);
                //        return;
                //    }
                //}

                if (IgnorarEvento)
                    return;

                // Marca que foi alterado
                IsDirty = true;

                // Marca que o objeto foi alterado e deve ser salvo durante o flush
                if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.INALTERADO || CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.SALVO)
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE = ObjectState.ALTERADO;

                // nao deixa que entre em um loop de eventos ao se modificar...
                IgnorarEvento = true;

                // [ Se não for broker e bunge... ]
                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2 &&
                    CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 3)
                {
                    if (!IsLoading)
                    {
                        // calcula e mostra o valor total                      
                        //lblValorTotalItemIndenizacao.Text = CalculaValorTotalItemIndenizacao().ToString(CSGlobal.DecimalStringFormat);

                    }
                }

                IgnorarEvento = false;
            }
            catch (OverflowException)
            {
                MessageBox.AlertErro(Activity, "Número de caracteres máximo atingido.");
            }
        }

        private void TxtValorUnitarioSemADF_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (!ValidaFormatacaoNumerica())
                    return;

                if (IgnorarEvento)
                    return;

                if (!string.IsNullOrEmpty(txtValorUnitarioSemADF.Text))
                    if (txtValorUnitarioSemADF.Text.Contains("."))
                    {
                        txtValorUnitarioSemADF.Text = txtValorUnitarioSemADF.Text.Replace(".", ",");
                        txtValorUnitarioSemADF.SetSelection(txtValorUnitarioSemADF.Text.Length);
                    }

                if (IsBunge())
                {
                    if (IgnorarEvento)
                        return;

                    if (IsBunge() &&
                        ZerarDesconto &&
                        txtDescIncond.Text != string.Empty)
                    {
                        IgnorarEventoVlrUnitario = true;
                        txtDescIncond.Text = string.Empty;
                        IgnorarEventoVlrUnitario = false;
                    }
                }

                if (!IsBunge())
                {
                    if (txtValorUnitarioSemADF.Text != string.Empty &&
                        lblValorTabela.Text != string.Empty &&
                        CSGlobal.StrToDecimal(txtValorUnitarioSemADF.Text) >
                        CSGlobal.StrToDecimal(lblValorTabela.Text))
                    {
                        MessageBox.ShowShortMessageBottom(Activity, "Valor unitário maior que o valor de tabela");
                        txtValorUnitarioSemADF.Text = txtValorUnitarioSemADF.Text.Remove(txtValorUnitarioSemADF.Text.Length - 1);
                        txtValorUnitarioSemADF.SetSelection(txtValorUnitarioSemADF.Text.Length);
                        return;
                    }

                    if (IgnorarEvento)
                        return;

                    MarcaQueOObjetoFoiAlteradoEDeveSerSalvoDuranteOFlush();

                    IgnorarEvento = true;
                }

                if (!IsBroker() &&
                    !IsBunge())
                {
                    CalculaDescontoIncond();
                    //CalcularDesconto = false;

                    //if (!CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.CALCULO_DESCONTO ||
                    //    !ExecutarRegraCalculoDesconto)
                    //{
                    //    DesconsiderarDesconto = true;
                    //    CSGlobal.Vlr_Desconto = false;
                    //}
                    //else
                    //{
                    //    DesconsiderarDesconto = false;
                    //    CSGlobal.Vlr_Desconto = true;
                    //}
                    RecalculaValorAdicionalFinanceiro();
                    CalculoValorDoDesconto();
                    RecalculaValorFinalDoItemPraLabel();
                    RecalculaValorFinalDoItemPraTextBox();
                    CalculaEMostraValorTotal();
                    //DesconsiderarDesconto = true;
                    //CSGlobal.Vlr_Desconto = false;
                }
                IgnorarEvento = false;
            }
            catch (OverflowException)
            {
                MessageBox.AlertErro(Activity, "Número de caracteres máximo atingido.");
                txtValorUnitarioSemADF.Text = string.Empty;
            }
        }

        private void TxtDescIncond_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (!ValidaFormatacaoNumerica())
                    return;

                if (IgnorarEvento)
                    return;

                if (!string.IsNullOrEmpty(txtDescIncond.Text))
                {
                    if (CSGlobal.StrToDecimal(txtDescIncond.Text) >= 100)
                    {
                        txtDescIncond.Text.Remove(txtDescIncond.Text.Length - 1, 1);
                        txtDescIncond.SetSelection(txtDescIncond.Text.Length);
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(txtDescIncond.Text))
                    if (txtDescIncond.Text.Contains("."))
                    {
                        txtDescIncond.Text = txtDescIncond.Text.Replace(".", ",");
                        txtDescIncond.SetSelection(txtDescIncond.Text.Length);
                    }

                MarcaQueOObjetoFoiAlteradoEDeveSerSalvoDuranteOFlush();

                IgnorarEvento = true;

                if (IsBunge())
                {
                    if (!IgnorarEventoVlrUnitario)
                    {
                        if (valorTabelaBunge != 0 &&
                            IsDirty)
                        {
                            txtValorUnitarioSemADF.Text = valorTabelaBunge.ToString(CSGlobal.DecimalStringFormat);
                            valorTabelaBunge = CSGlobal.StrToDecimal(lblValorTabela.Text);
                        }
                    }
                    else
                        IgnorarEventoVlrUnitario = false;
                }

                //if (txtDescIncond.Text != "a")
                //{
                decimal valorTabela = CSGlobal.StrToDecimal(lblValorTabela.Text);
                decimal percentualDesconto = CSGlobal.StrToDecimal(txtDescIncond.Text);
                decimal result = 0;

                // calcula o novo valor com desconto
                result = System.Math.Round((valorTabela * (CSGlobal.StrToDecimal(txtDescIncond.Text) / 100)), 2, MidpointRounding.AwayFromZero);
                //if (!CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.CALCULO_DESCONTO &&
                //    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE != ObjectState.NOVO)

                //if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE != ObjectState.NOVO)
                //{
                //txtValorUnitarioSemADF.Text = (CSProdutos.Current.PRECOS_PRODUTO.Current.VLR_PRODUTO - CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_DESCONTO_UNITARIO).ToString(CSGlobal.DecimalStringFormat);
                //}
                //else
                //{
                if (!IsBunge())
                    txtValorUnitarioSemADF.Text = (CSProdutos.Current.PRECOS_PRODUTO.Current.VLR_PRODUTO - result).ToString(CSGlobal.DecimalStringFormat);
                //ExecutarRegraCalculoDesconto = false;
                //DesconsiderarDesconto = false;
                //CSGlobal.Vlr_Desconto = true;
                //}
                //}
                //else
                //{
                //    //if (txtDescIncond.Text == string.Empty)
                //    //{
                //    txtValorUnitarioSemADF.Text = (CSProdutos.Current.PRECOS_PRODUTO.Current.VLR_PRODUTO - CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_DESCONTO_UNITARIO).ToString(CSGlobal.DecimalStringFormat);
                //    //}
                //    //else
                //    //    txtValorUnitarioSemADF.Text = lblValorTabela.Text;
                //}
                if (!IsBroker() &&
                    !IsBunge())
                {
                    RecalculaValorAdicionalFinanceiro();
                    CalculoValorDoDesconto();
                    RecalculaValorFinalDoItemPraLabel();
                    RecalculaValorFinalDoItemPraTextBox();
                    CalculaEMostraValorTotal();
                }
                RecalculaValoresPadrao();
                //DesconsiderarDesconto = false;
                //CSGlobal.Vlr_Desconto = true;
                IgnorarEvento = false;
            }
            catch (OverflowException)
            {
                MessageBox.AlertErro(Activity, "Número de caracteres máximo atingido.");
                txtDescIncond.Text = string.Empty;
            }
        }

        private void TxtQtdeUnidade_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (IgnorarEvento)
                    return;

                if (CSProdutos.Current.COD_UNIDADE_MEDIDA == "UN")
                {
                    MessageBox.ShowShortMessageCenter(Activity, "Este produto só pode ser vendido inteiro.");

                    if (txtQtdeUnidade.Text.Length == 1)
                        txtQtdeUnidade.Text = string.Empty;

                    IgnorarEvento = false;
                    return;
                }

                MarcaQueOObjetoFoiAlteradoEDeveSerSalvoDuranteOFlush();

                IgnorarEvento = true;

                //if (!ValidaSeQuantidadeDiferenteDeZero(txtQtdeUnidade))
                //{
                //    IgnorarEvento = false;
                //    return;
                //}

                if (!IsBunge())
                {
                    // Busca o valor de tabela do PDA
                    decimal valorTabela = ((CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto)((CSItemCombo)cboTabelaPreco.SelectedItem).Valor).VLR_PRODUTO;

                    if (CSGlobal.StrToInt(txtQtdeUnidade.Text) == 0)
                    {
                        decimal valorDesconto = valorTabela * (CSGlobal.StrToDecimal(txtDescIncond.Text) / 100);
                        lblValorTabela.Text = valorTabela.ToString(CSGlobal.DecimalStringFormat);
                        txtValorUnitarioSemADF.Text = (valorTabela - valorDesconto).ToString(CSGlobal.DecimalStringFormat);

                    }
                    else
                    {
                        decimal valorUnitario = valorTabela;
                        decimal valorAcrescimoUnitario = 0;

                        // Faz o calculo do acrescimo para o valor unitario
                        valorAcrescimoUnitario = ((valorUnitario * CSProdutos.Current.PRC_ACRESCIMO_QTDE_UNITARIA) / 100);

                        // muda o valor de tabela acrescentando o valor de acrescimo por te aberto a caixa
                        lblValorTabela.Text = (valorTabela + valorAcrescimoUnitario).ToString(CSGlobal.DecimalStringFormat);

                        IgnorarEvento = true;

                        //Se o valor Vlr Unit. s/ADF: nao mudou nao precisa refazer os calculos
                        if (txtValorUnitarioSemADF.Text != (valorTabela + valorAcrescimoUnitario).ToString(CSGlobal.DecimalStringFormat))
                        {
                            valorUnitario = (valorTabela + valorAcrescimoUnitario);

                            //calcula o valor unitario abatendo o desconto para nao zerar o % de desconto
                            //quando tiver que calcular o valor de acrescimo para quantidade unitaria
                            valorUnitario = valorUnitario * (CSGlobal.StrToDecimal(txtDescIncond.Text) / 100);
                            txtValorUnitarioSemADF.Text = (valorTabela + valorAcrescimoUnitario - valorUnitario).ToString(CSGlobal.DecimalStringFormat);
                        }
                    }
                }
                if (!IsBroker() &&
                    !IsBunge())
                {
                    //CalculaDescontoIncond();
                    RecalculaValorAdicionalFinanceiro();
                    CalculoValorDoDesconto();
                    RecalculaValorFinalDoItemPraLabel();
                    RecalculaValorFinalDoItemPraTextBox();
                    CalculaEMostraValorTotal();
                }
                RecalculaValoresPadrao();
                IgnorarEvento = false;
            }
            catch (OverflowException)
            {
                MessageBox.AlertErro(Activity, "Número de caracteres máximo atingido.");
                txtQtdeUnidade.Text = string.Empty;
            }
            catch (System.Exception ex)
            {
                MessageBox.AlertErro(Activity, ex.Message);
            }
        }

        public static void BtnCalcular_Click(object sender, EventArgs e)
        {
            try
            {
                var precoProduto = (CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto)((CSItemCombo)cboTabelaPreco.SelectedItem).Valor;
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.COD_TABELA_PRECO = precoProduto.COD_TABELA_PRECO;

                if (IsBroker())
                {
                    bool cancelaEvento = false;

                    if (CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO == 20)
                    {
                        if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                            CSPDVs.Current.POLITICA_BROKER_2014.INDENIZACAO = true;
                        else
                            CSPDVs.Current.POLITICA_BROKER.INDENIZACAO = true;
                    }
                    else
                    {
                        if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                            CSPDVs.Current.POLITICA_BROKER_2014.INDENIZACAO = false;
                        else
                            CSPDVs.Current.POLITICA_BROKER.INDENIZACAO = false;
                    }

                    using (CSItemsPedido.CSItemPedido item = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current)
                    {
                        item.PRC_DESCONTO = CSGlobal.StrToDecimal(txtDescIncond.Text);
                        item.QTD_PEDIDA_INTEIRA = CSGlobal.StrToInt(txtQtdeInteiro.Text);
                        item.QTD_PEDIDA_UNIDADE = CSGlobal.StrToInt(txtQtdeUnidade.Text);

                        if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                            item.CalculaValor2014();
                        else
                            item.CalculaValor();

                        //ValidaPercentualDescontoBroker();
                        if (CSGlobal.StrToDecimal(txtDescIncond.Text) > item.PRC_DESCONTO_MAXIMO && CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO != 20 /*&& CSGlobal.CalculaPrecoNestle*/)
                        {
                            if (sender != null)
                                MessageBox.ShowShortMessageCenter(ActivityContext, "O percentual de desconto não pode ser maior que \"" + item.PRC_DESCONTO_MAXIMO.ToString(CSGlobal.DecimalStringFormat) + "\".");

                            txtDescIncond.RequestFocus();
                            cancelaEvento = true;
                        }

                        if (!cancelaEvento)
                        {
                            // [ atualiza o valor total do item ]
                            lblValorTotalItem.Text = item.VLR_TOTAL_ITEM.ToString(CSGlobal.DecimalStringFormat);
                            lblValorUnitario.Text = item.VLR_ITEM_UNIDADE.ToString(CSGlobal.DecimalStringFormat);
                            lblValorFinalItem.Text = item.VLR_ITEM_INTEIRA.ToString(CSGlobal.DecimalStringFormat);
                            txtValorFinalItem.Text = item.VLR_ITEM_INTEIRA.ToString(CSGlobal.DecimalStringFormat);
                            lblValorDescontoUnitario.Text = item.PRC_DESCONTO_UNITARIO.ToString(CSGlobal.DecimalStringFormat);
                        }
                    }
                }
                else if (IsBunge())
                {
                    ZerarDesconto = false;

                    using (CSItemsPedido.CSItemPedido item = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current)
                    {
                        if (item.VLR_ITEM_UNIDADE != 0 &&
                            txtValorUnitarioSemADF.Text == string.Empty)
                            txtValorUnitarioSemADF.Text = item.VLR_ITEM_UNIDADE.ToString(CSGlobal.DecimalStringFormat);

                        item.PRC_DESCONTO = CSGlobal.StrToDecimal(txtDescIncond.Text);
                        item.QTD_PEDIDA_INTEIRA = CSGlobal.StrToInt(txtQtdeInteiro.Text);
                        item.QTD_PEDIDA_UNIDADE = CSGlobal.StrToInt(txtQtdeUnidade.Text);
                        item.CalcularValorBunge(CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO.COD_PRODUTO, CSEmpresa.Current.COD_NOTEBOOK1, CSPDVs.Current.COD_PDV, DateTime.Now, CSProdutos.Current, CSGlobal.StrToInt(txtQtdeInteiro.Text), CSGlobal.StrToInt(txtQtdeUnidade.Text), CSGlobal.StrToDecimal(txtDescIncond.Text), CSGlobal.StrToDecimal(txtValorUnitarioSemADF.Text));

                        lblValorTotalItem.Text = item.VLR_TOTAL_ITEM.ToString(CSGlobal.DecimalStringFormat);
                        lblValorUnitario.Text = item.VLR_ITEM_UNIDADE.ToString(CSGlobal.DecimalStringFormat);
                        txtValorUnitarioSemADF.Text = item.VLR_ITEM_UNIDADE.ToString(CSGlobal.DecimalStringFormat);
                        lblValorFinalItem.Text = item.VLR_ITEM_INTEIRA.ToString(CSGlobal.DecimalStringFormat);
                        txtValorFinalItem.Text = item.VLR_ITEM_INTEIRA.ToString(CSGlobal.DecimalStringFormat);
                        lblValorDescontoUnitario.Text = item.PRC_DESCONTO_UNITARIO.ToString(CSGlobal.DecimalStringFormat);

                        if (string.IsNullOrEmpty(lblValorTabela.Text))
                            lblValorTabela.Text = item.VLR_TABELA_BUNGE.ToString(CSGlobal.DecimalStringFormat);

                        ZerarDesconto = true;
                        VendaBungePermitida = true;
                    }
                }
            }
            catch (OverflowException)
            {
                MessageBox.AlertErro(ActivityContext, "Número de caracteres máximo atingido");
                txtQtdeInteiro.Text = string.Empty;
                txtQtdeUnidade.Text = string.Empty;
            }
            catch (System.Exception ex)
            {
                VendaBungePermitida = false;
                MessageBox.AlertErro(ActivityContext, ex.Message);
            }
        }

        private void MarcaQueOObjetoFoiAlteradoEDeveSerSalvoDuranteOFlush()
        {
            try
            {
                IsDirty = true;
                // Marca que o objeto foi alterado e deve ser salvo durante o flush
                if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.INALTERADO || CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.SALVO)
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE = ObjectState.ALTERADO;
            }
            catch (System.Exception ex)
            {

            }
        }

        private void TxtQtdeInteiro_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            try
            {
                if (aceitaPontuacaoQtdeInteiro)
                {
                    if (!string.IsNullOrEmpty(txtQtdeInteiro.Text))
                        if (txtQtdeInteiro.Text.Contains("."))
                        {
                            txtQtdeInteiro.Text = txtQtdeInteiro.Text.Replace(".", ",");
                            txtQtdeInteiro.SetSelection(txtQtdeInteiro.Text.Length);
                        }

                    if (txtQtdeInteiro.Text.Contains(","))
                    {
                        int posicao = txtQtdeInteiro.Text.IndexOf(',');

                        if (txtQtdeInteiro.Text.Substring(posicao + 1, txtQtdeInteiro.Text.Length - posicao - 1).Length > 2)
                        {
                            txtQtdeInteiro.Text = txtQtdeInteiro.Text.Remove(txtQtdeInteiro.Text.Length - 1);
                            txtQtdeInteiro.SetSelection(txtQtdeInteiro.Text.Length);
                        }

                        else
                        {
                            if (!ValidaFormatacaoNumerica())
                                return;
                        }
                    }
                    else
                    {
                        if (!ValidaFormatacaoNumerica())
                            return;
                    }
                }
                else
                {
                    if (txtQtdeInteiro.Text != string.Empty &&
                        txtQtdeInteiro.Text.Contains(","))
                    {
                        txtQtdeInteiro.Text = txtQtdeInteiro.Text.Remove(txtQtdeInteiro.Text.Length - 1);
                        txtQtdeInteiro.SetSelection(txtQtdeInteiro.Text.Length);
                    }
                    else if (txtQtdeInteiro.Text != string.Empty &&
                             txtQtdeInteiro.Text.Contains("."))
                    {
                        txtQtdeInteiro.Text = txtQtdeInteiro.Text.Remove(txtQtdeInteiro.Text.Length - 1);
                        txtQtdeInteiro.SetSelection(txtQtdeInteiro.Text.Length);
                    }
                }

                if (IgnorarEvento)
                    return;

                MarcaQueOObjetoFoiAlteradoEDeveSerSalvoDuranteOFlush();

                IgnorarEvento = true;

                if (!IsBunge())
                {
                    decimal valorTabela = ((CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto)((CSItemCombo)cboTabelaPreco.SelectedItem).Valor).VLR_PRODUTO;

                    if (CSGlobal.StrToInt(txtQtdeUnidade.Text) == 0)
                    {
                        lblValorTabela.Text = valorTabela.ToString(CSGlobal.DecimalStringFormat);

                        valorTabela = CSGlobal.StrToDecimal(txtValorFinalItem.Text);
                    }
                    else
                    {
                        decimal valorUnitario = valorTabela;
                        decimal valorAcrescimoUnitario = 0;
                        decimal desconto = 0m;

                        valorAcrescimoUnitario = ((valorUnitario * CSProdutos.Current.PRC_ACRESCIMO_QTDE_UNITARIA) / 100);

                        lblValorTabela.Text = (valorTabela + valorAcrescimoUnitario).ToString(CSGlobal.DecimalStringFormat);

                        IgnorarEvento = true;

                        if (txtValorUnitarioSemADF.Text != (valorTabela + valorAcrescimoUnitario).ToString(CSGlobal.DecimalStringFormat))
                        {
                            valorTabela = CSGlobal.StrToDecimal(txtValorFinalItem.Text);
                            desconto = 0m;

                            valorUnitario = (valorTabela + valorAcrescimoUnitario);

                            valorUnitario = valorUnitario * (desconto / 100);
                        }
                    }
                }
                if (!IsBroker() &&
                    !IsBunge())
                {
                    RecalculaValorAdicionalFinanceiro();
                    CalculoValorDoDesconto();
                    RecalculaValorFinalDoItemPraLabel();
                    RecalculaValorFinalDoItemPraTextBox();
                    CalculaEMostraValorTotal();
                }
                RecalculaValoresPadrao();
                IgnorarEvento = false;
            }
            catch (OverflowException)
            {
                MessageBox.AlertErro(Activity, "Número de caracteres máximo atingido.");
                txtQtdeInteiro.Text = string.Empty;
            }
            catch (System.Exception ex)
            {
                MessageBox.AlertErro(Activity, ex.Message);
            }
        }

        private static void SetarValoresDaClasseParaValidacaoDesconto()
        {
            try
            {
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_ITEM_UNIDADE = CSGlobal.StrToDecimal(txtValorUnitarioSemADF.Text);
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_INTEIRA = CSGlobal.StrToDecimal(txtQtdeInteiro.Text);
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_UNIDADE = CSGlobal.StrToInt(txtQtdeUnidade.Text);
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_TOTAL_ITEM = CSGlobal.StrToDecimal(lblValorTotalItem.Text);
            }
            catch (System.Exception ex)
            {

            }
        }

        public static void ValidacoesVenda(bool proximoPasso, bool edicaoProduto)
        {
            try
            {
                string alerta = string.Empty;

                if (IsDirty)
                {
                    SetarValoresDaClasseParaValidacaoDesconto();

                    BtnCalcular_Click(null, null);

                    if (IsBunge())
                    {
                        if (!VendaBungePermitida)
                            return;
                    }

                    if (ValidaQuantidade())
                    {
                        //Se o usuário fez alguma alteração salva a tela e retorna, sem perguntar.                    
                        SalvarAlteracoes(proximoPasso, edicaoProduto);
                    }
                    else
                    {
                        if (((Cliente)ActivityContext).EdicaoProduto)
                        {
                            ((Cliente)ActivityContext).FinalizarEdicao(edicaoProduto);
                        }
                        else
                        {
                            if (((Cliente)ActivityContext).RotinaProdutosIndicados)
                            {
                                ((Cliente)ActivityContext).VoltarProdutosIndicados();
                            }
                            else
                            {
                                if (proximoPasso)
                                {
                                    ((Cliente)ActivityContext).PassoAtual++;

                                    ((Cliente)ActivityContext).AlterarFragment(true, false);
                                }
                                else
                                    ((Cliente)ActivityContext).VoltarPressionado();
                            }
                        }
                    }
                }
                //}
            }
            catch (System.Exception ex)
            {

            }
        }

        private static bool ValidaValorMaximoDigitadoNaoBroker()
        {
            try
            {
                // Valida ADF antes de salvar
                // Busca o valor de tabela do PDA
                decimal vlrTabela = CSGlobal.StrToDecimal(lblValorTabela.Text);
                // Busca a % do ADF			
                decimal vlrPRCFinanceiro = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_ADICIONAL_FINANCEIRO;
                decimal valorPermitido = (vlrTabela + ((vlrTabela * vlrPRCFinanceiro) / 100));
                string recebeValorDigitado = "0";

                // Atualiza codigo da tabela de preço
                // [ Preenche o codigo da tabela escolhido ]
                var precoProduto = (CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto)((CSItemCombo)cboTabelaPreco.SelectedItem).Valor;
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.COD_TABELA_PRECO = precoProduto.COD_TABELA_PRECO;

                if (txtValorFinalItem.Text != "")
                    recebeValorDigitado = txtValorFinalItem.Text;

                if (decimal.Parse(recebeValorDigitado) > decimal.Parse(valorPermitido.ToString(CSGlobal.DecimalStringFormat)) && IsBroker())
                {
                    valorPermitido = decimal.Round(valorPermitido, 2);
                    MessageBox.ShowShortMessageCenter(ActivityContext, "Valor digitado não pode ser maior que: " + valorPermitido + " ");
                    return false;
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaPercentualDescontoBroker()
        {
            try
            {
                if (CSGlobal.StrToDecimal(txtDescIncond.Text) > CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_DESCONTO_MAXIMO && CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO != 20)
                {
                    MessageBox.ShowShortMessageCenter(ActivityContext, "O percentual de desconto não pode ser maior que \"" + CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_DESCONTO_MAXIMO.ToString(CSGlobal.DecimalStringFormat) + "\".");
                    txtDescIncond.RequestFocus();
                    return false;
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaSeQuantidadeFoiInformada()
        {
            try
            {
                if (!ValidaQuantidade())
                {
                    MessageBox.ShowShortMessageCenter(ActivityContext, "Informe a quantidade.");
                    txtQtdeInteiro.RequestFocus();
                    return false;
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaEstoqueProntaEntrega()
        {
            try
            {
                if (CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.IND_PRONTA_ENTREGA)
                {
                    if ((CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO.QTD_ESTOQUE_PRONTA_ENTREGA - GetQtdPedidaUnidade()) < 0)
                    {
                        MessageBox.ShowShortMessageCenter(ActivityContext, "Saldo insuficiente.");
                        txtQtdeInteiro.RequestFocus();
                        return false;
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static decimal GetQtdPedidaUnidadeIndenizacao()
        {
            try
            {
                decimal Qtd = 0;

                //switch (CSProdutos.Current.COD_UNIDADE_MEDIDA)
                //{
                //    case "CX":
                //    case "DZ":
                //        Qtd = (CSGlobal.StrToInt(txtQtdeInteiroIndenizacao.Text) * CSProdutos.Current.QTD_UNIDADE_EMBALAGEM) + CSGlobal.StrToInt(txtQtdeUnidadeIndenizacao.Text);
                //        break;
                //    default:
                //        Qtd = CSGlobal.StrToDecimal(txtQtdeInteiroIndenizacao.Text) + CSGlobal.StrToInt(txtQtdeUnidadeIndenizacao.Text);
                //        break;
                //}

                return Qtd;
            }
            catch (System.Exception ex)
            {
                return 0;
            }
        }

        private static bool ValidaQuantidadeAbatimento()
        {
            try
            {
                if (GetQtdPedidaUnidadeIndenizacao() > GetQtdPedidaUnidade() && CSEmpresa.Current.IND_INDENIZACAO_MAIORVENDA == false)
                {
                    MessageBox.ShowShortMessageCenter(ActivityContext, "A quantidade do abatimento não pode ser maior que a quantidade do item do pedido.");
                    txtQtdeInteiro.RequestFocus();
                    return false;
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaQuantidadeUnidadeIndenizacao()
        {
            try
            {
                //if (CSGlobal.StrToInt(txtQtdeUnidadeIndenizacao.Text) != 0)
                //{
                //    if (CSProdutos.Current.COD_UNIDADE_MEDIDA == "CX" || CSProdutos.Current.COD_UNIDADE_MEDIDA == "DZ")
                //    {
                //        if (CSGlobal.StrToInt(txtQtdeUnidadeIndenizacao.Text) >= CSProdutos.Current.QTD_UNIDADE_EMBALAGEM)
                //        {
                //            MessageBox.ShowShortMessageCenter(ActivityContext, "A quantidade fracionaria não pode ser maior ou igual a: " + CSProdutos.Current.QTD_UNIDADE_EMBALAGEM.ToString());
                //            txtQtdeUnidadeIndenizacao.RequestFocus();
                //            return false;
                //        }
                //    }
                //    else
                //    {
                //        MessageBox.ShowShortMessageCenter(ActivityContext, "Este produto só pode ser abatido inteiro.");
                //        txtQtdeUnidade.RequestFocus();
                //        return false;
                //    }
                //}
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaQuantidadeUnitatariaMenorQuePermitido()
        {
            try
            {
                if (CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO == 1 || CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO == 21 || (CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO != 1 && CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO != 21 && CSEmpresa.Current.IND_VALIDA_QTDMULTIPLA_NAO_VENDA == "S"))
                    if (GetQtdPedidaUnidade() < CSProdutos.Current.PRECOS_PRODUTO.Current.QTD_MINIMA_PEDIDA)
                    {
                        decimal qtdMinima = CSProdutos.Current.PRECOS_PRODUTO.Current.QTD_MINIMA_PEDIDA;
                        string quantidade = string.Empty;

                        if (CSProdutos.Current.COD_UNIDADE_MEDIDA == "CX" ||
                            CSProdutos.Current.COD_UNIDADE_MEDIDA == "DZ")
                        {
                            var unidade = qtdMinima % CSProdutos.Current.QTD_UNIDADE_EMBALAGEM;
                            var inteiro = Convert.ToInt32((qtdMinima - unidade) / CSProdutos.Current.QTD_UNIDADE_EMBALAGEM);

                            quantidade = string.Format("{0}/{1}", inteiro, unidade);
                        }
                        else
                            quantidade = qtdMinima.ToString();

                        MessageBox.ShowShortMessageCenter(ActivityContext, "A quantidade não pode ser menor que: " + quantidade);
                        txtQtdeInteiro.RequestFocus();
                        return false;
                    }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaQuantidadeUnitariaMaiorQuePermitido()
        {
            try
            {
                if (GetQtdPedidaUnidade() > CSProdutos.Current.PRECOS_PRODUTO.Current.QTD_MAXIMA_PEDIDA)
                {
                    MessageBox.ShowShortMessageCenter(ActivityContext, "A quantidade não pode ser maior que: " + CSProdutos.CSProduto.ConverteUnidadesParaMedida(CSProdutos.Current.PRECOS_PRODUTO.Current.QTD_MAXIMA_PEDIDA, CSProdutos.Current.COD_UNIDADE_MEDIDA, CSProdutos.Current.QTD_UNIDADE_EMBALAGEM).ToString());
                    txtQtdeInteiro.RequestFocus();
                    return false;
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaQuantidadeInteiraEUnitaria()
        {
            try
            {
                if (CSGlobal.StrToDecimal(txtQtdeInteiro.Text) != 0 || CSGlobal.StrToInt(txtQtdeUnidade.Text) != 0)
                {
                    // verifica se é necessário validar as quantidades
                    if (CSProdutos.Current.PRECOS_PRODUTO.Current.QTD_MINIMA_PEDIDA != 0 || CSProdutos.Current.PRECOS_PRODUTO.Current.QTD_MAXIMA_PEDIDA != 0)
                    {
                        // Verifica se o valor informado é menor que o permitido
                        if (!ValidaQuantidadeUnitatariaMenorQuePermitido())
                            return false;

                        // Verifica se o valor informado é maior que o permitido
                        if (!ValidaQuantidadeUnitariaMaiorQuePermitido())
                            return false;
                    }

                    /* Validação sobre a quantidade*/
                    if (CSGlobal.StrToInt(txtQtdeUnidade.Text) > 0)
                    {
                        if (CSProdutos.Current.COD_UNIDADE_MEDIDA == "CX" || CSProdutos.Current.COD_UNIDADE_MEDIDA == "DZ")
                        {
                            if (CSGlobal.StrToInt(txtQtdeUnidade.Text) >= CSProdutos.Current.QTD_UNIDADE_EMBALAGEM)
                            {
                                MessageBox.ShowShortMessageCenter(ActivityContext, "A quantidade fracionaria não pode ser maior ou igual a: " + CSProdutos.Current.QTD_UNIDADE_EMBALAGEM.ToString());
                                txtQtdeUnidade.RequestFocus();
                                return false;
                            }
                        }
                        else
                        {
                            MessageBox.ShowShortMessageCenter(ActivityContext, "Este produto só pode ser abatido inteiro.");
                            txtQtdeUnidade.RequestFocus();
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaDescontoMaiorQueCemPorCento()
        {
            try
            {
                if (CSGlobal.StrToDecimal(txtDescIncond.Text) > 100)
                {
                    MessageBox.ShowShortMessageCenter(ActivityContext, "O valor de desconto não pode ser maior que 100%.");
                    txtDescIncond.RequestFocus();
                    return false;
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool AdfMaiorQueValorTotal()
        {
            try
            {
                if (Convert.ToDecimal(lblValorAdicionalFinanceiro.Text) > Convert.ToDecimal(lblValorTotalItem.Text))
                {
                    MessageBox.ShowShortMessageBottom(ActivityContext, "Valor de ADF maior que Valor total de venda.");
                    return true;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaDados()
        {
            try
            {
                if (!IsBroker())
                {
                    if (!ValidaValorMaximoDigitadoNaoBroker())
                        return false;
                }
                else
                {
                    if (!ValidaPercentualDescontoBroker())
                        return false;
                }

                if (!ValidaSeQuantidadeFoiInformada())
                    return false;

                if (!ValidaEstoqueProntaEntrega())
                    return false;

                if (!ValidaQuantidadeAbatimento())
                    return false;

                if (!ValidaQuantidadeUnidadeIndenizacao())
                    return false;

                if (!ValidaQuantidadeInteiraEUnitaria())
                    return false;

                if (!ValidaDescontoMaiorQueCemPorCento())
                    return false;

                if (!ValidaValorDaTabela())
                    return false;

                if (!ValidaAbatimento())
                    return false;

                if (!IsBroker())
                {
                    if (!ValidacaoNaoBroker())
                        return false;
                }

                if (!ValidaValorMinimoPraVenda())
                    return false;

                if (!ValidaQuantidadeMultipla())
                    return false;

                if (!ValidaSaldoParaConcederDesconto())
                    return false;

                if (!ValidaPercentualDeLucratividade())
                    return false;

                if (AdfMaiorQueValorTotal())
                    return false;

                if (DescontoSemQuantidade())
                    return false;

                return ValidaBloqueios();
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool DescontoSemQuantidade()
        {
            try
            {
                if (CSGlobal.StrToDecimal(txtQtdeInteiro.Text) == 0 &&
                    CSGlobal.StrToDecimal(txtQtdeUnidade.Text) == 0 &&
                    CSGlobal.StrToDecimal(txtDescIncond.Text) > 0)
                {
                    MessageBox.ShowShortMessageBottom(ActivityContext, "Desconto sem quantidade de venda.");
                    return true;
                }

                return false;
            }
            catch (System.Exception ex)
            {
                throw new System.Exception(ex.Message);
            }
        }

        private static bool ValidaValorDaTabela()
        {
            try
            {
                if (CSGlobal.StrToDecimal(lblValorTabela.Text) == 0)
                {
                    MessageBox.ShowShortMessageCenter(ActivityContext, "O valor da tabela invalido.");
                    cboTabelaPreco.RequestFocus();
                    return false;
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static void SalvaDados()
        {
            try
            {
                CSItemsPedido.CSItemPedido itempedido = null;
                CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto precoProduto = null;
                decimal valorVerbaExtra, valorVerbaNormal, valorDesconto;
                decimal vlr_impostos = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_TOTAL_IMPOSTO_BROKER;

                if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.NOVO)
                    itempedido = new CSItemsPedido.CSItemPedido();
                else
                    itempedido = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current;

                itempedido.VLR_TOTAL_IMPOSTO_BROKER = vlr_impostos;

                if (itempedido.STATE == ObjectState.NOVO ||
                    itempedido.STATE == ObjectState.ALTERADO ||
                    itempedido.STATE == ObjectState.NOVO_ALTERADO)
                {
                    // [ Guarda valores antes das alterações ]
                    valorVerbaExtra = itempedido.VLR_VERBA_EXTRA;
                    valorVerbaNormal = itempedido.VLR_VERBA_NORMAL;
                    valorDesconto = itempedido.VLR_DESCONTO;

                    // Guarda qual o produto atual
                    itempedido.PRODUTO = CSProdutos.Current;

                    //// Preenche o percentual do adicional financeiro a partir do current que é usado como base para os outros items criados
                    //if (itempedido.QTD_PEDIDA_TOTAL == 0)
                    //    itempedido.PRC_ADICIONAL_FINANCEIRO = 0;
                    //else
                    //    itempedido.PRC_ADICIONAL_FINANCEIRO = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_ADICIONAL_FINANCEIRO;

                    //// Preenche o valor adicional financeiro
                    //if (itempedido.QTD_PEDIDA_TOTAL == 0)
                    //    itempedido.VLR_ADICIONAL_FINANCEIRO = 0;
                    //else
                    //    itempedido.VLR_ADICIONAL_FINANCEIRO = CSGlobal.StrToDecimal(lblValorAdicionalFinanceiro.Text);

                    // Preenche o preço de desconto
                    itempedido.PRC_DESCONTO = CSGlobal.StrToDecimal(txtDescIncond.Text);

                    // Preenche a quantidade pedida
                    itempedido.QTD_PEDIDA_INTEIRA = CSGlobal.StrToDecimal(txtQtdeInteiro.Text);
                    itempedido.QTD_PEDIDA_UNIDADE = CSGlobal.StrToInt(txtQtdeUnidade.Text);

                    // Preenche a quantidade pedida de indenização
                    //itempedido.QTD_INDENIZACAO_INTEIRA = CSGlobal.StrToDecimal(txtQtdeInteiroIndenizacao.Text);
                    //itempedido.QTD_INDENIZACAO_UNIDADE = CSGlobal.StrToInt(txtQtdeUnidadeIndenizacao.Text);

                    //if (itempedido.QTD_INDENIZACAO_INTEIRA != 0 || itempedido.QTD_INDENIZACAO_UNIDADE != 0)
                    //    //itempedido.VLR_INDENIZACAO_UNIDADE = decimal.Round(CSGlobal.StrToDecimal(txtValorFinalItemIndenizacao.Text) / (decimal)CSProdutos.Current.QTD_UNIDADE_MEDIDA, 4);
                    //    itempedido.VLR_INDENIZACAO_UNIDADE = CSGlobal.StrToDecimal(txtValorFinalItemIndenizacao.Text);
                    //else
                        itempedido.VLR_INDENIZACAO_UNIDADE = 0;

                    decimal valorTabela = 0m;
                    //decimal desconto = 0m;

                    //if (DesconsiderarDesconto ||
                    //    (CSGlobal.StrToDecimal(txtValorFinalItem.Text) < CSGlobal.StrToDecimal(lblValorTabela.Text) &&
                    //    !CSGlobal.Vlr_Desconto))
                    //{
                    valorTabela = CSGlobal.StrToDecimal(txtValorUnitarioSemADF.Text);
                    //desconto = 0m;
                    //}
                    //else
                    //{
                    //    valorTabela = CSGlobal.StrToDecimal(lblValorTabela.Text);
                    //    desconto = CSGlobal.StrToDecimal(txtDescIncond.Text);
                    //}

                    decimal inteira = CSGlobal.StrToDecimal(txtQtdeInteiro.Text);
                    decimal partida = CSGlobal.StrToDecimal(txtQtdeUnidade.Text);
                    decimal caixa = CSProdutos.Current.UNIDADES_POR_CAIXA;

                    //decimal precoUnitario = CSGlobal.StrToDecimal(lblValorTabela.Text) / caixa;
                    //decimal precoUnitarioDesconto = precoUnitario * (CSGlobal.StrToDecimal(txtDescIncond.Text) / 100);
                    decimal descontoTotal = 0m;

                    // Diferanca entre o valor de tabela e o valor informado 
                    //itempedido.VLR_DESCONTO_UNITARIO = ((itempedido.PRODUTO.PRECOS_PRODUTO.Current.VLR_PRODUTO + (itempedido.PRODUTO.PRECOS_PRODUTO.Current.VLR_PRODUTO *(CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_ADICIONAL_FINANCEIRO / 100))) * itempedido.PRC_DESCONTO / 100);
                    //itempedido.VLR_DESCONTO_UNITARIO = valorTabela - ((valorTabela - (Math.Truncate((valorTabela * (itempedido.PRC_DESCONTO / 100)) * 100) / 100)));

                    itempedido.VLR_DESCONTO_UNITARIO = CSGlobal.StrToDecimal(lblValorDescontoUnitario.Text);

                    //if (DesconsiderarDesconto ||
                    //(CSGlobal.StrToDecimal(txtValorFinalItem.Text) < CSGlobal.StrToDecimal(lblValorTabela.Text) &&
                    //	!CSGlobal.Vlr_Desconto))
                    //{
                    //descontoTotal = ((itempedido.VLR_DESCONTO_UNITARIO / caixa) * ((inteira * caixa) + partida));
                    //}
                    //else
                    //{
                    descontoTotal = System.Math.Round(((itempedido.VLR_DESCONTO_UNITARIO / caixa) * ((inteira * caixa) + partida)), 2, MidpointRounding.AwayFromZero);
                    //itempedido.VLR_DESCONTO_UNITARIO = precoUnitarioDesconto * caixa;
                    //}

                    if (itempedido.QTD_PEDIDA_TOTAL == 0)
                        itempedido.PRC_ADICIONAL_FINANCEIRO = 0;
                    else
                        itempedido.PRC_ADICIONAL_FINANCEIRO = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_ADICIONAL_FINANCEIRO;

                    //  o percentual do adicional sobre o valor unitario
                    if (itempedido.QTD_PEDIDA_TOTAL == 0)
                        itempedido.VLR_ADICIONAL_UNITARIO = 0;
                    else
                        itempedido.VLR_ADICIONAL_UNITARIO = System.Math.Round((CSGlobal.StrToDecimal(lblValorAdicionalFinanceiro.Text) / caixa), 2, MidpointRounding.AwayFromZero);

                    //decimal PrecoComDesconto = valorTabela * ((100 - desconto) / 100);
                    decimal PrecoComDescontoUnitario = valorTabela / caixa;
                    decimal PrecoComAdicionalDescontoUnitario = PrecoComDescontoUnitario * (1 + (itempedido.PRC_ADICIONAL_FINANCEIRO / 100));
                    decimal Quantidade = itempedido.QTD_PEDIDA_TOTAL;
                    decimal Valor = System.Math.Round((Quantidade * PrecoComAdicionalDescontoUnitario), 2, MidpointRounding.AwayFromZero);

                    // Preenche o valor unitário
                    if (Quantidade > 0)
                        itempedido.VLR_ITEM_UNIDADE = System.Math.Round(Valor / Quantidade, 2, MidpointRounding.AwayFromZero);
                    else
                        itempedido.VLR_ITEM_UNIDADE = 0;

                    // Preenche o valor total
                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2 &&
                        CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 3)
                        itempedido.VLR_TOTAL_ITEM = Valor;
                    else
                    {
                        itempedido.VLR_TOTAL_ITEM = CSGlobal.StrToDecimal(lblValorTotalItem.Text);
                        itempedido.VLR_ITEM_UNIDADE = System.Math.Round(itempedido.VLR_TOTAL_ITEM / Quantidade, 2, MidpointRounding.AwayFromZero);
                    }

                    // Preenche o valor de desconto
                    // valor do desconto total * qtde unitaria geral
                    //itempedido.VLR_DESCONTO =
                    //(itempedido.VLR_DESCONTO_UNITARIO * CSGlobal.StrToDecimal(txtQtdeInteiro.Text)) +
                    //(((itempedido.VLR_DESCONTO_UNITARIO / CSProdutos.Current.QTD_UNIDADE_EMBALAGEM) * CSGlobal.StrToDecimal(txtQtdeUnidade.Text)));

                    itempedido.VLR_DESCONTO = descontoTotal;

                    itempedido.PRC_DESCONTO = CSGlobal.StrToDecimal(txtDescIncond.Text);

                    //itempedido.VLR_PESO_PRODUTO_TOTAL = (itempedido.PRODUTO.VLR_PESO_PRODUTO * CSGlobal.StrToDecimal(txtQtdeInteiro.Text)) + ((itempedido.PRODUTO.VLR_PESO_PRODUTO / itempedido.PRODUTO.QTD_UNIDADE_EMBALAGEM) * CSGlobal.StrToDecimal(txtQtdeUnidade.Text));

                    // [ Preenche o codigo da tabela escolhido ]
                    precoProduto = (CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto)((CSItemCombo)cboTabelaPreco.SelectedItem).Valor;
                    itempedido.COD_TABELA_PRECO = precoProduto.COD_TABELA_PRECO;

                    // [ Calcula do valor da verba extra ]
                    itempedido.VLR_VERBA_EXTRA = (precoProduto.VLR_VERBA_EXTRA * itempedido.QTD_PEDIDA_TOTAL) / itempedido.PRODUTO.UNIDADES_POR_CAIXA;

                    if (CSEmpresa.Current.TIPO_CALCULO_VERBA != CSEmpresa.CALCULO_VERBA.NENHUM)
                    {
                        decimal valorSaldoDesconto = 0;
                        decimal valorVenda = 0;

                        // [ Pega o valor do produto vendido sem o adicional ]
                        valorVenda = itempedido.VLR_TOTAL_ITEM - itempedido.VLR_ADICIONAL_FINANCEIRO_TOTAL;

                        // [ Pega o saldo atual do vendedor ]
                        valorSaldoDesconto = CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.VAL_SALDO_DESCONTO;

                        // [ Calcula o valor da verba normal ]
                        switch (CSEmpresa.Current.TIPO_CALCULO_VERBA)
                        {
                            case CSEmpresa.CALCULO_VERBA.PERCENTUAL_VALOR_PEDIDO:
                                {
                                    valorVenda += itempedido.VLR_ADICIONAL_FINANCEIRO_TOTAL;

                                    // [ Valor da venda é menor que (preço tabela - % verba)? ]
                                    if (valorVenda < itempedido.VLR_TABELA_PRECO_ITEM_MENOS_PCT_VERBA_NORMAL &&
                                        !CSEmpresa.Current.IND_ATUALIZAR_VERBA_PED_ABAIXOMINIMO)
                                    {
                                        itempedido.VLR_VERBA_NORMAL = 0;

                                    }
                                    else
                                    {
                                        itempedido.VLR_VERBA_NORMAL = CSGlobal.Round((valorVenda * CSEmpresa.Current.PCT_VERBA_NORMAL) / 100, 2);
                                    }

                                    break;
                                }

                            case CSEmpresa.CALCULO_VERBA.DIFERENCA_VALOR_TABELA:
                                {
                                    itempedido.VLR_VERBA_NORMAL = CSGlobal.Round(valorVenda - itempedido.VLR_TABELA_PRECO_ITEM_MENOS_PCT_VERBA_NORMAL, 2);
                                    break;
                                }
                        }

                        // [ Permite atualização do saldo para verba normal? ]
                        if (!CSEmpresa.Current.IND_VLR_VERBA_PEDIDO_NOVO_ATUSALDO)
                            itempedido.VLR_VERBA_NORMAL = 0;

                        // [ Atualiza saldo descontando valores anteriores ]
                        valorSaldoDesconto += itempedido.VLR_VERBA_NORMAL - valorVerbaNormal;

                        // [ Permite atualização do saldo para verba extra? ]
                        if (!CSEmpresa.Current.IND_VLR_VERBA_EXTRA_ATUSALDO)
                            itempedido.VLR_VERBA_EXTRA = 0;

                        // [ Atualiza saldo descontando valores anteriores ]
                        valorSaldoDesconto += itempedido.VLR_VERBA_EXTRA - valorVerbaExtra;

                        // [ Atualizou o saldo anteriormente? ]
                        if (CSPDVs.Current.PEDIDOS_PDV.Current.IND_VLR_DESCONTO_ATUSALDO)
                            valorSaldoDesconto += valorDesconto;

                        // [ Permite atualização do saldo para desconto? ]
                        if (CSEmpresa.Current.TIPO_CALCULO_VERBA != CSEmpresa.CALCULO_VERBA.DIFERENCA_VALOR_TABELA)
                        {
                            valorSaldoDesconto -= itempedido.VLR_DESCONTO;
                            CSPDVs.Current.PEDIDOS_PDV.Current.IND_VLR_DESCONTO_ATUSALDO = true;

                        }
                        else
                        {
                            CSPDVs.Current.PEDIDOS_PDV.Current.IND_VLR_DESCONTO_ATUSALDO = false;
                        }

                        // [ Atualiza o valor do saldo ]
                        CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.VAL_SALDO_DESCONTO = valorSaldoDesconto;
                    }

                    itempedido.STATE = ObjectState.NOVO_ALTERADO;

                    if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.NOVO)
                    {
                        // Adiciona o item de pedido na coleção
                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Add(itempedido);
                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current = itempedido;
                    }

                    //if (CSGlobal.PedidoSugerido)
                    //{
                    //    if (Convert.ToInt32(lblQtdSugerida.Text) > 0)
                    //    {
                    //        if (txtQtdeInteiro.Text != lblQtdSugerida.Text)
                    //            itempedido.IND_UTILIZA_QTD_SUGERIDA = false;
                    //        else
                    //            itempedido.IND_UTILIZA_QTD_SUGERIDA = true;
                    //    }
                    //}
                    //else
                    //{
                    //    itempedido.IND_UTILIZA_QTD_SUGERIDA = false;
                    //}

                    if (CSPDVs.Current.PEDIDOS_PDV.Current.STATE == ObjectState.SALVO)
                        CSPDVs.Current.PEDIDOS_PDV.Current.STATE = ObjectState.ALTERADO;

                    itempedido.AtualizaImagem();

                    // Recebe o valor de estoque ja calculado (lblSaldoEstoque.Text)
                    if (CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.IND_PRONTA_ENTREGA)
                        CSProdutos.Current.QTD_ESTOQUE_PRONTA_ENTREGA = GetQtdPedidaUnidadeEstoque(lblSaldoEstoque.Text);
                    else
                        CSProdutos.Current.QTD_ESTOQUE = GetQtdPedidaUnidadeEstoque(lblSaldoEstoque.Text);
                }

            }
            catch (System.Exception ex)
            {
                MessageBox.AlertErro(ActivityContext, "Erro ao salvar produto pedido");
            }
        }

        private static decimal GetQtdPedidaUnidadeEstoque(string Qtd)
        {
            try
            {
                decimal quantidade;
                string qtdUnidade = "";
                int posBarra;
                int posNegativo;
                //Verifica se a quantidade e negativa
                posNegativo = Qtd.IndexOf("-");
                if (posNegativo > (-1))
                    Qtd = Qtd.Substring(1, Qtd.Length - 1);

                string qtdInteira = Qtd;

                posBarra = Qtd.IndexOf("/");
                if (posBarra > (-1))
                {
                    // Pega valor antes da barra
                    qtdInteira = Qtd.Substring(0, posBarra);
                    // PEga valor depois da barra

                    if (posBarra + 1 + (Qtd.Length - posBarra) > Qtd.Length)
                        qtdUnidade = Qtd.Substring(posBarra + 1, Qtd.Length - (posBarra + 1));
                    else
                        qtdUnidade = Qtd.Substring(posBarra + 1, Qtd.Length - posBarra);
                }
                else
                {
                    qtdInteira = Qtd;

                    var qtds = Qtd.Split('/');
                    qtdInteira = qtds[0];
                    if (qtds.Length == 2)
                        qtdUnidade = qtds[1];
                    // estava com problema do sinal de (-)
                    if (qtdInteira == "0-")
                        qtdInteira = "0";
                }

                switch (CSProdutos.Current.COD_UNIDADE_MEDIDA)
                {
                    case "CX":
                    case "DZ":
                        quantidade = (CSGlobal.StrToInt(qtdInteira) * CSProdutos.Current.QTD_UNIDADE_EMBALAGEM) + CSGlobal.StrToInt(qtdUnidade);
                        break;
                    default:
                        quantidade = CSGlobal.StrToDecimal(qtdInteira) + CSGlobal.StrToDecimal(qtdUnidade);
                        break;
                }

                //Verifica se a quantidade é uma quantidade negativa
                if (posNegativo > (-1))
                    quantidade = quantidade * -1;

                return quantidade;
            }
            catch (System.Exception ex)
            {
                return 0;
            }
        }

        public static void SalvarAlteracoes(bool proximoPasso, bool edicaoProduto)
        {
            // Verifica se os dados estao ok para salvar
            if (ValidaDados())
            {
                // Chama a rotina de salvamento dos dados
                try
                {
                    if (CSEmpresa.Current.IND_LIBERA_VENDA_PRODUTO_SEM_ESTOQUE == "N" &&
                lblSaldoEstoque.Text.Contains("-"))
                    {
                        MessageBox.Alert(ActivityContext, "A quantidade vendida é maior que o estoque disponivel. Deseja continuar?", "Continuar", (_sender, _e) =>
                        {

                            CSGlobal.BloquearSaidaCliente = true;
                            MessageBox.ShowShortMessageCenter(ActivityContext, "Salvando produto...");
                            SalvaDados();

                            //Salva o pedido para manter backup
                            ((Cliente)ActivityContext).SalvarPedido(proximoPasso, edicaoProduto);

                            //if (ListaProdutosPedido.EdicaoSelecionada)
                            //    ListaProdutosPedido.EdicaoSelecionada = false;

                            IsDirty = false;
                        }
                    , "Não",
                    (_sender, _e) =>
                    {
                        return;
                    }, false
                    );
                    }
                    else
                    {
                        CSGlobal.BloquearSaidaCliente = true;
                        MessageBox.ShowShortMessageCenter(ActivityContext, "Salvando produto...");
                        SalvaDados();

                        //Salva o pedido para manter backup
                        ((Cliente)ActivityContext).SalvarPedido(proximoPasso, edicaoProduto);

                        //if (ListaProdutosPedido.EdicaoSelecionada)
                        //    ListaProdutosPedido.EdicaoSelecionada = false;
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.AlertErro(ActivityContext, ex.Message);
                }
            }
        }

        private static bool ValidaQuantidade()
        {
            try
            {
                if (CSGlobal.StrToDecimal(txtQtdeInteiro.Text) == 0 &&
                    CSGlobal.StrToInt(txtQtdeUnidade.Text) == 0 //&&
                    //CSGlobal.StrToInt(txtQtdeInteiroIndenizacao.Text) == 0 &&
                    //CSGlobal.StrToInt(txtQtdeUnidadeIndenizacao.Text) == 0 &&
                    //CSGlobal.StrToDecimal(txtValorFinalItemIndenizacao.Text) == 0
                    )
                {
                    return false;
                }

                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private bool ValidaFormatacaoNumerica()
        {
            try
            {
                if (aceitaPontuacaoQtdeInteiro)
                {
                    if (txtQtdeInteiro.Text != string.Empty &&
                        StringFormatter.NaoDecimal(txtQtdeInteiro.Text))
                    {
                        txtQtdeInteiro.Text = txtQtdeInteiro.Text.Remove(txtQtdeInteiro.Text.Length - 1);
                        txtQtdeInteiro.SetSelection(txtQtdeInteiro.Text.Length);
                        return false;
                    }
                }

                if (txtDescIncond.Text != string.Empty &&
                    StringFormatter.NaoDecimal(txtDescIncond.Text))
                {
                    txtDescIncond.Text = txtDescIncond.Text.Remove(txtDescIncond.Text.Length - 1);
                    txtDescIncond.SetSelection(txtDescIncond.Text.Length);
                    return false;
                }
                else
                {
                    if (txtDescIncond.Text.Contains(','))
                    {
                        int posicao = txtDescIncond.Text.IndexOf(',');

                        if (txtDescIncond.Text.Substring(posicao + 1, txtDescIncond.Text.Length - posicao - 1).Length > 2)
                        {
                            txtDescIncond.Text = txtDescIncond.Text.Remove(txtDescIncond.Text.Length - 1);
                            txtDescIncond.SetSelection(txtDescIncond.Text.Length);
                            return false;
                        }
                    }
                }

                //if (txtValorFinalItemIndenizacao.Text != string.Empty &&
                //    StringFormatter.NaoDecimal(txtValorFinalItemIndenizacao.Text))
                //{
                //    txtValorFinalItemIndenizacao.Text = txtValorFinalItemIndenizacao.Text.Remove(txtValorFinalItemIndenizacao.Text.Length - 1);
                //    txtValorFinalItemIndenizacao.SetSelection(txtValorFinalItemIndenizacao.Text.Length);
                //    return false;
                //}

                if (txtValorUnitarioSemADF.Text != string.Empty &&
                    StringFormatter.NaoDecimal(txtValorUnitarioSemADF.Text))
                {
                    txtValorUnitarioSemADF.Text = txtValorUnitarioSemADF.Text.Remove(txtValorUnitarioSemADF.Text.Length - 1);
                    txtValorUnitarioSemADF.SetSelection(txtValorUnitarioSemADF.Text.Length);
                    return false;
                }
                else
                {
                    if (txtValorUnitarioSemADF.Text.Contains(','))
                    {
                        int posicao = txtValorUnitarioSemADF.Text.IndexOf(',');

                        if (txtValorUnitarioSemADF.Text.Substring(posicao + 1, txtValorUnitarioSemADF.Text.Length - posicao - 1).Length > 2)
                        {
                            txtValorUnitarioSemADF.Text = txtValorUnitarioSemADF.Text.Remove(txtValorUnitarioSemADF.Text.Length - 1);
                            txtValorUnitarioSemADF.SetSelection(txtValorUnitarioSemADF.Text.Length);
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private void CboTabelaPreco_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                decimal VlrTabela1 = 0;
                if (txtQtdeUnidade.Text != "000" || txtQtdeUnidade.Text != "0")
                    VlrTabela1 = ((CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto)((CSItemCombo)cboTabelaPreco.SelectedItem).Valor).VLR_PRODUTO;
                decimal vlrUni = VlrTabela1;
                decimal vlrAcreUni = 0;
                decimal QtdeInt = CSGlobal.StrToDecimal(txtQtdeInteiro.Text);
                Recalc_Desconto = false;

                CSProdutos.Current.PRECOS_PRODUTO.Current = (CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto)((CSItemCombo)cboTabelaPreco.SelectedItem).Valor;

                if (!IsBunge())
                    lblValorTabela.Text = CSProdutos.Current.PRECOS_PRODUTO.Current.VLR_PRODUTO.ToString(CSGlobal.DecimalStringFormat);

                if (!IsBunge())
                    txtValorUnitarioSemADF.Text = (CSProdutos.Current.PRECOS_PRODUTO.Current.VLR_PRODUTO - CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_DESCONTO_UNITARIO).ToString(CSGlobal.DecimalStringFormat);

                lblTrib.Text = CSProdutos.Current.PRECOS_PRODUTO.Current.COD_TRIBUTACAO.ToString();

                vlrAcreUni = ((vlrUni * CSProdutos.Current.PRC_ACRESCIMO_QTDE_UNITARIA) / 100);

                vlrAcreUni = 0;

                if (!IsBunge())
                    lblValorTabela.Text = (VlrTabela1 + vlrAcreUni).ToString(CSGlobal.DecimalStringFormat);

                IgnorarEvento = false;

                if (txtValorUnitarioSemADF.Text != (VlrTabela1 + vlrAcreUni).ToString(CSGlobal.DecimalStringFormat))
                {
                    vlrUni = (VlrTabela1 + vlrAcreUni);
                    vlrUni = vlrUni * (CSGlobal.StrToDecimal(txtDescIncond.Text) / 100);
                    if (!IsBunge())
                        txtValorUnitarioSemADF.Text = (CSProdutos.Current.PRECOS_PRODUTO.Current.VLR_PRODUTO - CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_DESCONTO_UNITARIO).ToString(CSGlobal.DecimalStringFormat);
                }
                if (txtQtdeUnidade.Text == "000" || txtQtdeUnidade.Text == "0")
                {
                    decimal ValorTabela = ((CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto)((CSItemCombo)cboTabelaPreco.SelectedItem).Valor).VLR_PRODUTO;

                    lblValorTabela.Text = ValorTabela.ToString(CSGlobal.DecimalStringFormat);
                    txtValorUnitarioSemADF.Text = (CSProdutos.Current.PRECOS_PRODUTO.Current.VLR_PRODUTO - CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_DESCONTO_UNITARIO).ToString(CSGlobal.DecimalStringFormat);
                }
                this.Recalc_Desconto = true;
                TxtDescIncond_TextChanged(null, null);
            }
            catch (System.Exception ex)
            {
            }
        }

        private void FindViewsById(View view)
        {
            //if (!CSGlobal.PedidoSugerido)
            //{
            //    lblMediaQ = view.FindViewById<TextView>(Resource.Id.lblMediaQ);
            //    lblEstoqueAnterior = view.FindViewById<TextView>(Resource.Id.lblEstoqueAnterior);
            //    lblEstoqueVenda = view.FindViewById<TextView>(Resource.Id.lblEstoqueVenda);
            //    lblEstoqueAtual = view.FindViewById<TextView>(Resource.Id.lblEstoqueAtual);
            //    lblEstoqueGiro = view.FindViewById<TextView>(Resource.Id.lblEstoqueGiro);
            //    btnMedia = view.FindViewById<Button>(Resource.Id.btnMedia);
            //    btnGiro = view.FindViewById<Button>(Resource.Id.btnGiro);
            //}

            lblCalcular = view.FindViewById<TextView>(Resource.Id.lblCalcular);
            //lblQ1 = view.FindViewById<TextView>(Resource.Id.lblQ1);
            //lblQ2 = view.FindViewById<TextView>(Resource.Id.lblQ2);
            //lblQ3 = view.FindViewById<TextView>(Resource.Id.lblQ3);
            txtQtdeInteiro = view.FindViewById<EditText>(Resource.Id.txtQtdeInteiro);
            txtQtdeUnidade = view.FindViewById<EditText>(Resource.Id.txtQtdeUnidade);
            txtDescIncond = view.FindViewById<EditText>(Resource.Id.txtDescIncond);
            lblValorTotalItem = view.FindViewById<TextView>(Resource.Id.lblValorTotalItem);
            //txtQtdeInteiroIndenizacao = view.FindViewById<EditText>(Resource.Id.txtQtdeInteiroIndenizacao);
            //txtQtdeUnidadeIndenizacao = view.FindViewById<EditText>(Resource.Id.txtQtdeUnidadeIndenizacao);
            txtValorFinalItem = view.FindViewById<TextView>(Resource.Id.txtValorFinalItem);
            cboTabelaPreco = view.FindViewById<Spinner>(Resource.Id.cboTabelaPreco);
            txtValorUnitarioSemADF = view.FindViewById<EditText>(Resource.Id.txtValorUnitarioSemADF);
            lblValorTabela = view.FindViewById<TextView>(Resource.Id.lblValorTabela);
            lblCodigoProduto = view.FindViewById<TextView>(Resource.Id.lblCodigoProduto);
            lblProduto = view.FindViewById<TextView>(Resource.Id.lblProduto);
            lblUM = view.FindViewById<TextView>(Resource.Id.lblUM);
            lblOrganizVendas = view.FindViewById<TextView>(Resource.Id.lblOrganizVendas);
            lblValorDescontoUnitario = view.FindViewById<TextView>(Resource.Id.lblValorDescontoUnitario);
            lblValorAdicionalFinanceiro = view.FindViewById<TextView>(Resource.Id.lblValorAdicionalFinanceiro);
            lblValorFinalItem = view.FindViewById<TextView>(Resource.Id.lblValorFinalItem);
            //txtValorFinalItemIndenizacao = view.FindViewById<EditText>(Resource.Id.txtValorFinalItemIndenizacao);
            lblPctLucratividade = view.FindViewById<TextView>(Resource.Id.lblPctLucratividade);
            lblSaldoEstoque = view.FindViewById<TextView>(Resource.Id.lblSaldoEstoque);
            lblPz = view.FindViewById<TextView>(Resource.Id.lblPz);
            lblTrib = view.FindViewById<TextView>(Resource.Id.lblTrib);
            lblValorUnitario = view.FindViewById<TextView>(Resource.Id.lblValorUnitario);
            //lblInfoQ1 = view.FindViewById<TextView>(Resource.Id.lblInfoQ1);
            //lblInfoQ2 = view.FindViewById<TextView>(Resource.Id.lblInfoQ2);
            //lblInfoQ3 = view.FindViewById<TextView>(Resource.Id.lblInfoQ3);
            //lblInfoMedia = view.FindViewById<TextView>(Resource.Id.lblInfoMedia);
            lblIndenizacao = view.FindViewById<TextView>(Resource.Id.lblIndenizacao);
            //lblEstoque = view.FindViewById<TextView>(Resource.Id.lblEstoque);
            //lblInfo1 = view.FindViewById<TextView>(Resource.Id.lblInfo1);
            //lblInfo2 = view.FindViewById<TextView>(Resource.Id.lblInfo2);
            //lblInfo3 = view.FindViewById<TextView>(Resource.Id.lblInfo3);
            //lblInfo4 = view.FindViewById<TextView>(Resource.Id.lblInfo4);
            lblValorUnitarioSemADF = view.FindViewById<TextView>(Resource.Id.lblValorUnitarioSemADF);
            lblOrganizVendasTit = view.FindViewById<TextView>(Resource.Id.lblOrganizVendasTit);
            lblDescPctLucratividade = view.FindViewById<TextView>(Resource.Id.lblDescPctLucratividade);
            //lblQtdeIndenizacaoInt = view.FindViewById<TextView>(Resource.Id.lblQtdeIndenizacaoInt);
            //lblQtdeIndenizacaoUnit = view.FindViewById<TextView>(Resource.Id.lblQtdeIndenizacaoUnit);
            //lblValorUnitarioIndenizacao = view.FindViewById<TextView>(Resource.Id.lblValorUnitarioIndenizacao);
            btnCalcular = view.FindViewById<Button>(Resource.Id.btnCalcular);
            lblDesc = view.FindViewById<TextView>(Resource.Id.lblDesc);
            lblOrganizVendasBunge = view.FindViewById<TextView>(Resource.Id.lblOrganizVendasBunge);
            lblOrganizVendasTitBunge = view.FindViewById<TextView>(Resource.Id.lblOrganizVendasTitBunge);
            lblDescIncond = view.FindViewById<TextView>(Resource.Id.lblDescIncond);
            //imvAbatimento = view.FindViewById<ImageView>(Resource.Id.imvAbatimento);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            if (CSProdutos.Current == null)
                ((Cliente)ActivityContext).OnBackPressed();
            else
                ExibirInformacoes();
        }

        private static bool IsBroker()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
        }

        private static bool IsBunge()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 3;
        }

        private class ThreadExibirInformacoes : AsyncTask
        {
            ArrayAdapter adapter;
            int tabelaPadrao = 0;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                adapter = new ArrayAdapter(ActivityContext, Android.Resource.Layout.SimpleSpinnerItem);
                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                CarregaComboBoxTabelaPreco();

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                lblCodigoProduto.Text = CSProdutos.Current.DESCRICAO_APELIDO_PRODUTO;
                lblProduto.Text = CSProdutos.Current.DSC_PRODUTO;
                lblUM.Text = CSProdutos.Current.DSC_UNIDADE_MEDIDA;

                if (adapter.Count > 0)
                {
                    cboTabelaPreco.Adapter = adapter;
                    cboTabelaPreco.SetSelection(tabelaPadrao);
                }

                CSProdutos.Current.PRECOS_PRODUTO.Current = (CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto)((CSItemCombo)cboTabelaPreco.SelectedItem).Valor;

                if (!IsBunge())
                    lblValorTabela.Text = CSProdutos.Current.PRECOS_PRODUTO.Current.VLR_PRODUTO.ToString(CSGlobal.DecimalStringFormat);
                //lblQ1.Text = "";
                //lblQ2.Text = "";
                //lblQ3.Text = "";

                if (!CSGlobal.PedidoSugerido)
                {
                    //lblMediaQ.Text = "";

                    //lblEstoqueAnterior.Text = "";
                    //lblEstoqueVenda.Text = "";
                    //lblEstoqueAtual.Text = "";
                    //lblEstoqueGiro.Text = "";

                    txtQtdeInteiro.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_INTEIRA == decimal.Zero ? "" : CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_INTEIRA.ToString();
                    txtQtdeUnidade.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_UNIDADE == decimal.Zero ? "" : CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_UNIDADE.ToString();
                    txtDescIncond.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_DESCONTO == decimal.Zero ? "" : CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_DESCONTO.ToString();
                    lblValorTotalItem.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_TOTAL_ITEM.ToString(CSGlobal.DecimalStringFormat);

                    //if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_INDENIZACAO_INTEIRA > 0)
                    //    txtQtdeInteiroIndenizacao.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_INDENIZACAO_INTEIRA.ToString();
                    //else
                    //    txtQtdeInteiroIndenizacao.Text = "";

                    //if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_INDENIZACAO_UNIDADE > 0)
                    //    txtQtdeUnidadeIndenizacao.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_INDENIZACAO_UNIDADE.ToString();

                    if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.LOCK_QTD == true)
                    {
                        txtQtdeInteiro.Enabled = false;
                        txtQtdeUnidade.Enabled = false;
                        txtDescIncond.Enabled = false;
                        txtValorFinalItem.Enabled = false;
                        cboTabelaPreco.Enabled = false;
                    }
                    else
                    {
                        txtQtdeInteiro.Enabled = true;
                        txtQtdeUnidade.Enabled = true;
                        txtDescIncond.Enabled = true;
                        txtValorFinalItem.Enabled = true;
                        cboTabelaPreco.Enabled = true;
                    }

                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2 &&
                        CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 3)
                    {
                        if (CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO.IND_PAGAMENTO_ANTECIPADO)
                        {
                            txtDescIncond.Enabled = false;
                            txtValorUnitarioSemADF.Enabled = false;
                            txtValorFinalItem.Enabled = false;
                        }
                        else
                        {
                            txtDescIncond.Enabled = true;
                            txtValorUnitarioSemADF.Enabled = true;
                            txtValorFinalItem.Enabled = true;
                        }
                    }

                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 3)
                    {
                        if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE != ObjectState.NOVO)
                        {
                            txtValorUnitarioSemADF.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_ITEM_UNIDADE.ToString(CSGlobal.DecimalStringFormat);
                        }
                    }

                    if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.NOVO)
                    {
                        if (CSProdutos.Current.PRECOS_PRODUTO.Current == null)
                        {
                            try
                            {
                                CSProdutos.Current.PRECOS_PRODUTO.Current = CSProdutos.Current.PRECOS_PRODUTO[0];
                                if (CSProdutos.Current.PRECOS_PRODUTO.Current == null)
                                    throw new System.Exception();
                            }
                            catch (System.Exception)
                            {
                                MessageBox.AlertErro(ActivityContext, "Preço do produto não cadastrado.\r\nNão é possivel realizar esta venda.");
                                return;
                            }
                        }

                        if (!IsBunge())
                            txtValorUnitarioSemADF.Text = (CSProdutos.Current.PRECOS_PRODUTO.Current.VLR_PRODUTO - CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_DESCONTO_UNITARIO).ToString(CSGlobal.DecimalStringFormat);
                    }
                    else
                    {
                        if (!IsBunge())
                            txtValorUnitarioSemADF.Text = (CSProdutos.Current.PRECOS_PRODUTO.Current.VLR_PRODUTO - CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_DESCONTO_UNITARIO).ToString(CSGlobal.DecimalStringFormat);
                    }

                    if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_UNIDADE > 0)
                    {
                        decimal valorUnitario = ((CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto)((CSItemCombo)cboTabelaPreco.SelectedItem).Valor).VLR_PRODUTO;
                        decimal valorAcrescimoUnitario = 0;

                        valorAcrescimoUnitario = (valorUnitario * CSProdutos.Current.PRC_ACRESCIMO_QTDE_UNITARIA) / 100;

                        lblValorTabela.Text = (valorUnitario + valorAcrescimoUnitario).ToString(CSGlobal.DecimalStringFormat);
                    }

                    if (!IsBunge())
                        lblOrganizVendas.Text = CSProdutos.Current.GRUPO_COMERCIALIZACAO.COD_SETOR_BROKER;
                    else
                        lblOrganizVendasBunge.Text = CSProdutos.Current.GRUPO_COMERCIALIZACAO.COD_SETOR_BROKER;

                    if (CSPDVs.Current.COD_GER_BROKER != "" && IsBroker())
                        lblOrganizVendas.Text = CSPDVs.Current.COD_GER_BROKER;

                    lblValorDescontoUnitario.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_DESCONTO_UNITARIO.ToString(CSGlobal.DecimalStringFormat);

                    if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_ADICIONAL_FINANCEIRO == 0)
                    {
                        lblValorAdicionalFinanceiro.Text = (CSGlobal.StrToDecimal(txtValorUnitarioSemADF.Text) * (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_ADICIONAL_FINANCEIRO / 100)).ToString(CSGlobal.DecimalStringFormat);

                    }
                    else
                    {
                        txtValorUnitarioSemADF.Text = ((CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_ITEM_UNIDADE - CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_ADICIONAL_FINANCEIRO)).ToString(CSGlobal.DecimalStringFormat);
                        lblValorAdicionalFinanceiro.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_ADICIONAL_FINANCEIRO.ToString(CSGlobal.DecimalStringFormat);
                    }
                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2 &&
                        CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 3)
                    {
                        lblValorFinalItem.Text = (CSGlobal.StrToDecimal(txtValorUnitarioSemADF.Text) + CSGlobal.StrToDecimal(lblValorAdicionalFinanceiro.Text)).ToString(CSGlobal.DecimalStringFormat);
                        txtValorFinalItem.Text = (CSGlobal.StrToDecimal(txtValorUnitarioSemADF.Text) + CSGlobal.StrToDecimal(lblValorAdicionalFinanceiro.Text)).ToString(CSGlobal.DecimalStringFormat);

                        //if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_INDENIZACAO_UNIDADE != 0)
                        //    txtValorFinalItemIndenizacao.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_INDENIZACAO_UNIDADE.ToString(CSGlobal.DecimalStringFormat);
                        //else
                        //    txtValorFinalItemIndenizacao.Text = "";

                    }
                    else
                    {
                        if (CSProdutos.Current.PRECOS_PRODUTO.Current != null)
                        {
                            lblValorFinalItem.Text = CSProdutos.Current.PRECOS_PRODUTO.Current.VLR_PRODUTO.ToString(CSGlobal.DecimalStringFormat);
                            txtValorFinalItem.Text = CSProdutos.Current.PRECOS_PRODUTO.Current.VLR_PRODUTO.ToString(CSGlobal.DecimalStringFormat);
                        }
                    }

                    if (txtQtdeInteiro.Text != "0")
                    {
                        lblSaldoEstoque.Text = CSProdutos.CSProduto.ConverteUnidadesParaMedida((CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.IND_PRONTA_ENTREGA ? CSProdutos.Current.QTD_ESTOQUE_PRONTA_ENTREGA : CSProdutos.Current.QTD_ESTOQUE),
                            CSProdutos.Current.COD_UNIDADE_MEDIDA,
                            CSProdutos.Current.QTD_UNIDADE_EMBALAGEM);
                    }

                    if (IsBroker())
                    {
                        lblPz.Text = CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO.CODPRZPGT.ToString();
                        BtnCalcular_Click(null, null);
                    }
                    else
                    {
                        lblPz.Text = CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO.DIAVCM.ToString();
                        try
                        {
                            BtnCalcular_Click(null, null);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.AlertErro(ActivityContext, ex.Message);
                        }
                    }
                }

                CSGlobal.Focus(ActivityContext, txtQtdeInteiro);

                progress.Dismiss();

                base.OnPostExecute(result);
            }

            private void CarregaComboBoxTabelaPreco()
            {
                try
                {
                    for (int i = 0; i < CSProdutos.Current.PRECOS_PRODUTO.Count; i++)
                    {
                        CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto preco = CSProdutos.Current.PRECOS_PRODUTO[i];

                        CSItemCombo ic = new CSItemCombo();
                        ic.Texto = preco.COD_TABELA_PRECO.ToString() + " " + preco.DSC_TABELA_PRECO;
                        ic.Valor = preco;
                        adapter.Add(ic);

                        if (CSPDVs.Current.COD_TABPRECO_PADRAO == preco.COD_TABELA_PRECO)
                            tabelaPadrao = i;
                    }
                }
                catch (System.Exception ex)
                {

                }
            }
        }

        public void ExibirInformacoes()
        {
            progress = new ProgressDialogCustomizado(Activity, thisLayoutInflater).Customizar();
            progress.Show();

            new ThreadExibirInformacoes().Execute();
        }

        private void CalculaDescontoIncond()
        {
            try
            {
                if (txtValorUnitarioSemADF.Text != "")
                {
                    decimal valorTabela = CSGlobal.StrToDecimal(lblValorTabela.Text);
                    decimal valorUnidade = CSGlobal.StrToDecimal(txtValorUnitarioSemADF.Text);
                    decimal percentualDesconto = CSGlobal.StrToDecimal(txtDescIncond.Text);
                    decimal result = 0;
                    decimal valorDescontoAtual = 0;
                    decimal valorDesconto = 0;

                    if (valorTabela > 0)
                        result = 100 - ((100 * valorUnidade) / valorTabela);

                    valorDescontoAtual = (valorTabela * percentualDesconto) / 100;
                    valorDescontoAtual = CSGlobal.StrToDecimal(valorDescontoAtual.ToString(CSGlobal.DecimalStringFormat));
                    valorDesconto = (valorTabela * result) / 100;
                    valorDesconto = CSGlobal.StrToDecimal(valorDesconto.ToString(CSGlobal.DecimalStringFormat));

                    if (this.Recalc_Desconto && valorDescontoAtual != valorDesconto)
                    {
                        txtDescIncond.Text = result.ToString(CSGlobal.DecimalStringFormat);
                    }
                }
                else
                {
                    txtDescIncond.Text = "100";
                }
                RecalculaValorFinalDoItemPraLabel();
                RecalculaValorFinalDoItemPraTextBox();
                CalculaEMostraValorTotal();
            }
            catch (System.Exception ex)
            {

            }
        }

        private void RecalculaValorAdicionalFinanceiro()
        {
            // recalcula o valor do adicional financeiro
            lblValorAdicionalFinanceiro.Text = (CSGlobal.StrToDecimal(txtValorUnitarioSemADF.Text) * (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_ADICIONAL_FINANCEIRO / 100)).ToString(CSGlobal.DecimalStringFormat);
        }

        private void CalculoValorDoDesconto()
        {
            // calculo o valor de desconto
            lblValorDescontoUnitario.Text = (CSGlobal.StrToDecimal(lblValorTabela.Text) - CSGlobal.StrToDecimal(txtValorUnitarioSemADF.Text)).ToString(CSGlobal.DecimalStringFormat);
        }

        private void RecalculaValorFinalDoItemPraLabel()
        {
            lblValorFinalItem.Text = (CSGlobal.StrToDecimal(txtValorUnitarioSemADF.Text) + CSGlobal.StrToDecimal(lblValorAdicionalFinanceiro.Text)).ToString(CSGlobal.DecimalStringFormat);
        }

        private void RecalculaValorFinalDoItemPraTextBox()
        {
            lblValorFinalItem.Text = (CSGlobal.StrToDecimal(txtValorUnitarioSemADF.Text) + CSGlobal.StrToDecimal(lblValorAdicionalFinanceiro.Text)).ToString(CSGlobal.DecimalStringFormat);
            txtValorFinalItem.Text = (CSGlobal.StrToDecimal(txtValorUnitarioSemADF.Text) + CSGlobal.StrToDecimal(lblValorAdicionalFinanceiro.Text)).ToString(CSGlobal.DecimalStringFormat);
        }

        private void CalculaEMostraValorTotal()
        {
            lblValorTotalItem.Text = CalculaValorTotalItem().ToString(CSGlobal.DecimalStringFormat);
        }

        private decimal CalculaValorTotalItem()
        {
            decimal valorTotalItem = 0;
            //decimal valorTotalUnitario = 0;
            //decimal valorFinalItem = 0;
            //decimal valorAdfUnitario = 0;
            //decimal valorPrcFinanceiro = 0;
            //decimal valorUnitario = 0;

            try
            {
                decimal tabela = 0m;
                //decimal desconto = 0m;

                //if (DesconsiderarDesconto ||
                //    (CSGlobal.StrToDecimal(txtValorFinalItem.Text) < CSGlobal.StrToDecimal(lblValorTabela.Text) &&
                //    !CSGlobal.Vlr_Desconto))
                //{
                tabela = CSGlobal.StrToDecimal(txtValorUnitarioSemADF.Text);
                //    desconto = 0m;
                //}
                //else
                //{
                //    tabela = CSGlobal.StrToDecimal(lblValorTabela.Text);
                //    desconto = CSGlobal.StrToDecimal(txtDescIncond.Text);
                //}

                decimal PrecoComDesconto = tabela;

                decimal PrecoComDescontoUnitario = PrecoComDesconto / CSProdutos.Current.QTD_UNIDADE_EMBALAGEM;

                decimal PrecoComAdicionalDescontoUnitario = PrecoComDescontoUnitario * (1 + (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_ADICIONAL_FINANCEIRO / 100));

                decimal Quantidade = (CSProdutos.Current.QTD_UNIDADE_EMBALAGEM * CSGlobal.StrToDecimal(txtQtdeInteiro.Text)) + CSGlobal.StrToDecimal(txtQtdeUnidade.Text);

                valorTotalItem = Quantidade * PrecoComAdicionalDescontoUnitario;

                lblValorUnitario.Text = PrecoComAdicionalDescontoUnitario.ToString(CSGlobal.DecimalStringFormat);

            }
            catch (System.Exception ex)
            {
                MessageBox.AlertErro(Activity, ex.Message);
            }

            return valorTotalItem;
        }

        private void RecalculaValoresPadrao()
        {
            CalculaEMostraPercentualDeLucratividade();

            RecalculaSaldoDoEstoque();
        }

        private void CalculaEMostraPercentualDeLucratividade()
        {
            lblPctLucratividade.Text = CalculaPercentualLucratividade().ToString(CSGlobal.DecimalStringFormat);
        }

        private static decimal CalculaPercentualLucratividade()
        {
            decimal pctLucratividade = 0;
            try
            {
                if (CSEmpresa.Current.TIPO_CALCULO_LUCRATIVIDADE != 0 && CSProdutos.Current.VLR_CUSTO_GERENCIAL > 0 && CSGlobal.StrToDecimal(lblValorFinalItem.Text) > 0)
                    if (CSEmpresa.Current.TIPO_CALCULO_LUCRATIVIDADE == 1)
                        pctLucratividade = ((CSGlobal.StrToDecimal(lblValorFinalItem.Text) / CSProdutos.Current.VLR_CUSTO_GERENCIAL) * 100) - 100;
                    else
                        pctLucratividade = 100 - ((CSProdutos.Current.VLR_CUSTO_GERENCIAL / CSGlobal.StrToDecimal(lblValorFinalItem.Text)) * 100);

            }
            catch (System.Exception ex)
            {
                MessageBox.AlertErro(ActivityContext, ex.Message);
            }
            return pctLucratividade;
        }

        private void RecalculaSaldoDoEstoque()
        {
            if (CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO != 20)
            {
                lblSaldoEstoque.Text = CSProdutos.CSProduto.ConverteUnidadesParaMedida((CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.IND_PRONTA_ENTREGA ? CSProdutos.Current.QTD_ESTOQUE_PRONTA_ENTREGA : CSProdutos.Current.QTD_ESTOQUE) + ((quantidadeVendidoAnterior - GetQtdPedidaUnidade())),
                                                                                        CSProdutos.Current.COD_UNIDADE_MEDIDA,
                                                                                        CSProdutos.Current.QTD_UNIDADE_EMBALAGEM);
            }
            else
            {
                lblSaldoEstoque.Text = CSProdutos.CSProduto.ConverteUnidadesParaMedida((CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.IND_PRONTA_ENTREGA ? CSProdutos.Current.QTD_ESTOQUE_PRONTA_ENTREGA : CSProdutos.Current.QTD_ESTOQUE),
                                                                                        CSProdutos.Current.COD_UNIDADE_MEDIDA,
                                                                                        CSProdutos.Current.QTD_UNIDADE_EMBALAGEM);
            }
        }
        private static decimal GetQtdPedidaUnidade()
        {
            try
            {
                decimal Qtd = 0;
                switch (CSProdutos.Current.COD_UNIDADE_MEDIDA)
                {
                    case "CX":
                    case "DZ":
                        Qtd = (CSGlobal.StrToInt(txtQtdeInteiro.Text) * CSProdutos.Current.QTD_UNIDADE_EMBALAGEM) + CSGlobal.StrToInt(txtQtdeUnidade.Text);
                        break;
                    default:
                        Qtd = CSGlobal.StrToDecimal(txtQtdeInteiro.Text) + CSGlobal.StrToInt(txtQtdeUnidade.Text);
                        break;
                }
                return Qtd;
            }
            catch (System.Exception ex)
            {
                return 0;
            }
        }

        private static bool ValidaAbatimento()
        {
            try
            {
                //if (GetQtdPedidaUnidadeIndenizacao() > 0)
                //{
                if (!ValidaAbatimentoInvalido())
                    return false;

                decimal VlrAbatimentoTabela = CalculaValorTotalItemIndenizacao();

                //if (CSGlobal.StrToDecimal(txtValorFinalItemIndenizacao.Text) > VlrAbatimentoTabela)
                //{
                //    MessageBox.ShowShortMessageCenter(ActivityContext, "O valor do abatimento (" + CSGlobal.StrToDecimal(txtValorFinalItemIndenizacao.Text).ToString(CSGlobal.DecimalStringFormat) + ") maior que o preço de venda (" + VlrAbatimentoTabela.ToString(CSGlobal.DecimalStringFormat) + ").");
                //    //return false;
                //}
                //}
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaAbatimentoInvalido()
        {
            try
            {
                //if ((CSGlobal.StrToDecimal(txtQtdeInteiroIndenizacao.Text) != 0 ||
                //    CSGlobal.StrToDecimal(txtQtdeUnidadeIndenizacao.Text) != 0) &&
                //    CSGlobal.StrToDecimal(txtValorFinalItemIndenizacao.Text) == 0)
                //{
                //    MessageBox.ShowShortMessageCenter(ActivityContext, "Valor de abatimento inválido.");
                //    txtValorFinalItemIndenizacao.RequestFocus();
                //    return false;
                //}

                //if (CSGlobal.StrToDecimal(txtValorFinalItemIndenizacao.Text) != 0 &&
                //    CSGlobal.StrToDecimal(txtQtdeInteiroIndenizacao.Text) == 0 &&
                //    CSGlobal.StrToDecimal(txtQtdeUnidadeIndenizacao.Text) == 0)
                //{
                //    MessageBox.ShowShortMessageCenter(ActivityContext, "Quantidade de abatimento inválida.");
                //    txtValorFinalItemIndenizacao.RequestFocus();
                //    return false;
                //}

                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidacaoNaoBroker()
        {
            try
            {
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.AplicaDescontoMaximoProdutoTabPreco();

                if (!ValidaPercentualDescontoMaiorQuePermitidoNaoBroker())
                {
                    MessageBox.ShowShortMessageCenter(ActivityContext, "O percentual de desconto não pode ser maior que \"" + CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_DESCONTO_MAXIMO.ToString(CSGlobal.DecimalStringFormat) + "\".");
                    txtDescIncond.RequestFocus();
                    return false;
                }

                ValidaPercentualDescontoNaoBroker();

                if (!IsBunge())
                {
                    if (!ValidaValorUnitarioMaiorQueValorTabela())
                        return false;
                }

                return true;
            }
            catch (System.Exception ex)
            {
                //CSGlobal.GravarLog("ProdutoPedido-ValidacaoNaoBroker", ex.Message, ex.InnerException != null ? ex.InnerException.ToString() : "", ex.StackTrace);
                return false;
            }
        }

        private static bool ValidaPercentualDescontoMaiorQuePermitidoNaoBroker()
        {
            try
            {
                if (CSEmpresa.Current.IND_VALIDA_PCT_MAXIMO_DESCONTO == "S" && CSGlobal.StrToDecimal(txtDescIncond.Text) > CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_DESCONTO_MAXIMO)
                {
                    return false;
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static void ValidaPercentualDescontoNaoBroker()
        {
            try
            {
                if (CSGlobal.StrToDecimal(txtDescIncond.Text) > CSProdutos.Current.PRC_MAXIMO_DESCONTO && CSProdutos.Current.PRC_MAXIMO_DESCONTO > 0)
                    MessageBox.ShowShortMessageCenter(ActivityContext, "Atenção, percentual de desconto maior que \"" + CSProdutos.Current.PRC_MAXIMO_DESCONTO.ToString(CSGlobal.DecimalStringFormat) + "\".");
            }
            catch (System.Exception ex)
            {

            }
        }

        private static bool ValidaValorUnitarioMaiorQueValorTabela()
        {
            try
            {
                if (CSGlobal.StrToDecimal(txtValorUnitarioSemADF.Text) > CSGlobal.StrToDecimal(lblValorTabela.Text))
                {
                    MessageBox.ShowShortMessageCenter(ActivityContext, "O valor unitario nao pode ser maior que o valor de tabela.");
                    return false;
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaValorMinimoPraVenda()
        {
            try
            {
                if (string.Compare(CSEmpresa.Current.IND_BLOQUEIO_VALOR_MINIMO, "S") == 0)
                {
                    decimal VlrMinimoVenda = (CSGlobal.StrToDecimal(lblValorFinalItem.Text) - CSGlobal.StrToDecimal(lblValorAdicionalFinanceiro.Text));

                    if ((VlrMinimoVenda < CSProdutos.Current.VLR_MINIMO_PEDIDO))
                    {
                        MessageBox.ShowShortMessageCenter(ActivityContext, "Valor unitário de venda - adicional financeiro: " + VlrMinimoVenda.ToString(CSGlobal.DecimalStringFormat) + " menor que o permitido: " + CSProdutos.Current.VLR_MINIMO_PEDIDO.ToString(CSGlobal.DecimalStringFormat));
                        return false;

                    }
                    else
                    {
                        // Busca o valor que deve ser vendido a partir da quantidade
                        decimal VlrPrecoEscalonado = CSBloqueiosVendasEscalonadas.GetFaixaEscalonada(CSProdutos.Current, CSPDVs.Current, CSEmpregados.Current, Convert.ToInt32(GetQtdPedidaUnidade()));
                        if ((VlrPrecoEscalonado != -1) && ((CSGlobal.StrToDecimal(lblValorFinalItem.Text) - CSGlobal.StrToDecimal(lblValorAdicionalFinanceiro.Text)) < VlrPrecoEscalonado))
                        {
                            MessageBox.ShowShortMessageCenter(ActivityContext, "Valor unitário do produto fora da faixa de preço:" + VlrPrecoEscalonado.ToString(CSGlobal.DecimalStringFormat));
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaQuantidadePartida()
        {
            try
            {
                decimal QtdMinima = CSProdutos.Current.PRECOS_PRODUTO.Current.QTD_MINIMA_PEDIDA;
                int QtdEmbalagem = CSProdutos.Current.QTD_UNIDADE_EMBALAGEM;
                //QtdMinimaUni = 0;
                decimal qtdVenda = 0;

                if (CSProdutos.Current.COD_UNIDADE_MEDIDA != "CX" && CSProdutos.Current.COD_UNIDADE_MEDIDA != "DZ")
                {
                    //QtdMinimaUni = decimal.ToInt32(QtdMinima);
                    qtdVenda = CSGlobal.StrToDecimal(txtQtdeInteiro.Text);
                }
                else
                {
                    //QtdMinimaUni = decimal.ToInt32(QtdMinima - (QtdEmbalagem * ((QtdMinima / QtdEmbalagem) - Convert.ToDecimal(0.5))));
                    if (QtdMinima < QtdEmbalagem)
                        qtdVenda = CSGlobal.StrToDecimal(txtQtdeUnidade.Text);
                    else
                        qtdVenda = (CSGlobal.StrToInt(txtQtdeInteiro.Text) * QtdEmbalagem) + CSGlobal.StrToDecimal(txtQtdeUnidade.Text);
                }

                // Colocado se nao dava erro division by zero
                if (QtdMinima == 0)
                    return true;

                // Verifica se a quantidade unitaria informada é multiplo da quantidade minima
                if ((qtdVenda % QtdMinima) != 0)
                    return false;
                else
                    return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaQuantidadeMultipla()
        {
            try
            {
                if (CSProdutos.Current.PRECOS_PRODUTO.Current.BOL_QTDUNITARIA_MULTIPLA_MINIMO)
                {
                    if (CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO == 1 || CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO == 21 || (CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO != 1 && CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO != 21 && CSEmpresa.Current.IND_VALIDA_QTDMULTIPLA_NAO_VENDA == "S"))
                    {
                        if (!ValidaQuantidadePartida())
                        {
                            MessageBox.ShowShortMessageCenter(ActivityContext, "Quantidade de venda não é multipla da quantidade minima: \"" + CSProdutos.Current.PRECOS_PRODUTO.Current.QTD_MINIMA_PEDIDA.ToString() + "\".");
                            return false;
                        }
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static decimal CalculaValorSaldoDesconto()
        {
            try
            {
                decimal valorSaldoDesconto, valorVenda, valorTabela;
                CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto precoProduto = null;
                CSItemsPedido.CSItemPedido itempedido = null;

                itempedido = new CSItemsPedido.CSItemPedido();

                // [ Pega o saldo atual do vendedor ]
                valorSaldoDesconto = CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.VAL_SALDO_DESCONTO;

                if (CSEmpresa.Current.TIPO_CALCULO_VERBA != CSEmpresa.CALCULO_VERBA.NENHUM)
                {
                    itempedido.PRODUTO = CSProdutos.Current;
                    itempedido.QTD_PEDIDA_INTEIRA = CSGlobal.StrToDecimal(txtQtdeInteiro.Text);
                    itempedido.QTD_PEDIDA_UNIDADE = CSGlobal.StrToInt(txtQtdeUnidade.Text);

                    // [ Pega o valor do produto vendido sem o adicional ]
                    valorVenda = CSGlobal.StrToDecimal(lblValorTotalItem.Text) - CSGlobal.StrToDecimal(lblValorAdicionalFinanceiro.Text);

                    // Preenche o valor de desconto
                    // valor do desconto total * qtde unitaria geral
                    itempedido.VLR_DESCONTO =
                        (CSGlobal.StrToDecimal(lblValorDescontoUnitario.Text) * CSGlobal.StrToDecimal(txtQtdeInteiro.Text)) +
                        (((CSGlobal.StrToDecimal(lblValorDescontoUnitario.Text) / itempedido.PRODUTO.QTD_UNIDADE_EMBALAGEM) * CSGlobal.StrToDecimal(txtQtdeUnidade.Text)));

                    // [ Pega o valor de tabela do produto ]
                    valorTabela = valorVenda + itempedido.VLR_DESCONTO;

                    // [ Preenche o codigo da tabela escolhido ]
                    precoProduto = (CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto)((CSItemCombo)cboTabelaPreco.SelectedItem).Valor;

                    // [ Calcula do valor da verba extra ]
                    itempedido.VLR_VERBA_EXTRA = (precoProduto.VLR_VERBA_EXTRA * itempedido.QTD_PEDIDA_TOTAL) / itempedido.PRODUTO.UNIDADES_POR_CAIXA;

                    // [ Calcula o valor da verba normal ]
                    switch (CSEmpresa.Current.TIPO_CALCULO_VERBA)
                    {
                        case CSEmpresa.CALCULO_VERBA.PERCENTUAL_VALOR_PEDIDO:
                            {
                                // [ Valor da venda é menor que (preço tabela - % verba)? ]
                                if (valorVenda < ((valorTabela / (100 + CSEmpresa.Current.PCT_VERBA_NORMAL)) * 100) &&
                                    !CSEmpresa.Current.IND_ATUALIZAR_VERBA_PED_ABAIXOMINIMO)
                                {
                                    itempedido.VLR_VERBA_NORMAL = 0;

                                }
                                else
                                {
                                    itempedido.VLR_VERBA_NORMAL = CSGlobal.Round((valorVenda * CSEmpresa.Current.PCT_VERBA_NORMAL) / 100, 2);
                                }

                                break;
                            }

                        case CSEmpresa.CALCULO_VERBA.DIFERENCA_VALOR_TABELA:
                            {
                                itempedido.VLR_VERBA_NORMAL = CSGlobal.Round(valorVenda - ((valorTabela / (100 + CSEmpresa.Current.PCT_VERBA_NORMAL)) * 100), 2);

                                break;
                            }
                    }

                    // [ Permite atualização do saldo para verba normal? ]
                    if (!CSEmpresa.Current.IND_VLR_VERBA_PEDIDO_NOVO_ATUSALDO)
                        itempedido.VLR_VERBA_NORMAL = 0;

                    // [ Atualiza saldo descontando valores anteriores ]
                    valorSaldoDesconto += itempedido.VLR_VERBA_NORMAL - CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_VERBA_NORMAL;

                    // [ Permite atualização do saldo para verba extra? ]
                    if (!CSEmpresa.Current.IND_VLR_VERBA_EXTRA_ATUSALDO)
                        itempedido.VLR_VERBA_EXTRA = 0;

                    // [ Atualiza saldo descontando valores anteriores ]
                    valorSaldoDesconto += itempedido.VLR_VERBA_EXTRA - CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_VERBA_EXTRA;

                    // [ Atualizou o saldo anteriormente? ]
                    if (CSPDVs.Current.PEDIDOS_PDV.Current.IND_VLR_DESCONTO_ATUSALDO)
                        valorSaldoDesconto += CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_DESCONTO;

                    // [ Permite atualização do saldo para desconto? ]
                    if (CSEmpresa.Current.TIPO_CALCULO_VERBA != CSEmpresa.CALCULO_VERBA.DIFERENCA_VALOR_TABELA)
                        valorSaldoDesconto -= itempedido.VLR_DESCONTO;
                }

                return valorSaldoDesconto;
            }
            catch (System.Exception ex)
            {
                return 0;
            }
        }

        private static bool ValidaSaldoParaConcederDesconto()
        {
            try
            {
                if (CSGlobal.StrToDecimal(txtDescIncond.Text) == 0 &&
                    CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO.COD_TIPO_CONDICAO_PAGAMENTO == 1)
                {
                    return true;
                }
                else
                {
                    saldoDesconto = CalculaValorSaldoDesconto();

                    if (CSEmpresa.Current.IND_LIMITE_DESCONTO && saldoDesconto < 0)
                    {
                        MessageBox.ShowShortMessageCenter(ActivityContext, "Desconto não poderá ser dado, pois este ultrapassa o valor do saldo para desconto.");

                        return false;
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaPercentualDeLucratividade()
        {
            try
            {
                decimal pctLucratividade = 0;
                if (CSEmpresa.Current.TIPO_CALCULO_LUCRATIVIDADE == 1 || CSEmpresa.Current.TIPO_CALCULO_LUCRATIVIDADE == 2)
                {
                    pctLucratividade = CalculaPercentualLucratividade();

                    if (pctLucratividade < CSProdutos.Current.PCT_MINIMO_LUCRATIVIDADE)
                    {

                        if (CSEmpresa.Current.IND_VISUALIZA_LUCRATIVIDADE.Trim() == "S")
                            MessageBox.ShowShortMessageCenter(ActivityContext, "Percentual de lucratividade calculado:[" + pctLucratividade.ToString(CSGlobal.DecimalStringFormat) + "] menor que o minímo permitido para este produto:[" + CSProdutos.Current.PCT_MINIMO_LUCRATIVIDADE.ToString(CSGlobal.DecimalStringFormat) + "].");
                        else
                            MessageBox.ShowShortMessageCenter(ActivityContext, "Percentual de lucratividade calculado menor que o minímo permitido para este produto.");

                        if (CSEmpresa.Current.CONFIGURACAO_LUCRATIVIDADE_ITEM_PEDIDO == 'B')
                            return false;
                    }
                }
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaBloqueios()
        {
            try
            {
                if (IsBroker() &&
                    CSGlobal.PedidoComCombo)
                    return true;

                string[] aBloqueios = CSProdutos.Current.PRECOS_PRODUTO.Current.GetRetornaBloqueios(CSPDVs.Current, CSPDVs.Current.PEDIDOS_PDV.Current);

                //Remove valores nulos
                aBloqueios = aBloqueios.Where(b => !string.IsNullOrEmpty(b)).ToArray();

                //Se teve bloqueio/advertencia
                if (aBloqueios.Length > 0)
                {
                    if (aBloqueios[0] != "")
                    {
                        ((Cliente)ActivityContext).BloqueioProduto(aBloqueios, false);
                    }

                    if (aBloqueios[0] == "B")
                        return false;
                }
                string[] aBloqueiosProduto = CSProdutos.Current.PRECOS_PRODUTO.Current.GetRetornaBloqueiosProduto(CSPDVs.Current, CSPDVs.Current.PEDIDOS_PDV.Current, CSProdutos.Current.COD_PRODUTO);

                aBloqueiosProduto = aBloqueiosProduto.Where(b => b != null).ToArray();

                if (aBloqueiosProduto[0] != "")
                {
                    ((Cliente)ActivityContext).BloqueioProduto(aBloqueiosProduto, true);
                }

                if (aBloqueiosProduto[0] == "A" || aBloqueiosProduto[0] == "")
                    return true;
                else
                    return false;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
    }
}