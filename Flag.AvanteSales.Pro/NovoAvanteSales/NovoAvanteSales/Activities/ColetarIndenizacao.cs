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
using AvanteSales.Pro.Controles;
using AvanteSales.Pro.Dialogs;
using AvanteSales.Pro.Formatters;
using AvanteSales.SystemFramework;

namespace AvanteSales.Pro.Activities
{
    [Activity(Label = "ColetarIndenizacao", ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/AvanteSalesTheme")]
    public class ColetarIndenizacao : AppCompatActivity
    {
        Android.Support.V7.Widget.Toolbar tbToolbar;
        TextView lblCodPdv;
        TextView lblNomePdv;
        EditText dpCadastro;
        EditText dpDevolucao;
        TextView lblIndenizacao;
        TextView lblVendedor;
        TextView lblVolume;
        TextView lblValor;
        TextView lblPesoTotal;
        EditText txtNumNota;
        EditText txtResponsavel;
        EditText txtSerie;
        Spinner cboGrupoComercializacao;
        Spinner cboCondicaoPagamento;
        Button btnNovoProduto;
        Button btnListaProdutos;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            CSTiposDistribPolicitcaPrecos.Current.COD_TIPO_DISTRIBUICAO_POLITICA = 2;
            SetContentView(Resource.Layout.coletar_indenizacao);

            FindViewsById();

            CarregarDadosIniciais();

            CarregarComboGrupoComercializacao();

            OperacaoPadrao();

            CarregarCondicaoPagamento();

            Eventos();

            SetSupportActionBar(tbToolbar);
            SupportActionBar.SetHomeButtonEnabled(true);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowTitleEnabled(false);

            dpCadastro.AddTextChangedListener(new Mask(dpCadastro, "##/##/####"));
            dpDevolucao.AddTextChangedListener(new Mask(dpDevolucao, "##/##/####"));
        }

        private void OperacaoPadrao()
        {
            var adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);

            for (int i = 0; i < CSPDVs.Current.OPERACOES.Count; i++)
            {
                CSOperacoes.CSOperacao operacao = (CSOperacoes.CSOperacao)CSPDVs.Current.OPERACOES[i];

                if (operacao.COD_OPERACAO_CFO == 1)
                    CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.OPERACAO = operacao;
            }
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

        private void CarregarCondicaoPagamento()
        {
            var adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            cboCondicaoPagamento.Adapter = adapter;
            int condicoesInativasAteAgora = 0;
            //int PosicaoCondicaoPadrao = 0;
            // Preenche o combo de condições de pagamento
            for (int i = 0; i < CSCondicoesPagamento.Items.Count; i++)
            {
                CSCondicoesPagamento.CSCondicaoPagamento condpag = CSCondicoesPagamento.Items[i];
                if (condpag.IND_ATIVO == true)
                {
                    int PrioridadeCondicaoPagamento = condpag.PRIORIDADE_CONDICAO_PAGAMENTO;
                    int PrioridadeCondicaoPagamentoCspdvCurrent = CSPDVs.Current.CONDICAO_PAGAMENTO.PRIORIDADE_CONDICAO_PAGAMENTO;
                    int OperacaoCFO = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.OPERACAO.COD_OPERACAO_CFO;
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
                                if ((CSPDVs.Current.CODCNDPGT == 1) || (CSPDVs.Current.CODCNDPGT == 2))
                                {
                                    if (condpag.CODPRZCLIENTE == 0)
                                    {
                                        ((ArrayAdapter)cboCondicaoPagamento.Adapter).Add(ic);
                                    }
                                }
                                else
                                {
                                    ((ArrayAdapter)cboCondicaoPagamento.Adapter).Add(ic);
                                }
                            }
                        }
                        else
                        {
                            ((ArrayAdapter)cboCondicaoPagamento.Adapter).Add(ic);
                        }
                    }
                }
                else
                    condicoesInativasAteAgora++;
            }


