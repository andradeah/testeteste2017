using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using AvanteSales.Pro.Dialogs;
using AvanteSales.Pro.Formatters;
using AvanteSales.SystemFramework;
using AvanteSales.SystemFramework.BusinessLayer;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "ProdutoIndenizacao", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ProdutoIndenizacao : AppCompatActivity
    {
        TextView lblCodPdv;
        TextView lblNomePdv;
        Android.Support.V7.Widget.Toolbar tbToolbar;
        TextView lblCodigoProduto;
        TextView lblProduto;
        TextView lblTaxaIndenizacao;
        TextView lblValorSemIndenizacao;
        TextView lblValorTotalItem;
        TextView lblValorUnitario;
        TextView lblPeso;
        TextView lblQtdeUnitaria;
        Spinner cboMotivo;
        EditText txtTaxaIndenizacao;
        EditText txtQtdeInteiro;
        EditText txtQtdeUnidade;
        Button btnCalcular;
        Button btnCancelar;
        private bool m_IsDirty;
        CSItemsIndenizacao.CSItemIndenizacao item = null;
        private static int ultimoMotivo;

        public bool IsDirty
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

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            IsDirty = true;

            SetContentView(Resource.Layout.produto_indenizacao);

            FindViewsById();

            CarregarComboMotivo();

            cboMotivo.SetSelection(ultimoMotivo);

            CarregarDados();

            Eventos();

            SetSupportActionBar(tbToolbar);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(false);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    OnBackPressed();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void CarregarComboMotivo()
        {
            cboMotivo.Clear();

            var adapter = cboMotivo.SetDefaultAdapter();

            var motivos = new CSMotivosIndenizacao();
            // Preenche o combo
            foreach (CSMotivosIndenizacao.CSMotivoIndenizacao motivoAtual in motivos)
            {
                CSItemCombo ic = new CSItemCombo();

                ic.Texto = motivoAtual.DSC_MOTIVO_INDENIZACAO;
                ic.Valor = motivoAtual;

                adapter.Add(ic);
            }
        }

        protected override void OnStart()
        {
            base.OnStart();

            item = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current;

            if (item.PRODUTO.COD_UNIDADE_MEDIDA == "UN")
            {
                lblQtdeUnitaria.Visibility = ViewStates.Gone;
                txtQtdeUnidade.Visibility = ViewStates.Gone;
            }

            if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.STATE != ObjectState.NOVO)
                CarregarInformacoesSalvas();
        }

        private void CarregarInformacoesSalvas()
        {
            var itemIndenizado = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current;

            txtTaxaIndenizacao.Text = itemIndenizado.PCT_TAXA_INDENIZACAO.ToString();
            //txtQtdeInteiro.Text = (itemIndenizado.QTD_INDENIZACAO / itemIndenizado.PRODUTO.QTD_UNIDADE_EMBALAGEM).ToString();
            txtQtdeInteiro.Text = item.QTD_INDENIZACAO_INTEIRA.ToString();
            txtQtdeUnidade.Text = item.QTD_INDENIZACAO_UNIDADE.ToString();
            btnCalcular_Click(null, null);
        }

        private void Eventos()
        {
            btnCalcular.Click += new EventHandler(btnCalcular_Click);
            txtTaxaIndenizacao.TextChanged += new EventHandler<Android.Text.TextChangedEventArgs>(txtTaxaIndenizacao_TextChanged);
            txtQtdeInteiro.TextChanged += new EventHandler<Android.Text.TextChangedEventArgs>(txtQtdeInteiro_TextChanged);
            cboMotivo.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(cboMotivo_ItemSelected);
            btnCancelar.Click += new EventHandler(btnCancelar_Click);
        }

        void btnCancelar_Click(object sender, EventArgs e)
        {
            CancelarIndenizacao();
        }

        private void CancelarIndenizacao()
        {
            MessageBox.Alert(this, "Deseja cancelar a indenização para este produto?", "Cancelar indenização", (sender, e) =>
            {
                base.OnBackPressed();
            },true);
        }

        void cboMotivo_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (!IsDirty)
                IsDirty = true;
        }

        private void LimparUltimoCaractereEPosicionarCursor(EditText campo)
        {
            campo.Text = campo.Text.Remove(campo.Text.Length - 1);
            campo.SetSelection(campo.Text.Length);
        }

        private bool CaracteresValidosQtdInteira()
        {
            if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO.COD_UNIDADE_MEDIDA == "KG" ||
                CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO.COD_UNIDADE_MEDIDA == "LT")
            {
                if (StringFormatter.NaoDecimal(txtQtdeInteiro.Text))
                {
                    LimparUltimoCaractereEPosicionarCursor(txtQtdeInteiro);
                    return false;
                }

                if (txtQtdeInteiro.Text.Contains(","))
                {
                    int posicao = txtQtdeInteiro.Text.IndexOf(',');

                    if (txtQtdeInteiro.Text.Substring(posicao + 1, txtQtdeInteiro.Text.Length - posicao - 1).Length > 2)
                    {
                        LimparUltimoCaractereEPosicionarCursor(txtQtdeInteiro);
                        return false;
                    }
                }
            }
            else
            {
                if (txtQtdeInteiro.Text != string.Empty &&
                    txtQtdeInteiro.Text.Contains(","))
                {
                    LimparUltimoCaractereEPosicionarCursor(txtQtdeInteiro);
                    return false;
                }
            }

            return true;
        }

        void txtQtdeInteiro_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtQtdeInteiro.Text))
                if (txtQtdeInteiro.Text.Contains("."))
                {
                    txtQtdeInteiro.Text = txtQtdeInteiro.Text.Replace(".", ",");
                    txtQtdeInteiro.SetSelection(txtQtdeInteiro.Text.Length);
                }

            if (!IsDirty)
                IsDirty = true;

            if (txtQtdeInteiro.Text != string.Empty)
            {
                if (!CaracteresValidosQtdInteira())
                    return;
            }
        }

        void txtTaxaIndenizacao_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (!IsDirty)
                IsDirty = true;

            if (!string.IsNullOrEmpty(txtTaxaIndenizacao.Text))
                if (txtTaxaIndenizacao.Text.Contains("."))
                {
                    txtTaxaIndenizacao.Text = txtTaxaIndenizacao.Text.Replace(".", ",");
                    txtTaxaIndenizacao.SetSelection(txtTaxaIndenizacao.Text.Length);
                }
        }

        void btnCalcular_Click(object sender, EventArgs e)
        {
            if (DadosValidosEPreenchidos())
            {
                if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                    CSPDVs.Current.POLITICA_BROKER_2014.INDENIZACAO = false;
                else
                    CSPDVs.Current.POLITICA_BROKER.INDENIZACAO = false;

                item.QTD_INDENIZACAO_INTEIRA = CSGlobal.StrToInt(txtQtdeInteiro.Text);
                item.QTD_INDENIZACAO_UNIDADE = CSGlobal.StrToInt(txtQtdeUnidade.Text);
                item.PCT_TAXA_INDENIZACAO = Convert.ToDecimal(txtTaxaIndenizacao.Text);

                if (item.STATE != ObjectState.NOVO)
                    item.PRODUTO = CSProdutos.GetProduto(item.COD_PRODUTO);
                else
                    item.PRODUTO = CSProdutos.Current;

                if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                    item.CalculaValor2014();
                else
                    item.CalculaValor();

                decimal taxaIndenizacao = 0m;

                if (txtTaxaIndenizacao.Text != string.Empty)
                    taxaIndenizacao = (Convert.ToDecimal(txtTaxaIndenizacao.Text) / 100);
                else
                    taxaIndenizacao = 0m;

                decimal valorInteiro = item.QTD_INDENIZACAO_INTEIRA * item.VLR_INDENIZACAO;
                decimal valorDecimal = (item.VLR_INDENIZACAO / item.PRODUTO.QTD_UNIDADE_EMBALAGEM) * item.QTD_INDENIZACAO_UNIDADE;

                decimal valorIndenizacaoInteiro = item.QTD_INDENIZACAO_INTEIRA * (item.VLR_INDENIZACAO * (item.PCT_TAXA_INDENIZACAO / 100));
                decimal valorIndenizacaoDecimal = ((item.VLR_INDENIZACAO * (item.PCT_TAXA_INDENIZACAO / 100)) / item.PRODUTO.QTD_UNIDADE_EMBALAGEM) * item.QTD_INDENIZACAO_UNIDADE;

                lblValorSemIndenizacao.Text = (valorInteiro + valorDecimal).ToString(CSGlobal.DecimalStringFormat);
                lblValorTotalItem.Text = (valorIndenizacaoInteiro + valorIndenizacaoDecimal).ToString(CSGlobal.DecimalStringFormat);
                lblTaxaIndenizacao.Text = (item.VLR_INDENIZACAO * taxaIndenizacao).ToString(CSGlobal.DecimalStringFormat);
                lblValorUnitario.Text = valorIndenizacaoInteiro == 0 ? (1 * (item.VLR_INDENIZACAO * (item.PCT_TAXA_INDENIZACAO / 100))).ToString(CSGlobal.DecimalStringFormat) : valorIndenizacaoInteiro.ToString(CSGlobal.DecimalStringFormat);
                lblPeso.Text = ((item.QTD_INDENIZACAO_INTEIRA * item.PRODUTO.VLR_PESO_PRODUTO) + (item.PRODUTO.VLR_PESO_PRODUTO / item.PRODUTO.QTD_UNIDADE_EMBALAGEM) * item.QTD_INDENIZACAO_UNIDADE).ToString(CSGlobal.DecimalStringFormat);

                item.VLR_INDENIZACAO = Math.Round((valorIndenizacaoInteiro + valorIndenizacaoDecimal), 2, MidpointRounding.AwayFromZero);
            }
        }

        private void CarregarDados()
        {
            lblCodPdv.Text = CSPDVs.Current.COD_PDV.ToString();
            lblNomePdv.Text = CSPDVs.Current.DSC_RAZAO_SOCIAL;
            lblCodigoProduto.Text = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO.DESCRICAO_APELIDO_PRODUTO;
            lblProduto.Text = Produtos.ExibirDescricaoProduto == false ? CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO.DSC_APELIDO_PRODUTO : CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO.DSC_PRODUTO;

            var motivos = new CSMotivosIndenizacao();
            int i = 0;

            foreach (CSMotivosIndenizacao.CSMotivoIndenizacao motivoAtual in motivos)
            {
                if (motivoAtual.COD_MOTIVO_INDENIZACAO == CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.MOTIVO_INDENIZACAO)
                    cboMotivo.SetSelection(i);

                i++;
            }
        }

        private void FindViewsById()
        {
            lblCodPdv = FindViewById<TextView>(Resource.Id.lblCodPdv);
            lblNomePdv = FindViewById<TextView>(Resource.Id.lblNomePdv);
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            lblCodigoProduto = FindViewById<TextView>(Resource.Id.lblCodigoProduto);
            lblProduto = FindViewById<TextView>(Resource.Id.lblProduto);
            lblTaxaIndenizacao = FindViewById<TextView>(Resource.Id.lblTaxaIndenizacao);
            lblValorSemIndenizacao = FindViewById<TextView>(Resource.Id.lblValorSemIndenizacao);
            lblValorTotalItem = FindViewById<TextView>(Resource.Id.lblValorTotalItem);
            lblValorUnitario = FindViewById<TextView>(Resource.Id.lblValorUnitario);
            lblPeso = FindViewById<TextView>(Resource.Id.lblPeso);
            lblQtdeUnitaria = FindViewById<TextView>(Resource.Id.lblQtdeUnitaria);
            txtTaxaIndenizacao = FindViewById<EditText>(Resource.Id.txtTaxaIndenizacao);
            txtQtdeInteiro = FindViewById<EditText>(Resource.Id.txtQtdeInteiro);
            txtQtdeUnidade = FindViewById<EditText>(Resource.Id.txtQtdeUnidade);
            btnCalcular = FindViewById<Button>(Resource.Id.btnCalcular);
            cboMotivo = FindViewById<Spinner>(Resource.Id.cboMotivo);
            btnCancelar = FindViewById<Button>(Resource.Id.btnCancelar);
        }

        public override void OnBackPressed()
        {
            if (IsDirty)
            {
                if (DadosValidosEPreenchidos())
                {
                    btnCalcular_Click(null, null);

                    item.COD_INDENIZACAO = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.COD_INDENIZACAO;
                    item.COD_PRODUTO = item.PRODUTO.COD_PRODUTO;
                    item.QTD_INDENIZACAO = (item.QTD_INDENIZACAO_INTEIRA * item.PRODUTO.QTD_UNIDADE_EMBALAGEM) + item.QTD_INDENIZACAO_UNIDADE;
                    item.MOTIVO_INDENIZACAO = ((CSMotivosIndenizacao.CSMotivoIndenizacao)((CSItemCombo)cboMotivo.SelectedItem).Valor).COD_MOTIVO_INDENIZACAO;
                    item.PCT_TAXA_INDENIZACAO = Convert.ToDecimal(txtTaxaIndenizacao.Text);
                    item.VLR_INDENIZACAO = Convert.ToDecimal(lblValorTotalItem.Text);
                    item.VOLUME_INDENIZACAO = item.QTD_INDENIZACAO_INTEIRA;
                    item.PESO = Convert.ToDecimal(lblPeso.Text);
                    item.VLR_UNITARIO_INDENIZACAO = Convert.ToDecimal(lblValorUnitario.Text);

                    if (item.STATE == ObjectState.NOVO)
                        CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Add(item);

                    item.AtualizaImagem();

                    SetResult(Result.Ok);

                    base.OnBackPressed();

                    ultimoMotivo = cboMotivo.SelectedItemPosition;
                }
            }
            else
                base.OnBackPressed();
        }

        private bool DadosValidosEPreenchidos()
        {
            if (txtTaxaIndenizacao.Text == string.Empty)
            {
                MessageBox.Alert(this, "Taxa de indenização é obrigatória.");
                return false;
            }
            else
            {
                decimal result;

                if (!decimal.TryParse(txtTaxaIndenizacao.Text, out result))
                {
                    MessageBox.Alert(this, "Taxa de indenização inválida");
                    return false;
                }

                if (txtTaxaIndenizacao.Text == "0")
                {
                    MessageBox.Alert(this, "Taxa de indenização não pode ser 0.");
                    return false;
                }
                else if (!CSPDVs.Current.IND_ESPECIAL_INDENIZACAO_BROKER &&
                        (Convert.ToDecimal(txtTaxaIndenizacao.Text) > CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO.PCT_TAXA_MAX_INDENIZACAO))
                {
                    MessageBox.Alert(this, "Taxa de indenização maior que a permitida ( "
                        + CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO.PCT_TAXA_MAX_INDENIZACAO + "% ).");
                    return false;
                }
                else if (Convert.ToDecimal(txtTaxaIndenizacao.Text) > 100)
                {
                    MessageBox.Alert(this, "Taxa de Indenização não pode ser maior que 100%.");
                    return false;
                }
            }

            int inteiro = string.IsNullOrEmpty(txtQtdeInteiro.Text) ? 0 : Convert.ToInt32(txtQtdeInteiro.Text);
            int unitaria = string.IsNullOrEmpty(txtQtdeUnidade.Text) ? 0 : Convert.ToInt32(txtQtdeUnidade.Text);

            if ((string.IsNullOrEmpty(txtQtdeInteiro.Text) &&
                string.IsNullOrEmpty(txtQtdeUnidade.Text)) ||
                (inteiro == 0 && unitaria == 0))
            {
                MessageBox.Alert(this, "Quantidade de indenização é obrigatória.");
                return false;
            }

            if (!string.IsNullOrEmpty(txtQtdeUnidade.Text))
            {
                if (Convert.ToInt32(txtQtdeUnidade.Text) >= CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Current.PRODUTO.QTD_UNIDADE_EMBALAGEM)
                {
                    MessageBox.Alert(this, "Quantidade unitária é maior ou igual à quantidade da embalagem.");
                    return false;
                }
            }

            return true;
        }
    }
}