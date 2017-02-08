using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using AvanteSales.Pro.Formatters;
using AvanteSales.SystemFramework;
using AvanteSales.SystemFramework.CSPDV;
using SQLiteDataReader = Mono.Data.Sqlite.SqliteDataReader;

namespace AvanteSales.Pro.Fragments
{
    public class Pedido : Android.Support.V4.App.Fragment
    {
        static LayoutInflater thisLayoutInflater;
        private const int frmIndenizacao = 0;
        private const int frmProdutoRamoAtividade = 1;
        private const int frmMensagemPedido = 2;
        private const int frmIndenizacao_validacao = 3;
        private const int frmListaProdutosPedido = 4;
        private const int frmProdutos = 5;
        private const int frmPedidoSugerido = 6;
        private const int frmProdutosIndicados = 7;
        private const int frmFoto = 8;
        private string NomeFoto;
        public static decimal Vlr_antes_exclusao = 0m;
        private static bool CarregandoDados;
        #region [ Controles ]

        private TextView lblPolitica;
        private static TextView lblPedido;
        private TextView lblDataEntrega;
        private TextView lblEmissor;
        private static Spinner cboEmissor;
        private static Spinner cboOperacao;
        private static Spinner cboCondicao;
        private static TextView lblAdf;
        private static TextView lblVlrDesc;
        private static TextView lblSaldoCred;
        private static TextView lblTotalPed;
        private TextView lblSaldoDct;
        private static TextView lblSaldoDesconto;
        private TextView lblDescPctLucratividade;
        private static TextView lblPctLucratividade;
        private TextView lblDescontoIndenizacao;
        private static TextView txtDescontoIndenizacao;
        private static CheckBox chkIndenizacao;
        private static Button btnListaProdutos;
        private Button btnNovoProduto;
        private static TextView lblVolume;
        private static TextView lblPesoBruto;
        static ProgressDialog progressDialogVerificar;
        static ProgressDialog progressDialogIndicados;
        static ProgressDialog progressDialogRecalculo;

        #endregion

        #region [ Variáveis ]

        private static bool m_IsDirty = false;
        private static bool IsLoading;
        private static object condicaoPagamentoAnterior;
        private static bool blIndenizacaoItem = true;
        private static bool blEstoqueProntaEntrega = false;

        private static ProgressDialog progressDialog;
        static Android.Support.V4.App.FragmentActivity ActivityContext;
        #endregion

        #region [ Propriedades ]

        public static bool PEDIDO_PENDENTE;

        private static bool IsDirty
        {
            get
            {
                return m_IsDirty;
            }
            set
            {
                if (IsLoading)
                    return;
                if (value == true)
                {
                    if (CSPDVs.Current.PEDIDOS_PDV.Current.STATE != ObjectState.NOVO)
                        CSPDVs.Current.PEDIDOS_PDV.Current.STATE = ObjectState.ALTERADO;
                }
                m_IsDirty = value;
            }
        }

        internal static void ValidacoesSalvarPedido()
        {
            try
            {
                string alerta = string.Empty;

                //if (!CSEmpregados.Current.VendedorDentroExpediente(ref alerta))
                //{
                //    MessageBox.Alert(CurrentActivity, alerta, "OK",
                //        (_sender, _e) =>
                //        {
                //            EventoAtivado = false;
                //            SetResult(Result.FirstUser, new Intent().PutExtra("COD_PEDIDO", CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO));
                //            base.OnBackPressed();
                //        },false);
                //}
                //else
                Fechar();

            }
            catch (Exception ex)
            {
            }
        }

        #endregion

