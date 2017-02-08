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
using Java.Lang;

namespace AvanteSales.Pro.Dialogs
{
    public class DialogFragmentDigitacaoCombo : Android.Support.V4.App.DialogFragment
    {
        int Produto;
        static CSProdutos.CSProduto Combo;
        private EditText txtQuantidade;
        private Button btnOK;
        private Button btnCancelar;
        private int QuantidadeMaxima = 0;
        private int QuantidadeVendida = 0;
        static ProgressDialog progress;
        LayoutInflater thisLayoutInflater;
        static Android.Support.V4.App.FragmentActivity CurrentActivity;
        static Android.Support.V4.App.DialogFragment thisDialog;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.dialog_fragment_digitacao_combo, container, false);
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            Produto = Arguments.GetInt("PRODUTO");
            Combo = CSProdutos.GetProduto(Produto);
            FindViewsById(view);
            Eventos();
            QuantidadeMaxima = Arguments.GetInt("QTD_MAX");
            QuantidadeVendida = Arguments.GetInt("QTD_VENDIDA");
            thisLayoutInflater = inflater;
            CurrentActivity = Activity;
            thisDialog = this;
            return view;
        }

        private void Eventos()
        {
            btnCancelar.Click += BtnCancelar_Click;
            btnOK.Click += BtnOK_Click;
        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            if (txtQuantidade.Text == string.Empty ||
               (txtQuantidade.Text != string.Empty && CSGlobal.StrToInt(txtQuantidade.Text) == 0))
            {
                MessageBox.ShowShortMessageBottom(Activity, "Digite a quantidade de venda.");
            }
            else
            {
                VenderComboSelecionado();
            }
        }

        private void VenderComboSelecionado()
        {
            if (ValidaDados())
            {
                CSGlobal.QtdeItemCombo = Convert.ToInt32(txtQuantidade.Text);

                progress = new ProgressDialogCustomizado(Activity, thisLayoutInflater).Customizar();
                progress.Show();

                new ThreadVendaCombo().Execute();
            }
        }

        private class ThreadVendaCombo : AsyncTask
        {
            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                AddProdutoCombo(Combo.COD_PRODUTO);

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                ((Cliente)CurrentActivity).AtualizarValorParcial();
                ((Cliente)CurrentActivity).NavegarParaPasso(10);
                thisDialog.Dismiss();
                ((Cliente)CurrentActivity).MenuClicado = false;
                progress.Dismiss();
            }

            private void AddProdutoCombo(int produtoCombo)
            {
                try
                {
                    CSItemsPedido.CSItemPedido itempedido = null;
                    decimal prcadf;
                    decimal valorProduto;
                    int qtde_produto = CSGlobal.QtdeItemCombo;
                    bool PoliticaBroker = IsBroker();
                    bool excluirItemPedido = false;

                    CSPDVs.Current.PEDIDOS_PDV.Add(CSPDVs.Current.PEDIDOS_PDV.Current);

                    foreach (CSProdutos.CSProduto produto in CSProdutos.Items)
                    {
                        if (produto.COD_PRODUTO_CONJUNTO == produtoCombo)
                        {
                            itempedido = new CSItemsPedido.CSItemPedido();

                            CSProdutos.Current = produto;
                            itempedido.PRODUTO = CSProdutos.Current;
                            itempedido.COD_ITEM_COMBO = produto.COD_PRODUTO_CONJUNTO;
                            itempedido.LOCK_QTD = true;
                            itempedido.PRC_ADICIONAL_FINANCEIRO = CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO.PRC_ADICIONAL_FINANCEIRO;

                            itempedido.QTD_PEDIDA_TOTAL = (CSProdutos.Current.QTD_PRODUTO_COMPOSICAO * (decimal)qtde_produto);

                            if (itempedido.PRODUTO.COD_TABELA_PRECO_COMBO == -1)
                            {
                                itempedido.COD_TABELA_PRECO = CSPDVs.Current.COD_TABPRECO_PADRAO;

                                foreach (CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto preco in produto.PRECOS_PRODUTO)
                                {
                                    if (CSPDVs.Current.COD_TABPRECO_PADRAO == preco.COD_TABELA_PRECO)
                                    {
                                        produto.PRECOS_PRODUTO.Current = preco;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                itempedido.COD_TABELA_PRECO = itempedido.PRODUTO.COD_TABELA_PRECO_COMBO;

                                foreach (CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto preco in produto.PRECOS_PRODUTO)
                                {
                                    // [ Busca tabela de preço padrão do combo ]
                                    if (itempedido.PRODUTO.COD_TABELA_PRECO_COMBO == preco.COD_TABELA_PRECO)
                                    {
                                        produto.PRECOS_PRODUTO.Current = preco;
                                        break;
                                    }
                                }
                            }

                            // Aplicar o desconto maximo                     
                            if (PoliticaBroker)
                                itempedido.AplicaDescontoMaximoQuandoNaoFoiCalculaPrecoNestle();
                            else
                                itempedido.AplicaDescontoMaximoProdutoTabPreco();

                            if (IsBroker())
                            {
                                // Preenche o percentual de desconto do item do combo
                                if (CSEmpresa.Current.IND_VALIDA_PCT_MAXIMO_DESCONTO == "S" &&
                                    CSProdutos.Current.PCT_DESCONTO_PRODUTO_COMPOSICAO > itempedido.PRC_DESCONTO_MAXIMO)
                                {
                                    itempedido.PRC_DESCONTO = itempedido.PRC_DESCONTO_MAXIMO;
                                }
                                else
                                    itempedido.PRC_DESCONTO = CSProdutos.Current.PCT_DESCONTO_PRODUTO_COMPOSICAO;
                            }
                            else
                                itempedido.PRC_DESCONTO = CSProdutos.Current.PCT_DESCONTO_PRODUTO_COMPOSICAO;

                            itempedido.STATE = ObjectState.NOVO_ALTERADO;

                            // Adiciona o item de pedido na coleção
                            CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Add(itempedido);
                            CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current = itempedido;

                            // Troca a politica para carrega as informação do produto, não 
                            // utilizando a politica da nestle, pois o mesmo será recalculado 
                            // quando retorna ao pedido.
                            if (PoliticaBroker)
                                CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 1;

                            // Seta que ainda nao exite nenhum item pedido feito ou seja é um item de pedido novo
                            // pega o valor do adf da primeira tela e coloca no novo item de pedido
                            prcadf = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_ADICIONAL_FINANCEIRO;
                            CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_ADICIONAL_FINANCEIRO = prcadf;

                            if (CSProdutos.Current.PRECOS_PRODUTO == null || CSProdutos.Current.PRECOS_PRODUTO.Count == 0)
                            {
                                if (!PoliticaBroker)
                                    MessageBox.ShowShortMessageCenter(CurrentActivity, "Preço do produto (" + CSProdutos.Current.DSC_PRODUTO + ")\n não cadastrado.\r\nNão é possivel realizar esta venda.");
                                else
                                    MessageBox.ShowShortMessageCenter(CurrentActivity, "Cliente ou Produto (" + CSProdutos.Current.DSC_PRODUTO + ") com informações incompletas no cadastro Nestlê!\nNão é possivel realizar esta venda.");

                                itempedido.STATE = ObjectState.DELETADO;
                                excluirItemPedido = true;
                                break;

                            }
                            if (PoliticaBroker)
                            {
                                if (!CSProdutos.GetProdutoPoliticaBroker(CSProdutos.Current.COD_PRODUTO, CSPDVs.Current.COD_PDV, CSProdutos.Current.GRUPO_COMERCIALIZACAO.COD_SETOR_BROKER))
                                {
                                    MessageBox.ShowShortMessageCenter(CurrentActivity, "Cliente ou Produto (" + CSProdutos.Current.DSC_PRODUTO + ") com informações incompletas no cadastro Nestlê!\nNão é possivel realizar esta venda.");
                                    itempedido.STATE = ObjectState.DELETADO;
                                    excluirItemPedido = true;
                                    break;
                                }

                                CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 2;
                            }

                            if (!PoliticaBroker)
                            {
                                if (CSProdutos.Current.PRECOS_PRODUTO.Current == null)
                                {
                                    if (produto.PRECOS_PRODUTO.Items.Count > 0)
                                    {
                                        produto.PRECOS_PRODUTO.Current = produto.PRECOS_PRODUTO.Items[0];
                                        valorProduto = produto.PRECOS_PRODUTO.Current.VLR_PRODUTO;
                                    }
                                    else
                                    {
                                        valorProduto = 0;
                                    }
                                }
                                else
                                {
                                    valorProduto = produto.PRECOS_PRODUTO.Current.VLR_PRODUTO;
                                }

                                if (itempedido.QTD_PEDIDA_UNIDADE != 0)
                                    valorProduto += (valorProduto * (CSProdutos.Current.PRC_ACRESCIMO_QTDE_UNITARIA / 100));

                                itempedido.VLR_DESCONTO = CSGlobal.Round((valorProduto * (itempedido.PRC_DESCONTO / 100)), 2);
                                //itempedido.VLR_DESCONTO_UNITARIO = CSGlobal.Round(itempedido.VLR_DESCONTO / (decimal)itempedido.PRODUTO.QTD_UNIDADE_EMBALAGEM, 2);
                                itempedido.VLR_DESCONTO_UNITARIO = itempedido.VLR_DESCONTO;

                                // Calcula o valor total do desconto do item
                                itempedido.VLR_DESCONTO = ((decimal)itempedido.VLR_DESCONTO * itempedido.QTD_PEDIDA_INTEIRA) +
                                                           CSGlobal.Round(CSGlobal.Round(itempedido.VLR_DESCONTO / (decimal)itempedido.PRODUTO.QTD_UNIDADE_EMBALAGEM, 3) * itempedido.QTD_PEDIDA_UNIDADE, 2);

                                valorProduto -= (valorProduto * (itempedido.PRC_DESCONTO / 100));
                                valorProduto += (valorProduto * (itempedido.PRC_ADICIONAL_FINANCEIRO / 100));

                                itempedido.VLR_ITEM_UNIDADE = CSGlobal.Round(valorProduto, 2);

                                decimal valorTotal = valorProduto * itempedido.QTD_PEDIDA_INTEIRA;
                                valorTotal += CSGlobal.Round(valorProduto / (decimal)itempedido.PRODUTO.QTD_UNIDADE_EMBALAGEM, 3) * itempedido.QTD_PEDIDA_UNIDADE;
                                valorTotal = CSGlobal.Round(valorTotal, 2);

                                itempedido.VLR_TOTAL_ITEM = valorTotal;
                            }
                            else
                            {
                                if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                                    itempedido.CalculaValor2014();
                                else
                                    itempedido.CalculaValor();
                            }

                            decimal vlr_total = itempedido.VLR_TOTAL_ITEM;
                            decimal vlr_desconto = itempedido.VLR_DESCONTO;
                            decimal vlr_desconto_unitario = itempedido.VLR_DESCONTO_UNITARIO;
                            decimal vlr_unidade = itempedido.VLR_ITEM_UNIDADE;

                            CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Add(itempedido);
                            itempedido.AtualizaImagem();
                        }
                    }

                    if (excluirItemPedido)
                    {
                        foreach (CSItemsPedido.CSItemPedido itemPedido in CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Items)
                        {
                            if (itemPedido.COD_ITEM_COMBO == produtoCombo)
                            {
                                itemPedido.STATE = ObjectState.DELETADO;
                                itemPedido.AtualizaImagem();
                            }
                        }

                        CSGlobal.PedidoComCombo = true;

                        if (PoliticaBroker)
                            CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 2;
                    }
                }
                catch (System.Exception ex)
                {
                    MessageBox.AlertErro(CurrentActivity, ex.Message);
                }
            }

            private bool IsBroker()
            {
                return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
            }
        }

        private bool ValidaDados()
        {
            int saldo = 0;

            if ((Convert.ToInt32(txtQuantidade.Text) + QuantidadeVendida) > QuantidadeMaxima)
            {
                saldo = QuantidadeMaxima - QuantidadeVendida;

                if (saldo > 0)
                    MessageBox.Alert(Activity, ("Quantidade máxima de venda: (" + QuantidadeMaxima + ")\nDisponível: (" + saldo + ")"));
                else
                    MessageBox.ShowShortMessageCenter(Activity, "Quantidade de venda máxima do combo atingida.");

                return false;
            }

            return true;
        }

        private void BtnCancelar_Click(object sender, EventArgs e)
        {
            Dismiss();
        }

        private void FindViewsById(View view)
        {
            txtQuantidade = view.FindViewById<EditText>(Resource.Id.txtQuantidade);
            btnOK = view.FindViewById<Button>(Resource.Id.btnOK);
            btnCancelar = view.FindViewById<Button>(Resource.Id.btnCancelar);
        }
    }
}