            if (cboCondicaoPagamento.Adapter != null)
            {
                for (int x = 0; x < cboCondicaoPagamento.Adapter.Count; x++)
                {
                    if (((CSCondicoesPagamento.CSCondicaoPagamento)((CSItemCombo)cboCondicaoPagamento.Adapter.GetItem(x)).Valor).COD_CONDICAO_PAGAMENTO == CSPDVs.Current.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO)
                    {
                        cboCondicaoPagamento.SetSelection(x);
                    }
                }
            }
        }

        private void Eventos()
        {
            btnNovoProduto.Click += new EventHandler(btnNovoProduto_Click);
            btnListaProdutos.Click += new EventHandler(btnListaProdutos_Click);
            cboCondicaoPagamento.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(cboCondicaoPagamento_ItemSelected);
        }

        void cboCondicaoPagamento_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            try
            {
                CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.CONDICAO_PAGAMENTO = ((CSCondicoesPagamento.CSCondicaoPagamento)((CSItemCombo)cboCondicaoPagamento.SelectedItem).Valor);

                // [ Invalida objeto de cálculo de preços broker ]
                CSPDVs.Current.POLITICA_BROKER = null;
                CSPDVs.Current.POLITICA_BROKER_2014 = null;

                Type t;

                if (CSEmpresa.Current.IND_UTILIZA_PRICE_2014)
                    t = CSPDVs.Current.POLITICA_BROKER_2014.GetType();
                else
                    t = CSPDVs.Current.POLITICA_BROKER.GetType();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Falha ao inicializar tabela de variáveis!"))
                {
                    MessageBox.Alert(this, ex.Message,"OK", (_sender, _e) => { base.Finish(); },false);
                    btnNovoProduto.Visibility = ViewStates.Gone;
                }
                else
                    MessageBox.Alert(this, ex.Message);
            }
        }

        void btnListaProdutos_Click(object sender, EventArgs e)
        {
            if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Where(p => p.STATE != ObjectState.DELETADO).Count() > 0)
            {
                Intent i = new Intent();
                i.SetClass(this, typeof(ListaProdutosIndenizacao));
                this.StartActivity(i);
            }
        }

        void btnNovoProduto_Click(object sender, EventArgs e)
        {
            if (DadosPreenchidos())
            {
                GuardarInformacoes();
                Intent i = new Intent();
                i.SetClass(this, typeof(Produtos));
                i.PutExtra("txtDescontoIndenizacao", "0");
                i.PutExtra("txtAdf", "0");
                this.StartActivity(i);
            }
            else
                MessageBox.Alert(this, "Todos os dados devem ser preenchidos para a inclusão de itens.");
        }

        private bool DadosPreenchidos()
        {
            if (txtNumNota.Text != string.Empty &&
                txtSerie.Text != string.Empty &&
                txtResponsavel.Text != string.Empty &&
                (cboGrupoComercializacao.Adapter != null || cboGrupoComercializacao.Adapter.Count > 0))
                return true;
            else
                return false;
        }

        private void CarregarComboGrupoComercializacao()
        {
            // Limpa o combo
            cboGrupoComercializacao.Clear();

            var adapter = cboGrupoComercializacao.SetDefaultAdapter();

            CSGruposComercializacao grupos = new CSGruposComercializacao(true);

            // Preenche o combo
            foreach (CSGruposComercializacao.CSGrupoComercializacao grp in grupos)
            {
                CSItemCombo ic = new CSItemCombo();
                ic.Texto = grp.DES_GRUPO_COMERCIALIZACAO;
                ic.Valor = grp;
                adapter.Add(ic);
            }

            // Coloca como default o último grupo.
            if (cboGrupoComercializacao.Adapter.Count > 0)
                cboGrupoComercializacao.SetSelection(0);
        }

        protected override void OnStart()
        {
            CarregarInformacoes();

            btnListaProdutos.Text = "Lista ( " +
             CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Where(p => p.STATE != ObjectState.DELETADO).Count().ToString() +
             " )";

            base.OnStart();
        }

        public override void OnBackPressed()
        {
            if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Where(p => p.STATE != ObjectState.DELETADO).Count() > 0)
            {
                GuardarInformacoes();
                CSPDVs.Current.PEDIDOS_INDENIZACAO.Flush();
            }
            else if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Where(p => p.STATE != ObjectState.DELETADO).Count() == 0 &&
                    CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.STATE != ObjectState.NOVO)
            {
                CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.STATE = ObjectState.DELETADO;
                CSPDVs.Current.PEDIDOS_INDENIZACAO.Flush();
            }

            CSPDVs.Current.PEDIDOS_INDENIZACAO.Dispose();

            base.OnBackPressed();
        }

        private void GuardarInformacoes()
        {
            CSIndenizacoes.CSIndenizacao indenizacao = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current;

            indenizacao.COD_GRUPO_COMERCIALIZACAO = ((CSGruposComercializacao.CSGrupoComercializacao)((CSItemCombo)cboGrupoComercializacao.SelectedItem).Valor).COD_GRUPO_COMERCIALIZACAO;
            indenizacao.CONDICAO_PAGAMENTO = ((CSCondicoesPagamento.CSCondicaoPagamento)((CSItemCombo)cboCondicaoPagamento.SelectedItem).Valor);
            indenizacao.DAT_CADASTRO = Convert.ToDateTime(dpCadastro.Text);

            if (dpDevolucao.Text != string.Empty)
                indenizacao.DAT_NOTA_DEVOLUCAO = Convert.ToDateTime(dpDevolucao.Text);

            indenizacao.NOME_RESPONSAVEL = txtResponsavel.Text;
            indenizacao.NUM_NOTA_DEVOLUCAO = Convert.ToInt32(txtNumNota.Text);
            indenizacao.SERIE_NOTA = txtSerie.Text;
            indenizacao.STATUS = "A";
            indenizacao.VLR_TOTAL = Convert.ToDecimal(lblValor.Text);
            indenizacao.VOLUME_INDENIZACAO = Convert.ToDecimal(lblVolume.Text);
            //indenizacao.PESO_BRUTO = indenizacao.ITEMS_INDENIZACAO.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Sum(p => p.PESO);
        }

        private void CarregarInformacoes()
        {
            lblIndenizacao.Text = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.COD_INDENIZACAO.ToString();
            lblVendedor.Text = CSEmpregados.Current.COD_EMPREGADO.ToString();
            lblValor.Text = (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Where(b => b.STATE != ObjectState.DELETADO).Sum(p => p.VLR_INDENIZACAO)).ToString(CSGlobal.DecimalStringFormat);
            lblVolume.Text = (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Where(b => b.STATE != ObjectState.DELETADO).Sum(p => p.VOLUME_INDENIZACAO)).ToString(CSGlobal.DecimalStringFormat);

            CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.PESO_BRUTO = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Sum(p => p.PESO);
            lblPesoTotal.Text = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.PESO_BRUTO.ToString(CSGlobal.DecimalStringFormat);

            if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.ITEMS_INDENIZACAO.Cast<CSItemsIndenizacao.CSItemIndenizacao>().Where(i => i.STATE != ObjectState.DELETADO).Count() > 0)
            //CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.STATE != ObjectState.NOVO)
            {
                dpDevolucao.Enabled = false;

                txtNumNota.Text = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.NUM_NOTA_DEVOLUCAO.ToString();
                txtSerie.Text = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.SERIE_NOTA;
                txtResponsavel.Text = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.NOME_RESPONSAVEL;
                dpDevolucao.Text = CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.DAT_NOTA_DEVOLUCAO.HasValue ? CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.DAT_NOTA_DEVOLUCAO.Value.ToString("dd/MM/yyyy") : string.Empty;

                CSGruposComercializacao classeGrupoComercializacao = new CSGruposComercializacao();
                var grupos = classeGrupoComercializacao.GrupoComercializacaoFiltrado();
                int i = 0;

                for (i = 0; i < cboGrupoComercializacao.Adapter.Count; i++)
                {
                    var condgpr = ((CSGruposComercializacao.CSGrupoComercializacao)((CSItemCombo)cboGrupoComercializacao.Adapter.GetItem(i)).Valor);

                    if (condgpr.COD_GRUPO_COMERCIALIZACAO == CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.COD_GRUPO_COMERCIALIZACAO)
                        cboGrupoComercializacao.SetSelection(i);
                }

                cboGrupoComercializacao.Enabled = false;

                for (i = 0; i < cboCondicaoPagamento.Adapter.Count; i++)
                {
                    var condpag = ((CSCondicoesPagamento.CSCondicaoPagamento)((CSItemCombo)cboCondicaoPagamento.Adapter.GetItem(i)).Valor);

                    if (condpag.COD_CONDICAO_PAGAMENTO == CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.CONDICAO_PAGAMENTO.COD_CONDICAO_PAGAMENTO)
                        cboCondicaoPagamento.SetSelection(i);
                }

                cboCondicaoPagamento.Enabled = false;
            }
            else
            {
                cboGrupoComercializacao.Enabled = true;
                cboCondicaoPagamento.Enabled = true;
            }
        }

        private void CarregarDadosIniciais()
        {
            lblCodPdv.Text = CSPDVs.Current.COD_PDV.ToString();
            lblNomePdv.Text = CSPDVs.Current.DSC_RAZAO_SOCIAL;
            string dataAtual = DateTime.Now.ToString("dd/MM/yyyy");
            dpCadastro.Text = dataAtual;

            if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current != null)
            {
                if (CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.STATE == ObjectState.NOVO)
                {
                    CSPDVs.Current.PEDIDOS_INDENIZACAO.Current.COD_INDENIZACAO = CSIndenizacoes.ProximoCodigoIndenizacao();
                }
            }
        }

        private void FindViewsById()
        {
            lblCodPdv = FindViewById<TextView>(Resource.Id.lblCodPdv);
            lblNomePdv = FindViewById<TextView>(Resource.Id.lblNomePdv);
            tbToolbar = FindViewById<Android.Support.V7.Widget.Toolbar>(Resource.Id.tbToolbar);
            dpCadastro = FindViewById<CustomDatePicker>(Resource.Id.dpCadastro);
            dpDevolucao = FindViewById<CustomDatePicker>(Resource.Id.dpDevolucao);
            lblIndenizacao = FindViewById<TextView>(Resource.Id.lblIndenizacao);
            lblVendedor = FindViewById<TextView>(Resource.Id.lblVendedor);
            lblVolume = FindViewById<TextView>(Resource.Id.lblVolume);
            lblValor = FindViewById<TextView>(Resource.Id.lblValor);
            txtNumNota = FindViewById<EditText>(Resource.Id.txtNumNota);
            txtResponsavel = FindViewById<EditText>(Resource.Id.txtResponsavel);
            txtSerie = FindViewById<EditText>(Resource.Id.txtSerie);
            cboGrupoComercializacao = FindViewById<Spinner>(Resource.Id.cboGrupoComercializacao);
            cboCondicaoPagamento = FindViewById<Spinner>(Resource.Id.cboCondicaoPagamento);
            btnNovoProduto = FindViewById<Button>(Resource.Id.btnNovoProduto);
            btnListaProdutos = FindViewById<Button>(Resource.Id.btnListaProdutos);
            lblPesoTotal = FindViewById<TextView>(Resource.Id.lblPesoTotal);
        }
    }
}