        private static bool ValidaQuantidadeItensNoPedidoMaiorQueZero()
        {
            try
            {
                if (CSPDVs.Current.PEDIDOS_PDV.Current != null)
                    return CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items.Cast<CSItemsPedido.CSItemPedido>().Count(p => p.STATE != ObjectState.DELETADO) > 0;
                else
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool ValidarComboCondicaoPagamento()
        {
            try
            {
                if (cboCondicao.Adapter.Count == 0)
                {
                    MessageBox.AlertErro(ActivityContext, "Criação de pedido impedida, informação de condição de pagamento incompleta.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool AdfMaiorQueValorFinalItem()
        {
            try
            {
                if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(it => it.VLR_ADICIONAL_UNITARIO > it.VLR_TOTAL_ITEM).Count() > 0)
                {
                    MessageBox.AlertErro(ActivityContext, "Valor de ADF maior que Valor total de venda do item: " + CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(it => it.VLR_ADICIONAL_UNITARIO > it.VLR_TOTAL_ITEM).Select(p => p.PRODUTO.COD_PRODUTO + " - " + p.PRODUTO.DSC_APELIDO_PRODUTO).FirstOrDefault());
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaDados()
        {
            try
            {
                if (!ValidarComboCondicaoPagamento())
                    return false;

                if (!ValidaBloqueios())
                    return false;

                if (AdfMaiorQueValorFinalItem())
                {
                    return false;
                }

                if (!ValidaQuantidadeItensNoPedidoMaiorQueZero())
                {
                    MessageBox.ShowShortMessageCenter(ActivityContext, "O pedido deve conter pelo menos um item.");
                    return false;
                }

                if (!ValidaValorDoPedidoComLimitePDV())
                    return false;

                return ValidaDados2();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaPercentualLucratividade()
        {
            try
            {
                if (CSEmpresa.Current.TIPO_CALCULO_LUCRATIVIDADE == 1 || CSEmpresa.Current.TIPO_CALCULO_LUCRATIVIDADE == 2)
                {
                    if (CSGlobal.StrToDecimal(lblPctLucratividade.Text) < CSEmpresa.Current.PCT_MINIMO_LUCRATIVIDADE)
                    {
                        if (CSEmpresa.Current.IND_VISUALIZA_LUCRATIVIDADE.Trim() == "S")
                            MessageBox.ShowShortMessageCenter(ActivityContext, "Percentual de lucratividade calculado:[" + lblPctLucratividade.Text + "] menor que o minímo permitido para este pedido:[" + CSEmpresa.Current.PCT_MINIMO_LUCRATIVIDADE.ToString(CSGlobal.DecimalStringFormat) + "].");
                        else
                            MessageBox.ShowShortMessageCenter(ActivityContext, "Percentual de lucratividade calculado menor que o minímo permitido para este pedido.");

                        if (CSEmpresa.Current.CONFIGURACAO_LUCRATIVIDADE_PEDIDO == 'B')
                            return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaDados2()
        {
            try
            {
                if (!ValidaValorADF())
                    return false;

                if (!ValidaValorAbatimento())
                    return false;

                if (!ValidaPercentualLucratividade())
                    return false;

                if (!ValidaValorMinimoCondicaoPagamento())
                    return false;

                return ValidaDados3();
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaValorMinimoCondicaoPagamento()
        {
            try
            {
                CSPedidosPDV.CSPedidoPDV pedido = CSPDVs.Current.PEDIDOS_PDV.Current;

                if (CSGlobal.PedidoComCombo)
                {
                    var codCombo = pedido.ITEMS_PEDIDOS[0].PRODUTO.COD_PRODUTO_CONJUNTO;

                    var combo = CSProdutos.Items.Cast<CSProdutos.CSProduto>().Where(p => p.COD_PRODUTO == codCombo).FirstOrDefault();

                    if (combo.IND_LIBERAR_CONDICAO_PAGAMENTO)
                        return true;
                }

                if (pedido.VLR_TOTAL_PEDIDO < pedido.CONDICAO_PAGAMENTO.VLR_MINIMO_PEDIDO)
                {
                    string mensagem = "Pedido com valor abaixo do mínimo da condição de pagamento!";

                    // [ se for apenas advertência... ]
                    if (pedido.CONDICAO_PAGAMENTO.TPO_MENSAGEM_ABAIXO_MINIMO == CSCondicoesPagamento.TipoMensagemAbaixoMinimo.ADVERTENCIA)
                    {
                        MessageBox.Alert(ActivityContext, mensagem + " Deseja continuar desta maneira?", "Valor Mínimo", ValidaValorMinimoCondicaoPagamento_Click_Yes, "Cancelar", null, true);
                        return false;
                    }
                    else
                    {
                        MessageBox.ShowShortMessageCenter(ActivityContext, mensagem);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool SalvarPedido()
        {
            try
            {
                if (blIndenizacaoItem == false)
                    CSPDVs.Current.PEDIDOS_PDV.Current.CalculaRateioIndenizacao(CSGlobal.StrToDecimal(txtDescontoIndenizacao.Text));

                if (CSGlobal.PedidoSugerido)
                {
                    CSGlobal.ExisteProdutoColetadoPerda = false;
                }

                if ((CSItemCombo)cboCondicao.SelectedItem != null)
                {
                    foreach (CSItemsPedido.CSItemPedido itemPedido in CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS)
                    {
                        itemPedido.PRC_ADICIONAL_FINANCEIRO = ((CSCondicoesPagamento.CSCondicaoPagamento)((CSItemCombo)cboCondicao.SelectedItem).Valor).PRC_ADICIONAL_FINANCEIRO;

                        if (CSGlobal.PedidoSugerido)
                        {
                            if (itemPedido.QTD_INDENIZACAO_EXIBICAO > 0)
                                CSGlobal.ExisteProdutoColetadoPerda = true;
                        }
                    }

                    CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO = ((CSCondicoesPagamento.CSCondicaoPagamento)((CSItemCombo)cboCondicao.SelectedItem).Valor).COD_CONDICAO_PAGAMENTO;
                }

                if (!chkIndenizacao.Checked)
                {
                    CSPDVs.Current.PEDIDOS_PDV.Current.NUM_DOC_INDENIZACAO = "";
                    CSPDVs.Current.PEDIDOS_PDV.Current.COD_MOT_INDENIZACAO = -1;
                    CSPDVs.Current.PEDIDOS_PDV.Current.COD_TIPO_MOT_INDENIZACAO = -1;
                }

                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                    Emissor();
                else
                {
                    //// Busca o emissor
                    //CSEmissoresPDV.CSEmissorPDV emissor = (CSEmissoresPDV.CSEmissorPDV)((CSItemCombo)cboEmissor.SelectedItem).Valor;

                    //CSPDVs.Current.PEDIDOS_PDV.Current.COD_PDV_SOLDTO = emissor.COD_PDV_SOLDTO;

                    CSPDVs.Current.PEDIDOS_PDV.Current.COD_PDV_SOLDTO = CSPDVs.Current.COD_PDV;
                }

                // Acerta Politica de calculo do preço
                CSPDVs.Current.PEDIDOS_PDV.Current.COD_POLITICA_CALCULO_PRECO = CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA;

                CSPDVs.Current.PEDIDOS_PDV.Flush();

                if (CSEmpresa.Current.IND_LIMITE_DESCONTO)
                    CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.Flush();

                // Atualiza Pedido de Indenização
                if ((CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2) &&
                    CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO == 20 &&
                    CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Count > 0)
                {
                    // Atualiza Pedido de Indenização                    
                    foreach (CSPedidosIndenizacao.CSPedidoIndenizacao pedidoIndenizacao in CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Items)
                    {
                        if (pedidoIndenizacao.COD_PEDIDO == CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO)
                            CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Current = pedidoIndenizacao;
                    }

                    CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Flush();
                }

                // Mostra o codigo do pedido salvo
                ActivityContext.RunOnUiThread(() => { lblPedido.Text = CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO.ToString(); });

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static void Emissor()
        {
            try
            {
                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                {
                    var emissorSelecionado = ((CSEmissoresPDV.CSEmissorPDV)(((CSItemCombo)cboEmissor.SelectedItem).Valor)).COD_PDV_SOLDTO;

                    CSPDVs.Current.PEDIDOS_PDV.Current.COD_PDV_SOLDTO = emissorSelecionado;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static void FlushPedido()
        {
            try
            {
                if (CSEmpresa.Current.IND_UTILIZA_FLEXX_GPS == "S")
                {
                    if (!string.IsNullOrEmpty(CSPDVs.Current.DSC_NOME_FOTO) &&
                        !CSPDVs.Current.BOL_FOTO_VALIDADA)
                        CSPDVs.Current.GravarImagemValidada();
                }

                //AtualizaHoraDeSaidaDoPDV();
                MessageBox.ShowShortMessageBottom(ActivityContext, "Salvando pedido...");
                var pedidoSalvo = SalvarPedido();
                RefreshDadosTela();

                if (pedidoSalvo &&
                    CSEmpresa.UtilizaNovoAtributoPedido)
                    CSPedidosPDV.MARCAR_PEDIDO_SALVO_CORRETAMENTE(CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO);

                if (CSPDVs.Current.POSSUI_PEDIDO_PENDENTE)
                    CSPDVs.Current.POSSUI_PEDIDO_PENDENTE = false;

                if (CSGlobal.PedidoSugerido)
                {
                    CSGlobal.PedidoSugerido = false;
                }

                if (CSPDVs.Current.HISTORICOS_MOTIVO != null &&
                    CSPDVs.Current.HISTORICOS_MOTIVO.Items != null &&
                    CSPDVs.Current.HISTORICOS_MOTIVO.Items.Count > 0)
                {
                    foreach (CSHistoricosMotivo.CSHistoricoMotivo hismot in CSPDVs.Current.HISTORICOS_MOTIVO.Items)
                        hismot.STATE = ObjectState.DELETADO;

                    // Flush apagando o resgitros deletados
                    CSPDVs.Current.HISTORICOS_MOTIVO.Flush();

                    // TODO: Marcar o pdv como positivado
                    CSPDVs.Current.IND_POSITIVADO = true;
                }

                CSGlobal.BloquearSaidaCliente = false;
                CSPDVs.Current.PEDIDOS_PDV.Current = null;
                ((Cliente)ActivityContext).AbrirPedidos();
            }
            catch (Exception ex)
            {
            }
        }

        private static void SalvarPedidoEmail()
        {
            try
            {
                if (CSEmpresa.Current.IND_UTILIZA_ENVIO_EMAIL.ToUpper() == "N")
                {
                    CSPDVs.Current.PEDIDOS_PDV.Current.IND_EMAIL_ENVIAR = false;
                    FlushPedido();
                }
                else
                {
                    if (CSPDVs.Current.EMAILS.Cast<CSPDVEmails.CSPDVEmail>().Where(em => em.COD_TIPO_EMAIL == 2).Count() > 0)
                    {
                        MessageBox.Alert(ActivityContext, "Deseja enviar e-mail deste pedido?", "Enviar",
                            (_sender, _e) =>
                            {
                                CSPDVs.Current.PEDIDOS_PDV.Current.IND_EMAIL_ENVIAR = true;
                                FlushPedido();

                            }, "Cancelar", (_sender, _e) =>
                             {
                                 CSPDVs.Current.PEDIDOS_PDV.Current.IND_EMAIL_ENVIAR = false;
                                 FlushPedido();
                             }, false);
                    }
                    else
                    {
                        CSPDVs.Current.PEDIDOS_PDV.Current.IND_EMAIL_ENVIAR = false;
                        FlushPedido();
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        protected static void ValidaValorMinimoCondicaoPagamento_Click_Yes(object sender, DialogClickEventArgs e)
        {
            try
            {
                if (ValidaDados3())
                {
                    SalvarPedidoEmail();
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static bool ValidaClienteEmissor()
        {
            try
            {
                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2 &&
                    ((TextView)cboEmissor.GetChildAt(0)).Text == string.Empty)
                {
                    MessageBox.ShowShortMessageCenter(ActivityContext, "Selecione um emissor");
                    cboEmissor.RequestFocus();
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaDesconto()
        {
            try
            {
                CSPedidosPDV.CSPedidoPDV pedido = CSPDVs.Current.PEDIDOS_PDV.Current;

                decimal desconto = CSGlobal.StrToDecimal(txtDescontoIndenizacao.Text);

                if (desconto > 0)
                {
                    var pctMaximoIndenizacao = CSPDVs.Current.PCT_MAXIMO_INDENIZACAO <= 0 ? CSEmpresa.Current.PCT_MAXIMO_INDENIZACAO : CSPDVs.Current.PCT_MAXIMO_INDENIZACAO;

                    // [ Pega o valor total do pedido ]
                    decimal valorTotal = pedido.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(pd => pd.STATE != ObjectState.DELETADO).Sum(p => p.VLR_TOTAL_ITEM) - pedido.VLR_ADICIONAL_FINANCEIRO;
                    decimal descontoMaximo = (valorTotal * pctMaximoIndenizacao) / 100;
                    decimal valorSaldoVerba = pedido.EMPREGADO.VAL_SALDO_DESCONTO;

                    // [ Verifica saldo do vendedor ]
                    if (pedido.IND_VLR_INDENIZACAO_ATUSALDO)
                        valorSaldoVerba += pedido.VLR_INDENIZACAO;

                    // [ Verifica valor máximo permitido ]
                    if (desconto > descontoMaximo)
                    {
                        if (chkIndenizacao.Checked)
                            MessageBox.ShowShortMessageCenter(ActivityContext, "Valor da indenização [" + desconto.ToString(CSGlobal.DecimalStringFormat) + "] é maior que o permitido [" + descontoMaximo.ToString(CSGlobal.DecimalStringFormat) + "].");
                        else
                            MessageBox.ShowShortMessageCenter(ActivityContext, "Desconto incondicional [" + desconto.ToString(CSGlobal.DecimalStringFormat) + "] é maior que o permitido [" + descontoMaximo.ToString(CSGlobal.DecimalStringFormat) + "].");

                        return false;

                        // [ TODO: Verifica valor saldo da verba ]
                    }
                    else if (desconto > valorSaldoVerba &&
                            CSEmpresa.Current.TIPO_CALCULO_VERBA != 3)
                    {
                        if (chkIndenizacao.Checked)
                            MessageBox.ShowShortMessageCenter(ActivityContext, "Valor da indenização [" + desconto.ToString(CSGlobal.DecimalStringFormat) + "] é maior que o saldo da verba [" + valorSaldoVerba.ToString(CSGlobal.DecimalStringFormat) + "].");
                        else
                            MessageBox.ShowShortMessageCenter(ActivityContext, "Desconto incondicional [" + desconto.ToString(CSGlobal.DecimalStringFormat) + "] é maior que o saldo da verba [" + valorSaldoVerba.ToString(CSGlobal.DecimalStringFormat) + "].");

                        return false;

                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaDados3()
        {
            try
            {
                if (!ValidaClienteEmissor())
                    return false;

                if (!ValidaDesconto())
                    return false;

                if (!ValidaPedidoIndenizacao())
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaPedidoIndenizacao()
        {
            try
            {
                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2 &&
                    CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO == 20)
                {
                    if (CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Count == 0)
                    {
                        MessageBox.ShowShortMessageCenter(ActivityContext, "Informações referente a nota de devolução da indenização não informada.");
                        return false;
                    }

                    if (CSPDVs.Current.PEDIDOS_PDV.Current.VLR_TOTAL_PEDIDO > CSEmpresa.Current.VAL_LIMNF_IDZ)
                    {
                        if (CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Current.NUM_DOCUMENTO_DEVOLUCAO == 0 ||
                            CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Current.VALOR_DOCUMENTO_DEVOLUCAO == 0)
                        {
                            MessageBox.ShowShortMessageCenter(ActivityContext, "Informações referente a nota de devolução da indenização não informada.");
                            return false;
                        }
                    }

                    if (CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Current.COD_MOTIVO_DESTINO == -1 ||
                        CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Current.COD_MOTIVO_CONDICAO_RETIRADA == -1 ||
                        CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDOS_INDENIZACAO.Current.COD_MOTIVO_RESUMO_PROBLEMA == -1)
                    {

                        MessageBox.ShowShortMessageCenter(ActivityContext, "Informações referente a nota de devolução da indenização não informada.");
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaValorAbatimento()
        {
            try
            {
                if (CSGlobal.StrToDecimal(lblTotalPed.Text) <= 0)
                {
                    MessageBox.ShowShortMessageCenter(ActivityContext, "O valor do abatimento e maior ou igual ao valor do pedido.");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private static bool ValidaValorADF()
        {
            try
            {
                if (CSGlobal.StrToDecimal(lblAdf.Text) > 100)
                {
                    MessageBox.ShowShortMessageCenter(ActivityContext, "O valor do ADF não pode ser maior que 100%");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        protected static void ValidaValorDoPedidoComLimitePDV_Click_Yes(object sender, DialogClickEventArgs e)
        {
            try
            {
                if (ValidaDados2())
                {
                    SalvarPedidoEmail();
                    //this.ShowShortMessageBottom("Salvando pedido...");
                    //var pedidoSalvo = SalvarPedido();
                    //RefreshDadosTela();
                    //SetResultPedido(pedidoSalvo);

                    //if (pedidoSalvo &&
                    //   CSEmpresa.UtilizaNovoAtributoPedido)
                    //    CSPedidosPDV.MARCAR_PEDIDO_SALVO_CORRETAMENTE(CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO);

                    //if (CSPDVs.Current.POSSUI_PEDIDO_PENDENTE)
                    //    CSPDVs.Current.POSSUI_PEDIDO_PENDENTE = false;

                    //if (CSGlobal.PedidoSugerido &&
                    //    !CSGlobal.PedidoTroca)
                    //    CSGlobal.AcabouDeGerarPedidoSugerido = true;

                    //Finish();
                }
            }
            catch (Exception ex)
            {
            }
        }

        protected static void ValidaValorDoPedidoComLimitePDV_Click_No(object sender, DialogClickEventArgs e)
        {
            try
            {
                if (CSGlobal.PedidoSugerido)
                    CSGlobal.AcabouDeGerarPedidoSugerido = false;

                DeletaPedidoAtual();
            }
            catch (Exception ex)
            {
            }
        }

        public static void DeletaPedidoAtual()
        {
            try
            {
                CSPDVs.Current.PEDIDOS_PDV.Current.STATE = ObjectState.DELETADO;

                CSPDVs.Current.PEDIDOS_PDV.Flush();
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS = null;

                if (CSEmpresa.Current.IND_LIMITE_DESCONTO)
                    CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO.Flush();

                ((Cliente)ActivityContext).AbrirPedidos();
            }
            catch (Exception ex)
            {
            }
        }

        private static bool ValidaValorDoPedidoComLimitePDV()
        {
            try
            {
                CSPedidosPDV.CSPedidoPDV pedido = CSPDVs.Current.PEDIDOS_PDV.Current;


                decimal vlrLimiteCreditoValidar = CSPDVs.Current.VLR_SALDO_CREDITO_ATUALIZADO;

                // Se a condicao de pagamento anterior for Venda, ele soma
                if ((pedido.STATE == ObjectState.ALTERADO && condicaoPagamentoAnterior != null && ((CSCondicoesPagamento.CSCondicaoPagamento)condicaoPagamentoAnterior).COD_TIPO_CONDICAO_PAGAMENTO == 2))
                    vlrLimiteCreditoValidar += pedido.VLR_TOTAL_PEDIDO_INALTERADO;
                else
                    vlrLimiteCreditoValidar += pedido.VLR_TOTAL_PEDIDO;

                // TODO: verificar se o limite de credito é para todos os pedidos do PDV ou somente o atual
                if (pedido.CONDICAO_PAGAMENTO.COD_TIPO_CONDICAO_PAGAMENTO == 2 && (pedido.VLR_TOTAL_PEDIDO > vlrLimiteCreditoValidar))
                {
                    if (CSEmpresa.Current.IND_LIMITE_CREDITO == "N")
                    {
                        MessageBox.ShowShortMessageCenter(ActivityContext, "O valor do pedido ultrapassa o saldo de crédito do PDV");
                        return false;
                    }
                    else
                    {
                        MessageBox.Alert(ActivityContext, "O valor do pedido ultrapassa o saldo de crédito do PDV.\nDeseja continuar?\nCaso não continue todas as alterações feitas no pedido serão descartadas.", "Salvar", ValidaValorDoPedidoComLimitePDV_Click_Yes, "Cancelar", ValidaValorDoPedidoComLimitePDV_Click_No, false);
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        protected static void Fechar_Click_Yes(object sender, DialogClickEventArgs e)
        {
            try
            {
                if (ValidaDados())
                {
                    SalvarPedidoEmail();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void Fechar()
        {
            try
            {
                if (ValidaQuantidadeItensNoPedidoMaiorQueZero())
                {
                    Fechar_Click_Yes(null, null);
                }
                else if (PdvSemPedido())
                {
                    ((Cliente)ActivityContext).NavegarParaPasso(1);
                    ((Cliente)ActivityContext).OnBackPressed();
                }
                else
                {
                    if (CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDO_EDITADO)
                        DeletaPedidoAtual();
                    else
                    {
                        CSPDVs.Current.PEDIDOS_PDV.Current = null;
                        ((Cliente)ActivityContext).AbrirPedidos();
                    }
                }
                //else
                //{
                //    if (cboCondicao.Adapter.Count == 0 &&
                //        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Count == 0)
                //    {
                //        EventoAtivado = false;
                //        SetResult(Result.Canceled, new Intent().PutExtra("COD_PEDIDO", CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO));
                //        base.OnBackPressed();
                //    }
                //    else
                //    {
                //        if (CSPDVs.Current.PEDIDOS_PDV.Current != null)
                //        {
                //            EventoAtivado = false;
                //            DeletaPedidoAtual();
                //            //this.ShowShortMessageCenter("Salvando pedido...");
                //            SetResult(Result.Canceled, new Intent().PutExtra("COD_PEDIDO", CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO));
                //        }
                //        base.OnBackPressed();
                //    }
                //}
            }
            catch (Exception ex)
            {
            }
        }

        public static bool PdvSemPedido()
        {
            return CSPDVs.Current.PEDIDOS_PDV.Items.Count == 0;
        }

        public override void OnResume()
        {
            if (CSPDVs.Current.PEDIDOS_PDV.Current != null &&
                CSPDVs.Current.PEDIDOS_PDV.Current.PEDIDO_EDITADO)
                cboOperacao.Enabled = false;

            IsLoading = true;

            if (CSEmpresa.UtilizaNovoAtributoPedido)
                PEDIDO_PENDENTE = true;

            if (!CarregandoDados)
            {
                progressDialog = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
                progressDialog.Show();

                new ThreadAtualizarControles().Execute();
            }

            base.OnResume();
        }

        private class ThreadAtualizarControles : AsyncTask
        {
            string vlrProds;
            string saldoCred;
            string saldoDesconto;
            string strVolume;
            int qtdProdutos;
            string strPeso;

            private static bool ExisteModificacaoEmAlgumProduto()
            {
                try
                {
                    if (CSPDVs.Current.PEDIDOS_PDV.Current != null)
                        return CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Count(c => c.STATE != ObjectState.INALTERADO) > 0;
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                vlrProds = CSPDVs.Current.PEDIDOS_PDV.Current.VLR_TOTAL_PEDIDO.ToString(CSGlobal.DecimalStringFormat);
                saldoCred = CSPDVs.Current.VLR_SALDO_CREDITO_ATUALIZADO.ToString(CSGlobal.DecimalStringFormat);
                saldoDesconto = CSEmpregados.Current.VAL_SALDO_DESCONTO.ToString(CSGlobal.DecimalStringFormat);

                if (ExisteModificacaoEmAlgumProduto())
                    IsDirty = true;

                strVolume = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(c => c.STATE != ObjectState.DELETADO).Sum(c => c.QTD_PEDIDA_INTEIRA).ToString();

                qtdProdutos = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(c => c.STATE != ObjectState.DELETADO).Count();

                strPeso = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(c => c.STATE != ObjectState.DELETADO).Sum(c => Math.Round((c.PRODUTO.VLR_PESO_PRODUTO * c.QTD_PEDIDA_TOTAL) / c.PRODUTO.QTD_UNIDADE_EMBALAGEM, 2)).ToString(CSGlobal.DecimalStringFormat);

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                lblTotalPed.Text = vlrProds;
                lblSaldoCred.Text = saldoCred;
                lblSaldoDesconto.Text = saldoDesconto;

                // [ Busca o valor do desconto incondicional ou indenização ]
                if ((CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO != -1 || CSGlobal.PedidoSugerido) && CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Count > 0)
                {
                    // [ Verifica se é uma indenização ][Desativado]
                    chkIndenizacao.Checked = true;
                    if (chkIndenizacao.Text == "Abatimento?")
                    {
                        txtDescontoIndenizacao.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items.Cast<CSItemsPedido.CSItemPedido>().Where(it => it.STATE != ObjectState.DELETADO).Sum(p => p.VLR_INDENIZACAO_UNIDADE).ToString(CSGlobal.DecimalStringFormat);
                    }
                    else
                    {
                        txtDescontoIndenizacao.Text = CSPDVs.Current.PEDIDOS_PDV.Current.VLR_INDENIZACAO.ToString(CSGlobal.DecimalStringFormat);
                    }
                }
                else
                {
                    txtDescontoIndenizacao.Text = 0.ToString(CSGlobal.DecimalStringFormat);
                    chkIndenizacao.Checked = true;
                }

                lblVolume.Text = strVolume;

                AlteraTextoBtnProdutos(qtdProdutos);
                lblPesoBruto.Text = strPeso;

                CarregandoDados = false;

                progressDialog.Dismiss();
                //new ThreadTempoRestante().Execute();
            }
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.pedido, container, false);
            FindViewsById(view);
            Eventos();
            ConfiguraTela();
            thisLayoutInflater = inflater;
            return view;
        }

        private void LimpaVariaveisEstaticas()
        {
            blIndenizacaoItem = true;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                CarregandoDados = true;
                IsLoading = true;
                ActivityContext = ((Cliente)Activity);
                LimpaVariaveisEstaticas();
                cboOperacao.Adapter = null;
                cboCondicao.Adapter = null;

                if (CSPDVs.Current.PEDIDOS_PDV.Current == null)
                {
                    var pedido = new CSPedidosPDV.CSPedidoPDV();
                    CSPDVs.Current.PEDIDOS_PDV.Current = pedido;
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS = null;
                    CSPDVs.Current.PEDIDOS_PDV.Current.STATE = ObjectState.NOVO;
                    CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO = CSEmpregados.Current;
                    CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO = CSPDVs.Current.OPERACOES.Cast<CSOperacoes.CSOperacao>().Where(o => o.COD_OPERACAO_CFO == 1).FirstOrDefault();
                    CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO = CSCondicoesPagamento.GetCondicaPagamento(CSPDVs.Current.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO);
                }

                if (CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO == -1)
                {
                    lblPedido.Text = "<Novo>";
                    CSPDVs.Current.PEDIDOS_PDV.Current.EMPREGADO = CSEmpregados.Current;
                    lblDataEntrega.Text = CSEmpresa.Current.DATA_ENTREGA.ToString("dd/MM/yy");

                    if (CSEmpresa.Current.IND_UTILIZA_FLEXX_GPS == "S")
                    {
                        if (string.IsNullOrEmpty(CSPDVs.Current.PEDIDOS_PDV.Current.NUM_LATITUDE_LOCALIZACAO))
                        {
                            CSPDVs.Current.PEDIDOS_PDV.Current.NUM_LATITUDE_LOCALIZACAO = CSGlobal.GetLatitudeFlexxGPS();
                            CSPDVs.Current.PEDIDOS_PDV.Current.NUM_LONGITUDE_LOCALIZACAO = CSGlobal.GetLongitudeFlexxGPS();
                        }
                    }
                }
                else
                {
                    lblPedido.Text = CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO.ToString();
                    lblDataEntrega.Text = CSPDVs.Current.PEDIDOS_PDV.Current.DATA_ENTREGA.ToString("dd/MM/yy");

                    if (CSGlobal.PedidoComCombo &&
                        CSPDVs.Current.POSSUI_PEDIDO_PENDENTE)
                    {
                        RecalculaValoresPedido();
                    }
                }

                Inicializacao(Arguments.GetBoolean("edicaoPedido", false));
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(Activity, ex.Message);
            }

            base.OnViewCreated(view, savedInstanceState);
        }

        public void RecalculaValoresPedido()
        {
            try
            {
                if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items.Count > 0 /*&& CSGlobal.CalculaPrecoNestle*/)
                {
                    progressDialogRecalculo = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
                    progressDialogRecalculo.Show();

                    if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2 &&
                       blIndenizacaoItem == false)
                        CSPDVs.Current.PEDIDOS_PDV.Current.DesfazRateioIndenizacao();

                    ThreadPool.QueueUserWorkItem(o => ThreadRecalculaValoresPedido());
                }
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(ActivityContext, ex.Message);
            }
        }

        private void Inicializacao(bool edicao)
        {
            progressDialog = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
            progressDialog.Show();

            if (!edicao &&
                !((Cliente)ActivityContext).MotivoNaoCompraProdutoIndicado &&
                  CSMotivos.ItemsMotivoIndicados.Count > 0 &&
                  CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(i => i.STATE != ObjectState.DELETADO).ToList().Count > 0)
                new ThreadMotivoIndicados().Execute();
            else
            {
                ((Cliente)ActivityContext).MotivoNaoCompraProdutoIndicado = false;
                new ThreadInicializacao().Execute();
            }
        }

        private class ThreadMotivoIndicados : AsyncTask
        {
            private bool IsBroker()
            {
                return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
            }

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                try
                {
                    var a = CSProdutos.Items;
                    int qtdProdutosIndicados = 0;

                    if (IsBroker())
                        qtdProdutosIndicados = CSProdutos.Items.Cast<CSProdutos.CSProduto>().Where(p => p.IND_PROD_ESPECIFICO_CATEGORIA).GroupBy(pd => pd.COD_PRODUTO).Count();
                    else
                        qtdProdutosIndicados = CSProdutos.Items.Cast<CSProdutos.CSProduto>().Where(p => p.IND_PROD_ESPECIFICO_CATEGORIA && p.PRECOS_PRODUTO != null && p.PRECOS_PRODUTO.Count > 0).GroupBy(pd => pd.COD_PRODUTO).Count();

                    int qtdProdutosIndicadosVendidos = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(i => i.STATE != ObjectState.DELETADO && i.PRODUTO.IND_PROD_ESPECIFICO_CATEGORIA).Count();

                    if (qtdProdutosIndicados == qtdProdutosIndicadosVendidos)
                    {
                        CSPDVs.Current.PEDIDOS_PDV.Current.COD_MOTIVO = null;
                    }
                    else
                    {
                        return 2;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.AlertErro(ActivityContext, ex.Message);
                }

                return 1;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                if (Convert.ToInt32(result) == 2)
                    ((Cliente)ActivityContext).MotivoNaoCompraIndicado();
                else
                    new ThreadInicializacao().Execute();
            }
        }

        private class ThreadInicializacao : AsyncTask
        {
            decimal totalDesconto = 0;
            decimal pctLucratividade = 0;
            decimal totalIndenizacaoPedido = 0;
            ArrayAdapter adapterEmissor;
            ArrayAdapter adapterOperacoes;
            int indexEmissor;
            int indexOperacao = -1;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                CarregaListViewItemPedido();

                // [ Carrega o combo com emissores do clientes ]
                CarregaComboEmissor();

                // [ Carrega o combo com as operações ]
                CarregaComboOperacoes();

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                lblVlrDesc.Text = totalDesconto.ToString(CSGlobal.DecimalStringFormat);
                lblPctLucratividade.Text = pctLucratividade.ToString(CSGlobal.DecimalStringFormat);
                txtDescontoIndenizacao.Text = totalIndenizacaoPedido.ToString(CSGlobal.DecimalStringFormat);

                cboEmissor.Adapter = adapterEmissor;
                cboOperacao.Adapter = adapterOperacoes;

                cboEmissor.SetSelection(indexEmissor);
                cboOperacao.SetSelection(indexOperacao);

                cboCondicao.Adapter = null;

                if (CSPDVs.Current != null &&
                    CSPDVs.Current.PEDIDOS_PDV.Current != null)
                    new ThreadSelectedOperacao().Execute();
                else
                    progressDialog.Dismiss();
            }

            private void CarregaComboOperacoes()
            {
                try
                {
                    adapterOperacoes = new ArrayAdapter(ActivityContext, Android.Resource.Layout.SimpleSpinnerItem);
                    adapterOperacoes.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                    int PositionTroca = 0;
                    blEstoqueProntaEntrega = false;

                    // Preenche o combo de operação
                    for (int i = 0; i < CSPDVs.Current.OPERACOES.Count; i++)
                    {
                        CSOperacoes.CSOperacao operacao = (CSOperacoes.CSOperacao)CSPDVs.Current.OPERACOES[i];

                        CSItemCombo ic = new CSItemCombo();

                        ic.Texto = operacao.DSC_OPERACAO;
                        ic.Valor = operacao;

                        if (ic.Texto.ToUpper() == "TROCA")
                            PositionTroca = i;

                        adapterOperacoes.Add(ic);

                        // Se não for um novo pedido seleciona a operação do pedido
                        if (CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO != -1)
                        {
                            if (CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO == operacao.COD_OPERACAO)
                            {
                                indexOperacao = i;
                                blEstoqueProntaEntrega = CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.IND_PRONTA_ENTREGA;
                            }
                        }
                    }

                    if (CSGlobal.PedidoTroca)
                        indexOperacao = PositionTroca;
                    else
                    {
                        if (indexOperacao == -1 && adapterOperacoes.Count > 0)
                            indexOperacao = 0;
                    }
                }
                catch (Exception ex)
                {

                }
            }

            private void CarregaComboEmissor()
            {
                try
                {
                    adapterEmissor = new ArrayAdapter(ActivityContext, Android.Resource.Layout.SimpleSpinnerItem);
                    adapterEmissor.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                    // Preenche o combo de emissor
                    for (int i = 0; i < CSPDVs.Current.EmissoresPDV.Count; i++)
                    {
                        CSEmissoresPDV.CSEmissorPDV emissor = CSPDVs.Current.EmissoresPDV[i];
                        CSItemCombo ic = new CSItemCombo();

                        ic.Texto = emissor.COD_PDV_SOLDTO.ToString() + " - " + emissor.DSC_RAZAO_SOCIAL;
                        ic.Valor = emissor;

                        adapterEmissor.Add(ic);

                        // Se não for um novo pedido seleciona emissor do pedido
                        if (CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO != -1)
                        {
                            if (CSPDVs.Current.PEDIDOS_PDV.Current.COD_PDV_SOLDTO == emissor.COD_PDV_SOLDTO)
                                indexEmissor = i;
                        }
                    }

                    if (cboEmissor.SelectedItem == null && adapterEmissor.Count > 0)
                        indexEmissor = 0;
                }
                catch (Exception ex)
                {

                }
            }

            private void CarregaListViewItemPedido()
            {
                try
                {
                    decimal totalVenda = 0;
                    decimal custoTotal = 0;
                    decimal totalIndenizacao = 0;

                    blIndenizacaoItem = true;

                    foreach (CSItemsPedido.CSItemPedido itempedido in CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items)
                    {
                        if (itempedido.STATE != ObjectState.DELETADO)
                        {
                            // [ Valor da Indenização do item ]                    
                            totalIndenizacao = itempedido.VLR_INDENIZACAO_UNIDADE;

                            totalVenda += itempedido.VLR_TOTAL_ITEM;

                            // Calculo do custo do produto com a caixa fechada
                            custoTotal += itempedido.PRODUTO.VLR_CUSTO_GERENCIAL * itempedido.QTD_PEDIDA_INTEIRA;
                            // Calculo do custo do produto com a caixa aberta
                            custoTotal += ((itempedido.PRODUTO.VLR_CUSTO_GERENCIAL / itempedido.PRODUTO.QTD_UNIDADE_EMBALAGEM) * itempedido.QTD_PEDIDA_UNIDADE);

                            if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                                totalDesconto += itempedido.PRC_DESCONTO_UNITARIO;
                            else
                                totalDesconto += itempedido.VLR_DESCONTO;

                            totalIndenizacaoPedido += totalIndenizacao;
                        }
                    }

                    pctLucratividade += GetPercentualLucratividade(totalVenda, custoTotal);

                    CSPDVs.Current.PEDIDOS_PDV.Current.VLR_INDENIZACAO = totalIndenizacaoPedido;
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void ThreadRecalculaValoresPedido()
        {
            try
            {
                var pedido = CSPDVs.Current.PEDIDOS_PDV.Current;


                pedido.RecalculaPedidoCondicaoPagamento();

                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2 &&
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Count > 0 && blIndenizacaoItem == false)
                    CSPDVs.Current.PEDIDOS_PDV.Current.CalculaRateioIndenizacao();

                RefreshDadosTela();

                IsDirty = true;
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (progressDialogRecalculo != null)
                {
                    progressDialogRecalculo.Dismiss();
                    progressDialogRecalculo.Dispose();
                }
            }
        }

        private static void RefreshDadosTela()
        {
            try
            {
                ActivityContext.RunOnUiThread(() =>
                {
                    CarregaListViewItemPedido();

                    // Busca o valor total do pedido
                    lblTotalPed.Text = CSPDVs.Current.PEDIDOS_PDV.Current.VLR_TOTAL_PEDIDO.ToString(CSGlobal.DecimalStringFormat);

                    // Busca o valor do saldo de credito disponivel para o PDV
                    lblSaldoCred.Text = CSPDVs.Current.VLR_SALDO_CREDITO_ATUALIZADO.ToString(CSGlobal.DecimalStringFormat);

                    // Busca o valor do saldo de desconto disponivel para o EMPREGADO
                    lblSaldoDesconto.Text = CSEmpregados.Current.VAL_SALDO_DESCONTO.ToString(CSGlobal.DecimalStringFormat);

                    lblVolume.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(c => c.STATE != ObjectState.DELETADO).Sum(c => c.QTD_PEDIDA_INTEIRA).ToString();

                    int qtdProdutos = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(c => c.STATE != ObjectState.DELETADO).Count();
                    AlteraTextoBtnProdutos(qtdProdutos);

                    lblPesoBruto.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(c => c.STATE != ObjectState.DELETADO).Sum(c => Math.Round((c.PRODUTO.VLR_PESO_PRODUTO * c.QTD_PEDIDA_TOTAL) / c.PRODUTO.QTD_UNIDADE_EMBALAGEM, 2)).ToString(CSGlobal.DecimalStringFormat);
                });
            }
            catch (Exception ex)
            {

            }
        }

        private static void AlteraTextoBtnProdutos(int qtdProdutos)
        {
            try
            {
                if (qtdProdutos == 0)
                {
                    btnListaProdutos.Visibility = ViewStates.Gone;
                }
                else
                {
                    btnListaProdutos.Visibility = ViewStates.Visible;
                    btnListaProdutos.Text = string.Format("Lista ({0})", qtdProdutos);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static void CarregaListViewItemPedido()
        {
            try
            {
                decimal totalDesconto = 0;
                decimal pctLucratividade = 0;
                decimal totalVenda = 0;
                decimal custoTotal = 0;
                decimal totalIndenizacao = 0;
                decimal totalIndenizacaoPedido = 0;

                //blIndenizacaoItem = false;
                blIndenizacaoItem = true;

                // Mostra o cursor
                //Cursor.Current = Cursors.WaitCursor;

                // Limpa o listview
                //lvwProdutos.Items.Clear();
                //this.Refresh();

                // Lista os pedido existentes do PDV
                foreach (CSItemsPedido.CSItemPedido itempedido in CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items)
                {
                    if (itempedido.STATE != ObjectState.DELETADO)
                    {
                        // [ Valor da Indenização do item ]                    
                        totalIndenizacao = itempedido.VLR_INDENIZACAO_UNIDADE;

                        totalVenda += itempedido.VLR_TOTAL_ITEM;

                        // Calculo do custo do produto com a caixa fechada
                        custoTotal += itempedido.PRODUTO.VLR_CUSTO_GERENCIAL * itempedido.QTD_PEDIDA_INTEIRA;
                        // Calculo do custo do produto com a caixa aberta
                        custoTotal += ((itempedido.PRODUTO.VLR_CUSTO_GERENCIAL / itempedido.PRODUTO.QTD_UNIDADE_EMBALAGEM) * itempedido.QTD_PEDIDA_UNIDADE);

                        if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                            totalDesconto += itempedido.PRC_DESCONTO_UNITARIO;
                        else
                            totalDesconto += itempedido.VLR_DESCONTO;

                        totalIndenizacaoPedido += totalIndenizacao;
                    }
                }

                pctLucratividade += GetPercentualLucratividade(totalVenda, custoTotal);

                // Mostra o total dos descontos do pedido
                lblVlrDesc.Text = totalDesconto.ToString(CSGlobal.DecimalStringFormat);
                lblPctLucratividade.Text = pctLucratividade.ToString(CSGlobal.DecimalStringFormat);

                // [ Valor da Indenização do produto ]
                txtDescontoIndenizacao.Text = totalIndenizacaoPedido.ToString(CSGlobal.DecimalStringFormat);
                CSPDVs.Current.PEDIDOS_PDV.Current.VLR_INDENIZACAO = totalIndenizacaoPedido;
            }
            catch (Exception ex)
            {

            }
        }

        private static decimal GetPercentualLucratividade(decimal vendaTotal, decimal custoTotal)
        {
            decimal pctLucratividade = 0;

            try
            {
                if (CSEmpresa.Current.TIPO_CALCULO_LUCRATIVIDADE != 0 && vendaTotal > 0 && custoTotal > 0)
                    if (CSEmpresa.Current.TIPO_CALCULO_LUCRATIVIDADE == 1)
                        pctLucratividade = ((vendaTotal / custoTotal) * 100) - 100;
                    else
                        pctLucratividade = 100 - ((custoTotal / vendaTotal) * 100);

            }
            catch (Exception)
            {
                MessageBox.AlertErro(ActivityContext, "Erro ao calcular percentual de lucratividade do pedido");
            }

            return pctLucratividade;
        }

        private void Eventos()
        {
            btnListaProdutos.Click += BtnListaProdutos_Click;
            btnNovoProduto.Click += BtnNovoProduto_Click;
            cboCondicao.ItemSelected += CboCondicao_ItemSelected;
            cboOperacao.ItemSelected += CboOperacao_ItemSelected;
            chkIndenizacao.CheckedChange += ChkIndenizacao_CheckedChange;
        }

        private void ChkIndenizacao_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            try
            {
                // [ Se não for broker... ]
                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA != 2)
                {
                    if (chkIndenizacao.Checked)
                    {
                        lblDescontoIndenizacao.Text = "Valor Abat.:";
                    }
                    else
                    {
                        lblDescontoIndenizacao.Text = "Desc Incond:";
                    }

                    IsDirty = true;
                }
            }
            catch (Exception ex)
            {

            }
        }

        private static bool EstoqueProntaEntrega()
        {
            try
            {
                foreach (CSItemsPedido.CSItemPedido itempedido in CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items)
                {
                    if (itempedido.STATE != ObjectState.DELETADO)
                    {
                        decimal estoqueProntaEntrega = itempedido.PRODUTO.QTD_ESTOQUE_PRONTA_ENTREGA - itempedido.QTD_PEDIDA_TOTAL;
                        if (estoqueProntaEntrega < 0)
                        {
                            MessageBox.ShowShortMessageCenter(ActivityContext, "Saldo de pronta entrega insuficiente.\r\nProduto:(" + itempedido.PRODUTO.DESCRICAO_APELIDO_PRODUTO + ") - " + itempedido.PRODUTO.DSC_APELIDO_PRODUTO.Trim());
                            return false;

                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        private class ThreadSelectedOperacao : AsyncTask
        {
            ArrayAdapter adapter;
            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                // Verifica se existe um item selecionado
                if (cboOperacao.SelectedItem == null)
                    return false;

                // [ Verifica se tem saldo de estoque para pronta entrega ]
                if (!blEstoqueProntaEntrega && !IsLoading && ((CSOperacoes.CSOperacao)((CSItemCombo)cboOperacao.SelectedItem).Valor).IND_PRONTA_ENTREGA)
                    if (!EstoqueProntaEntrega())
                    {
                        //for (int i = 0; i < cboOperacao.Adapter.Count; i++)
                        //{
                        //    if (((CSOperacoes.CSOperacao)((ArrayAdapter)cboOperacao.Adapter).GetItem(i)).COD_OPERACAO == CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO)
                        //    {
                        //        cboOperacao.SetSelection(i);
                        //        break;
                        //    }
                        //}

                        return false;
                    }

                // Guarda qual foi a condição de pagamento selecionada solicitada
                CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO = (CSOperacoes.CSOperacao)((CSItemCombo)cboOperacao.SelectedItem).Valor;

                // [ Caso troque a operção do pedido, acerta saldo do estoque ]
                if (blEstoqueProntaEntrega != CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.IND_PRONTA_ENTREGA && !IsLoading)
                {
                    foreach (CSItemsPedido.CSItemPedido itempedido in CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items)
                    {
                        if (blEstoqueProntaEntrega)
                        {
                            itempedido.PRODUTO.QTD_ESTOQUE_PRONTA_ENTREGA += itempedido.QTD_PEDIDA_TOTAL;
                            itempedido.PRODUTO.QTD_ESTOQUE -= itempedido.QTD_PEDIDA_TOTAL;
                        }
                        else
                        {
                            itempedido.PRODUTO.QTD_ESTOQUE += itempedido.QTD_PEDIDA_TOTAL;
                            itempedido.PRODUTO.QTD_ESTOQUE_PRONTA_ENTREGA -= itempedido.QTD_PEDIDA_TOTAL;
                        }
                    }
                }

                blEstoqueProntaEntrega = CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.IND_PRONTA_ENTREGA;

                CarregaComboBoxCondicaoPagamento();

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                cboCondicao.Adapter = adapter;

                if (CSPDVs.Current != null &&
                    CSPDVs.Current.PEDIDOS_PDV.Current != null)
                {
                    if (CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO != -1)
                    {
                        if (cboCondicao.Adapter != null)
                        {
                            for (int x = 0; x < cboCondicao.Adapter.Count; x++)
                            {
                                if (((CSCondicoesPagamento.CSCondicaoPagamento)((CSItemCombo)cboCondicao.Adapter.GetItem(x)).Valor).COD_CONDICAO_PAGAMENTO == CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO)
                                {
                                    cboCondicao.SetSelection(x);
                                }
                            }
                        }
                    }
                    else if (CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO == -1)
                    {
                        if (cboCondicao.Adapter != null)
                        {
                            for (int x = 0; x < cboCondicao.Adapter.Count; x++)
                            {
                                if (((CSCondicoesPagamento.CSCondicaoPagamento)((CSItemCombo)cboCondicao.Adapter.GetItem(x)).Valor).COD_CONDICAO_PAGAMENTO == CSPDVs.Current.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO)
                                {
                                    cboCondicao.SetSelection(x);
                                }
                            }
                        }
                    }

                    //Valida se o combo foi carregado
                    if (!ValidarComboCondicaoPagamento())
                    {
                        // Seta que o pedido foi alterado e tem que ser gravado no flush
                        if (CSPDVs.Current.PEDIDOS_PDV.Current.STATE != ObjectState.NOVO)
                            CSPDVs.Current.PEDIDOS_PDV.Current.STATE = ObjectState.ALTERADO;

                        // Marca que foi alterado algo
                        IsDirty = true;
                    }

                    new ThreadAtualizarControles().Execute();
                }
                else
                    progressDialog.Dismiss();
            }

            private static bool ExisteModificacaoEmAlgumProduto()
            {
                try
                {
                    return CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Count(c => c.STATE != ObjectState.INALTERADO) > 0;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            private class ThreadAtualizarControles : AsyncTask
            {
                string vlrProds;
                string saldoCred;
                string saldoDesconto;
                string strVolume;
                int qtdProdutos;
                string strPeso;

                protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
                {
                    vlrProds = CSPDVs.Current.PEDIDOS_PDV.Current.VLR_TOTAL_PEDIDO.ToString(CSGlobal.DecimalStringFormat);
                    saldoCred = CSPDVs.Current.VLR_SALDO_CREDITO_ATUALIZADO.ToString(CSGlobal.DecimalStringFormat);
                    saldoDesconto = CSEmpregados.Current.VAL_SALDO_DESCONTO.ToString(CSGlobal.DecimalStringFormat);

                    if (ExisteModificacaoEmAlgumProduto())
                        IsDirty = true;

                    strVolume = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(c => c.STATE != ObjectState.DELETADO).Sum(c => c.QTD_PEDIDA_INTEIRA).ToString();

                    qtdProdutos = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(c => c.STATE != ObjectState.DELETADO).Count();

                    strPeso = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(c => c.STATE != ObjectState.DELETADO).Sum(c => Math.Round((c.PRODUTO.VLR_PESO_PRODUTO * c.QTD_PEDIDA_TOTAL) / c.PRODUTO.QTD_UNIDADE_EMBALAGEM, 2)).ToString(CSGlobal.DecimalStringFormat);

                    return true;
                }

                protected override void OnPostExecute(Java.Lang.Object result)
                {
                    base.OnPostExecute(result);

                    lblTotalPed.Text = vlrProds;
                    lblSaldoCred.Text = saldoCred;
                    lblSaldoDesconto.Text = saldoDesconto;

                    if (CSPDVs.Current != null &&
                        CSPDVs.Current.PEDIDOS_PDV.Current != null)
                    {
                        // [ Busca o valor do desconto incondicional ou indenização ]
                        if ((CSPDVs.Current.PEDIDOS_PDV.Current.COD_PEDIDO != -1 || CSGlobal.PedidoSugerido) && CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Count > 0)
                        {
                            // [ Verifica se é uma indenização ][Desativado]
                            chkIndenizacao.Checked = true;
                            if (chkIndenizacao.Text == "Abatimento?")
                            {
                                txtDescontoIndenizacao.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items.Cast<CSItemsPedido.CSItemPedido>().Where(it => it.STATE != ObjectState.DELETADO).Sum(p => p.VLR_INDENIZACAO_UNIDADE).ToString(CSGlobal.DecimalStringFormat);
                            }
                            else
                            {
                                txtDescontoIndenizacao.Text = CSPDVs.Current.PEDIDOS_PDV.Current.VLR_INDENIZACAO.ToString(CSGlobal.DecimalStringFormat);
                            }
                        }
                        else
                        {
                            txtDescontoIndenizacao.Text = 0.ToString(CSGlobal.DecimalStringFormat);
                            chkIndenizacao.Checked = true;
                        }

                        lblVolume.Text = strVolume;

                        AlteraTextoBtnProdutos(qtdProdutos);
                        lblPesoBruto.Text = strPeso;
                    }

                    CarregandoDados = false;
                    progressDialog.Dismiss();
                }
            }

            private static bool ValidarComboCondicaoPagamento()
            {
                try
                {
                    if (cboCondicao.Adapter.Count == 0)
                    {
                        MessageBox.AlertErro(ActivityContext, "Criação de pedido impedida, informação de condição de pagamento incompleta.");
                        return false;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            private void CarregaComboBoxCondicaoPagamento()
            {
                try
                {
                    adapter = new ArrayAdapter(ActivityContext, Android.Resource.Layout.SimpleSpinnerItem);
                    adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

                    int condicoesInativasAteAgora = 0;
                    //int PosicaoCondicaoPadrao = 0;
                    // Preenche o combo de condições de pagamento
                    for (int i = 0; i < CSCondicoesPagamento.Items.Count; i++)
                    {
                        CSCondicoesPagamento.CSCondicaoPagamento condpag = CSCondicoesPagamento.Items[i];
                        // Mostra somente as condições ativas
                        if (condpag.IND_ATIVO == true)
                        {
                            // Lista somente as prioridades menores ou iguais a do PDV
                            // Acrescentada condicao de mostrar somente condições de pagamento 
                            // conforme operacao do cfo selecionada ou seja
                            // codigo operacao CFO = 1 somente condicoes pagamento tipo = 1 ou 2
                            // codigo operacao CFO <> 1 somente condicao pagamento = 3

                            int PrioridadeCondicaoPagamento = condpag.PRIORIDADE_CONDICAO_PAGAMENTO;
                            int PrioridadeCondicaoPagamentoCspdvCurrent = CSPDVs.Current.CONDICAO_PAGAMENTO.PRIORIDADE_CONDICAO_PAGAMENTO;
                            int OperacaoCFO = CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO.COD_OPERACAO_CFO;
                            int CodTipoCondicaoPagamento = condpag.COD_TIPO_CONDICAO_PAGAMENTO;

                            if (PrioridadeCondicaoPagamento <= PrioridadeCondicaoPagamentoCspdvCurrent &&
                                (((OperacaoCFO == 1 ||
                                OperacaoCFO == 21) &&
                                (CodTipoCondicaoPagamento == 1 || CodTipoCondicaoPagamento == 2))) ||
                                ((OperacaoCFO != 1 &&
                                OperacaoCFO != 21 &&
                                CodTipoCondicaoPagamento == 3)))
                            {
                                // Cria o item para inserir
                                CSItemCombo ic = new CSItemCombo();
                                ic.Texto = condpag.DSC_CONDICAO_PAGAMENTO;
                                ic.Valor = condpag;

                                // Se for broker....
                                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                                {
                                    if (condpag.CODPRZPGT.Trim() != "")
                                    {
                                        // Mostrar somente condicoes a vista
                                        if ((CSPDVs.Current.CODCNDPGT == 1) || (CSPDVs.Current.CODCNDPGT == 2))
                                        {
                                            // Verifica se a condicao é a vista
                                            if (condpag.CODPRZCLIENTE == 0)
                                            {
                                                // Adiciona o item no combo
                                                adapter.Add(ic);
                                            }
                                        }
                                        else
                                        {
                                            // Adiciona o item no combo
                                            adapter.Add(ic);
                                        }
                                    }
                                }
                                else if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 3)
                                {
                                    if (condpag.CODIGO_PRAZO_BUNGE.Trim() != "")
                                    {
                                        // Adiciona o item no combo
                                        adapter.Add(ic);
                                    }
                                }
                                else
                                {
                                    // Adiciona o item no combo
                                    adapter.Add(ic);
                                }
                            }
                        }
                        else
                            condicoesInativasAteAgora++;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void CboOperacao_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (CarregandoDados)
                    return;

                if (CSPDVs.Current != null &&
                    CSPDVs.Current.PEDIDOS_PDV.Current != null)
                {
                    progressDialog = new ProgressDialogCustomizado(ActivityContext, thisLayoutInflater).Customizar();
                    progressDialog.Show();

                    new ThreadSelectedOperacao().Execute();
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void CboCondicao_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                if (CarregandoDados)
                    return;

                int cndPgtAnt = 0;
                int cndPgtAtual = 0;

                // Verifica se existe um item selecionado
                if (cboCondicao.SelectedItem == null)
                    return;

                // recebe valor da condicao de pagamento anterior para poder realizar calculo de verba
                condicaoPagamentoAnterior = CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO;

                // Guarda qual foi a operação solicitada
                GuardaOperacaoSolicitada();

                // [ Se broker... ]
                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                {
                    // [ Invalida objeto de cálculo de preços broker ]
                    CSPDVs.Current.POLITICA_BROKER = null;
                    CSPDVs.Current.POLITICA_BROKER_2014 = null;

                    //Type t;

                    //if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                    //    t = CSPDVs.Current.POLITICA_BROKER_2014.GetType();
                    //else
                    //    t = CSPDVs.Current.POLITICA_BROKER.GetType();
                }

                // Busca o valor do Adf
                lblAdf.Text = ((CSCondicoesPagamento.CSCondicaoPagamento)((CSItemCombo)cboCondicao.SelectedItem).Valor).PRC_ADICIONAL_FINANCEIRO.ToString(CSGlobal.DecimalStringFormat);
                //SE existir condicao de pagamento anterior
                if (condicaoPagamentoAnterior != null)
                    cndPgtAnt = ((CSCondicoesPagamento.CSCondicaoPagamento)condicaoPagamentoAnterior).COD_CONDICAO_PAGAMENTO;

                //Se ja foi configurada a condicao de pagamento
                //verifica se e preciso fazer recalculo
                if (CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO != null)
                {
                    cndPgtAtual = CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO;
                    //Recalculo Broker nao utiliza % ADF depende se a condicao de pagamento foi alterada
                    //if ((CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2) &&
                    //    cndPgtAnt != cndPgtAtual && cndPgtAnt > 0 &&
                    //    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Count > 0)
                    if (cndPgtAnt != cndPgtAtual && cndPgtAnt > 0 &&
                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Count > 0)
                    {
                        // Mostra os dados com os valores alterados
                        RecalculaValoresPedido();
                        //RefreshDadosTela();
                    }
                }
                // Seta que o pedido foi alterado e tem que ser gravado no flush
                if (CSPDVs.Current.PEDIDOS_PDV.Current.STATE != ObjectState.NOVO)
                    CSPDVs.Current.PEDIDOS_PDV.Current.STATE = ObjectState.ALTERADO;

                // Marca que foi alterado algo
                IsDirty = true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Falha ao inicializar tabela de variáveis!"))
                {
                    MessageBox.AlertErro(ActivityContext, "Falha ao inicializar tabela de variáveis!");
                }
                else
                    MessageBox.AlertErro(ActivityContext, ex.Message);
            }
        }

        private void GuardaOperacaoSolicitada()
        {
            try
            {
                if (CSGlobal.PedidoSugerido)
                {
                    CSPDVs.Current.PEDIDOS_PDV.Current.OPERACAO = CSPDVs.Current.OPERACOES.Cast<CSOperacoes.CSOperacao>().Where(o => o.COD_OPERACAO_CFO == 1).FirstOrDefault();
                    CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO = CSCondicoesPagamento.GetCondicaPagamento(CSPDVs.Current.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO);
                }
                else
                {
                    var itemSelecionado = (CSItemCombo)cboCondicao.SelectedItem;
                    if (itemSelecionado != null)
                    {
                        CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO = (CSCondicoesPagamento.CSCondicaoPagamento)itemSelecionado.Valor;
                    }
                    else
                    {
                        CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO = null;
                    }
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                MessageBox.AlertErro(ActivityContext, ex.Message);
#endif
                CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO = null;
            }
        }

        private bool DadosParaNovoProdutoValidos()
        {
            try
            {
                GuardaOperacaoSolicitada();

                if (CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO == null)
                {
                    MessageBox.ShowShortMessageCenter(ActivityContext, "A condição de pagamento não pode ser vazia.");
                    return false;
                }

                if (cboEmissor.Adapter == null)
                {
                    MessageBox.ShowShortMessageCenter(ActivityContext, "O emissor não pode ser vazio.");
                    return false;
                }

                if (CSGlobal.PedidoComCombo)
                {
                    MessageBox.ShowShortMessageCenter(ActivityContext, "Não se pode incluir produtos em um pedido combo.");
                    return false;
                }

                if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(c => c.STATE != ObjectState.DELETADO).Count() == 0)
                {
                    CSGlobal.PedidoComCombo = false;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private bool IsBroker()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
        }

        private void BtnNovoProduto_Click(object sender, EventArgs e)
        {
            try
            {
                if (!DadosParaNovoProdutoValidos())
                    return;

                IsLoading = false;

                if (((Cliente)Activity).LinhaSelecionada == null)
                    ((Cliente)Activity).NavegarParaPasso(1);
                else
                {
                    if (IsBroker())
                        ((Cliente)Activity).NavegarParaPasso(2);
                    else
                        ((Cliente)Activity).NavegarParaPasso(3);
                }

                //i.SetClass(this, typeof(Produtos));
                //i.PutExtra("txtDescontoIndenizacao", txtDescontoIndenizacao.Text);
                //i.PutExtra("txtAdf", lblAdf.Text);
                //this.StartActivityForResult(i, frmProdutos);
            }
            catch (Exception ex)
            {

            }
        }

        public static string[] GetRetornaBloqueios(CSPDVs.CSPDV pdv, CSPedidosPDV.CSPedidoPDV pdd)
        {
            try
            {
                string[] aBlqTabPre = new string[9];

                foreach (CSItemsPedido.CSItemPedido produto in pdd.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.STATE != ObjectState.DELETADO).ToList())
                {
                    StringBuilder sqlQuery = new StringBuilder();
                    sqlQuery.Append("SELECT COD_TABELA_BLOQUEIO,COD_BLOQUEIO,COD_SUB_GRUPO_TABELA_BLOQUEIO,TIPO_BLOQUEIO ");

                    if (CSEmpresa.ColunaExiste("BLOQUEIO_TABELA_PRECO", "COD_UF"))
                        sqlQuery.AppendLine(",COD_UF");

                    sqlQuery.AppendLine("    FROM BLOQUEIO_TABELA_PRECO ");
                    sqlQuery.AppendFormat("  WHERE COD_TABELA_PRECO = {0}", produto.COD_TABELA_PRECO.ToString());
                    sqlQuery.AppendLine("    ORDER BY COD_TABELA_BLOQUEIO,COD_BLOQUEIO");

                    aBlqTabPre[0] = "";

                    // Busca todos os bloqueios de tabela de preço configurados
                    using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
                    {
                        while (sqlReader.Read())
                        {
                            switch (sqlReader.GetInt32(0))
                            {
                                case 1:
                                    {
                                        if (sqlReader.GetInt32(1) == pdd.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO)
                                        {
                                            if (sqlReader.GetString(3) == "B")
                                            {
                                                aBlqTabPre[0] = "B";
                                                aBlqTabPre[1] = string.Format("Condição de pagamento bloqueada na tabela {0}.", produto.COD_TABELA_PRECO);

                                            }
                                        }
                                        break;
                                    }

                                case 2:
                                    {
                                        if (sqlReader.GetInt32(1) == pdv.COD_CATEGORIA)
                                        {
                                            if (sqlReader.GetString(3) == "B")
                                            {
                                                aBlqTabPre[0] = "B";
                                                aBlqTabPre[2] = "Categoria do cliente bloqueada na tabela " + produto.COD_TABELA_PRECO.ToString() + ".";
                                            }
                                        }
                                        break;
                                    }

                                case 3:
                                    {
                                        if (sqlReader.GetInt32(1) == pdv.COD_GRUPO)
                                        {
                                            if (sqlReader.GetString(3) == "B")
                                            {
                                                aBlqTabPre[0] = "B";
                                                aBlqTabPre[3] = "Grupo do cliente com bloqueio na tabela " + produto.COD_TABELA_PRECO.ToString() + ".";

                                            }
                                        }
                                        break;
                                    }

                                case 4:
                                    {
                                        if (sqlReader.GetInt32(2) == pdv.COD_GRUPO &&
                                            sqlReader.GetInt32(1) == pdv.COD_CLASSIFICACAO)
                                        {
                                            if (sqlReader.GetString(3) == "B")
                                            {
                                                aBlqTabPre[0] = "B";
                                                aBlqTabPre[4] = "Classificação cliente com bloqueio na tabela " + produto.COD_TABELA_PRECO.ToString() + ".";
                                            }
                                        }
                                        break;
                                    }

                                case 5:
                                    {
                                        if (sqlReader.GetInt32(1) == pdv.COD_SEGMENTACAO)
                                        {
                                            if (sqlReader.GetString(3) == "B")
                                            {
                                                aBlqTabPre[0] = "B";
                                                aBlqTabPre[5] = "Segmento do cliente com bloqueio na tabela " + produto.COD_TABELA_PRECO.ToString() + ".";

                                            }
                                        }
                                        break;
                                    }

                                case 6:
                                    {
                                        if (sqlReader.GetInt32(1) == pdv.COD_UNIDADE_NEGOCIO)
                                        {
                                            if (sqlReader.GetString(3) == "B")
                                            {
                                                aBlqTabPre[0] = "B";
                                                aBlqTabPre[6] = "Negócio do cliente com bloqueio na tabela " + produto.COD_TABELA_PRECO.ToString() + ".";

                                            }
                                        }
                                        break;
                                    }

                                case 7:
                                    {
                                        if (sqlReader.GetInt32(1) == pdd.EMPREGADO.COD_EMPREGADO)
                                        {
                                            if (sqlReader.GetString(3) == "B")
                                            {
                                                aBlqTabPre[0] = "B";
                                                aBlqTabPre[7] = "Vendedor bloqueado na tabela " + produto.COD_TABELA_PRECO.ToString() + ".";

                                            }
                                        }
                                        break;
                                    }
                                case 8:
                                    {
                                        if (CSEmpresa.ColunaExiste("BLOQUEIO_TABELA_PRECO", "COD_UF"))
                                            sqlQuery.AppendLine(",COD_UF");
                                        {
                                            if (pdv.ENDERECOS_PDV.Count > 0 &&
                                                sqlReader.GetInt32(1) == pdv.ENDERECOS_PDV[0].COD_CIDADE &&
                                                sqlReader.GetString(4) == pdv.ENDERECOS_PDV[0].DSC_UF)
                                            {
                                                if (sqlReader.GetString(3) == "B")
                                                {
                                                    aBlqTabPre[0] = "B";
                                                    aBlqTabPre[8] = "Cidade do cliente bloqueado na tabela " + produto.COD_TABELA_PRECO.ToString() + ".";

                                                }
                                            }
                                        }
                                        break;
                                    }
                            }
                        }

                        // Fecha o reader
                        sqlReader.Close();
                        sqlReader.Dispose();
                    }

                    if (aBlqTabPre.Length <= 0 ||
                        aBlqTabPre[0] != "B")
                    {

                        aBlqTabPre = new string[9];

                        sqlQuery = new StringBuilder();

                        sqlQuery.AppendLine("SELECT COD_TABELA_BLOQUEIO,COD_BLOQUEIO,COD_SUB_GRUPO_TABELA_BLOQUEIO,TIPO_BLOQUEIO ");

                        if (CSEmpresa.ColunaExiste("BLOQUEIO_PRODUTO_TABELA_PRECO", "COD_UF"))
                            sqlQuery.AppendLine(",COD_UF");

                        sqlQuery.AppendLine("  FROM BLOQUEIO_PRODUTO_TABELA_PRECO ");
                        sqlQuery.AppendFormat(" WHERE COD_TABELA_PRECO = {0} ", produto.COD_TABELA_PRECO.ToString());
                        sqlQuery.AppendFormat(" AND COD_PRODUTO = {0} ", produto.PRODUTO.COD_PRODUTO.ToString());
                        sqlQuery.AppendLine(" ORDER BY COD_TABELA_BLOQUEIO,COD_BLOQUEIO");

                        aBlqTabPre[0] = "";

                        // Busca todos os bloqueios de tabela de preço configurados
                        using (SQLiteDataReader sqlReader = CSDataAccess.Instance.ExecuteReader(sqlQuery.ToString()))
                        {
                            while (sqlReader.Read())
                            {
                                switch (sqlReader.GetInt32(0))
                                {
                                    case 1:
                                        {
                                            if (sqlReader.GetInt32(1) == pdd.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO)
                                            {
                                                if (sqlReader.GetString(3) == "B")
                                                {
                                                    aBlqTabPre[0] = "B";
                                                    aBlqTabPre[1] = "Condição de pagamento bloqueada na tabela " + produto.COD_TABELA_PRECO.ToString() + " e produto " + produto.PRODUTO.DSC_PRODUTO + ".";
                                                }
                                            }
                                            break;
                                        }

                                    case 2:
                                        {
                                            if (sqlReader.GetInt32(1) == pdv.COD_CATEGORIA)
                                            {
                                                if (sqlReader.GetString(3) == "B")
                                                {
                                                    aBlqTabPre[0] = "B";
                                                    aBlqTabPre[1] = "Categoria do cliente bloqueada na tabela " + produto.COD_TABELA_PRECO.ToString() + " e produto " + produto.PRODUTO.DSC_PRODUTO + ".";
                                                }
                                            }
                                            break;
                                        }

                                    case 3:
                                        {
                                            if (sqlReader.GetInt32(1) == pdv.COD_GRUPO)
                                            {
                                                if (sqlReader.GetString(3) == "B")
                                                {
                                                    aBlqTabPre[0] = "B";
                                                    aBlqTabPre[1] = "Grupo do cliente com bloqueio na tabela " + produto.COD_TABELA_PRECO.ToString() + " e produto " + produto.PRODUTO.DSC_PRODUTO + ".";
                                                }
                                            }
                                            break;
                                        }

                                    case 4:
                                        {
                                            if (sqlReader.GetInt32(2) == pdv.COD_GRUPO &&
                                                sqlReader.GetInt32(1) == pdv.COD_CLASSIFICACAO)
                                            {
                                                if (sqlReader.GetString(3) == "B")
                                                {
                                                    aBlqTabPre[0] = "B";
                                                    aBlqTabPre[1] = "Classificação cliente com bloqueio na tabela " + produto.COD_TABELA_PRECO.ToString() + " e produto " + produto.PRODUTO.DSC_PRODUTO + ".";
                                                }
                                            }
                                            break;
                                        }

                                    case 5:
                                        {
                                            if (sqlReader.GetInt32(1) == pdv.COD_SEGMENTACAO)
                                            {
                                                if (sqlReader.GetString(3) == "B")
                                                {
                                                    aBlqTabPre[0] = "B";
                                                    aBlqTabPre[1] = "Segmento do cliente com bloqueio na tabela " + produto.COD_TABELA_PRECO.ToString() + " e produto " + produto.PRODUTO.DSC_PRODUTO + ".";
                                                }
                                            }
                                            break;
                                        }

                                    case 6:
                                        {
                                            if (sqlReader.GetInt32(1) == pdv.COD_UNIDADE_NEGOCIO)
                                            {
                                                if (sqlReader.GetString(3) == "B")
                                                {
                                                    aBlqTabPre[0] = "B";
                                                    aBlqTabPre[1] = "Negócio do cliente com bloqueio na tabela " + produto.COD_TABELA_PRECO.ToString() + " e produto " + produto.PRODUTO.DSC_PRODUTO + ".";
                                                }
                                            }
                                            break;
                                        }

                                    case 7:
                                        {
                                            if (sqlReader.GetInt32(1) == pdd.EMPREGADO.COD_EMPREGADO)
                                            {
                                                if (sqlReader.GetString(3) == "B")
                                                {
                                                    aBlqTabPre[0] = "B";
                                                    aBlqTabPre[1] = "Vendedor bloqueado na tabela " + produto.COD_TABELA_PRECO.ToString() + " e produto " + produto.PRODUTO.DSC_PRODUTO + ".";
                                                }
                                            }
                                            break;
                                        }
                                    case 8:
                                        {
                                            if (CSEmpresa.ColunaExiste("BLOQUEIO_TABELA_PRECO", "COD_UF"))
                                                sqlQuery.AppendLine(",COD_UF");
                                            {
                                                if (pdv.ENDERECOS_PDV.Count > 0 &&
                                                    sqlReader.GetInt32(1) == pdv.ENDERECOS_PDV[0].COD_CIDADE &&
                                                    sqlReader.GetString(4) == pdv.ENDERECOS_PDV[0].DSC_UF)
                                                {
                                                    if (sqlReader.GetString(3) == "B")
                                                    {
                                                        aBlqTabPre[0] = "B";
                                                        aBlqTabPre[1] = "Cidade do cliente bloqueado na tabela " + produto.COD_TABELA_PRECO.ToString() + " e produto " + produto.PRODUTO.DSC_PRODUTO + ".";
                                                    }
                                                }
                                            }
                                            break;
                                        }
                                }
                            }

                            // Fecha o reader
                            sqlReader.Close();
                            sqlReader.Dispose();
                        }
                    }
                }

                return aBlqTabPre;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro na busca de bloqueios de tabela de preço", ex);
            }
        }

        private static bool ValidaBloqueios()
        {
            try
            {
                if (CSGlobal.PedidoComCombo)
                    return true;

                string[] aBloqueios = GetRetornaBloqueios(CSPDVs.Current, CSPDVs.Current.PEDIDOS_PDV.Current);

                //Remove valores nulos
                aBloqueios = aBloqueios.Where(b => !string.IsNullOrEmpty(b)).ToArray();

                //Se teve bloqueio/advertencia
                if (aBloqueios.Length > 0)
                {
                    if (aBloqueios[0] == "B") //Se for bloqueio
                    {
                        MessageBox.AlertErro(ActivityContext, aBloqueios[1]);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void BtnListaProdutos_Click(object sender, EventArgs e)
        {
            try
            {
                IsLoading = false;

                if (ValidaBloqueios())
                {
                    ((Cliente)Activity).AbrirListaProdutos((int)ActivitiesNames.Pedido, txtDescontoIndenizacao.Text, lblAdf.Text);
                }
            }
            catch (Exception ex)
            {

            }
        }

        private void FindViewsById(View view)
        {
            lblPedido = view.FindViewById<TextView>(Resource.Id.lblPedido);
            lblDataEntrega = view.FindViewById<TextView>(Resource.Id.lblDataEntrega);
            lblEmissor = view.FindViewById<TextView>(Resource.Id.lblEmissor);
            cboEmissor = view.FindViewById<Spinner>(Resource.Id.cboEmissor);
            cboOperacao = view.FindViewById<Spinner>(Resource.Id.cboOperacao);
            cboCondicao = view.FindViewById<Spinner>(Resource.Id.cboCondicao);
            lblAdf = view.FindViewById<TextView>(Resource.Id.lblAdf);
            lblVlrDesc = view.FindViewById<TextView>(Resource.Id.lblVlrDesc);
            lblSaldoCred = view.FindViewById<TextView>(Resource.Id.lblSaldoCred);
            lblTotalPed = view.FindViewById<TextView>(Resource.Id.lblTotalPed);
            lblSaldoDct = view.FindViewById<TextView>(Resource.Id.lblSaldoDct);
            lblSaldoDesconto = view.FindViewById<TextView>(Resource.Id.lblSaldoDesconto);
            lblDescPctLucratividade = view.FindViewById<TextView>(Resource.Id.lblDescPctLucratividade);
            lblPctLucratividade = view.FindViewById<TextView>(Resource.Id.lblPctLucratividade);
            lblDescontoIndenizacao = view.FindViewById<TextView>(Resource.Id.lblDescontoIndenizacao);
            txtDescontoIndenizacao = view.FindViewById<TextView>(Resource.Id.txtDescontoIndenizacao);
            chkIndenizacao = view.FindViewById<CheckBox>(Resource.Id.chkIndenizacao);
            btnListaProdutos = view.FindViewById<Button>(Resource.Id.btnListaProdutos);
            btnNovoProduto = view.FindViewById<Button>(Resource.Id.btnNovoProduto);
            lblPolitica = view.FindViewById<TextView>(Resource.Id.lblPolitica);
            lblVolume = view.FindViewById<TextView>(Resource.Id.lblVolume);
            lblPesoBruto = view.FindViewById<TextView>(Resource.Id.lblPesoBruto);
        }

        private void ConfiguraTela()
        {
            try
            {
                lblDescontoIndenizacao.Visibility = ViewStates.Visible;
                txtDescontoIndenizacao.Visibility = ViewStates.Visible;
                chkIndenizacao.Visibility = ViewStates.Visible;
                lblSaldoDct.Visibility = ViewStates.Visible;
                lblSaldoDesconto.Visibility = ViewStates.Visible;
                lblDescPctLucratividade.Visibility = ViewStates.Visible;
                lblPctLucratividade.Visibility = ViewStates.Visible;
                lblEmissor.Visibility = ViewStates.Visible;
                cboEmissor.Visibility = ViewStates.Visible;

                if (CSGlobal.PedidoSugerido)
                    cboOperacao.Enabled = false;

                // [ Se for broker... ]
                if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
                {
                    lblPolitica.Text = "BRK";
                    lblDescontoIndenizacao.Visibility = ViewStates.Gone;
                    txtDescontoIndenizacao.Visibility = ViewStates.Gone;
                    chkIndenizacao.Visibility = ViewStates.Gone;

                    // [ Verifica saldo verba ]
                    if (!CSEmpresa.Current.IND_LIMITE_DESCONTO)
                    {
                        lblSaldoDct.Visibility = ViewStates.Gone;
                        lblSaldoDesconto.Visibility = ViewStates.Gone;
                        lblDescPctLucratividade.Visibility = ViewStates.Gone;
                        lblPctLucratividade.Visibility = ViewStates.Gone;
                    }
                }
                else if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 1)
                {
                    lblPolitica.Text = "FLX";
                    lblEmissor.Visibility = ViewStates.Gone;
                    cboEmissor.Visibility = ViewStates.Gone;

                    // [ Verifica tipo de calculo da lucratividade ]
                    // [ 0 - Não Calcular    ]
                    // [ 1 - Venda por Custo ]
                    // [ 2 - Custo por Venda ]
                    if (CSEmpresa.Current.TIPO_CALCULO_LUCRATIVIDADE == 0 || CSEmpresa.Current.IND_VISUALIZA_LUCRATIVIDADE.Trim() == "N")
                    {
                        lblDescPctLucratividade.Visibility = ViewStates.Gone;
                        lblPctLucratividade.Visibility = ViewStates.Gone;
                    }

                    // [ Verifica saldo verba ]
                    if (!CSEmpresa.Current.IND_LIMITE_DESCONTO)
                    {
                        lblSaldoDct.Visibility = ViewStates.Gone;
                        lblSaldoDesconto.Visibility = ViewStates.Gone;
                    }

                    // Troca a descrição de Indenização para abatimento
                    chkIndenizacao.Text = "Abatimento?";
                }
                else
                {
                    lblPolitica.Text = "BNG";
                }

                // [ Atualiza saldo do vendedor ]
                // [ 1 - Descontar do saldo     ]
                // [ 2 - Não descontar          ]
                // [ 3 - Indenização bloqueada  ]
                if (CSEmpresa.Current.IND_VLR_INDENIZACAO_ATUSALDO == 3)
                {
                    txtDescontoIndenizacao.Enabled = false;
                    chkIndenizacao.Enabled = false;
                }
            }
            catch (Exception ex)
            {
            }
        }
    }
}