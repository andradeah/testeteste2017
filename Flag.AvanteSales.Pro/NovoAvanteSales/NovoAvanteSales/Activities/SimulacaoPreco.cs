using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AvanteSales.Pro.Controles;
using AvanteSales.Pro.Dialogs;
using AvanteSales.Pro.Formatters;
using AvanteSales.Pro.Fragments;
using AvanteSales.SystemFramework;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "SimulacaoPreco")]
    public class SimulacaoPreco : AppCompatActivity, TextView.IOnEditorActionListener
    {
        private const int PDV = 1;
        private const int ITEM = 2;
        private const int LISTA = 3;

        CSProdutos.CSProduto Produto = null;
        static ProgressDialog progress;

        Android.Support.V7.Widget.Toolbar tbToolbar;
        TextView lblCodPdv;
        TextView lblNomePdv;

        private Spinner cboCondicaoPagamento;
        private TextView txtItem;
        private TextView tvDescricao;
        private TextView tvUnidadeMedida;
        private EditText txtQtdInteira;
        private EditText txtQtdUnitaria;
        private TextView tvPrecoTabela;
        private Spinner cboTabelaPreco;
        private EditText txtDesconto;
        private TextView tvTotalProduto;
        private TextView tvQtdUnitaria;
        private TextView tvDesconto;
        private static TextView tvTotalPedido;
        private ImageButton btnPesquisarProduto;
        private Button btnAdicionarLista;
        private static Button btnListaProdutos;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.simulacao_preco);

            FindViewsById();

            Eventos();

            SetSupportActionBar(tbToolbar);

            Inicializacao();
        }

        private void Eventos()
        {
            txtItem.SetOnEditorActionListener(this);
            txtItem.TextChanged += TxtItem_TextChanged;
            cboTabelaPreco.ItemSelected += CboTabelaPreco_ItemSelected;
            txtQtdInteira.TextChanged += TxtQtdInteira_TextChanged;
            txtQtdUnitaria.TextChanged += TxtQtdUnitaria_TextChanged;
            txtDesconto.TextChanged += TxtDesconto_TextChanged;
            cboCondicaoPagamento.ItemSelected += CboCondicaoPagamento_ItemSelected;
            btnPesquisarProduto.Click += BtnPesquisarProduto_Click;
            btnAdicionarLista.Click += BtnAdicionarLista_Click;
            btnListaProdutos.Click += BtnListaProdutos_Click;
        }

        private void BtnListaProdutos_Click(object sender, EventArgs e)
        {
            AbrirListaProdutos();
        }

        private void BtnAdicionarLista_Click(object sender, EventArgs e)
        {
            if (tvDescricao.Text == string.Empty)
                MessageBox.AlertErro(this, "Pesquise o produto para validação.");
            else
                AdicionarAoPedido();
        }

        private void AbrirListaProdutos()
        {
            if (CSPDVs.Current.PEDIDOS_PDV.Current != null &&
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.STATE != ObjectState.DELETADO).Count() > 0)
            {
                Intent i = new Intent();
                i.SetClass(this, typeof(ListaProdutosIndenizacao));
                i.PutExtra("ultimaActivity", (int)ActivitiesNames.SimulacaoPreco);
                i.PutExtra("txtDescontoIndenizacao", string.Empty);
                i.PutExtra("txtAdf", string.Empty);
                this.StartActivityForResult(i, LISTA);
            }
        }

        private void AdicionarAoPedido()
        {
            if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current != null)
            {
                if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE == ObjectState.DELETADO)
                {
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.ATUALIZAR_SALDO_DESCONTO = false;
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE = ObjectState.NOVO_ALTERADO;
                }

                if (txtQtdInteira.Text != string.Empty)
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_INTEIRA = Convert.ToDecimal(txtQtdInteira.Text);
                else
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_INTEIRA = 0;

                if (txtQtdUnitaria.Text != string.Empty)
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_UNIDADE = Convert.ToInt32(txtQtdUnitaria.Text);
                else
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_UNIDADE = 0;

                if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.QTD_PEDIDA_TOTAL == 0)
                    MessageBox.AlertErro(this, "Quantidade de venda inválida");
                else
                {
                    if (txtDesconto.Text != string.Empty)
                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_DESCONTO = Convert.ToDecimal(txtDesconto.Text);
                    else
                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_DESCONTO = 0;

                    if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE != ObjectState.ALTERADO)
                        CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Add(CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current);

                    tvTotalPedido.Text = CSPDVs.Current.PEDIDOS_PDV.Current.VLR_TOTAL_PEDIDO.ToString(CSGlobal.DecimalStringFormat);

                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current = null;

                    LimparCamposItem(true);
                    txtItem.Text = string.Empty;
                    //cboTabelaPreco.Clear();

                    txtItem.RequestFocus();

                    progress = new ProgressDialogCustomizado(this, LayoutInflater).Customizar();
                    progress.Show();

                    new ThreadAtualizarInformacoes().Execute();
                }
            }
        }

        private class ThreadAtualizarInformacoes : AsyncTask
        {
            decimal totalPedido;
            string qtdItens;

            protected override Java.Lang.Object DoInBackground(params Java.Lang.Object[] @params)
            {
                if (CSPDVs.Current != null &&
               CSPDVs.Current.PEDIDOS_PDV.Current != null &&
               CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Count > 0)
                    totalPedido = CSPDVs.Current.PEDIDOS_PDV.Current.VLR_TOTAL_PEDIDO;

                if (CSPDVs.Current.PEDIDOS_PDV.Current != null)
                {
                    qtdItens = "Lista ( " + CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Cast<CSItemsPedido.CSItemPedido>().Where(p => p.STATE != ObjectState.DELETADO).Count() + " )";
                }

                return true;
            }

            protected override void OnPostExecute(Java.Lang.Object result)
            {
                base.OnPostExecute(result);

                tvTotalPedido.Text = totalPedido.ToString(CSGlobal.DecimalStringFormat);
                btnListaProdutos.Text = qtdItens;

                if (progress != null)
                    progress.Dismiss();
            }
        }

        private void BtnPesquisarProduto_Click(object sender, EventArgs e)
        {
            PesquisaProduto();
        }

        private void CboCondicaoPagamento_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            AtribuiValorCondicaoPagamento();

            if (TodosCamposNecessariosPreenchidos())
            {
                CalculaValores();
            }

            if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Count > 0)
                RecalculaCalculaValores();
        }

        private bool CaracteresValidosDesconto()
        {
            if (StringFormatter.NaoDecimal(txtDesconto.Text))
            {
                LimparUltimoCaractereEPosicionarCursor(txtDesconto);
                return false;
            }

            if (txtDesconto.Text.Contains(','))
            {
                int posicao = txtDesconto.Text.IndexOf(',');

                if (txtDesconto.Text.Substring(posicao + 1, txtDesconto.Text.Length - posicao - 1).Length > 2)
                {
                    LimparUltimoCaractereEPosicionarCursor(txtDesconto);
                    return false;
                }
            }

            return true;
        }

        private void TxtDesconto_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (Produto == null)
                return;

            if (!string.IsNullOrEmpty(txtDesconto.Text))
                if (txtDesconto.Text.Contains("."))
                {
                    txtDesconto.Text = txtDesconto.Text.Replace(".", ",");
                    txtDesconto.SetSelection(txtDesconto.Text.Length);
                }

            if (txtDesconto.Text.Length > 0)
            {
                if (!CaracteresValidosDesconto())
                    return;
            }

            if (TodosCamposNecessariosPreenchidos())
            {
                CalculaValores();
            }

            if (txtDesconto.Text.Length > 0)
            {
                if (CSGlobal.StrToDecimal(txtDesconto.Text) > CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_DESCONTO_MAXIMO)
                {
                    MessageBox.AlertErro(this, "O percentual de desconto não pode ser maior que \"" + CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_DESCONTO_MAXIMO.ToString(CSGlobal.DecimalStringFormat) + "\".");
                    txtDesconto.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_DESCONTO_MAXIMO.ToString(CSGlobal.DecimalStringFormat);
                    return;
                }
            }
        }

        private bool QuantidadeUnitariaValida(int QtdUnitaria)
        {
            return QtdUnitaria < CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO.QTD_UNIDADE_EMBALAGEM;
        }

        private void TxtQtdUnitaria_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (TodosCamposNecessariosPreenchidos())
            {
                if (txtQtdUnitaria.Text != string.Empty)
                {
                    if (!QuantidadeUnitariaValida(Convert.ToInt32(txtQtdUnitaria.Text)))
                    {
                        MessageBox.AlertErro(this, "Quantidade unitária inválida.");

                        LimparUltimoCaractereEPosicionarCursor(txtQtdUnitaria);

                        return;
                    }
                }

                CalculaValores();
            }

            if (txtQtdInteira.Text.Length == 0 &&
                txtQtdUnitaria.Text.Length == 0)
            {
                tvTotalProduto.Text = string.Empty;
            }
        }

        private void TxtQtdInteira_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtQtdInteira.Text))
                if (txtQtdInteira.Text.Contains("."))
                {
                    txtQtdInteira.Text = txtQtdInteira.Text.Replace(".", ",");
                    txtQtdInteira.SetSelection(txtQtdInteira.Text.Length);
                }

            if (TodosCamposNecessariosPreenchidos())
            {
                if (txtQtdInteira.Text != string.Empty)
                {
                    if (!CaracteresValidosQtdInteira())
                        return;
                }

                CalculaValores();
            }

            if (txtQtdInteira.Text.Length == 0 &&
                txtQtdUnitaria.Text.Length == 0)
            {
                tvTotalProduto.Text = string.Empty;
            }
        }

        private bool CaracteresValidosQtdInteira()
        {
            if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO.COD_UNIDADE_MEDIDA == "KG" ||
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO.COD_UNIDADE_MEDIDA == "LT")
            {
                if (StringFormatter.NaoDecimal(txtQtdInteira.Text))
                {
                    LimparUltimoCaractereEPosicionarCursor(txtQtdInteira);
                    return false;
                }

                if (txtQtdInteira.Text.Contains(","))
                {
                    int posicao = txtQtdInteira.Text.IndexOf(',');

                    if (txtQtdInteira.Text.Substring(posicao + 1, txtQtdInteira.Text.Length - posicao - 1).Length > 2)
                    {
                        LimparUltimoCaractereEPosicionarCursor(txtQtdInteira);
                        return false;
                    }
                }
            }
            else
            {
                if (txtQtdInteira.Text != string.Empty &&
                    txtQtdInteira.Text.Contains(","))
                {
                    LimparUltimoCaractereEPosicionarCursor(txtQtdInteira);
                    return false;
                }
            }

            return true;
        }

        private void LimparUltimoCaractereEPosicionarCursor(EditText campo)
        {
            campo.Text = campo.Text.Remove(campo.Text.Length - 1);
            campo.SetSelection(campo.Text.Length);
        }

        private bool TodosCamposNecessariosPreenchidos()
        {
            if (CSPDVs.Current == null)
            {
                return false;
            }

            if (Produto == null)
            {
                return false;
            }

            if (txtQtdInteira.Text == string.Empty &&
                txtQtdUnitaria.Text == string.Empty)
            {
                return false;
            }

            return true;
        }

        private void CboTabelaPreco_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (TodosCamposNecessariosPreenchidos())
            {
                AtribuiValorProdutoTabela(Produto);
                CalculaValores();
            }
        }

        private void TxtItem_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            LimparCamposItem(false);
        }

        private void LimparCamposItem(bool LimparSemValidar)
        {
            if (txtItem.Text == string.Empty ||
                LimparSemValidar)
            {
                Produto = null;
                CSProdutos.Current = null;
                tvDescricao.Text = string.Empty;
                tvUnidadeMedida.Text = string.Empty;
                tvPrecoTabela.Text = string.Empty;
                txtQtdInteira.Text = string.Empty;
                txtQtdUnitaria.Text = string.Empty;
                txtDesconto.Text = string.Empty;
                tvTotalProduto.Text = string.Empty;
            }
        }

        private void Inicializacao()
        {
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(false);

            lblCodPdv.Text = CSPDVs.Current.COD_PDV.ToString();
            lblNomePdv.Text = CSPDVs.Current.DSC_RAZAO_SOCIAL;

            if (CSEmpresa.Current.IND_POLITICA_CALCULO_PRECO_MISTA)
            {
                if (CSPDVs.Current.CD_CLIENTE == string.Empty &&
                    CSPDVs.Current.DSC_CLIPPING_INFORMATIVO.ToUpper() != "BUNGE")
                {
                    if (CSPDVs.Current.CDGER0 == string.Empty)
                        CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 1;
                    else
                    {
                        MessageBox.Alert(this, "Pedido utilizando politica de preço FlexX?", "FlexX",
                        (sender, e) =>
                        {
                            CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 1;

                            if (CSPDVs.Current != null)
                                CarregaComboBoxCondicaoPagamento();
                        }, "Não",
                            (sender, e) =>
                            {
                                CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 2;

                                if (CSPDVs.Current != null)
                                    CarregaComboBoxCondicaoPagamento();
                            }, false);
                    }
                }
                else
                {
                    if (CSPDVs.Current.CDGER0 != string.Empty)
                    {
                        Android.Support.V7.App.AlertDialog.Builder alertDialog = new Android.Support.V7.App.AlertDialog.Builder(this);
                        alertDialog.SetTitle("Escolha a política de preço a utilizar:");
                        alertDialog.SetNeutralButton("Flexx", (_e, sender) =>
                        {
                            CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 1; if (CSPDVs.Current != null)
                                CarregaComboBoxCondicaoPagamento();
                        });
                        alertDialog.SetNeutralButton("Broker", (_e, sender) =>
                        {
                            CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 2; if (CSPDVs.Current != null)
                                CarregaComboBoxCondicaoPagamento();
                        });
                        alertDialog.SetNeutralButton("Bunge", (_e, sender) =>
                        {
                            CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 3; if (CSPDVs.Current != null)
                                CarregaComboBoxCondicaoPagamento();
                        });
                        alertDialog.SetCancelable(false);
                        alertDialog.Show();
                    }
                    else
                    {
                        MessageBox.Alert(this, "Pedido utilizando politica de preço FlexX?", "FlexX",
                        (sender, e) =>
                        {
                            CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 1;

                            if (CSPDVs.Current != null)
                                CarregaComboBoxCondicaoPagamento();
                        }, "Não",
                            (sender, e) =>
                            {
                                CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 3;

                                if (CSPDVs.Current != null)
                                    CarregaComboBoxCondicaoPagamento();
                            }, false);
                    }
                }
            }
            else
            {
                if (CSPDVs.Current != null)
                    CarregaComboBoxCondicaoPagamento();
            }

            progress = new ProgressDialogCustomizado(this, LayoutInflater).Customizar();
            progress.Show();

            new ThreadAtualizarInformacoes().Execute();

            ListaCliente.InformacoesPdvClick = false;
        }

        private void FindViewsById()
        {
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            lblCodPdv = FindViewById<TextView>(Resource.Id.lblCodPdv);
            lblNomePdv = FindViewById<TextView>(Resource.Id.lblNomePdv);
            cboCondicaoPagamento = FindViewById<Spinner>(Resource.Id.cboCondicaoPagamento);
            txtItem = FindViewById<EditText>(Resource.Id.txtItem);
            tvDescricao = FindViewById<TextView>(Resource.Id.tvDescricao);
            tvUnidadeMedida = FindViewById<TextView>(Resource.Id.tvUnidadeMedida);
            txtQtdInteira = FindViewById<EditText>(Resource.Id.txtQtdInteira);
            txtQtdUnitaria = FindViewById<EditText>(Resource.Id.txtQtdUnitaria);
            tvPrecoTabela = FindViewById<TextView>(Resource.Id.tvPrecoTabela);
            cboTabelaPreco = FindViewById<Spinner>(Resource.Id.cboTabelaPreco);
            txtDesconto = FindViewById<EditText>(Resource.Id.txtDesconto);
            tvTotalProduto = FindViewById<TextView>(Resource.Id.tvTotalProduto);
            tvQtdUnitaria = FindViewById<TextView>(Resource.Id.tvQtdUnitaria);
            tvDesconto = FindViewById<TextView>(Resource.Id.tvDesconto);
            tvTotalPedido = FindViewById<TextView>(Resource.Id.tvTotalPedido);
            btnPesquisarProduto = FindViewById<ImageButton>(Resource.Id.btnPesquisarProduto);
            btnAdicionarLista = FindViewById<Button>(Resource.Id.btnAdicionarLista);
            btnListaProdutos = FindViewById<Button>(Resource.Id.btnListaProdutos);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.Finish();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        public bool OnEditorAction(TextView v, [GeneratedEnum] ImeAction actionId, KeyEvent e)
        {
            switch (v.Id)
            {
                case Resource.Id.txtItem:
                    {
                        PesquisaProduto();
                    }
                    break;
            }
            return true;
        }

        private void PesquisaProduto()
        {
            if (CSPDVs.Current != null)
            {
                if (txtItem.Text != string.Empty)
                {
                    foreach (CSProdutos.CSProduto produtoAtual in CSProdutos.Items)
                    {
                        if (produtoAtual.COD_PRODUTO == Convert.ToInt32(txtItem.Text) ||
                            produtoAtual.DESCRICAO_APELIDO_PRODUTO == txtItem.Text)
                        {
                            Produto = produtoAtual;
                            break;
                        }
                        else
                            Produto = null;
                    }

                    if (Produto != null)
                    {
                        CSProdutos.Current = Produto;
                        PreencheCamposDeItem(Produto);

                        if (IsBroker())
                            CalculaValores();
                    }
                    else
                    {
                        Intent i = new Intent();
                        i.SetClass(this, typeof(ProcuraProduto));
                        //i.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                        i.PutExtra("ultimaActivity", (int)ActivitiesNames.SimulacaoPreco);
                        this.StartActivityForResult(i, ITEM);
                    }
                    //StartActivityForResult(new Intent(this, new ProcuraProduto().Class), ITEM);
                }

                else
                {
                    Intent i = new Intent();
                    i.SetClass(this, typeof(ProcuraProduto));
                    //i.SetFlags(ActivityFlags.ClearTop | ActivityFlags.NewTask);
                    i.PutExtra("ultimaActivity", (int)ActivitiesNames.SimulacaoPreco);
                    this.StartActivityForResult(i, ITEM);
                }
                //StartActivityForResult(new Intent(this, new ProcuraProduto().Class), ITEM);
            }
            else
            {
                MessageBox.AlertErro(this, "Campo 'Cliente' deve conter um PDV válido");
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            switch (requestCode)
            {
                case PDV:
                    {
                        if (resultCode == Result.Ok)
                        {
                            int CodPdv = data.GetIntExtra("codPDV", -1);

                            if (CodPdv != -1)
                            {
                                CSPDVs.Current = CSPDVs.Items.Cast<CSPDVs.CSPDV>().Where(p => p.COD_PDV == CodPdv).FirstOrDefault();

                                if (CSPDVs.Current != null)
                                {
                                    //PreencheCamposDePDV();
                                    CarregaComboBoxCondicaoPagamento();
                                }
                            }
                        }
                    }
                    break;

                case ITEM:
                    {
                        if (resultCode == Result.Ok && data != null)
                        {
                            int CodItem = data.GetIntExtra("codProduto", -1);

                            if (CodItem != -1)
                            {
                                Produto = null;

                                foreach (CSProdutos.CSProduto produtoAtual in CSProdutos.Items)
                                {
                                    if (produtoAtual.COD_PRODUTO == CodItem)
                                    {
                                        Produto = produtoAtual;
                                        break;
                                    }
                                    else
                                        Produto = null;
                                }

                                if (Produto != null)
                                {
                                    CSProdutos.Current = Produto;
                                    PreencheCamposDeItem(CSProdutos.Current);

                                    if (IsBroker())
                                        CalculaValores();
                                }
                            }
                        }
                    }
                    break;
                case LISTA:
                    {
                        progress = new ProgressDialogCustomizado(this, LayoutInflater).Customizar();
                        progress.Show();

                        new ThreadAtualizarInformacoes().Execute();
                    }
                    break;
            }
            base.OnActivityResult(requestCode, resultCode, data);
        }

        private void PreencheCamposDeItem(CSProdutos.CSProduto Produto)
        {
            txtQtdInteira.Text = string.Empty;
            txtQtdUnitaria.Text = string.Empty;
            txtDesconto.Text = string.Empty;
            tvTotalProduto.Text = string.Empty;

            if (CSPDVs.Current.PEDIDOS_PDV.Current == null)
            {
                CSPDVs.Current.PEDIDOS_PDV.Current = new CSPedidosPDV.CSPedidoPDV();
            }

            if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current == null)
            {
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current = new CSItemsPedido.CSItemPedido();
            }

            CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO = Produto;

            CarregaComboBoxTabelaPreco();
            txtItem.Text = Produto.DESCRICAO_APELIDO_PRODUTO;
            tvDescricao.Text = Produto.DSC_APELIDO_PRODUTO;
            tvUnidadeMedida.Text = Produto.COD_UNIDADE_MEDIDA + "/" + CSProdutos.Current.QTD_UNIDADE_EMBALAGEM.ToString();

            AtribuiValorCondicaoPagamento();
            AtribuiValorProdutoTabela(Produto);
            CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.AplicaDescontoMaximoProdutoTabPreco();

            if (Produto.COD_UNIDADE_MEDIDA == "UN")
            {
                tvQtdUnitaria.Visibility = ViewStates.Gone;
                txtQtdUnitaria.Visibility = ViewStates.Gone;
            }
            else
            {
                tvQtdUnitaria.Visibility = ViewStates.Visible;
                txtQtdUnitaria.Visibility = ViewStates.Visible;
            }

            if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_DESCONTO_MAXIMO == 0)
            {
                tvDesconto.Visibility = ViewStates.Gone;
                txtDesconto.Visibility = ViewStates.Gone;
            }
            else
            {
                tvDesconto.Visibility = ViewStates.Visible;
                txtDesconto.Visibility = ViewStates.Visible;
            }

            if (ProdutoExistenteNoPedido(Produto.COD_PRODUTO))
            {
                PreencherInformacoesComplementares();
            }
        }

        private void CarregaComboBoxCondicaoPagamento()
        {
            var adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cboCondicaoPagamento.Adapter = adapter;
            int condicoesInativasAteAgora = 0;
            // Preenche o combo de condições de pagamento
            for (int i = 0; i < CSCondicoesPagamento.Items.Count; i++)
            {
                CSCondicoesPagamento.CSCondicaoPagamento condpag = CSCondicoesPagamento.Items[i];
                // Mostra somente as condições ativas
                if (condpag.IND_ATIVO == true)
                {
                    // Lista somente as prioridades menores ou iguais a do PDV

                    if (condpag.PRIORIDADE_CONDICAO_PAGAMENTO <= CSPDVs.Current.CONDICAO_PAGAMENTO.PRIORIDADE_CONDICAO_PAGAMENTO)
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
                                        ((ArrayAdapter)cboCondicaoPagamento.Adapter).Add(ic);
                                    }
                                }
                                else
                                {
                                    // Adiciona o item no combo
                                    ((ArrayAdapter)cboCondicaoPagamento.Adapter).Add(ic);
                                }
                            }
                        }
                        else
                        {
                            // Adiciona o item no combo
                            ((ArrayAdapter)cboCondicaoPagamento.Adapter).Add(ic);
                        }

                        // Se for um novo pedido seleciona a condicao de pagamento padrao do PDV
                        if (CSPDVs.Current != null)
                        {
                            if (CSPDVs.Current.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO == condpag.COD_CONDICAO_PAGAMENTO)
                            {
                                cboCondicaoPagamento.SetSelection(i - condicoesInativasAteAgora);
                            }
                        }
                    }
                }
                else
                    condicoesInativasAteAgora++;
            }

            ValidarComboCondicaoPagamento();
        }

        private void ValidarComboCondicaoPagamento()
        {
            try
            {
                if (cboCondicaoPagamento.Adapter.Count == 0)
                {
                    MessageBox.Alert(this, "Simulação de preço impedida, informação de condição de pagamento incompleta.", "OK",
                        (_sender, _e) => { base.OnBackPressed(); }, false);
                }
            }
            catch (Exception ex)
            {
                MessageBox.AlertErro(this, ex.Message);
            }
        }

        private void CarregaComboBoxTabelaPreco()
        {
            try
            {
                int tabelaPadrao = 0;
                //int tabelaAnterior = 0;

                var adapter = cboTabelaPreco.SetDefaultAdapter();

                // Preenche os preços dos produtos
                for (int i = 0; i < CSProdutos.Current.PRECOS_PRODUTO.Count; i++)
                {
                    CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto preco = CSProdutos.Current.PRECOS_PRODUTO[i];

                    CSItemCombo ic = new CSItemCombo();
                    ic.Texto = preco.COD_TABELA_PRECO.ToString() + " " + preco.DSC_TABELA_PRECO;
                    ic.Valor = preco;
                    adapter.Add(ic);

                    // [ Busca tabela de preço padrão do pdv ]
                    if (CSPDVs.Current.COD_TABPRECO_PADRAO == preco.COD_TABELA_PRECO)
                        tabelaPadrao = i;
                }
                // [ Encontrou tabelas? ]
                if (adapter.Count > 0)
                {
                    // [ Seleciona tabela padrão ]
                    cboTabelaPreco.SetSelection(tabelaPadrao);
                }
            }
            catch
            {

            }
        }

        private bool IsBroker()
        {
            return CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2;
        }

        private void CalculaValores()
        {
            CSItemsPedido.CSItemPedido itemPedido = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current;

            if (!IsBroker())
            {
                itemPedido.VLR_TOTAL_ITEM = CalcularPreco();
            }

            else
            {
                if (txtDesconto.Text != string.Empty)
                    itemPedido.PRC_DESCONTO = Convert.ToDecimal(txtDesconto.Text);
                else
                    itemPedido.PRC_DESCONTO = 0;

                int qtdInteira;
                int qtdUnitaria;

                if (txtQtdInteira.Text != string.Empty)
                    qtdInteira = Convert.ToInt32(txtQtdInteira.Text);
                else
                    qtdInteira = 0;

                if (txtQtdUnitaria.Visibility == ViewStates.Visible)
                {
                    if (txtQtdUnitaria.Text != string.Empty)
                        qtdUnitaria = Convert.ToInt32(txtQtdUnitaria.Text);
                    else
                        qtdUnitaria = 0;
                }
                else
                    qtdUnitaria = 0;

                itemPedido.QTD_PEDIDA_INTEIRA = qtdInteira;
                itemPedido.QTD_PEDIDA_UNIDADE = qtdUnitaria;

                if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                    itemPedido.CalculaValor2014();
                else
                    itemPedido.CalculaValor();
            }

            tvTotalProduto.Text = itemPedido.VLR_TOTAL_ITEM.ToString(CSGlobal.DecimalStringFormat);
        }

        private void RecalculaCalculaValores()
        {
            foreach (CSItemsPedido.CSItemPedido itemPedido in CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS)
            {
                if (!IsBroker())
                {
                    decimal Valor = 0m;

                    decimal valorAdicionalFinanceiro;
                    decimal percentualAdicionalFinanceiro;
                    decimal valorTabela;
                    decimal vlrDesconto;

                    vlrDesconto = itemPedido.PRC_DESCONTO;

                    CSCondicoesPagamento.CSCondicaoPagamento CondicaoPagamento = (CSCondicoesPagamento.CSCondicaoPagamento)((CSItemCombo)cboCondicaoPagamento.SelectedItem).Valor;

                    if (cboTabelaPreco.SelectedItem == null)
                        return;

                    CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto TabelaPreco = (CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto)((CSItemCombo)cboTabelaPreco.SelectedItem).Valor;

                    itemPedido.PRODUTO.PRECOS_PRODUTO.Current = itemPedido.PRODUTO.PRECOS_PRODUTO.GetPrecoProduto(TabelaPreco.COD_TABELA_PRECO);

                    percentualAdicionalFinanceiro = CondicaoPagamento.PRC_ADICIONAL_FINANCEIRO;

                    valorAdicionalFinanceiro = itemPedido.PRODUTO.PRECOS_PRODUTO.Current.VLR_PRODUTO * (percentualAdicionalFinanceiro / 100);

                    valorTabela = Math.Round(itemPedido.PRODUTO.PRECOS_PRODUTO.Current.VLR_PRODUTO - (itemPedido.PRODUTO.PRECOS_PRODUTO.Current.VLR_PRODUTO * (vlrDesconto / 100)), 2, MidpointRounding.AwayFromZero);

                    decimal qtdInteira = itemPedido.QTD_PEDIDA_INTEIRA;
                    decimal qtdPartida = itemPedido.QTD_PEDIDA_UNIDADE;
                    decimal qtdCaixa = itemPedido.PRODUTO.UNIDADES_POR_CAIXA;
                    decimal descontoTotal = 0m;
                    decimal descontoUnitario = 0m;
                    decimal adicionalUnitario = 0m;

                    descontoUnitario = itemPedido.PRODUTO.PRECOS_PRODUTO.Current.VLR_PRODUTO - valorTabela;

                    descontoTotal = Math.Round(((descontoUnitario / qtdCaixa) * ((qtdInteira * qtdCaixa) + qtdPartida)), 2, MidpointRounding.AwayFromZero);

                    adicionalUnitario = Math.Round(valorAdicionalFinanceiro / qtdCaixa, 2, MidpointRounding.AwayFromZero);

                    decimal PrecoComDescontoUnitario = valorTabela / qtdCaixa;
                    decimal PrecoComAdicionalDescontoUnitario = PrecoComDescontoUnitario * (1 + (percentualAdicionalFinanceiro / 100));
                    decimal Quantidade = (qtdInteira * qtdCaixa) + qtdPartida;
                    Valor = Math.Round((Quantidade * PrecoComAdicionalDescontoUnitario), 2, MidpointRounding.AwayFromZero);
                    decimal vlr_Unitario = Math.Round(PrecoComAdicionalDescontoUnitario * qtdCaixa, 2, MidpointRounding.AwayFromZero);
                    itemPedido.VLR_ITEM_UNIDADE = vlr_Unitario;

                    itemPedido.VLR_TOTAL_ITEM = Valor;
                }

                else
                {
                    if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                        itemPedido.CalculaValor2014();
                    else
                        itemPedido.CalculaValor();
                }
            }

            tvTotalPedido.Text = CSPDVs.Current.PEDIDOS_PDV.Current.VLR_TOTAL_PEDIDO.ToString(CSGlobal.DecimalStringFormat);
        }

        private decimal CalcularPreco()
        {
            decimal Valor = 0m;

            if (tvPrecoTabela.Text != string.Empty)
            {
                decimal valorAdicionalFinanceiro;
                decimal percentualAdicionalFinanceiro;
                decimal valorTabela;
                decimal vlrDesconto;

                if (txtDesconto.Text == string.Empty)
                    vlrDesconto = 0;
                else
                    vlrDesconto = CSGlobal.StrToDecimal(txtDesconto.Text);

                CSCondicoesPagamento.CSCondicaoPagamento CondicaoPagamento = (CSCondicoesPagamento.CSCondicaoPagamento)((CSItemCombo)cboCondicaoPagamento.SelectedItem).Valor;

                percentualAdicionalFinanceiro = CondicaoPagamento.PRC_ADICIONAL_FINANCEIRO;

                valorAdicionalFinanceiro = CSProdutos.Current.PRECOS_PRODUTO.Current.VLR_PRODUTO * (percentualAdicionalFinanceiro / 100);

                valorTabela = Math.Round(CSGlobal.StrToDecimal(tvPrecoTabela.Text) - (CSGlobal.StrToDecimal(tvPrecoTabela.Text) * (vlrDesconto / 100)), 2, MidpointRounding.AwayFromZero);

                decimal qtdInteira = CSGlobal.StrToDecimal(txtQtdInteira.Text);
                decimal qtdPartida = CSGlobal.StrToDecimal(txtQtdUnitaria.Text);
                decimal qtdCaixa = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRODUTO.UNIDADES_POR_CAIXA;
                decimal descontoTotal = 0m;
                decimal descontoUnitario = 0m;
                decimal adicionalUnitario = 0m;

                descontoUnitario = CSGlobal.StrToDecimal(tvPrecoTabela.Text) - valorTabela;

                descontoTotal = Math.Round(((descontoUnitario / qtdCaixa) * ((qtdInteira * qtdCaixa) + qtdPartida)), 2, MidpointRounding.AwayFromZero);

                adicionalUnitario = Math.Round(valorAdicionalFinanceiro / qtdCaixa, 2, MidpointRounding.AwayFromZero);

                decimal PrecoComDescontoUnitario = valorTabela / qtdCaixa;
                decimal PrecoComAdicionalDescontoUnitario = PrecoComDescontoUnitario * (1 + (percentualAdicionalFinanceiro / 100));
                decimal Quantidade = (qtdInteira * qtdCaixa) + qtdPartida;
                Valor = Math.Round((Quantidade * PrecoComAdicionalDescontoUnitario), 2, MidpointRounding.AwayFromZero);
                decimal vlr_Unitario = Math.Round(PrecoComAdicionalDescontoUnitario * qtdCaixa, 2, MidpointRounding.AwayFromZero);
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.VLR_ITEM_UNIDADE = vlr_Unitario;
            }

            return Valor;
        }

        private void PreencherInformacoesComplementares()
        {
            var itemAtual = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current;

            decimal qtdInteira = itemAtual.QTD_PEDIDA_INTEIRA;
            int qtdUnitaria = itemAtual.QTD_PEDIDA_UNIDADE;

            if (qtdInteira > 0)
                txtQtdInteira.Text = qtdInteira.ToString();

            if (qtdUnitaria > 0)
                txtQtdUnitaria.Text = qtdUnitaria.ToString();

            if (CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_DESCONTO > 0)
                txtDesconto.Text = CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.PRC_DESCONTO.ToString();
        }

        private bool ProdutoExistenteNoPedido(int COD_PRODUTO)
        {
            foreach (CSItemsPedido.CSItemPedido itemPedido in CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS)
            {
                if (itemPedido.PRODUTO.COD_PRODUTO == COD_PRODUTO &&
                    itemPedido.STATE != ObjectState.DELETADO)
                {
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current = itemPedido;
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.ATUALIZAR_SALDO_DESCONTO = false;
                    CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.STATE = ObjectState.ALTERADO;
                    return true;
                }
            }

            return false;
        }

        private void AtribuiValorProdutoTabela(CSProdutos.CSProduto Produto)
        {
            try
            {
                CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto TabelaPreco = (CSProdutos.CSProduto.CSPrecosProdutos.CSPrecoProduto)((CSItemCombo)cboTabelaPreco.SelectedItem).Valor;

                CSProdutos.Current.PRECOS_PRODUTO.Current = Produto.PRECOS_PRODUTO.GetPrecoProduto(TabelaPreco.COD_TABELA_PRECO);

                tvPrecoTabela.Text = CSProdutos.Current.PRECOS_PRODUTO.Current.VLR_PRODUTO.ToString(CSGlobal.DecimalStringFormat);

                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current.COD_TABELA_PRECO = TabelaPreco.COD_TABELA_PRECO;
            }
            catch
            {
                txtItem.Text = string.Empty;
                MessageBox.AlertErro(this, "Produto sem tabela de preço.");
            }
        }

        private void AtribuiValorCondicaoPagamento()
        {
            if (CSPDVs.Current.PEDIDOS_PDV.Current == null)
            {
                CSPDVs.Current.PEDIDOS_PDV.Current = new CSPedidosPDV.CSPedidoPDV();
                CSPDVs.Current.PEDIDOS_PDV.Current.ITEMS_PEDIDOS.Current = new CSItemsPedido.CSItemPedido();
            }

            CSCondicoesPagamento.CSCondicaoPagamento CondicaoPagamento = (CSCondicoesPagamento.CSCondicaoPagamento)((CSItemCombo)cboCondicaoPagamento.SelectedItem).Valor;

            CSPDVs.Current.PEDIDOS_PDV.Current.CONDICAO_PAGAMENTO = CondicaoPagamento;

            if (CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA == 2)
            {
                // [ Invalida objeto de cálculo de preços broker ]
                CSPDVs.Current.POLITICA_BROKER = null;
                CSPDVs.Current.POLITICA_BROKER_2014 = null;

                Type t;

                if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                    t = CSPDVs.Current.POLITICA_BROKER_2014.GetType();
                else
                    t = CSPDVs.Current.POLITICA_BROKER.GetType();
            }
        }
    }
}